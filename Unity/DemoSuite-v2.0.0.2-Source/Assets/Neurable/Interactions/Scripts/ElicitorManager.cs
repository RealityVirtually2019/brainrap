/*
 * Copyright 2017 Neurable Inc.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Neurable.Core;

namespace Neurable.Interactions
{
    /*
     * An Implementation of the NeurableContext meant for manual triggering and registration of flashes.
     * assigns its own groups, sets datastream metadata, and registers randomized flash events with the Neurable API
     */
    public class ElicitorManager : MonoBehaviour
    {
        [Header("Initialization Options")]
        public bool ActivateOnStart = true;
        public bool MakeTagsVisible = true; // Make tags visible to API at start

        [Header("Advanced Settings")]
        public bool OverrideUserSettings = false;
        public bool useEyeWeighting = true;
        public API.Types.Sensitivity newSensitivity;
        public List<NeurableTag> TagObjects;
        private List<NeurableTag> sortedTagList;

        [Header("Flash Options")]
        public float DelayMultiplier1Object = 2f; // delay between flashes
        public float DelayMultiplier2Object = 1f; // delay between flashes
        public float flashDelayBase = 0.1f;       // delay between flashes

        public enum FlashLogic
        {
            FOVEATED,
            SINGLE
        };

        public FlashLogic flashLogic = FlashLogic.FOVEATED;

        [Header("FOVEATED Flash Options")]
        public int numTagsToGatherUsingEyes = 2;
        public float maxEyeDistance = 50f;
        public float foveatedActivityTimer = 1f;
        public bool EyeDebugMode = false;
        protected TimedTagEnable timedTags;

        [HideInInspector]
        public int numRows = 0, numCols = 0;

        protected List<NeurableTag[]> groups;

        protected NeurableTag[] lastFlashed;
        protected System.Random RNG;

        protected bool isAnimating = false;
        protected bool keepAnimating = false;

        delegate void UpdatePositionCast(bool forceRegistration = false);

        UpdatePositionCast UpdatePositions;

        #region MonoBehavior Triggers

        protected virtual void Awake()
        {
            groups = new List<NeurableTag[]>();
            timedTags = new TimedTagEnable(foveatedActivityTimer);
            RNG = new System.Random();
        }

        protected virtual void Start()
        {
            if (!NeurableUser.Instantiated)
                throw new MissingComponentException("Manual Context Requires NeurableUser Component");

            if (OverrideUserSettings)
            {
                NeurableUser.Instance.UseHybridEyeSystem = useEyeWeighting;
                NeurableUser.Instance.HybridSensitivity = newSensitivity;
            }

            gatherTags();
            if (ActivateOnStart) StartAnim();
        }

        protected virtual void Update()
        {
            if (flashLogic == FlashLogic.FOVEATED) timedTags.SubtractTime(Time.deltaTime);
        }

        protected virtual void OnEnable()
        {
            if (ActivateOnStart) StartAnim();
        }

        protected virtual void OnDisable()
        {
            StopAnim();
        }

        #endregion

        #region Tag Assignment

        // Search hierarchy for NeurableTag Objects. Only looks through children
        public void gatherTags()
        {
            if (TagObjects != null && TagObjects.Count > 0) removeTags(TagObjects.ToArray());
            NeurableTag[] tagobjs = GetComponentsInChildren<NeurableTag>();
            addTags(tagobjs);
        }

        public virtual void addTags(NeurableTag[] t)
        {
            bool preserveAnim = isAnimating;
            StopAnim();
            if (t != null)
            {
                if (TagObjects == null) TagObjects = new List<NeurableTag>();
                for (int i = 0; i < t.Length; ++i)
                {
                    NeurableTag tobj = t[i];
                    if (MakeTagsVisible && flashLogic != FlashLogic.FOVEATED)
                        tobj.NeurableEnabled = true;
                    else
                        tobj.NeurableEnabled = false;
                    if (!TagObjects.Contains(tobj))
                    {
                        UpdatePositions += tobj.UpdatePosition;
                        TagObjects.Add(tobj);
                    }
                }
            }

            if (preserveAnim) StartAnim();
        }

        public virtual void addTag(NeurableTag t)
        {
            NeurableTag[] t_arr = {t};
            addTags(t_arr);
        }

        public virtual void removeTags(NeurableTag[] t)
        {
            bool preserveAnimation = isAnimating;
            StopAnim();
            if (TagObjects != null && TagObjects.Count > 0)
            {
                foreach (NeurableTag tobj in t)
                {
                    UpdatePositions -= tobj.UpdatePosition;
                    tobj.NeurableEnabled = false;
                    TagObjects.Remove(tobj);
                }
            }

            if (preserveAnimation) StartAnim();
        }

        public virtual void removeTag(NeurableTag t)
        {
            NeurableTag[] t_arr = {t};
            removeTags(t_arr);
        }

        #endregion

        #region Animation Loop Handling

        protected Coroutine animationCoroutine;

        public virtual void StartAnim()
        {
            if (TagObjects != null && TagObjects.Count > 0 && !isAnimating && isActiveAndEnabled
                && gameObject.activeInHierarchy)
            {
                keepAnimating = true;
                createGroups();
                if (animationCoroutine != null) StopCoroutine(animationCoroutine);
                animationCoroutine = StartCoroutine(flashRoutine());
            }
        }

        public virtual void StopAnim()
        {
            keepAnimating = false;
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            isAnimating = false;
            foreach (NeurableTag t in TagObjects)
            {
                t.NeurableEnabled = false;
            }

            if (NeurableUser.Instantiated) NeurableUser.Instance.User.ClearEvents();
        }

        public virtual void PauseAnim(float duration)
        {
            StopAnim();
            Invoke("StartAnim", duration);
        }

        #endregion

        #region Group Generation

        public virtual void createGroups()
        {
            if (groups == null)
                groups = new List<NeurableTag[]>();
            else
            {
                if (flashLogic != FlashLogic.FOVEATED) enableGroups(false);
                groups.Clear();
            }

            if (TagObjects == null || TagObjects.Count == 0) return;

            switch (flashLogic)
            {
                case FlashLogic.FOVEATED:
                    createGroups_eye();
                    break;
                case FlashLogic.SINGLE:
                    createGroups_singleton();
                    break;
                default:
                    createGroups_singleton();
                    break;
            }

            if (flashLogic != FlashLogic.FOVEATED) enableGroups(true);
        }

        // create groups with a signle tag in them
        protected void createGroups_singleton()
        {
            foreach (NeurableTag t in TagObjects)
            {
                NeurableTag[] g = new NeurableTag[1];
                g[0] = t;
                groups.Add(g);
            }
        }

        List<NeurableTag> tagsNearestEye;
        bool dataReady = false;

        public List<NeurableTag> getTagsNearEye()
        {
            if (!dataReady)
            {
                dataReady = (NeurableUser.Instantiated && NeurableUser.Instance.Ready);
            }

            if ((!NeurableUser.Instantiated) || (!dataReady && !EyeDebugMode) || TagObjects == null
                || TagObjects.Count <= 0)
            {
                return new List<NeurableTag>();
            }

            Dictionary<NeurableTag, float> tagDistances = new Dictionary<NeurableTag, float>();
            foreach (NeurableTag t in TagObjects)
            {
                if (!t.NeurableVisible) continue;
                float distance = NeurableUser.Instance.NeurableCam.FocalDistance(t.ProjectedPosition);
                if (distance < maxEyeDistance) tagDistances.Add(t, distance);
            }

            var sortedTagDistances = tagDistances.OrderBy(obj => obj.Value);

            tagsNearestEye = new List<NeurableTag>();
            foreach (KeyValuePair<NeurableTag, float> ntp in sortedTagDistances)
            {
                tagsNearestEye.Add(ntp.Key);
                if (tagsNearestEye.Count >= numTagsToGatherUsingEyes) break;
            }

            return tagsNearestEye;
        }

        protected virtual void createGroups_eye()
        {
            foreach (NeurableTag t in getTagsNearEye())
            {
                timedTags.EnableTag(t);
                NeurableTag[] g = new NeurableTag[1];
                g[0] = t;
                groups.Add(g);
            }
        }

        #endregion

        #region Group Utilities

        // Shuffle the array of groups to generate a unique order
        protected void shuffleGroups(int focusedID = -1)
        {
            if (flashLogic == FlashLogic.FOVEATED) createGroups();
            if (groups == null || groups.Count < 1) return;

            //swap each index with a random position
            for (int i = 0; i < groups.Count; i++)
            {
                int indexSwap = RNG.Next(groups.Count);
                NeurableTag[] temp = groups[i];
                groups[i] = groups[indexSwap];
                groups[indexSwap] = temp;
            }

            //check if focused is in first index
            bool isFocusedFirst = false;
            foreach (NeurableTag _tag in groups[0])
            {
                if (_tag.NeurableID == focusedID)
                {
                    isFocusedFirst = true;
                    break;
                }
            }

            //and if so, swap it with random index
            if (isFocusedFirst)
            {
                int indexSwap = RNG.Next(1, groups.Count - 1);
                NeurableTag[] temp = groups[0];
                groups[0] = groups[indexSwap];
                groups[indexSwap] = temp;
            }

            //check if the first group is == to the last group of the previous sequence. if so, swap it to the end.
            if (lastFlashed != null && groupsEqual(lastFlashed, groups[0]))
            {
                NeurableTag[] temp = groups[0];
                groups[0] = groups[groups.Count - 1];
                groups[groups.Count - 1] = temp;
            }

            lastFlashed = groups[groups.Count - 1];
        }

        public float FlashDelay
        {
            get
            {
                if (groups.Count == 1)
                    return flashDelayBase * DelayMultiplier1Object;
                else if (groups.Count == 2)
                    return flashDelayBase * DelayMultiplier2Object;
                else
                    return flashDelayBase;
            }
            set { flashDelayBase = value; }
        }

        // Set Enabled State of all Groups
        protected void enableGroups(bool state)
        {
            if (groups != null && groups.Count > 0)
            {
                foreach (NeurableTag[] g in groups)
                {
                    foreach (NeurableTag t in g)
                    {
                        t.NeurableEnabled = state;
                    }
                }
            }
        }

        // Checks if 2 groups share any tags to avoid double flashing.
        public static bool groupsEqual(NeurableTag[] g1, NeurableTag[] g2)
        {
            if (g1.Length != g2.Length)
            {
                return false;
            }

            foreach (NeurableTag t in g1)
            {
                int ID = t.NeurableID;
                bool tagInG2 = false;
                foreach (NeurableTag t2 in g2)
                {
                    if (t2.NeurableID == ID)
                    {
                        tagInG2 = true;
                        break;
                    }
                }

                if (!tagInG2)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        // while animation is active, loop through groups and flash them.
        public IEnumerator flashRoutine()
        {
            isAnimating = true;
            while (keepAnimating && isActiveAndEnabled)
            {
                shuffleGroups();
                if (groups.Count == 0) yield return new WaitForSecondsRealtime(FlashDelay);
                foreach (NeurableTag[] g in groups)
                {
                    if (!isAnimating) break;
                    yield return flashGroup(g);
                    yield return new WaitForSecondsRealtime(FlashDelay);
                }
            }

            isAnimating = false;
            yield return null;
        }

        /*
         * Flashes a group of tags, registering the event with the API
         * group: Array of P300Key Components to flash
         * target: int ID of the object with the players attention (training only).
         *			-1 (default) implies no target expects the users attention.
         * Labels:
         *		0: Unknown Data
         *		1: Expected P300
         *		2: Expected NO P300
         */
        protected IEnumerator flashGroup(NeurableTag[] group, int target = -1)
        {
            if (group.Length == 0) yield break;

            if (UpdatePositions != null) UpdatePositions();
            API.Tag[] tags = new API.Tag[group.Length];
            int i = 0;
            int label = 2;
            foreach (NeurableTag b in group)
            {
                if (b == null) yield break;
                b.Animate();
                if (target != -1)
                {
                    if (b.NeurableID == target)
                    {
                        label = 1;
                    }
                }
                else
                {
                    label = 0;
                }

                if (b.NeuroTag == null) yield break;
                tags[i++] = b.NeuroTag;
            }

            RegisterGroupFlashEvent(tags, label);
            yield return new WaitForSecondsRealtime(group[0].flashDuration);
        }

        /*
         * Handle logic for preparing to register an event
         * Set User Metadata as flashes come in
         * Label: 0 = Unknown
         * Label: 1 = Expected P300
         * Label: 2 = Expected NOT P300
         */
        void RegisterGroupFlashEvent(API.Tag[] tagGroup, int label)
        {
            string Trial = "0", NumSequences = "0", Sequence = "0", Group;
            Group = "[";
            for (int i = 0; i < tagGroup.Length; ++i)
            {
                API.Tag t = tagGroup[i];
                Group += t.GetPointer();
                Group += " ";
            }

            Group += "]";

            string[] m_arr = {Trial, NumSequences, Sequence, Group, label.ToString(), "1"};
            string metadata = String.Join(",", m_arr);

            NeurableUser.Instance.User.SetMetaData(metadata);
            switch (label)
            {
                case 1:
                    NeurableUser.Instance.User.RegisterTrainingEvent(tagGroup, true);
                    break;
                case 2:
                    NeurableUser.Instance.User.RegisterTrainingEvent(tagGroup, false);
                    break;
                case 0:
                    NeurableUser.Instance.User.RegisterEvent(tagGroup);
                    break;
            }
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }
    }

    public class TimedTagEnable
    {
        Dictionary<NeurableTag, float> TagMap;
        public float Lifespan = 1f;

        public TimedTagEnable(float lifespan)
        {
            TagMap = new Dictionary<NeurableTag, float>();
            Lifespan = lifespan;
        }

        // Enables the tag and Adds time to the tags timer. Returns true if tag was previously inactive
        public bool EnableTag(NeurableTag t)
        {
            if (t == null) return false;
            bool doChange = !t.NeurableEnabled;
            if (doChange) t.NeurableEnabled = true;
            TagMap[t] = Lifespan;
            return doChange;
        }

        // Enables the tag and Adds time to the tags timer. Returns true if any tags got disabled
        public bool SubtractTime(float delta)
        {
            if (TagMap == null || TagMap.Count == 0) return false;
            bool disabled = false;
            var tags = TagMap.Keys;
            for (int i = 0; i < tags.Count; i++)
            {
                NeurableTag t = tags.ElementAt(i);
                if (t == null) continue;
                if (t.NeurableEnabled)
                {
                    TagMap[t] -= delta;
                    if (TagMap[t] <= 0.0)
                    {
                        t.NeurableEnabled = false;
                        disabled = true;
                    }
                }
            }

            return disabled;
        }
    }
}

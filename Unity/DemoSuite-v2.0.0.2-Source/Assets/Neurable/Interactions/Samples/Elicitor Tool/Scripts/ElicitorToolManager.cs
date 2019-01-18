/*
* Copyright 2017 Neurable Inc.
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Neurable.Interactions.Samples
{
    public class ElicitorToolManager : ElicitorManager
    {
        public static ElicitorToolManager instance;

        [Header("Default Properties")]
        [Range(0.06f, 1f)]
        public float flashDuration = 0.06f;

        [Range(1f, 10f)]
        public float radius = 5f;

        [Range(0, 9)]
        public int numObjects = 3;

        [Range(0.1f, 10)]
        public float objectScale = 1f;

        [Range(0f, 1f)]
        public float intensity = 0.5f;

        [Range(0f, 1f)]
        public float hue = 0.5f;

        private int _selectedPrefab = 0;

        [Header("Neurable Tag Prefabs")]
        public NeurableTag[] tagPrefabOptions;

        [Header("Selection Notification")]
        public GameObject selectionTextNotification;

        [Header("Dropdown Refs")]
        public Dropdown tagPrefabSelector;

        public Dropdown tagElicitorSelector;

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        // Use this for initialization
        protected override void Awake()
        {
            instance = this;
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            if (tagPrefabSelector != null && tagPrefabOptions != null && tagPrefabOptions.Length > 0)
            {
                List<string> options = new List<string>();
                foreach (var t in tagPrefabOptions)
                {
                    options.Add(t.name);
                }

                tagPrefabSelector.AddOptions(options);
            }

            ChangePrefab(_selectedPrefab);
        }

        public void ToggleAnimation()
        {
            if (isAnimating)
                StopAnim();
            else
                StartAnim();
        }

        public void RemoveAllTags()
        {
            if (TagObjects.Count > 0)
            {
                NeurableTag[] ts = TagObjects.ToArray();
                removeTags(ts);
                foreach (var t in ts)
                {
                    Destroy(t.gameObject);
                }
            }
        }

        public void DrawTags()
        {
            if (_selectedPrefab < 0 || tagPrefabOptions.Length <= _selectedPrefab)
            {
                Debug.LogError(_selectedPrefab.ToString() + "/" + tagPrefabOptions.Length);
                throw new UnityException("No Prefabs in Manager");
            }

            if (numObjects == 0) RemoveAllTags();

            NeurableTag[] tagClones = new NeurableTag[numObjects];
            for (int i = 0; i < numObjects; ++i)
            {
                NeurableTag t = Instantiate(tagPrefabOptions[_selectedPrefab], transform);
                t.flashDuration = FlashDuration;
                tagClones[i] = t;
            }

            addTags(tagClones);
            ReorientTags();
            ChangeElicitor(_selectedElicitor);
            RewriteProperties();
        }

        public void RedrawTags()
        {
            bool preserveAnim = isAnimating;
            StopAnim();
            RemoveAllTags();
            DrawTags();
            if (preserveAnim) StartAnim();
        }

        public void ReorientTags()
        {
            if (TagObjects.Count <= 0) return;
            if (TagObjects.Count == 1)
            {
                TagObjects[0].transform.position = transform.position;
                return;
            }

            float radialDivision = 360f / (float) TagObjects.Count;
            for (int i = 0; i < TagObjects.Count; ++i)
            {
                Vector3 placementPosition = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (radialDivision * i + 90)) * radius,
                                                        Mathf.Sin(Mathf.Deg2Rad * (radialDivision * i + 90)) * radius,
                                                        0);
                NeurableTag t = TagObjects[i];
                t.transform.position = transform.position + placementPosition;
                t.transform.localScale =
                    tagPrefabOptions[_selectedPrefab].gameObject.transform.lossyScale * objectScale;
            }
        }

        #region Tag Properties

        public void RewriteProperties()
        {
            Intensity = intensity;
            FlashDuration = flashDuration;
            Radius = radius;
            ObjectScale = objectScale;
            Hue = hue;
        }

        public float ObjectScale
        {
            get { return objectScale; }
            set
            {
                objectScale = value;
                if (TagObjects.Count == 0) return;
                foreach (NeurableTag t in TagObjects)
                {
                    t.transform.localScale = tagPrefabOptions[_selectedPrefab].gameObject.transform.lossyScale * value;
                }
            }
        }

        public int NumObjects
        {
            get { return numObjects; }
            set
            {
                numObjects = value;
                RedrawTags();
                ChangeElicitor(_selectedElicitor);
            }
        }

        public float NumObjectsFloat
        {
            get { return NumObjects; }
            set { NumObjects = (int) value; }
        }

        public float Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                ReorientTags();
            }
        }

        public float FlashDuration
        {
            get { return flashDuration; }
            set
            {
                flashDuration = value;
                foreach (NeurableTag t in TagObjects)
                {
                    t.flashDuration = flashDuration;
                }
            }
        }

        public float Intensity
        {
            get { return intensity; }
            set
            {
                foreach (NeurableTag t in TagObjects)
                {
                    INeurableUtilityBranchable util = t.GetComponent<INeurableUtilityBranchable>();
                    if (util != null)
                    {
                        util.ElicitorIntensity = value;
                    }
                }

                intensity = value;
            }
        }

        public float Hue
        {
            get { return hue; }
            set
            {
                foreach (NeurableTag t in TagObjects)
                {
                    INeurableUtilityBranchable util = t.GetComponent<INeurableUtilityBranchable>();
                    if (util != null)
                    {
                        util.ElicitorHue = value;
                    }
                }

                hue = value;
            }
        }

        #endregion

        #region Prefab/Elicitor Options

        public void ChangePrefab(int option)
        {
            _selectedPrefab = option;
            GenerateElicitorList();
            tagElicitorSelector.value = 0;
            RedrawTags();
        }

        public void GenerateElicitorList()
        {
            if (tagElicitorSelector != null)
            {
                tagElicitorSelector.ClearOptions();
                INeurableUtilityBranchable util = tagPrefabOptions[_selectedPrefab]
                    .GetComponent<INeurableUtilityBranchable>();
                if (util != null) tagElicitorSelector.AddOptions(util.ControllableComponentStrings);
            }
        }

        int _selectedElicitor = 0;

        public void ChangeElicitor(int option)
        {
            _selectedElicitor = option;
            foreach (NeurableTag t in TagObjects)
            {
                INeurableUtilityBranchable util = t.GetComponent<INeurableUtilityBranchable>();
                if (util != null)
                {
                    util.ChangeComponent(option);
                }
            }

            RewriteProperties();
        }

        #endregion

        #region Selection Action

        protected GameObject instancedText;

        public void SpawnSelectionText()
        {
            StopAnim();
            if (selectionTextNotification != null) instancedText = Instantiate(selectionTextNotification, transform);
            if (instancedText != null) Destroy(instancedText, 1.5f);
            Invoke("StartAnim", 1.5f);
        }

        #endregion
    }
}

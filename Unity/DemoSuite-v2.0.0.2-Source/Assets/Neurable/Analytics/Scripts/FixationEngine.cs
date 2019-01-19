using System.Collections.Generic;
using UnityEngine;
using Neurable.Core;

namespace Neurable.Analytics
{
    public class FixationEngine : MonoBehaviour
    {
        public static FixationEngine instance;
        private NeurableCamera gazeEngine;

        public enum RaycastType
        {
            LINE,
            SPHERE,
            LINE_ALL,
            SPHERE_ALL
        };

        [Header("Fixation Settings")]
        [SerializeField,
         Tooltip(
             "Amount of time that the object needs to be looked at until the Engine recognizes it. Can be overriden by objects.")]
        protected float defaultFixationDwellTimer = 0.25f;
        [SerializeField, Tooltip("Amount of time the user must ignore an object for it to end a fixation event.")]
        protected float fixationDecayTimer = 0.25f;

        [Header("Raycast Settings")]
        [SerializeField, Tooltip("What type of Raycast to use for gaze ray.")]
        protected RaycastType raycastType = RaycastType.LINE;
        [SerializeField, Tooltip("Layers that the Gaze Raycast Affects (as with Physics.Raycast)")]
        protected LayerMask raycastLayerMask = Physics.DefaultRaycastLayers;
        [SerializeField, Tooltip("When Using Spherecast, use this radius (as with Physics.Spherecast).")]
        protected float spherecastRadius = 0.5f;

        Dictionary<FixationID, FixationPoint>
            FixationPoints; // Dictionary mapping each FixationID to the proper Object (if alive)
        Dictionary<FixationID, float> FixationTimers;
        Dictionary<FixationID, FixationData> FixationEvents;

        #region MonoBehavior Hooks

        private void Awake()
        {
            if (instance != null) throw new System.Exception("Only One Fixation Engine Allowed");
            instance = this;

            FixationPoints = new Dictionary<FixationID, FixationPoint>();
            FixationTimers = new Dictionary<FixationID, float>();
            FixationEvents = new Dictionary<FixationID, FixationData>();
        }

        private void Start()
        {
            if (!NeurableUser.Instantiated)
                throw new MissingComponentException("Neurable Fixation Engine Requires a Neurable User");
            gazeEngine = NeurableUser.Instance.NeurableCam;
            if (gazeEngine == null)
                throw new MissingComponentException("Neurable Fixation Engine Requires a Neurable Camera");

            if (NeurableMentalStateEngine.instance == null)
                throw new MissingComponentException("Neurable Fixation Engine Requires an Affective State Engine");
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.deltaTime;
            float currentTime = GetUserTime();
            DecayTimers(deltaTime, currentTime);
            DoRayCast(deltaTime, currentTime);
        }

        private void OnDisable()
        {
            EndAllCurrentFixations();
        }

        #endregion

        #region Utilities

        public float GetUserTime()
        {
            // Get the Time as reported by the AffectiveStateEngine
            return NeurableMentalStateEngine.instance.stateTimeline.GetLatestTimestamp();
        }

        public FixationPoint GetFixationPointFromID(FixationID fixation)
        {
            if (FixationPoints.ContainsKey(fixation)) return FixationPoints[fixation];
            return null;
        }

        #endregion

        #region Fixation Handling

        // Reduce the timers of all Fixated objects
        private void DecayTimers(float deltaTime, float currentTime)
        {
            if (FixationTimers == null) return;
            FixationPoint[] keys = GetPointsOfInterest();
            foreach (var point in keys)
            {
                if (!point || !FixationTimers.ContainsKey(point.ID)) continue;
                if (FixationTimers[point.ID] > 0) FixationTimers[point.ID] -= deltaTime;
                if (point.Fixating)
                {
                    if (FixationTimers[point.ID] <= 0f)
                    {
                        if (!FixationEvents.ContainsKey(point.ID))
                        {
                            Debug.LogError("Fixation Point has decayed, but event was never started");
                            continue;
                        }

                        var fixEvent = FixationEvents[point.ID].EndEvent(currentTime);
                        point.LeaveFixation(FixationEvents[point.ID].GetTotalFixationDuration());
                        FixationTimers[point.ID] = 0f;
                        SendEventData(point.ID, fixEvent);
                    }
                }
            }
        }

        private void SendEventData(FixationID point, FixationEvent data)
        {
#if CVR_NEURABLE
            if (data != null && data.FixationDuration < 1f) return;

            var timeline = GetTimelineDuringEvent(data);
            if (timeline == null) return;
            var packet = new CognitiveVR.CustomEvent("Neurable Fixation");
            var fixPoint = FixationPoints[point];
            if(fixPoint && fixPoint.CognitiveHandle)
                packet.SetDynamicObject(fixPoint.CognitiveHandle.Id);
            packet.SetProperties(new Dictionary<string, object>
            {
                { "Object", point.Name },
                { "Start Time", data.StartTime },
                { "Fixation Duration", data.FixationDuration },
                { "Mean Arousal", timeline.GetAverage(AffectiveStateType.GrandMean) },
                { "Mean Stress", timeline.GetAverage(AffectiveStateType.Stress) },
                { "Mean Attention", timeline.GetAverage(AffectiveStateType.Attention) },
                { "Mean Calm", timeline.GetAverage(AffectiveStateType.Calm) },
                { "Mean Fatigue", timeline.GetAverage(AffectiveStateType.Fatigue) }
            });
            if (fixPoint != null)
                packet.Send(fixPoint.transform.position);
            else
                packet.Send();
#endif
        }


        // Use Gaze to raycast into the world and update relevant objects
        private void DoRayCast(float deltaTime, float currentTime)
        {
            RaycastHit fixationObject;
            RaycastHit[] fixationObjects = null;
            switch (raycastType) // choose gaze cast method
            {
                case RaycastType.LINE:
                    if (gazeEngine.GazeRaycast(hitInfo: out fixationObject, layerMask: raycastLayerMask))
                        fixationObjects = new RaycastHit[] {fixationObject};
                    break;
                case RaycastType.LINE_ALL:
                    fixationObjects = gazeEngine.GazeRaycastAll(layerMask: raycastLayerMask);
                    break;
                case RaycastType.SPHERE:
                    if (gazeEngine.GazeSpherecast(radius: spherecastRadius, hitInfo: out fixationObject,
                                                  layerMask: raycastLayerMask))
                        fixationObjects = new RaycastHit[] {fixationObject};
                    break;
                case RaycastType.SPHERE_ALL:
                    fixationObjects =
                        gazeEngine.GazeSpherecastAll(radius: spherecastRadius, layerMask: raycastLayerMask);
                    break;
            }

            if (fixationObjects != null && fixationObjects.Length > 0)
            {
                for (int i = 0; i < fixationObjects.Length; i++)
                {
                    var hit = fixationObjects[i];
                    if (hit.transform == null || hit.transform.gameObject == null) continue;
                    var _go = hit.transform.gameObject;
                    FixationPoint point = _go.GetComponent<FixationPoint>();
                    if (point == null) point = _go.GetComponentInParent<FixationPoint>();
                    UpdateFixation(point, deltaTime, currentTime); // run update on FixationPoint
                }
            }
        }

        // Update DataStructures following raycast
        private void UpdateFixation(FixationPoint point, float deltaTime, float currentTime)
        {
            if (point == null) return;
            if (FixationTimers == null) return;
            if (!FixationPoints.ContainsKey(point.ID)) FixationPoints.Add(point.ID, point);
            if (!FixationTimers.ContainsKey(point.ID)) FixationTimers.Add(point.ID, 0.0f);

            if (point.Fixating) // when already fixating, update timer and updatefixation
            {
                point.UpdateFixation(FixationEvents[point.ID].GetTotalFixationDuration());
                FixationTimers[point.ID] = fixationDecayTimer;
            }
            else // else increase dwell timer
            {
                FixationTimers[point.ID] += deltaTime; // compensate for decay. may cause early activation by 1 frame
                FixationTimers[point.ID] += deltaTime;
            }

            float fixationThreshold = defaultFixationDwellTimer;
            if (point.OverrideFixationDwell) fixationThreshold = point.DwellTimerOverride;
            if (FixationTimers[point.ID] > fixationThreshold) // if dwell timer hits threshold, activate fixation event
            {
                if (!FixationEvents.ContainsKey(point.ID)) FixationEvents.Add(point.ID, new FixationData());
                point.EnterFixation(FixationEvents[point.ID].GetTotalFixationDuration());
                FixationEvents[point.ID].StartEvent(currentTime);
            }
        }

        private void EndAllCurrentFixations()
        {
            if (FixationTimers == null) return;
            float currentTime = GetUserTime();
            var keys = GetPointsOfInterest_IDs();
            foreach (var id in keys)
            {
                if (FixationEvents.ContainsKey(id))
                {
                    FixationEvents[id].EndEvent(currentTime);
                }

                if (FixationTimers.ContainsKey(id)) FixationTimers[id] = 0f;
                var point = GetFixationPointFromID(id);
                if (point && point.Fixating)
                {
                    point.LeaveFixation(FixationEvents[id].GetTotalFixationDuration());
                }
            }
        }

        // Return List of points for which we have data
        public FixationPoint[] GetPointsOfInterest()
        {
            FixationPoint[] vals = new FixationPoint[FixationTimers.Count];
            FixationPoints.Values.CopyTo(vals, 0);
            return vals;
        }

        public FixationID[] GetPointsOfInterest_IDs()
        {
            FixationID[] vals = new FixationID[FixationPoints.Count];
            FixationPoints.Keys.CopyTo(vals, 0);
            return vals;
        }

        #endregion

        #region Pull AffectiveState Values

        public bool GetDataForPoint(FixationPoint point, out FixationData data)
        {
            if (point)
            {
                return GetDataForPoint(point.ID, out data);
            }
            else
            {
                data = null;
                return false;
            }
        }

        public bool GetDataForPoint(FixationID point, out FixationData data)
        {
            if (FixationEvents.ContainsKey(point))
            {
                data = FixationEvents[point];
                return true;
            }
            else
            {
                data = null;
                return false;
            }
        }

        public AffectiveStateTimeline GetTimelineDuringEvent(FixationEvent fixation)
        {
            if (NeurableMentalStateEngine.instance == null
                || NeurableMentalStateEngine.instance.stateTimeline == null || fixation == null)
                return null;
            return NeurableMentalStateEngine.instance.stateTimeline.GetSubTimeline(fixation.StartTime,
                                                                                      fixation.FixationDuration);
        }

        public List<AffectiveStateTimeline> GetAllTimelinesForFixation(FixationData fixation)
        {
            var timelines = new List<AffectiveStateTimeline>();
            foreach (var fixationEvent in fixation.FixationList)
            {
                timelines.Add(GetTimelineDuringEvent(fixationEvent));
            }

            return timelines;
        }

        public TimestampedValue GetMinimumStateOverEvents(AffectiveStateType type, List<AffectiveStateTimeline> events)
        {
            TimestampedValue minimum;
            minimum.timestamp = -1f;
            minimum.value = Mathf.Infinity;
            foreach (var timeline in events)
            {
                float localMin;
                timeline.minimums.GetValue(type, AveragingType.EMA, out localMin);
                if (localMin < minimum.value)
                {
                    timeline.minimums.GetTimestamp(type, out minimum.timestamp);
                    minimum.value = localMin;
                }
            }

            return minimum;
        }

        public TimestampedValue GetMaximumStateOverEvents(AffectiveStateType type, List<AffectiveStateTimeline> events)
        {
            TimestampedValue maximum;
            maximum.timestamp = -1f;
            maximum.value = Mathf.NegativeInfinity;
            foreach (var timeline in events)
            {
                float localMax;
                timeline.maximums.GetValue(type, AveragingType.EMA, out localMax);
                if (localMax > maximum.value)
                {
                    timeline.minimums.GetTimestamp(type, out maximum.timestamp);
                    maximum.value = localMax;
                }
            }

            return maximum;
        }

        #endregion
    }
}

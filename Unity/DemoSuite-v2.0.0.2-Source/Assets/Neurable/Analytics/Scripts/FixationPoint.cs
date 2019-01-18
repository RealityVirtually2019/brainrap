using UnityEngine;
using UnityEngine.Events;

namespace Neurable.Analytics
{
    [System.Serializable]
    public class FixationResponse : UnityEvent<float>
    {
    }

    public struct FixationID
    {
        public int ID;
        public string Name;
    }

    /*
     * A Fixation Point is the Object that the Fixation Engine is capable of tracking
     * Developers can add Fixation Responses to the OnEnterFixation/OnUpdateFixation/OnLeaveFixation Unity Events.
     * The Fixation Engine will track the object's fixation time and call these events with the total fixation time on the object
    */
    public class FixationPoint : MonoBehaviour
    {
        public FixationID ID;
        public bool OverrideFixationDwell = false;
        public float DwellTimerOverride = .5f;

        [SerializeField]
        protected FixationResponse OnEnterFixation;
        [SerializeField]
        protected FixationResponse OnUpdateFixation;
        [SerializeField]
        protected FixationResponse OnLeaveFixation;

        public bool Fixating = false;

        private void Awake()
        {
            ID.ID = this.GetInstanceID();
            ID.Name = name;
            ValidateRaycastHierarchy();
        }

        private bool ValidateRaycastHierarchy()
        {
            var collider = GetComponent<Collider>();
            if (collider == null) collider = GetComponentInChildren<Collider>();
            bool colliderCheck = (collider != null);
            if (!colliderCheck)
                throw new MissingComponentException("FixationPoint::" + name
                                                                      + " should have a Collider at or below them in the hierarchy");

            bool rbCheck = true;
            var rb = GetComponent<Rigidbody>();
            if (rb == null) rbCheck &= (GetComponentInParent<Rigidbody>() == null);
            if (!rbCheck)
                throw new System.Exception("FixationPoint::" + name
                                                             + " should be at or above the Rigidbody for this object in the hierarchy");

            return colliderCheck && rbCheck;
        }

#if CVR_NEURABLE
        static string CognitiveEngagementName = "NeurableFixation";
        public CognitiveVR.DynamicObject _cognitiveHandle;
        public CognitiveVR.DynamicObject CognitiveHandle
        {
            get
            {
                if (!_cognitiveHandle) _cognitiveHandle = GetComponent<CognitiveVR.DynamicObject>();
                return _cognitiveHandle;
            }
        }
#endif

        public void EnterFixation(float totalFixationTime)
        {
            if (!Fixating && OnEnterFixation != null) OnEnterFixation.Invoke(totalFixationTime);

#if CVR_NEURABLE
            if (CognitiveHandle)
                CognitiveHandle.BeginEngagement(CognitiveEngagementName, CognitiveHandle.Id);
#endif
            Fixating = true;
        }

        public void UpdateFixation(float totalFixationTime)
        {
            if (!Fixating) EnterFixation(totalFixationTime);
            if (OnUpdateFixation != null) OnUpdateFixation.Invoke(totalFixationTime);
        }

        public void LeaveFixation(float totalFixationTime)
        {
            if (Fixating && OnLeaveFixation != null) OnLeaveFixation.Invoke(totalFixationTime);
#if CVR_NEURABLE
            if (CognitiveHandle)
                CognitiveHandle.EndEngagement(CognitiveEngagementName, CognitiveHandle.Id);
#endif
            Fixating = false;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Neurable.Core;

namespace Neurable.Analytics
{
    public class NeurableMentalStateEngine : MonoBehaviour
    {
        public static NeurableMentalStateEngine instance;

        [SerializeField, Tooltip("Smoothing on Affective State Data?")]
        public AveragingType affectiveSmoothing = AveragingType.EMA;

        [SerializeField, Tooltip("Implement an Exponential Moving Average to Smooth Powerband Data")]
        public bool powerbandSmoothing = true;

        [Header("Events Called Whenever AffectiveState changes")]
        [Header("Parameters are (float)Timestamp, (float)Value")]
        [SerializeField]
        public MentalStateEvent onStressChange;
        [SerializeField]
        public MentalStateEvent onAttentionChange;
        [SerializeField]
        public MentalStateEvent onCalmChange;
        [SerializeField]
        public MentalStateEvent onFatigueChange;
        [SerializeField]
        public MentalStateEvent onGrandMeanChange;

        [Header("Events Called Whenever Powerband changes")]
        [Header("Parameters are (float)Timestamp, (float)Value")]
        [SerializeField]
        public MentalStateEvent onAlphaChange;
        [SerializeField]
        public MentalStateEvent onBetaChange;
        [SerializeField]
        public MentalStateEvent onGammaChange;
        [SerializeField]
        public MentalStateEvent onDeltaChange;
        [SerializeField]
        public MentalStateEvent onThetaChange;

        public float Attention
        {
            get { return _affectiveHelpers[AffectiveStateType.Attention].Value; }
        }

        public float Stress
        {
            get { return _affectiveHelpers[AffectiveStateType.Stress].Value; }
        }

        public float Calm
        {
            get { return _affectiveHelpers[AffectiveStateType.Calm].Value; }
        }

        public float Fatigue
        {
            get { return _affectiveHelpers[AffectiveStateType.Fatigue].Value; }
        }

        public float GrandMean
        {
            get { return _affectiveHelpers[AffectiveStateType.GrandMean].Value; }
        }

        public readonly AffectiveStateTimeline stateTimeline = new AffectiveStateTimeline();
        public readonly EventTimeline eventTimeline = new EventTimeline();
        public readonly PowerbandDataHandler powerbandHandler = new PowerbandDataHandler();

        private Dictionary<AffectiveStateType, AffectiveStateEngineHelper> _affectiveHelpers;
        private Dictionary<PowerbandType, PowerbandEngineHelper> _powerbandHelpers;
        private List<MentalDataEngineHelper> _allDataHelpers;

        #region Neurable User Data Handler

        public API.Types.DataCallback onMentalState; // Callback on new mentalstate

        // Callbacks to send to Neurable User 
        protected virtual void MentalDataHandler(double timestamp, String[] titles, double[] values)
        {
            if (stateTimeline == null)
            {
                Debug.Log("Returning null");
                return;
            }

            for (int i = 0; i < titles.Length; ++i)
            {
                var affectiveEnum = AffectiveStateHelper.GetEnumFromTitle(titles[i]);
                if (affectiveEnum != AffectiveStateType.None)
                {
                    AffectiveStateEngineHelper affectiveHelper;
                    if (_affectiveHelpers.TryGetValue(affectiveEnum, out affectiveHelper))
                    {
                        affectiveHelper.UpdateValue(timestamp, values[i]);
                    }
                }

                var powerbandEnum = PowerbandTypeHelper.GetEnumFromTitle(titles[i]);
                if (powerbandEnum != PowerbandType.None)
                {
                    PowerbandEngineHelper powerbandHelper;
                    if (_powerbandHelpers.TryGetValue(powerbandEnum, out powerbandHelper))
                    {
                        powerbandHelper.UpdateValue(timestamp, values[i]);
                    }
                }
            }
        }

        //This function marks the event using the most recent affective state timestamp
        // this is good if you want to later use your events with the statedata and ensure it is synced up
        public void MarkEvent(string eventName)
        {
            var mostRecentTimestamp = stateTimeline.GetLatestTimestamp();
            eventTimeline.MarkEvent(eventName, mostRecentTimestamp);
#if CVR_NEURABLE
            CognitiveVR.CustomEvent ev = new CognitiveVR.CustomEvent(eventName);
            ev.Send();
#endif
        }

        private void Awake()
        {
            if (instance != null)
            {
                throw new System.Exception("Only One Affective State Engine Allowed");
            }

            instance = this;

            _allDataHelpers = new List<MentalDataEngineHelper>();
            _affectiveHelpers = new Dictionary<AffectiveStateType, AffectiveStateEngineHelper>();
            CreateAffectiveHelper(AffectiveStateType.Attention, onAttentionChange);
            CreateAffectiveHelper(AffectiveStateType.Stress, onStressChange);
            CreateAffectiveHelper(AffectiveStateType.Calm, onCalmChange);
            CreateAffectiveHelper(AffectiveStateType.Fatigue, onFatigueChange);
            CreateAffectiveHelper(AffectiveStateType.GrandMean, onGrandMeanChange);

            _powerbandHelpers = new Dictionary<PowerbandType, PowerbandEngineHelper>();
            CreatePowerbandHelper(PowerbandType.Alpha, onAlphaChange);
            CreatePowerbandHelper(PowerbandType.Beta, onBetaChange);
            CreatePowerbandHelper(PowerbandType.Gamma, onGammaChange);
            CreatePowerbandHelper(PowerbandType.Delta, onDeltaChange);
            CreatePowerbandHelper(PowerbandType.Theta, onThetaChange);
        }

        private void CreateAffectiveHelper(AffectiveStateType type, MentalStateEvent onChange)
        {
            var helper = new AffectiveStateEngineHelper(this, type, onChange);
            _affectiveHelpers.Add(type, helper);
            _allDataHelpers.Add(helper);
        }

        private void CreatePowerbandHelper(PowerbandType type, MentalStateEvent onChange)
        {
            var helper = new PowerbandEngineHelper(this, type, onChange);
            _powerbandHelpers.Add(type, helper);
            _allDataHelpers.Add(helper);
        }

        private void Start()
        {
            SetupCallback();
        }

        private void SetupCallback()
        {
            if (!NeurableUser.Instantiated) return;

            onMentalState = MentalDataHandler;

            NeurableUser.Instance.User.SetDataCallback(onMentalState, API.Types.CallbackType.MENTAL_STATE);
        }

        // Use this for initialization
        private void OnEnable()
        {
            SetupCallback();
        }

        private void OnDisable()
        {
            if (!NeurableUser.Instantiated) return;

            NeurableUser.Instance.User.SetDataCallback(null, API.Types.CallbackType.MENTAL_STATE);
        }

        #endregion

        #region Update Indicies

        private void Update()
        {
            for (var i = 0; i < _allDataHelpers.Count; i++)
            {
                _allDataHelpers[i].OnUpdate();
            }
        }

        #endregion
    }
}

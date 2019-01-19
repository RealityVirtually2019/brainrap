namespace Neurable.Analytics
{
    public class AffectiveStateEngineHelper : MentalDataEngineHelper
    {
        private NeurableMentalStateEngine _engine;
        private AffectiveStateTimeline _stateTimeline;
        private AffectiveStateType _stateType;

        public AffectiveStateEngineHelper(NeurableMentalStateEngine engine,
                                          AffectiveStateType type,
                                          MentalStateEvent onChange)
        {
            _engine = engine;
            _stateTimeline = _engine.stateTimeline;
            _stateType = type;
            _onChange = onChange;
            name = "Neurable.Arousal." + type.ToString();
        }

        public override float Value
        {
            get
            {
                float val;
                _stateTimeline.GetLastStateValue(_stateType, _engine.affectiveSmoothing, out val);
                return val;
            }
        }

        protected override void SetValue(float time, float value)
        {
            var data = _stateTimeline.AddData(_stateType, time, value);
            _onChange.Invoke(time, data.exponentialMovingAverage);
        }
    }
}

namespace Neurable.Analytics
{
    public class PowerbandEngineHelper : MentalDataEngineHelper
    {
        private readonly NeurableMentalStateEngine _engine;
        private readonly PowerbandDataHandler _dataHandler;
        private readonly PowerbandType _stateType;

        public PowerbandEngineHelper(NeurableMentalStateEngine engine, PowerbandType type, MentalStateEvent onChange)
        {
            _engine = engine;
            _dataHandler = _engine.powerbandHandler;
            _stateType = type;
            _onChange = onChange;
            name = "Neurable.PowerBand." + type.ToString();
        }

        public override float Value
        {
            get
            {
                float val;
                _dataHandler.GetValue(_stateType, out val, _engine.powerbandSmoothing);
                return val;
            }
        }

        protected override void SetValue(float time, float value)
        {
            _dataHandler.AddData(_stateType, time, value);
            _onChange.Invoke(time, Value);
        }
    }
}

using System;

namespace Neurable.Analytics
{
    [Serializable]
    public abstract class MentalDataEngineHelper
    {
        public abstract float Value { get; }
        protected string name = "mentalData";

        private float _value = 0.0f;
        private float _updateTime = 0.0f;
        protected MentalStateEvent _onChange;

        public void UpdateValue(double time, double value)
        {
            _updateTime = (float) time;
            _value = (float) value;
        }

        public void OnUpdate()
        {
            if (_updateTime > 0.0)
            {
                SetValue(_updateTime, _value);
#if CVR_NEURABLE
                Portal.NeurableCognitiveInterface.RecordPoint(name, _value);
#endif
            }

            _updateTime = 0f;
        }

        protected abstract void SetValue(float time, float value);
    }
}

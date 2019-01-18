using System.Collections.Generic;

namespace Neurable.Analytics
{
    public class PowerbandDataHandler : Dictionary<PowerbandType, PowerbandData>
    {
        // Use Exponential Smoothing algorithm
        private readonly Dictionary<PowerbandType, float> exponentialMovingAverages;

        private const float WINDOW_SIZE = 10f;
        private float _smoothingFactor = -1e9f;

        private float SmoothingFactor
        {
            get
            {
                if (_smoothingFactor < 0f || _smoothingFactor > 1f)
                {
                    _smoothingFactor = 2f / (1f + WINDOW_SIZE); // 2 / (1 + WindowSize)
                }

                return _smoothingFactor;
            }
        }

        public PowerbandDataHandler()
        {
            exponentialMovingAverages = new Dictionary<PowerbandType, float>();
        }

        public void AddData(PowerbandType type, float time, float value)
        {
            PowerbandData point = new PowerbandData(time, value);
            this[type] = point;
            ApplyEma(type, value);
        }

        public bool GetValue(PowerbandType type, out float value, bool smoothValue)
        {
            PowerbandData data;
            if (!TryGetValue(type, out data))
            {
                value = -1e9f;
                return false;
            }

            if (smoothValue)
            {
                value = GetEma(type);
                return true;
            }

            value = data.value;
            return true;
        }

        private void ApplyEma(PowerbandType type, float value)
        {
            float priorEma; // previous EMA Calculation
            bool exists = (exponentialMovingAverages.TryGetValue(type, out priorEma));
            if (!exists)
            {
                exponentialMovingAverages[type] = value; // First Value is not averaged
            }
            else
            {
                exponentialMovingAverages[type] =
                    value * SmoothingFactor + (1 - SmoothingFactor) * priorEma; // subsequent values are averaged
            }
        }

        public float GetEma(PowerbandType type)
        {
            float ema = 0.0f;
            exponentialMovingAverages.TryGetValue(type, out ema);
            return ema;
        }
    }
}

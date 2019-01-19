namespace Neurable.Analytics
{
    public class AffectiveStateDataPoint
    {
        public float time;
        public AffectiveStateType type;
        public float value;
        public float exponentialMovingAverage;
        public float simpleMovingAverage;

        public AffectiveStateDataPoint(float time, AffectiveStateType type, float value, float ema, float sma)
        {
            this.time = time;
            this.type = type;
            this.value = value;
            exponentialMovingAverage = ema;
            simpleMovingAverage = sma;
        }

        #region CSV Export

        public string GetCsvData()
        {
            var text = value + ",";
            text += exponentialMovingAverage + ",";
            text += simpleMovingAverage;
            return text;
        }

        public static string GetCsvHeader(string type)
        {
            var text = type + " raw,";
            text += type + " EMA,";
            text += type + " SMA";
            return text;
        }

        #endregion
    }
}

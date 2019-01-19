using System.Collections.Generic;
using UnityEngine;

namespace Neurable.Analytics
{
    public class AffectiveStatesCollection : System.Collections.Specialized.ListDictionary
    {
        public void AddData(AffectiveStateDataPoint point)
        {
            AddData(point.type, point);
        }

        public void AddData(AffectiveStateType type, AffectiveStateDataPoint point)
        {
            this[type] = point;
        }

        public List<AffectiveStateDataPoint> GetPoints()
        {
            var points = new List<AffectiveStateDataPoint>();

            foreach (var value in Values)
            {
                points.Add((AffectiveStateDataPoint) value);
            }

            return points;
        }

        public bool GetTimestamp(AffectiveStateType stateType, out float time)
        {
            if (!Contains(stateType))
            {
                time = 0f;
                return false;
            }

            var dataPoint = (AffectiveStateDataPoint) this[stateType];
            time = dataPoint.time;
            return true;
        }

        public bool GetValue(AffectiveStateType stateType, AveragingType averagingType, out float value)
        {
            if (!Contains(stateType))
            {
                value = 0f;
                return false;
            }

            var dataPoint = (AffectiveStateDataPoint) this[stateType];
            switch (averagingType)
            {
                case AveragingType.None:
                    value = dataPoint.value;
                    break;
                case AveragingType.EMA:
                    value = dataPoint.exponentialMovingAverage;
                    break;
                case AveragingType.SMA:
                    value = dataPoint.simpleMovingAverage;
                    break;
                default:
                    Debug.Log("Unsupported value type: " + averagingType);
                    value = 0f;
                    return false;
            }

            return true;
        }

        #region CSV Export

        public string GetCsvData()
        {
            var text = "";
            for (int i = 0; i < 5; i++)
            {
                var type = (AffectiveStateType) i;
                var dataPoint = (AffectiveStateDataPoint) this[type];
                text += dataPoint.GetCsvData();
                text += (i != 4) ? "," : "";
            }

            return text;
        }

        public static string GetCsvHeader()
        {
            var text = "";
            for (int i = 0; i < 5; i++)
            {
                var type = (AffectiveStateType) i;
                text += AffectiveStateDataPoint.GetCsvHeader(AffectiveStateHelper.GetTitleFromEnum(type));
                text += (i != 4) ? "," : "";
            }

            return text;
        }

        #endregion
    }
}

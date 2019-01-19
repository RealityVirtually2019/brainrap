using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Neurable.Analytics
{
    public class AffectiveStateTimeline : SortedList<float, AffectiveStatesCollection>
    {
        public AffectiveStatesCollection
            maximums = new AffectiveStatesCollection(); // List of current local maximums recalculated in addData

        public AffectiveStatesCollection
            minimums = new AffectiveStatesCollection(); // List of current local minimums recalculated in addData

        private readonly Dictionary<AffectiveStateType, float> _averages = new Dictionary<AffectiveStateType, float>();
        private bool _averagesStale = true;

        private int _smaWindow = 10;
        private int _emaWindow = 10;

        // individual index update. Triggers Unity Event Calls
        public AffectiveStateDataPoint AddData(AffectiveStateType type, float time, float value)
        {
            _averagesStale = true;

            if (!ContainsKey(time))
            {
                this[time] = new AffectiveStatesCollection();
            }

            var ema = CaclulateEMA(type, value);
            var sma = CaclulateSMA(type, value);

            var dataPoint = new AffectiveStateDataPoint(time, type, value, ema, sma);

            UpdateMaxAndMin(type, dataPoint);

            this[time].AddData(dataPoint);
            return dataPoint;
        }

        public bool GetLastStateValue(AffectiveStateType stateType,
                                      AveragingType averagingType,
                                      out float value)
        {
            if (Count == 0)
            {
                Debug.LogWarning("Attempting to get most recent value when none exist");
                value = 0;
                return false;
            }

            return this[GetLatestTimestamp()].GetValue(stateType, averagingType, out value);
        }

        public float GetFirstTimestamp()
        {
            return Count == 0 ? 0f : Keys[0];
        }

        public float GetLatestTimestamp()
        {
            return Count == 0 ? 0f : Keys[Count - 1];
        }

        public AffectiveStatesCollection GetValueAtTime(float time)
        {
            return Values[GetIndexOfClosestTime(time)];
        }

        public AffectiveStateTimeline GetSubTimeline(float start, int samples)
        {
            var windowTimeline = new AffectiveStateTimeline();
            if (Count == 0)
            {
                return windowTimeline;
            }

            var startIndex = GetIndexOfClosestTimeAbove(start);

            for (var i = startIndex; i < Mathf.Min(Count - 1, (startIndex + samples)); i++)
            {
                var points = Values[i].GetPoints();
                for (var j = 0; j < points.Count; j++)
                {
                    windowTimeline.AddData(points[j].type, points[j].time, points[j].value);
                }
            }

            return windowTimeline;
        }

        public AffectiveStateTimeline GetSubTimeline(float start, float window)
        {
            var windowTimeline = new AffectiveStateTimeline();
            if (Count == 0)
            {
                return windowTimeline;
            }

            var startIndex = GetIndexOfClosestTimeAbove(start);
            var endIndex = GetIndexOfClosestTimeBelow(start + window);

            for (var i = startIndex; i < Mathf.Min(Count - 1, endIndex); i++)
            {
                var points = Values[i].GetPoints();
                for (var j = 0; j < points.Count; j++)
                {
                    windowTimeline.AddData(points[j].type, points[j].time, points[j].value);
                }
            }

            return windowTimeline;
        }

        //Get a copy of the current timeline 
        public AffectiveStateTimeline GetStaticCopyOfTimeline()
        {
            var windowTimeline = new AffectiveStateTimeline();
            if (Count < 2)
            {
                return windowTimeline;
            }

            //cutting off the last point as it could in theory be partially filled
            for (var i = 0; i < Count - 2; i++)
            {
                var points = Values[i].GetPoints();
                for (var j = 0; j < points.Count; j++)
                {
                    windowTimeline.AddData(points[j].type, points[j].time, points[j].value);
                }
            }

            return windowTimeline;
        }

        public void SetLiveAveragingMethod(AveragingType method, int window)
        {
            switch (method)
            {
                case AveragingType.EMA:
                    _emaWindow = window;
                    break;
                case AveragingType.SMA:
                    _smaWindow = window;
                    break;
            }
        }

        public void RecalculateMovingAverages(AveragingType method, int window)
        {
            SetLiveAveragingMethod(method, window);

            for (var i = 0; i < Count; i++)
            {
                RecalculateMovingAverage(method, i);
            }
        }

        public float GetAverage(AffectiveStateType value)
        {
            if (_averagesStale)
            {
                RecalculateStaticAverages();
            }

            return _averages[value];
        }

        #region CSV Export

        public void PrintDataPointsToCsv(string fileName)
        {
            var text = "Time, " + AffectiveStatesCollection.GetCsvHeader() + "\r\n";

            foreach (var keyValuePair in this)
            {
                text += keyValuePair.Key + ",";
                text += keyValuePair.Value.GetCsvData() + "\r\n";
            }

            File.WriteAllText(fileName, text);
        }

        public void PrintOverviewCsv(string fileName)
        {
            var text = "Affective State,Average Value,Max Value Time,Max Value,Min Value Time,Min Value\r\n";
            for (var i = 0; i < 5; i++)
            {
                var type = (AffectiveStateType) i;

                text += type + ",";
                text += GetAverage(type) + ",";

                float val;
                text += (maximums.GetTimestamp(type, out val)
                             ? val.ToString(CultureInfo.InvariantCulture)
                             : "") + ",";
                text += (maximums.GetValue(type, AveragingType.None, out val)
                             ? val.ToString(CultureInfo.InvariantCulture)
                             : "") + ",";
                text += (minimums.GetTimestamp(type, out val)
                             ? val.ToString(CultureInfo.InvariantCulture)
                             : "") + ",";
                text += (minimums.GetValue(type, AveragingType.None, out val)
                             ? val.ToString(CultureInfo.InvariantCulture)
                             : "");

                text += "\r\n";
            }

            File.WriteAllText(fileName, text);
        }

        public static string GetMultitimelineOverviewCsvHeader()
        {
            var text = "Start Time,End Time,";
            string[] headerBases = {"Average Value", "Max Value Time", "Max Value", "Min Value Time", "Min Value"};

            for (var i = 0; i < 5; i++)
            {
                var type = (AffectiveStateType) i;
                for (var j = 0; j < headerBases.Length; j++)
                {
                    text += type + " " + headerBases[j];
                    text += (j != headerBases.Length - 1) ? "," : "";
                }

                text += (i != 4) ? "," : "";
            }

            return text;
        }

        public string GetMultitimelineOverviewCsvData()
        {
            var text = "";

            text += GetFirstTimestamp() + ",";
            text += GetLatestTimestamp() + ",";

            for (var i = 0; i < 5; i++)
            {
                var type = (AffectiveStateType) i;

                text += GetAverage(type) + ",";

                float val;
                text += (maximums.GetTimestamp(type, out val) ? val.ToString(CultureInfo.InvariantCulture) : "") + ",";
                text += (maximums.GetValue(type, AveragingType.None, out val)
                             ? val.ToString(CultureInfo.InvariantCulture)
                             : "") + ",";
                text += (minimums.GetTimestamp(type, out val) ? val.ToString(CultureInfo.InvariantCulture) : "") + ",";
                text += (minimums.GetValue(type, AveragingType.None, out val)
                             ? val.ToString(CultureInfo.InvariantCulture)
                             : "");

                text += (i != 4) ? "," : "";
            }

            return text;
        }

        #endregion

        #region AddDataHelpers

        private void UpdateMaxAndMin(AffectiveStateType type, AffectiveStateDataPoint point)
        {
            UpdateMax(type, point);
            UpdateMin(type, point);
        }

        private void UpdateMax(AffectiveStateType type, AffectiveStateDataPoint point)
        {
            float currentMax;
            if (!maximums.GetValue(type, AveragingType.None, out currentMax))
            {
                maximums.AddData(point);
                return;
            }

            if (point.value > currentMax)
            {
                maximums.AddData(point);
            }
        }

        private void UpdateMin(AffectiveStateType type, AffectiveStateDataPoint point)
        {
            float currentMin;
            if (!minimums.GetValue(type, AveragingType.None, out currentMin))
            {
                minimums.AddData(point);
                return;
            }

            if (point.value < currentMin)
            {
                minimums.AddData(point);
            }
        }

        #endregion

        #region AveragingHelpers

        private void RecalculateStaticAverages()
        {
            var counts = new Dictionary<AffectiveStateType, int>();
            var sums = new Dictionary<AffectiveStateType, float>();

            for (var i = 0; i < Count; i++)
            {
                foreach (AffectiveStateDataPoint value in Values[i].Values)
                {
                    if (!counts.ContainsKey(value.type))
                    {
                        counts.Add(value.type, 0);
                        sums.Add(value.type, 0f);
                    }

                    counts[value.type]++;
                    sums[value.type] += value.value;
                }
            }

            foreach (var kv in counts)
            {
                _averages[kv.Key] = sums[kv.Key] / kv.Value;
            }

            _averagesStale = false;
        }

        private void RecalculateMovingAverage(AveragingType method, int index)
        {
            var stateCollection = Values[index];

            foreach (AffectiveStateDataPoint value in stateCollection.Values)
            {
                switch (method)
                {
                    case AveragingType.EMA:
                        value.exponentialMovingAverage = RecaclulateEMA(value.type, index, value.value);
                        break;
                    case AveragingType.SMA:
                        value.simpleMovingAverage = RecaclulateSMA(value.type, index, value.value);
                        break;
                }
            }
        }

        #region Exponential Moving Averages

        private float CaclulateEMA(AffectiveStateType type, float value)
        {
            if (Count <= 1)
            {
                return value; // First Value is not averaged
            }

            var timeBeforeThis = Keys[Count - 2];
            float priorEMA;
            this[timeBeforeThis].GetValue(type, AveragingType.EMA, out priorEMA);
            var smoothingFactor = 2f / (1f + _emaWindow);
            return value * smoothingFactor + (1 - smoothingFactor) * priorEMA;
        }

        private float RecaclulateEMA(AffectiveStateType type, int index, float value)
        {
            if (index < 1)
            {
                return value;
            }

            var timeBeforeThis = Keys[index - 1];
            float priorEMA;
            this[timeBeforeThis].GetValue(type, AveragingType.EMA, out priorEMA);
            var smoothingFactor = 2f / (1f + _emaWindow);
            return value * smoothingFactor + (1 - smoothingFactor) * priorEMA;
        }

        #endregion

        #region Simple Moving Averages

        private float CaclulateSMA(AffectiveStateType type, float value)
        {
            var timeStamps = Keys;
            var maxCount = Mathf.Min(timeStamps.Count, _smaWindow);
            var sum = value;
            var count = 1;

            //skip first element because that is this time and we already added it above
            for (var i = 2; i <= maxCount; i++)
            {
                var timeStamp = timeStamps[timeStamps.Count - i];
                float val;
                if (this[timeStamp].GetValue(type, AveragingType.None, out val))
                {
                    sum += val;
                    count++;
                }
            }

            return sum / count;
        }

        private float RecaclulateSMA(AffectiveStateType type, int index, float value)
        {
            var timeStamps = Keys;
            var maxCount = Mathf.Min((index + 1), _smaWindow);
            var sum = value;
            var count = 1;

            //skip first element because that is this time and we already added it above
            for (var i = 1; i <= maxCount; i++)
            {
                var timeStamp = timeStamps[index - i];
                float val;
                if (this[timeStamp].GetValue(type, AveragingType.None, out val))
                {
                    sum += val;
                    count++;
                }
            }

            return sum / count;
        }

        #endregion

        #endregion

        #region FindHelpers

        private int GetIndexOfClosestTime(float time)
        {
            if (Count == 0)
            {
                Debug.LogError("Attempting to find time in empty list");
                return -1;
            }

            int lowerBound, upperBound, index;

            if (BinarySearchHelper(time, out lowerBound, out upperBound, out index))
            {
                return index;
            }

            // note: at this point lower and upper bounds have swapped so that lowerBound is now greater than upperBound

            var lowerBoundDiff = Keys[lowerBound] - time;
            var upperBoundDiff = time - Keys[upperBound];

            return (lowerBoundDiff < upperBoundDiff) ? lowerBound : upperBound;
        }

        private int GetIndexOfClosestTimeAbove(float time)
        {
            if (Count == 0)
            {
                Debug.LogError("Attempting to find time in empty list");
                return -1;
            }

            int lowerBound, upperBound, index;

            if (BinarySearchHelper(time, out lowerBound, out upperBound, out index))
            {
                return index;
            }

            // note: at this point lower and upper bounds have swapped so that lowerBound is now greater than upperBound
            // also note in the event that there is no element greater than time you will just get the largest element that exists
            return Mathf.Min(Mathf.Max(lowerBound, 0), (Count - 1));
        }

        private int GetIndexOfClosestTimeBelow(float time)
        {
            if (Count == 0)
            {
                Debug.LogError("Attempting to find time in empty list");
                return -1;
            }

            int lowerBound, upperBound, index;

            if (BinarySearchHelper(time, out lowerBound, out upperBound, out index))
            {
                return index;
            }

            // note: at this point lower and upper bounds have swapped so that lowerBound is now greater than upperBound
            // also note in the event that there is no element greater than time you will just get the smallest element that exists
            return Mathf.Min(Mathf.Max(upperBound, 0), (Count - 1));
        }

        //If an exact match was found return true and set index
        //If no exact match found return false use lower and upper bounds
        private bool BinarySearchHelper(float time, out int lowerBound, out int upperBound, out int index)
        {
            lowerBound = 0;
            upperBound = Count - 1;

            if (time < Keys[lowerBound])
            {
                index = lowerBound;
                return true;
            }

            if (time > Keys[upperBound])
            {
                index = upperBound;
                return true;
            }

            while (lowerBound <= upperBound)
            {
                var midPoint = lowerBound + (upperBound - lowerBound) / 2;
                var comparison = Comparer.Compare(time, Keys[midPoint]);
                if (comparison < 0)
                {
                    upperBound = midPoint - 1;
                }
                else if (comparison > 0)
                {
                    lowerBound = midPoint + 1;
                }
                else
                {
                    index = midPoint;
                    return true;
                }
            }

            //set index to junk because we didn't find an exact match
            index = -1;
            return false;
        }

        #endregion
    }
}

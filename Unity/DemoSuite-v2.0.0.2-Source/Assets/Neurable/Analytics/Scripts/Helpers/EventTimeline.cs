using System.Collections.Generic;

namespace Neurable.Analytics
{
    public class EventTimeline : Dictionary<string, List<float>>
    {
        public void MarkEvent(string eventName, float timestamp)
        {
            if (!ContainsKey(eventName))
            {
                this[eventName] = new List<float>();
            }

            var list = this[eventName];

            var index = list.BinarySearch(timestamp);
            if (index < 0)
            {
                list.Insert(~index, timestamp);
            }
        }

        public List<float> GetEventTimes(string eventName)
        {
            if (!ContainsKey(eventName))
            {
                return new List<float>();
            }

            return this[eventName];
        }

        public List<string> GetEventNames()
        {
            var eventNames = new List<string>();

            foreach (var keyValuePair in this)
            {
                if (keyValuePair.Value.Count > 0)
                {
                    eventNames.Add(keyValuePair.Key);
                }
            }

            return eventNames;
        }

        public EventTimeline GetSubTimeline(float startTime, float windowPeriod)
        {
            var subTimeline = new EventTimeline();
            var endTime = startTime + windowPeriod;

            foreach (var keyValuePair in this)
            {
                var times = keyValuePair.Value;
                var startIndex = times.BinarySearch(startTime);
                startIndex = (startIndex < 0) ? ~startIndex : startIndex;

                for (int i = startIndex; ((i < times.Count) && (times[i] < endTime)); i++)
                {
                    subTimeline.MarkEvent(keyValuePair.Key, times[i]);
                }
            }

            return subTimeline;
        }
    }
}

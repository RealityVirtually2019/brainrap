using System.Collections.Generic;
using UnityEngine;

namespace Neurable.Analytics
{
    // FixationData is a list of FixationEvents associated to an object or context
    // When Fixation Events complete, they are added to a list
    // List can be parsed for metadata such as average, length, and first fixation.
    
    public class FixationData
    {
        public List<FixationEvent> FixationList;
        private FixationEvent ActiveEvent = null;
        private float TotalFixationTime = 0f;

        public FixationData()
        {
            FixationList = new List<FixationEvent>();
        }

        public void StartEvent(float timestamp)
        {
            if (ActiveEvent == null)
            {
                ActiveEvent = new FixationEvent(timestamp);
            }
        }

        public FixationEvent EndEvent(float endTime)
        {
            FixationEvent returnVal = ActiveEvent;
            if (ActiveEvent != null)
            {
                ActiveEvent.EndEvent(endTime);
                TotalFixationTime += ActiveEvent.FixationDuration;
                FixationList.Add(ActiveEvent);
                ActiveEvent = null;
            }

            return returnVal;
        }

        public float GetLongestFixationLength()
        {
            float maxLength = 0.0f;
            for (int i = 0; i < FixationList.Count; i++)
            {
                maxLength = Mathf.Max(maxLength, FixationList[i].FixationDuration);
            }

            return maxLength;
        }

        public float GetFirstFixationTimeStamp()
        {
            if (FixationList.Count > 0) return FixationList[0].StartTime;
            return -1f;
        }

        public float GetLastFixationTimeStamp()
        {
            if (FixationList.Count > 0) return FixationList[FixationList.Count - 1].EndTime;
            return -1f;
        }

        public float GetAverageGazeTime()
        {
            float average = 0.0f;
            for (int i = 0; i < FixationList.Count; i++)
            {
                average += FixationList[i].FixationDuration;
            }

            return average / FixationList.Count;
        }

        public float GetNumberFixations()
        {
            return FixationList.Count;
        }

        // Return the total duration.
        // if the user is not fixating on this event, will return a fixed value
        // else calculates the duration of the current active event
        public float GetTotalFixationDuration()
        {
            if (ActiveEvent != null)
            {
                float delta = FixationEngine.instance.GetUserTime() - ActiveEvent.StartTime;
                return TotalFixationTime + delta;
            }

            return TotalFixationTime;
        }

        public override string ToString()
        {
            List<string> result = new List<string>();
            for (int i = 0; i < FixationList.Count; i++)
            {
                result.Add(FixationList[i].ToString());
            }

            return string.Join(",", result.ToArray());
        }
    }
}
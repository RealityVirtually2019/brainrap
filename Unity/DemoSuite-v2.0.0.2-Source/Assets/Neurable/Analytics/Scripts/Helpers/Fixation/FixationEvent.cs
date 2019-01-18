namespace Neurable.Analytics
{
    // FixationEvent Encapsulates the start, end and length of fixation
    public class FixationEvent
    {
        public float StartTime;
        public float EndTime;
        public float FixationDuration;

        public FixationEvent()
        {
        }

        public FixationEvent(float startTime)
        {
            StartTime = startTime;
        }

        public void EndEvent(float endTime)
        {
            EndTime = endTime;
            FixationDuration = EndTime - StartTime;
        }

        public override string ToString()
        {
            return "[" + StartTime.ToString() + "," + EndTime.ToString() + "]";
        }
    }
}
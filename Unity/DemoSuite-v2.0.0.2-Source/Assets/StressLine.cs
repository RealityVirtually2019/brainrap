using UnityEngine;
using UnityEngine.UI;

    [RequireComponent(typeof(LineRenderer))]
    public class StressLine : MonoBehaviour
    {
        [Header("Passes Stress State Values into the Associated Slider Object")]
        [SerializeField]
        LineRenderer stressLine;

        LineRenderer LineObject
        {
            get
            {
                if (stressLine == null) stressLine = GetComponent<LineRenderer>();
                if (stressLine == null) stressLine = GetComponentInChildren<LineRenderer>();
                if (stressLine == null) stressLine = GetComponent<LineRenderer>();
                return stressLine;
            }
        }

        // NeurableAffectiveStateEngine returns a tuple of <time, value>.
        // This function takes the second value and passes it to the Slider
        public void UpdateLineWithStressState(float timestamp, float value)
        {
            if (LineObject != null)
                LineObject.SetPosition(0, new Vector3(0,value,0));
        }
    }


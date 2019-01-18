using UnityEngine;
using UnityEngine.UI;

namespace Neurable.Analytics.Samples
{
    public class CircleGraph : MonoBehaviour
    {
        public Image current;
        public Color color;
        public float maxValue = .5f;
        public float minValue = -.5f;

        private void Start()
        {
            current.color = color;
        }

        public void UpdateGraph(float timestamp, float value)
        {
            var scale = Mathf.InverseLerp(minValue, maxValue, value);
            current.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}

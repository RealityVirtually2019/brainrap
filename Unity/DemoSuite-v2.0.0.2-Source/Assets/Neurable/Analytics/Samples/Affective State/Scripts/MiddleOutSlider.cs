using UnityEngine;
using UnityEngine.UI;

namespace Neurable.Analytics.Samples
{
    public class MiddleOutSlider : MonoBehaviour
    {
        public Slider positiveSlider;
        public Slider negativeSlider;
        public Color positiveColor;
        public Color negativeColor;
        public float max = .5f;
        public float min = -.5f;

        private Image _positiveFill;
        private Image _negativeFill;

        private void Start()
        {
            _positiveFill = positiveSlider.fillRect.GetComponent<Image>();
            _positiveFill.color = positiveColor;
            _negativeFill = negativeSlider.fillRect.GetComponent<Image>();
            _negativeFill.color = negativeColor;

            positiveSlider.maxValue = max;
            positiveSlider.minValue = 0;
            negativeSlider.maxValue = Mathf.Abs(min);
            negativeSlider.minValue = 0;
        }

        public void UpdateSlider(float timestamp, float value)
        {
            if (value > 0)
            {
                positiveSlider.value = value;
                negativeSlider.value = 0;
            }
            else
            {
                positiveSlider.value = 0;
                negativeSlider.value = -value;
            }
        }
    }
}

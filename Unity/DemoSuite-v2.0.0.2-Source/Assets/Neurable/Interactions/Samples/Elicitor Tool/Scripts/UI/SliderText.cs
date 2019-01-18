using UnityEngine;
using UnityEngine.UI;

namespace Neurable.Interactions.Samples
{
    public class SliderText : MonoBehaviour
    {
        public Text text;
        public Slider source;

        enum SLIDER_TYPE
        {
            INT,
            FLOAT
        };

        SLIDER_TYPE sliderType = SLIDER_TYPE.FLOAT;

        // Use this for initialization
        void Awake()
        {
            if (text == null)
            {
                text = GetComponent<Text>();
            }

            if (text == null)
            {
                Debug.LogError("No Text Mesh attached :: " + name);
            }
        }

        void Start()
        {
            sliderType = source.wholeNumbers ? SLIDER_TYPE.INT : SLIDER_TYPE.FLOAT;
            UpdateSliderText(source);
        }

        public void UpdateSliderText(Slider slider)
        {
            switch (sliderType)
            {
                case SLIDER_TYPE.INT:
                    UpdateSliderTextInt(slider);
                    break;
                case SLIDER_TYPE.FLOAT:
                    UpdateSliderTextFloat(slider);
                    break;
                default:
                    break;
            }
        }

        protected void UpdateSliderTextInt(Slider slider)
        {
            if (slider == null)
            {
                Debug.LogError("No slider given");
                return;
            }

            int val = (int) slider.value;
            UpdateText(val.ToString());
        }

        protected void UpdateSliderTextFloat(Slider slider)
        {
            if (slider == null)
            {
                Debug.LogError("No slider given");
                return;
            }

            float val = (float) slider.value;
            UpdateText(val.ToString("#.00"));
        }

        protected void UpdateText(string s)
        {
            text.text = s;
        }
    }
}

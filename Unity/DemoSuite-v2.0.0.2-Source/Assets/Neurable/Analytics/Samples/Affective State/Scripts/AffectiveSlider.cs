using UnityEngine;
using UnityEngine.UI;

namespace Neurable.Analytics.Samples
{
    [RequireComponent(typeof(Slider))]
    public class AffectiveSlider : MonoBehaviour
    {
        [Header("Passes Affective State Values into the Associated Slider Object")]
        [SerializeField]
        Slider _sliderObject = null;

        Slider SliderObject
        {
            get
            {
                if (_sliderObject == null) _sliderObject = GetComponent<Slider>();
                if (_sliderObject == null) _sliderObject = GetComponentInChildren<Slider>();
                return _sliderObject;
            }
        }

        // NeurableAffectiveStateEngine returns a tuple of <time, value>.
        // This function takes the second value and passes it to the Slider
        public void UpdateSliderWithAffectiveState(float timestamp, float value)
        {
            if (SliderObject != null) SliderObject.value = value;
        }
    }
}

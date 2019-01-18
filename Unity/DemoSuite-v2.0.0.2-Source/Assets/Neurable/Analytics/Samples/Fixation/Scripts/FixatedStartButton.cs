using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Neurable.Analytics.Samples
{
    public class FixatedStartButton : MonoBehaviour
    {
        public Text TextBox;
        public string InitialText = "Fixate To Begin";
        public int SecondsToFixate = 3;

        public UnityEvent OnEndTimer;

        int fixateTime = 0;
        float lastFixationDuration = 0.0f;

        private void Awake()
        {
            if (TextBox == null) TextBox = GetComponent<Text>();
        }

        private void Start()
        {
            if (TextBox) TextBox.text = InitialText;
        }

        public void UpdateTimer(float input)
        {
            fixateTime = currentFixationTime(input);
            int reverse = SecondsToFixate - fixateTime;
            if (TextBox) TextBox.text = reverse.ToString();
            if (reverse <= 0) EndTimer();
        }

        int currentFixationTime(float totalFixationDuration)
        {
            return Mathf.FloorToInt(totalFixationDuration - lastFixationDuration);
        }

        private void EndTimer()
        {
            OnEndTimer.Invoke();
        }

        public void ResetTimer(float totalFixationDuration)
        {
            lastFixationDuration = totalFixationDuration;
            fixateTime = 0;
            if (TextBox) TextBox.text = InitialText;
        }
    }
}

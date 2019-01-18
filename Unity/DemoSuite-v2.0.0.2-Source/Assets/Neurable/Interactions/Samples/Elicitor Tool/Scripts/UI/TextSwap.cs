using UnityEngine;
using UnityEngine.UI;

namespace Neurable.Interactions.Samples
{
    public class TextSwap : MonoBehaviour
    {
        public bool ClickedState = false;
        public string Text1 = "Begin";
        public string Text2 = "Stop";

        public Text TextObject;

        // Use this for initialization
        void Start()
        {
            if (ClickedState)
                ChangeText(Text2);
            else
                ChangeText(Text1);
        }

        public void SwapTexts()
        {
            ClickedState = !ClickedState;
            if (ClickedState)
                ChangeText(Text2);
            else
                ChangeText(Text1);
        }

        private void ChangeText(string txt)
        {
            if (TextObject == null)
            {
                Debug.LogError("No Text Mesh Attached");
                return;
            }

            TextObject.text = txt;
        }
    }
}

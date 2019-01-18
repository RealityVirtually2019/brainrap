/*
 * Copyright 2017 Neurable Inc.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
 * Implementation of NeurableTag meant for Unity UI Buttons
 * implements the INeurableUtilityControllable interface for Intensity/Hue Control
 */

namespace Neurable.Interactions.Samples
{
    [RequireComponent(typeof(NeurableTag))]
    [RequireComponent(typeof(Button))]
    public class NeurableUIButton : MonoBehaviour, INeurableUtilityControllable
    {
        [Header("To make Existing Unity UI Buttons Neurable Enabled,")]
        [Header("Add the NeurableUIButton.InvokeButton function to")]
        [Header("the NeurableTag.OnTrigger Event. Add an Elicitor")]
        [Header("(such as NeurableUIButton.RunElicitor)")]
        [Header("to the NeurableTag.OnAnimate Event")]
        [Space]
        [Header("Neurable Elicitor Settings")]
        public Color elicitorColor = Color.cyan;  // Contrasting color to flash to
        public Color selectedColor = Color.green; // Color indicating selection

        private Color baseColor;
        private NeurableTag parentTag;
        private Image image;
        private Button button;

        protected virtual void Awake()
        {
            button = GetComponent<Button>();
            image = button.image;
            baseColor = image.color;
            getParentTag();
            StopFlash();
        }

        protected NeurableTag getParentTag()
        {
            if (parentTag == null) parentTag = GetComponent<NeurableTag>();
            return parentTag;
        }

        protected virtual void OnEnable()
        {
            if (getParentTag() != null) getParentTag().NeurableEnabled = true;
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            if (getParentTag() != null) getParentTag().NeurableEnabled = false;
        }

        // Use the onClick action configured by the button object
        public void InvokeButton()
        {
            button.onClick.Invoke();
        }

        Coroutine BlinkProcess;

        public void RunElicitor()
        {
            float duration = 0.5f;
            if (getParentTag() != null) duration = parentTag.flashDuration;
            StopFlash();
            BlinkProcess = StartCoroutine(blink(elicitorColor, baseColor, duration));
        }

        // Simple flash to indicate selection.
        public void ButtonSelectedIndicator()
        {
            StartCoroutine(blink(selectedColor, baseColor, 0.5f));
        }

        /*
         * Routine to swap the color for a period of time
         * Color to: Color to change to for duration of blink
         * Color from: Color to return to after blink
         * float duration: Length of the flash
         * bool wait: require animation to wait for other animations to complete
         */
        public IEnumerator blink(Color to, Color from, float duration)
        {
            if (image == null) yield break;

            float f_timer = 0f;
            while (f_timer <= duration / 2)
            {
                image.color = Color.Lerp(from, to, f_timer / (duration / 2));
                f_timer += Time.deltaTime;
                yield return null;
            }

            f_timer = 0f;
            while (f_timer <= duration / 2)
            {
                image.color = Color.Lerp(to, from, f_timer / (duration / 2));
                f_timer += Time.deltaTime;
                yield return null;
            }

            image.color = from;
            yield return null;
        }

        private void StopFlash()
        {
            if (BlinkProcess != null) StopCoroutine(BlinkProcess);
            if (image != null) image.color = baseColor;
        }

        [SerializeField]
        protected float _intensity = 0.5f;

        public float ElicitorIntensity
        {
            get { return _intensity; }
            set
            {
                if (value <= 0 || value > 1f) return;
                float h, s, v;
                Color.RGBToHSV(elicitorColor, out h, out s, out v);
                elicitorColor = Color.HSVToRGB(h, value, v);
                _intensity = value;
            }
        }

        [SerializeField]
        protected float _hue = 0.5f;

        public float ElicitorHue
        {
            get { return _hue; }
            set
            {
                if (value <= 0 || value > 1f) return;
                float h, s, v;
                Color.RGBToHSV(elicitorColor, out h, out s, out v);
                elicitorColor = Color.HSVToRGB(value, s, v);
                _hue = value;
            }
        }
    }
}

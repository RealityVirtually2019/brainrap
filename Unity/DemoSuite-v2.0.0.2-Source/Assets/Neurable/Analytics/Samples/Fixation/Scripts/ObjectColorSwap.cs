using UnityEngine;

namespace Neurable.Analytics.Samples
{
    public class ObjectColorSwap : MonoBehaviour
    {
        protected Renderer rend;
        protected Material instanceMaterial;

        [SerializeField]
        protected Material sourceMaterial;
        [Header("Color Thresholds")]
        [SerializeField]
        protected float MinTime = 0.0f;
        [SerializeField]
        protected Color ColdestColor = Color.blue;
        [SerializeField]
        protected float MaxTime = 10.0f;
        [SerializeField]
        protected Color HottestColor = Color.red;

        private void Awake()
        {
            rend = GetComponent<Renderer>();
            if (sourceMaterial == null) sourceMaterial = rend.material;
            instanceMaterial = Material.Instantiate(sourceMaterial);
            rend.material = instanceMaterial;
        }

        public void ChangeColor(Color color)
        {
            instanceMaterial.color = color;
        }

        public void LerpColorOfTotalTime(float totalTime)
        {
            Color targetColor;
            if (totalTime <= MinTime)
                targetColor = ColdestColor;
            else if (totalTime >= MaxTime)
                targetColor = HottestColor;
            else
            {
                float coldHue, hotHue, dump;
                Color.RGBToHSV(ColdestColor, out coldHue, out dump, out dump);
                Color.RGBToHSV(HottestColor, out hotHue, out dump, out dump);
                float midHue = Mathf.Lerp(coldHue, hotHue, (totalTime - MinTime) / (MaxTime - MinTime));
                targetColor = Color.HSVToRGB(midHue, 1, 1);
            }

            ChangeColor(targetColor);
        }
    }
}

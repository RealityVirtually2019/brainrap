using UnityEngine;

namespace Neurable.Interactions.Samples
{
    // This animation assumes that every object has a specific Material attached to it.
    // To use that Material; textures, colors, and other properties from Standard assets need to be imported into a new Material.
    // This script takes Shader Properties from one Material and imports them into a custom AmplifyHighlight Shader
    // The flash animation for this Material involves flipping the "_Highlighted" Property on the new material
    public class HighlightFlash : MonoBehaviour, INeurableUtilityControllable
    {
        public Material highlightMat;
        [SerializeField]
        private Material[][] InstancedMaterials;
        private Material[][] OriginalMaterials;

        /*
         * Script is intended for Amplify HighlightAnimated Shaders
         * With the following Properties
         */
        private string HighlightPropertyString = "_Highlighted";
        private string MinHighlightProperty = "_MinHighLightLevel";
        private string MaxHighlightProperty = "_MaxHighLightLevel";
        private string HighlightColorProperty = "_HighlightColor";

        /*
         * To use the Highlight shaders with generic materials, we
         * move shader properites over to the new Highlight Instance
         * Analog strings indicate the Properties of the original material to take
         * These strings can be changed publicly.
         */
        public string AlbedoAnalog = "_MainTex";
        public string EmissionAnalog = "_EmissionMap";
        public string OcclusionAnalog = "_OcclusionMap";
        public string NormalAnalog = "_BumpMap";
        public string ColorAnalog = "_Color";
        public string SmoothAnalog = "_Glossiness";

        private NeurableTag tagParent;
        private Renderer[] myRenderers;
        private bool ready = false;

        // Use this for initialization
        void Awake()
        {
            tagParent = GetComponent<NeurableTag>();
        }

        Renderer[] Renderers
        {
            get
            {
                if (myRenderers == null) myRenderers = GetComponentsInChildren<Renderer>();
                return myRenderers;
            }
        }

        public void ReplaceMaterials()
        {
            if (ready) return;
            if (Renderers == null) return;
            if (highlightMat == null) return;
            OriginalMaterials = new Material[Renderers.Length][];
            InstancedMaterials = new Material[Renderers.Length][];
            for (int i = 0; i < myRenderers.Length; i++)
            {
                OriginalMaterials[i] = Renderers[i].materials;
                InstancedMaterials[i] = new Material[Renderers[i].materials.Length];
                for (int j = 0; j < Renderers[i].materials.Length; j++)
                {
                    InstancedMaterials[i][j] = new Material(highlightMat);
                    if (Renderers[i].materials[j].HasProperty(AlbedoAnalog))
                        InstancedMaterials[i][j]
                            .SetTexture("_Albedo", Renderers[i].materials[j].GetTexture(AlbedoAnalog));
                    else
                        Debug.LogWarning("No Albedo on " + Renderers[i].gameObject.name);
                    if (Renderers[i].materials[j].HasProperty(NormalAnalog))
                        InstancedMaterials[i][j]
                            .SetTexture("_Normal", Renderers[i].materials[j].GetTexture(NormalAnalog));
                    else
                        Debug.LogWarning("No _Normal on " + Renderers[i].gameObject.name);
                    if (Renderers[i].materials[j].HasProperty(OcclusionAnalog))
                        InstancedMaterials[i][j]
                            .SetTexture("_Occlusion", Renderers[i].materials[j].GetTexture(OcclusionAnalog));
                    else
                        Debug.LogWarning("No _Occlusion on " + Renderers[i].gameObject.name);
                    if (Renderers[i].materials[j].HasProperty(EmissionAnalog))
                        InstancedMaterials[i][j]
                            .SetTexture("_Emission", Renderers[i].materials[j].GetTexture(EmissionAnalog));
                    else
                        Debug.LogWarning("No _Emission on " + Renderers[i].gameObject.name);
                    if (Renderers[i].materials[j].HasProperty(ColorAnalog))
                        InstancedMaterials[i][j].SetColor("_Color", Renderers[i].materials[j].GetColor(ColorAnalog));
                    else
                        Debug.LogWarning("No _Color on " + Renderers[i].gameObject.name);
                    if (Renderers[i].materials[j].HasProperty(SmoothAnalog))
                        InstancedMaterials[i][j]
                            .SetFloat("_Smoothness", Renderers[i].materials[j].GetFloat(SmoothAnalog));
                    else
                        Debug.LogWarning("No _Smoothness on " + Renderers[i].gameObject.name);
                }

                Renderers[i].materials = InstancedMaterials[i];
            }

            ready = true;
            ElicitorHue = _hue;
            ElicitorIntensity = _intensity;
        }

        void RevertMaterials()
        {
            if (!ready) return;
            for (int i = 0; i < Renderers.Length; i++)
            {
                Renderers[i].materials = OriginalMaterials[i];
            }

            ready = false;
        }

        void OnDisable()
        {
            StopHighlight();
            RevertMaterials();
        }

        void OnEnable()
        {
            ReplaceMaterials();
            StopHighlight();
        }

        public void RunElicitor()
        {
            if (tagParent != null) RunHighlightRoutine(tagParent.flashDuration);
        }

        public void RunHighlightRoutine(float animationDuration)
        {
            StartHighlight();
            Invoke("StopHighlight", animationDuration);
        }

        public void StartHighlight()
        {
            if (!ready) ReplaceMaterials();
            for (int i = 0; i < Renderers.Length; i++)
            {
                for (int j = 0; j < Renderers[i].materials.Length; j++)
                    Renderers[i].materials[j].SetFloat(HighlightPropertyString, 1f);
            }
        }

        public void StopHighlight()
        {
            if (!ready) return;
            for (int i = 0; i < Renderers.Length; i++)
            {
                for (int j = 0; j < Renderers[i].materials.Length; j++)
                    Renderers[i].materials[j].SetFloat(HighlightPropertyString, 0f);
            }
        }

        public float _intensity = 0.6f;

        public float ElicitorIntensity
        {
            get { return _intensity; }
            set
            {
                if (value > 0.0f && value <= 1.0f)
                {
                    _intensity = value;

                    if (!ready) return;
                    for (int i = 0; i < Renderers.Length; i++)
                    {
                        for (int j = 0; j < Renderers[i].materials.Length; j++)
                        {
                            if (!Renderers[i].materials[j].HasProperty(MinHighlightProperty)) continue;
                            if (!Renderers[i].materials[j].HasProperty(MaxHighlightProperty)) continue;
                            Renderers[i].materials[j].SetFloat(MinHighlightProperty, value);
                            Renderers[i].materials[j].SetFloat(MaxHighlightProperty, value);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Intensity must be between 0.0f and 1.0f");
                }
            }
        }

        public float _hue = 0.6f;

        public float ElicitorHue
        {
            get { return _hue; }
            set
            {
                if (value >= 0.0f && value <= 1.0f)
                {
                    _hue = value;
                    if (!ready) return;
                    for (int i = 0; i < Renderers.Length; i++)
                    {
                        for (int j = 0; j < Renderers[i].materials.Length; j++)
                        {
                            if (!Renderers[i].materials[j].HasProperty(HighlightColorProperty)) continue;
                            Color current = Renderers[i].materials[j].GetColor(HighlightColorProperty);
                            float h, s, v;
                            Color.RGBToHSV(current, out h, out s, out v);
                            Color newColor = Color.HSVToRGB(value, s, v);
                            Renderers[i].materials[j].SetColor(HighlightColorProperty, newColor);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Hue must be between 0.0f and 1.0f");
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neurable.Interactions.Samples
{
    public class OutlineFlash : MonoBehaviour, INeurableUtilityControllable
    {
        public Material outlineMaterial;
        [SerializeField]
        private Material[][] InstancedMaterials;
        private Material[][] OriginalMaterials;

        /*
         * Script is intended for Amplify HighlightAnimated Shaders
         * With the following Properties
         */
        private string OutlinePropertyString = "_ASEOutlineWidth";
        private string OutlineColorPropertyString = "_ASEOutlineColor";

        /*
         * To use the Outline shaders with generic materials, we
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

        // Use this for initialization
        void Awake()
        {
            tagParent = GetComponent<NeurableTag>();
        }

        void ReplaceMaterials()
        {
            Renderer[] myRenderers = GetComponentsInChildren<Renderer>();
            OriginalMaterials = new Material[myRenderers.Length][];
            InstancedMaterials = new Material[myRenderers.Length][];
            for (int i = 0; i < myRenderers.Length; i++)
            {
                OriginalMaterials[i] = myRenderers[i].materials;
                InstancedMaterials[i] = new Material[myRenderers[i].materials.Length];
                for (int j = 0; j < myRenderers[i].materials.Length; j++)
                {
                    InstancedMaterials[i][j] = new Material(outlineMaterial);
                    if (myRenderers[i].materials[j].HasProperty(AlbedoAnalog))
                        InstancedMaterials[i][j]
                            .SetTexture("_Albedo", myRenderers[i].materials[j].GetTexture(AlbedoAnalog));
                    else
                        Debug.LogWarning("No Albedo on " + myRenderers[i].gameObject.name);
                    if (myRenderers[i].materials[j].HasProperty(NormalAnalog))
                        InstancedMaterials[i][j]
                            .SetTexture("_Normal", myRenderers[i].materials[j].GetTexture(NormalAnalog));
                    else
                        Debug.LogWarning("No _Normal on " + myRenderers[i].gameObject.name);
                    if (myRenderers[i].materials[j].HasProperty(OcclusionAnalog))
                        InstancedMaterials[i][j]
                            .SetTexture("_Occlusion", myRenderers[i].materials[j].GetTexture(OcclusionAnalog));
                    else
                        Debug.LogWarning("No _Occlusion on " + myRenderers[i].gameObject.name);
                    if (myRenderers[i].materials[j].HasProperty(EmissionAnalog))
                        InstancedMaterials[i][j]
                            .SetTexture("_Emission", myRenderers[i].materials[j].GetTexture(EmissionAnalog));
                    else
                        Debug.LogWarning("No _Emission on " + myRenderers[i].gameObject.name);
                    if (myRenderers[i].materials[j].HasProperty(ColorAnalog))
                        InstancedMaterials[i][j].SetColor("_Color", myRenderers[i].materials[j].GetColor(ColorAnalog));
                    else
                        Debug.LogWarning("No _Color on " + myRenderers[i].gameObject.name);
                    if (myRenderers[i].materials[j].HasProperty(SmoothAnalog))
                        InstancedMaterials[i][j]
                            .SetFloat("_Smoothness", myRenderers[i].materials[j].GetFloat(SmoothAnalog));
                    else
                        Debug.LogWarning("No _Smoothness on " + myRenderers[i].gameObject.name);
                }

                myRenderers[i].materials = InstancedMaterials[i];
            }

            ElicitorHue = _hue;
        }

        void RevertMaterials()
        {
            Renderer[] myRenderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < myRenderers.Length; i++)
            {
                myRenderers[i].materials = OriginalMaterials[i];
            }
        }

        void OnDisable()
        {
            StopOutline();
            RevertMaterials();
        }

        void OnEnable()
        {
            ReplaceMaterials();
            StopOutline();
        }

        public void RunElicitor()
        {
            RunOutlineRoutine(tagParent.flashDuration);
        }

        public void RunOutlineRoutine(float animationDuration)
        {
            StartOutline();
            Invoke("StopOutline", animationDuration);
        }

        public float _intensityScalar = .5f;

        public void StartOutline()
        {
            for (int i = 0; i < InstancedMaterials.Length; i++)
            {
                for (int j = 0; j < InstancedMaterials[i].Length; j++)
                    InstancedMaterials[i][j].SetFloat(OutlinePropertyString, ElicitorIntensity * _intensityScalar);
            }
        }

        public void StopOutline()
        {
            for (int i = 0; i < InstancedMaterials.Length; i++)
            {
                for (int j = 0; j < InstancedMaterials[i].Length; j++)
                    InstancedMaterials[i][j].SetFloat(OutlinePropertyString, 0f);
            }
        }

        private float _intensity = 0.6f;

        public float ElicitorIntensity
        {
            get { return _intensity; }
            set
            {
                if (value > 0.0f && value <= 1.0f)
                {
                    _intensity = value;
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
                    for (int i = 0; i < InstancedMaterials.Length; i++)
                    {
                        for (int j = 0; j < InstancedMaterials[i].Length; j++)
                        {
                            Color current = InstancedMaterials[i][j].GetColor(OutlineColorPropertyString);
                            float h, s, v;
                            Color.RGBToHSV(current, out h, out s, out v);
                            Color newColor = Color.HSVToRGB(value, s, v);
                            InstancedMaterials[i][j].SetColor(OutlineColorPropertyString, newColor);
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

using System.Collections;
using UnityEngine;

namespace Neurable.Interactions.Samples
{
    public class MeshDeform : MonoBehaviour, INeurableUtilityControllable
    {
        public Material deformMat;
        [SerializeField]
        private Material[][] InstancedMaterials;
        private Material[][] OriginalMaterials;

        /*
         * Script is intended for Amplify HighlightAnimated Shaders
         * With the following Properties
         */
        private string DeformPropertyString = "_AddNoise";
        private string MaxDeformProperty = "_MaxNoise";
        public float DeformLimit = 0.03f;

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
                    InstancedMaterials[i][j] = new Material(deformMat);
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
            StopDeform();
            RevertMaterials();
        }

        void OnEnable()
        {
            ReplaceMaterials();
            StopDeform();
        }

        public void RunElicitor()
        {
            RunDeformation(tagParent.flashDuration);
        }

        bool deforming = false;
        Coroutine DeformationProcess;

        public void RunDeformation(float animationDuration)
        {
            if (!deforming)
            {
                if (DeformationProcess != null) StopCoroutine(DeformationProcess);
                DeformationProcess = StartCoroutine(DeformLerp(animationDuration));
            }
            else
                Debug.LogError("Overlap");
        }

        public void StartDeform()
        {
            for (int i = 0; i < InstancedMaterials.Length; i++)
            {
                for (int j = 0; j < InstancedMaterials[i].Length; j++)
                    InstancedMaterials[i][j].SetFloat(DeformPropertyString, 1f);
            }
        }

        public void StopDeform()
        {
            for (int i = 0; i < InstancedMaterials.Length; i++)
            {
                for (int j = 0; j < InstancedMaterials[i].Length; j++)
                    InstancedMaterials[i][j].SetFloat(DeformPropertyString, 0f);
            }
        }

        private IEnumerator DeformLerp(float animationDuration)
        {
            deforming = true;
            float d_timer = 0f, modifier = 0f;
            SetDeformation(modifier);
            StartDeform();
            while (d_timer <= animationDuration / 2)
            {
                modifier = d_timer / (animationDuration / 2);
                d_timer += Time.deltaTime;
                SetDeformation(modifier);
                yield return null;
            }

            d_timer = 0f;
            while (d_timer <= animationDuration / 2)
            {
                modifier = 1 - d_timer / (animationDuration / 2);
                SetDeformation(modifier);
                d_timer += Time.deltaTime;
                yield return null;
            }

            StopDeform();
            deforming = false;
            yield return null;
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
                    SetDeformation();
                }
                else
                {
                    Debug.LogError("Intensity must be between 0.0f and 1.0f");
                }
            }
        }

        private void SetDeformation(float modifier = 1f)
        {
            if (modifier < 0f || modifier > 1f) Debug.LogWarning("Modifer should be 0.0-1.0");
            float magnitude = ElicitorIntensity * DeformLimit * modifier;
            for (int i = 0; i < InstancedMaterials.Length; i++)
            {
                for (int j = 0; j < InstancedMaterials[i].Length; j++)
                {
                    InstancedMaterials[i][j].SetFloat(MaxDeformProperty, magnitude);
                }
            }
        }

        public float ElicitorHue
        {
            get { return 0.0f; }
            set { }
        }
    }
}

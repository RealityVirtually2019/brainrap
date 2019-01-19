/*
* Copyright 2017 Neurable Inc.
*/

using System.Collections;
using UnityEngine;

namespace Neurable.Interactions.Samples
{
    public class TextureSwapDecay : MonoBehaviour
    {
        public Renderer _TargetRenderer;
        public Material _SwapMaterial;
        public float _DecayTime = 0.7f;

        protected Material[] OldMaterials;
        protected Material[] NewMaterials;
        protected float initOpacity;

        // Use this for initialization
        void Start()
        {
            if (!_TargetRenderer || !_SwapMaterial)
            {
                Debug.LogError("NO RENDERER OR TEXTURE FOUND FOR TEXTURESWAP: " + name);
                return;
            }

            OldMaterials = _TargetRenderer.materials;
            NewMaterials = new Material[OldMaterials.Length + 1];

            int i = 0;
            foreach (Material _mat in OldMaterials)
            {
                NewMaterials[i++] = _mat;
            }

            NewMaterials[i++] = _SwapMaterial;

            initOpacity = _SwapMaterial.GetFloat("_Opacity");
        }

        public void SwapMat()
        {
            _TargetRenderer.materials = NewMaterials;
            StartCoroutine(fadeMat());
        }

        public void RevertMat()
        {
            _TargetRenderer.materials = OldMaterials;
            _SwapMaterial.SetFloat("_Opacity", initOpacity);
        }

        public IEnumerator fadeMat()
        {
            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / _DecayTime)
            {
                _SwapMaterial.SetFloat("_Opacity", Mathf.Lerp(initOpacity, 0, t));
                yield return null;
            }

            RevertMat();
            yield return null;
        }
    }
}

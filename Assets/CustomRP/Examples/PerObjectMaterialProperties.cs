using System;
using UnityEngine;

namespace CustomRP.Examples
{
    [DisallowMultipleComponent]
    public class PerObjectMaterialProperties : MonoBehaviour
    {
        private static int baseColorId = Shader.PropertyToID("_BaseColor");
        private static int cutoffId = Shader.PropertyToID("_Cutoff");
        
        [SerializeField]
        Color baseColor = Color.white;
        [SerializeField]
        float cutoff = 0.5f;

        private static MaterialPropertyBlock block;
        
        void OnValidate()
        {
            if (block == null)
            {
                block = new MaterialPropertyBlock();
            }
            block.SetColor(baseColorId, baseColor);
            block.SetFloat(cutoffId, cutoff);
            GetComponent<Renderer>().SetPropertyBlock(block);
        }

        private void Awake()
        {
            OnValidate();
        }
    }
}
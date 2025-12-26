using System;
using UnityEngine;

namespace CustomRP.Examples
{
    [DisallowMultipleComponent]
    public class PerObjectMaterialProperties : MonoBehaviour
    {
        private static int baseColorId = Shader.PropertyToID("_BaseColor");
        private static int cutoffId = Shader.PropertyToID("_Cutoff");
        private static int metallicId = Shader.PropertyToID("_Metallic");
        private static int smoothnessId = Shader.PropertyToID("_Smoothness");
        
        [SerializeField]
        Color baseColor = Color.white;
        [SerializeField]
        float cutoff = 0.5f;
        
        // 定义金属度和光滑度
        [SerializeField, Range(0f, 1f)]
        float metallic = 0f;
        [SerializeField, Range(0f, 1f)]
        float smoothness = 0.5f;

        private static MaterialPropertyBlock block;
        
        void OnValidate()
        {
            if (block == null)
            {
                block = new MaterialPropertyBlock();
            }
            block.SetColor(baseColorId, baseColor);
            block.SetFloat(cutoffId, cutoff);
            block.SetFloat(metallicId, metallic);
            block.SetFloat(smoothnessId, smoothness);
            
            GetComponent<Renderer>().SetPropertyBlock(block);
        }

        private void Awake()
        {
            OnValidate();
        }
    }
}
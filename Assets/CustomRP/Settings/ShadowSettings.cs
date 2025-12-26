//*****************************************
//创建人：BPS
//创建时间：2025.12.26
//功能说明：阴影设置
//*****************************************

using UnityEngine;

namespace CustomRP.Settings
{

    
    [System.Serializable]
    public class ShadowSettings
    {
        // 阴影最大距离
        [Min(0f)] public float maxDistance = 100f;

        // 阴影贴图大小
        public enum TextureSize
        {
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
            _8192 = 8192
        }
        
        [System.Serializable]
        public struct Directional
        {
            public TextureSize atlasSize;
        }

        // 默认尺寸为 1024
        public Directional directional = new Directional()
        {
            atlasSize = TextureSize._2048
        };
    }
}
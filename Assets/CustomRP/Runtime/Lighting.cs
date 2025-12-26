using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public class Lighting
    {
        private const string bufferName = "Lighting";
        
        CommandBuffer buffer = new CommandBuffer
        {
            name = bufferName
        };
        
        // 存储相机剔除后的结果数据
        CullingResults cullingResults;
        // 限制最大可见平行光数量为 4
        private const int maxDirLightCount = 4;
        
        // private static int dirLightColorId = Shader.PropertyToID("_DirectionalLightColor");
        // private static int dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");
        private static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
        private static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
        private static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

        // 存储可见光的颜色和方向
        private static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
        private static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];

        public void Setup(ScriptableRenderContext context, CullingResults cullingResults)
        {
            this.cullingResults = cullingResults;
            buffer.BeginSample(bufferName);
            // 发送光源数据
            // SetupDirectionalLight();
            SetupLights();
            
            buffer.EndSample(bufferName);
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }
        // 发送多个光源数据
        void SetupLights()
        {
            // 得到所有可见光
            NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;

            int dirLightCount = 0;
            
            for (int i = 0; i < visibleLights.Length; i++)
            {
                VisibleLight visibleLight = visibleLights[i];
                
                // 如果是方向光，我们才进行数据存储
                if (visibleLight.lightType == LightType.Directional)
                {
                    SetupDirectionalLight(dirLightCount++, ref visibleLight);
                    // 当灯光数量达到最大值，则终止循环
                    if (dirLightCount >= maxDirLightCount)
                    {
                        break;
                    }
                }
            }
            // 将数据发送给GPU
            buffer.SetGlobalInt(dirLightCountId, dirLightCount);
            buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
            buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
        }

        // 将可见光的光照颜色和方向存储到数组
        void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
        {
            dirLightColors[index] = visibleLight.finalColor;
            // 该矩阵的第三列是光源的前向向量
            dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        }
    }
}
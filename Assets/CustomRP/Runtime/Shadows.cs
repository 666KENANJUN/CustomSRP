//*****************************************
//创建人：BPS
//创建时间：2025.12.26
//功能说明：处理阴影
//*****************************************

using CustomRP.Settings;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public class Shadows
    {
        const string bufferName = "Shadows";
        
        CommandBuffer buffer = new CommandBuffer
        {
            name = bufferName
        };
        
        ScriptableRenderContext context;
        
        CullingResults cullingResults;
        
        ShadowSettings settings;

        // 可投射阴影的定向光数量
        private const int maxShadowedDirectionalLightCount = 4;

        struct ShadowedDirectionalLight
        {
            public int visibleLightIndex;
        }

        // 存储可投射阴影的可见光源的索引
        ShadowedDirectionalLight[] ShadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

        // 已存储的可投射阴影的定向光数量
        private int ShadowedDirectionalLightCount;

        private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
        
        public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
        {
            this.context = context;
            this.cullingResults = cullingResults;
            this.settings = settings;
            
            ShadowedDirectionalLightCount = 0;
        }

        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        // 存储可见光的阴影数据
        public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
        {
            // 存储可见光源的索引，前提是光源开启了阴影投射并且阴影强度不能为 0
            if (ShadowedDirectionalLightCount < maxShadowedDirectionalLightCount && light.shadows != LightShadows.None && light.shadowStrength > 0f
                // 还需要加上一个判断，是否在阴影最大投射距离内，有被该光源影响且需要投影的物体存在，如果没有就不需要渲染该光源的阴影贴图了
                && cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
            {
                ShadowedDirectionalLights[ShadowedDirectionalLightCount++] = new ShadowedDirectionalLight
                {
                    visibleLightIndex = visibleLightIndex
                };
            }
        }

        // 渲染阴影
        public void Render()
        {
            if (ShadowedDirectionalLightCount > 0)
            {
                RenderDirectionalShadows();
            }
        }
        
        // 渲染定向光阴影
        void RenderDirectionalShadows()
        { 
            // 创建renderTexture, 并指定该类型是阴影贴图
            int atlasSize = (int)settings.directional.atlasSize;
            buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            // 指定渲染数据存储到RT中
            buffer.SetRenderTarget(dirShadowAtlasId,RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            // 清除深度缓冲区
            buffer.ClearRenderTarget(true, false, Color.clear);
            
            buffer.BeginSample(bufferName);
            ExecuteBuffer();

            int split    = ShadowedDirectionalLightCount <= 1 ? 1 : 2;
            int tileSize = atlasSize / split;

            for (int i = 0; i < ShadowedDirectionalLightCount; i++)
            {
                RenderDirectionalShadows(i, split, tileSize);
            }
            
            buffer.EndSample(bufferName);
            ExecuteBuffer();
        }

        // 渲染单个定向光阴影
        // 阴影贴图本质也是一张深度图，它记录了从光源位置出发，到能看到的场景中距离它最近的表面位置（深度信息）。但是方向光并没有一个真实位置，我们要做地是找出与光的方向匹
        // 配的视图和投影矩阵，并给我们一个裁剪空间的立方体，该立方体与包含光源阴影的摄影机的可见区域重叠，这些数据的获取我们不用自己去实现，可以直接调用
        //    cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives方法，它需要9个参数。第1个是可见光的索引，第2、3、4个参数用于设置阴影级联数据，后面我们会处理
        //    它，第5个参数是阴影贴图的尺寸，第6个参数是阴影近平面偏移，我们先忽略它。最后三个参数都是输出参数，一个是视图矩阵，一个是投影矩阵，一个是ShadowSplitData对象，它描
        //    述有关给定阴影分割（如定向级联）的剔除信息。
        void RenderDirectionalShadows(int index, int split, int tileSize)
        {
            ShadowedDirectionalLight light          = ShadowedDirectionalLights[index];
            var                      shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
            
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, 0,1,Vector3.zero, tileSize,0f,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);

            shadowSettings.splitData = splitData;
            
            // 设置渲染视口
            SetTileViewport(index, split, tileSize);
            // 设置视图投影矩阵
            buffer.SetViewProjectionMatrices(viewMatrix,projectionMatrix);
            ExecuteBuffer();
            context.DrawShadows(ref shadowSettings);
        }

        void SetTileViewport(int index, int split, float tileSize)
        {
            // 计算索引图块的偏移位置
            Vector2 offset = new Vector2(index % split, index / split);
            // 设置渲染视口，拆分成多个图块
            buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        }

        // 释放临时渲染纹理
        public void Cleanup()
        {
            buffer.ReleaseTemporaryRT(dirShadowAtlasId);
            ExecuteBuffer();
        }
    }
}
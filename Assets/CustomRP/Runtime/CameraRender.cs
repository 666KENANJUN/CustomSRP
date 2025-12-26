using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//*****************************************
//创建人：BPS
//创建时间：2025.12.25
//功能说明：相机单独渲染
//         两个重要的接口：ScriptableRenderContext、CommandBuffer(命令缓冲区，是一个容器，保存了这些将要执行渲染的命令)
//*****************************************
namespace CustomRP.Runtime
{
    public partial class CameraRender
    {
        ScriptableRenderContext context;
        Camera camera;

        private const string bufferName = "Render Camera";
        CommandBuffer buffer = new CommandBuffer()
        {
            name = bufferName
        };
        
        static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        static ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");
        
        Lighting lighting = new Lighting();

        public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing)
        {
            this.context = context;
            this.camera = camera;
            // 设置命令缓冲区的名字
            PrepareBuffer();

            // 在Game 视图绘制的几何体也绘制到Scene视图中
            PrepareForSceneWindow();
            
            if (!Cull())
            {
                return;
            }

            Setup();
            
            lighting.Setup(context, cullingResults);
            // 绘制可见的几何体
            DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
            // 绘制SRP不支持的着色器类型
            DrawUnsupportedShaders();
            // 绘制Gizmos
            DrawGizmos();
            Submit();
        }

        /// <summary>
        /// 设置
        /// </summary>
        private void Setup()
        {
            //设置相机的属性和矩阵
            context.SetupCameraProperties(camera);
            // 得到相机的clear flags
            CameraClearFlags flags = camera.clearFlags;

            // 清空屏幕[是否需要清除深度数据、是否需要清除颜色数据，设置清除颜色数据的颜色]
            buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color,
                                     flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
            buffer.BeginSample(SampleName);
            ExecuteBuffer();
        }

        // 存储剔除后的结果数据
        CullingResults cullingResults;

        /// <summary>
        /// 剔除
        /// </summary>
        /// <returns></returns>
        bool Cull()
        {
            ScriptableCullingParameters p;
            if (camera.TryGetCullingParameters(out p))
            {
                cullingResults = context.Cull(ref p);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 绘制可见的几何体
        /// </summary>
        private void DrawVisibleGeometry(bool useDynamicBatching,bool useGPUInstancing)
        {
            // 遵循：不透明物体->绘制天空盒->绘制透明物体 的绘制顺序
            //      先绘制不透明物体，绘制天空盒的时候，经过深度测试，部分区域像素已经被不透明物体所占用，绘制天空盒的时候也就减少了绘制像素的数量，
            // 最后绘制透明物体，因为不会进行深度测试，所以可以通过颜色混合正确地绘制到屏幕上
            
            // 设置绘制顺序和指定渲染相机
            var sortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            // 设置渲染的Shader Pass 和排序模式
            var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
            {
                // 设置渲染时批处理的使用状态
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing
            };
            // 渲染CustomLit表示的pass块
            drawingSettings.SetShaderPassName(1, litShaderTagId);
            // 设置哪些类型的渲染队列可以被绘制
            var filterSettings  = new FilteringSettings(RenderQueueRange.opaque);
            
            // 1.绘制不透明物体
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
            
            // 2. 绘制天空盒
            context.DrawSkybox(camera);
            
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            // 只绘制RenderQueue为transparent的透明物体
            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            
            // 3. 绘制透明物体
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
            
        }

        /// <summary>
        /// 提交缓冲区渲染命令
        /// </summary>
        private void Submit()
        {
            buffer.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();
        }

        /// <summary>
        /// 执行缓冲区，然后清理
        /// </summary>
        private void ExecuteBuffer()
        {
            // 执行缓冲区命令
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }
    }
}

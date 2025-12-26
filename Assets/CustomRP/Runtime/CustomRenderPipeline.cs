using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//*****************************************
//创建人：BPS
//创建时间：2025.12.25
//功能说明：自定义渲染管线
//*****************************************
namespace CustomRP.Runtime
{
    public class CustomRenderPipeline : RenderPipeline
    {
        private CameraRender renderer = new CameraRender();
        bool useDynamicBatching , useGPUInstancing;

        public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher)
        {
            // 设置合批启用状态
            this.useDynamicBatching                              = useDynamicBatching;
            this.useGPUInstancing                                = useGPUInstancing;
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
            // 灯光使用线性光强
            GraphicsSettings.lightsUseLinearIntensity          = true;
        }
        

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
                renderer.Render(context, camera, useDynamicBatching, useGPUInstancing);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using CustomRP.Settings;
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
        ShadowSettings shadowSettings;

        public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, ShadowSettings shadowSettings)
        {
            // 设置合批启用状态
            this.useDynamicBatching                              = useDynamicBatching;
            this.useGPUInstancing                                = useGPUInstancing;
            this.shadowSettings                                  = shadowSettings;
            
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
            // 灯光使用线性光强
            GraphicsSettings.lightsUseLinearIntensity          = true;
        }
        

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (Camera camera in cameras)
            {
                renderer.Render(context, camera, useDynamicBatching, useGPUInstancing,shadowSettings);
            }
        }
    }
}

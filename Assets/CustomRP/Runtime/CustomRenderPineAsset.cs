using UnityEngine;
using UnityEngine.Rendering;

//*****************************************
//创建人：BPS
//创建时间：2025.12.25
//功能说明：自定义渲染管线资产
//*****************************************
namespace CustomRP.Runtime
{
    [CreateAssetMenu(fileName = "CustomRenderPineAsset", menuName = "Rendering/CreateCustomRenderPipeline")]
    public class CustomRenderPineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new CustomRenderPipeline();
        }
    }
}

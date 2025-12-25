using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

//*****************************************
//创建人：BPS
//创建时间：2025.12.25
//功能说明：分离编辑器的功能
//*****************************************
namespace CustomRP.Runtime
{
    public partial class CameraRender
    {
        private partial void DrawUnsupportedShaders();
        private partial void DrawGizmos();
        private partial void PrepareForSceneWindow();
        private partial void PrepareBuffer();
#if UNITY_EDITOR

        // SRP不支持的着色器标签类型
        private static ShaderTagId[] legacyShaderTagIds =
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrePassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM"),
        };

        // 绘制成使用错误材质的粉红颜色
        private static Material errorMaterial;

        /// <summary>
        /// 绘制SRP不支持的着色器类型
        /// </summary>
        private partial void DrawUnsupportedShaders()
        {
            if (errorMaterial == null)
            {
                errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
            }

            var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
            {
                overrideMaterial = errorMaterial
            };
            for (int i = 1; i < legacyShaderTagIds.Length; i++)
            {
                // 遍历数组逐个设置着色器的PassName , 从 i = 1 开始
                drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
            }

            // 使用默认设置即可，反正画出来的都是不支持的
            var filterSettings = FilteringSettings.defaultValue;
            // 绘制不支持的ShaderTag类型的物体
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
        }

        /// <summary>
        /// 绘制Gizmos
        /// </summary>
        private partial void DrawGizmos()
        {
            if (Handles.ShouldRenderGizmos())
            {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
        }

        /// <summary>
        /// 在Game视图中绘制的几何体也绘制到Scene视图中
        /// </summary>
        private partial void PrepareForSceneWindow()
        {
            if (camera.cameraType == CameraType.SceneView)
            {
                // 如果切换到了Scene视图，调用此方法完成绘制
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
        }

        string SampleName { get; set; }
        private partial void PrepareBuffer()
        {
            Profiler.BeginSample("Editor Only");
            buffer.name = SampleName = camera.name;
            Profiler.EndSample();
        }
#else
        const string SampleName = bufferName;
#endif
    }
}

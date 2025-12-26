using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

//*****************************************
//创建人：BPS
//创建时间：
//功能说明：
//*****************************************
public class CustomShaderGUI : ShaderGUI
{
    private MaterialEditor editor;
    private Object[] materials;
    private MaterialProperty[] properties;

    private bool showPresets;

    private bool Clipping
    {
        set => SetProperty("_Clipping", "_CLIPPING", value);
    }

    private bool PremultiplyAlpha
    {
        set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
    }

    private BlendMode SrcBlend
    {
        set => SetProperty("_SrcBlend", (float)value);
    }

    private BlendMode DstBlend
    {
        set => SetProperty("_DstBlend", (float)value);
    }

    private bool ZWrite
    {
        set => SetProperty("_ZWrite", value ? 1f : 0f);
    }

    RenderQueue RenderQueue
    {
        set
        {
            foreach (Material material in materials)
            {
                material.renderQueue = (int)value;
            }
        }
    }
    
    bool         HasProperty(string name) => FindProperty(name, properties, false) != null;
    private bool HasPremultiplyAlpha      => HasProperty("_PremulAlpha");

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);
        editor          = materialEditor;
        materials       = materialEditor.targets;
        this.properties = properties;

        EditorGUILayout.Space();
        showPresets = EditorGUILayout.Foldout(showPresets, "Presets", true);

        if (showPresets)
        {
            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();
        }
    }

    // 设置材质属性
    bool SetProperty(string name, float value)
    {
        MaterialProperty property = FindProperty(name, properties, false);
        if (property != null)
        {
            property.floatValue = value;
            return true;
        }
        return false;
    }

    // 设置关键字状态
    void SetKeyword(string keyword, bool enabled)
    {
        if (enabled)
        {
            foreach (Material material in materials)
            {
                material.EnableKeyword(keyword);
            }
        }
        else
        {
            foreach (Material material in materials)
            {
                material.DisableKeyword(keyword);
            }
        }
    }

    // 同时设置关键字和属性
    void SetProperty(string name, string keyword, bool value)
    {
        if (SetProperty(name, value ? 1f : 0f))
        {
            SetKeyword(keyword, value);
        }
    }

    /// <summary>
    /// 给每种渲染模式创建一个按钮，点击它之后可以一键配置所有相关需要调节的属性
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    bool PresetButton(string name)
    {
        if (GUILayout.Button(name))
        {
            // 属性重置
            editor.RegisterPropertyChangeUndo(name);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 不透明渲染模式
    /// </summary>
    void OpaquePreset()
    {
        if (PresetButton("Opaque"))
        {
            Clipping         = false;
            PremultiplyAlpha = false;
            SrcBlend         = BlendMode.One;
            DstBlend         = BlendMode.Zero;
            ZWrite           = true;
            RenderQueue      = RenderQueue.Geometry;
        }
    }

    /// <summary>
    /// 裁剪模式，跟不透明模式差不多
    /// </summary>
    void ClipPreset()
    {
        if (PresetButton("Clip"))
        {
            Clipping         = true;
            PremultiplyAlpha = false;
            SrcBlend         = BlendMode.One;
            DstBlend         = BlendMode.Zero;
            ZWrite           = true;
            RenderQueue      = RenderQueue.AlphaTest;
        }
    }

    /// <summary>
    /// 标准的透明渲染模式
    /// </summary>
    void FadePreset()
    {
        if (PresetButton("Fade"))
        {
            Clipping         = false;
            PremultiplyAlpha = false;
            SrcBlend         = BlendMode.SrcAlpha;
            DstBlend         = BlendMode.OneMinusSrcAlpha;
            ZWrite           = false;
            RenderQueue      = RenderQueue.Transparent;
        }
    }

    /// <summary>
    /// 跟透明模式差不多，但是预乘了透明度，可以应用于拥有正确照明的半透明表面
    /// </summary>
    void TransparentPreset()
    {
        if (HasPremultiplyAlpha && PresetButton("Transparent"))
        {
            Clipping         = false;
            PremultiplyAlpha = true;
            SrcBlend         = BlendMode.One;
            DstBlend         = BlendMode.OneMinusSrcAlpha;
            ZWrite           = false;
            RenderQueue      = RenderQueue.Transparent;
        }
    }
}
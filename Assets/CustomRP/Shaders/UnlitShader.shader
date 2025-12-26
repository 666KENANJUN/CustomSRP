// 此Shader 为不受光照影响的Unlit Shader
Shader "CustomRP/Unlit"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        //透明度测试的阈值
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Toggle(_CLIPPING)] _Clipping("Alpha Clipping", Float) = 0
        
        // 设置混合模式
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
        // 默认写入深度缓冲区
        [Enum(Off,0,On,1)] _ZWrite("Z Write", Float) = 1
    }
    SubShader
    {
        Pass
        {
            // 定义混合模式
            Blend[_SrcBlend][_DstBlend]
            // 是否写入深度
            ZWrite[_ZWrite]
            HLSLPROGRAM
            // shader_feature 可以让Unity根据不同的定义条件或关键字编译多次，生成多个着色器变体。然后通过外部代码或者材质面板上的开关来启用某个关键字，
            // 加载对应的着色器变种版本来执行某些特定功能，是项目开发中比较常用的一种手段。
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"
            ENDHLSL
        }
    }

    CustomEditor "CustomShaderGUI"
}

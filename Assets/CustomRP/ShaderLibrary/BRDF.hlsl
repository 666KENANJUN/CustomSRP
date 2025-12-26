// BRDF相关库
#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED
#include "./Surface.hlsl"
#include "./Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

// 电介质的反射率平均约 0.04。作为我们最小反射率
#define MIN_REFLECTIVITY 0.04

// 计算不反射的值，将范围从0-1 调整到 0-0.96，与URP一样
float OneMinusReflectivity(float metallic)
{
    float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

struct BRDF
{
    float3 diffuse;
    float3 specular;
    float roughness;
};
// 获得给定表面的BRDF数据
BRDF GetBRDF(Surface surface)
{
    BRDF brdf;
    // 物体表面对光线的反射率 会受到金属度的影响，物体的金属度越高，其自身反照率越不明显，对周围环境景象的反射就越清晰。达到最大时就完全反射显示了周围的环境景象。
    float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
    brdf.diffuse = surface.color * oneMinusReflectivity;
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);

    // 光滑度转为实际的粗糙度
    float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    return brdf;
}

// 根据公式计算镜面反射强度
float SpecularStrength(Surface surface, BRDF brdf, Light light)
{
    float3 h = SafeNormalize(light.direction + surface.viewDirection);
    float nh2 = Square(saturate(dot(surface.normal, h)));
    float lh2 = Square(saturate(dot(light.direction, h)));
    float r2 = Square(brdf.roughness);
    float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
    float normalization = brdf.roughness * 4.0 + 2.0;
    return r2 / (d2 * max(0.1, lh2) * normalization);
}

// 直接光照的表面颜色
float3 DirectBRDF(Surface surface, BRDF brdf, Light light)
{
    return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

#endif
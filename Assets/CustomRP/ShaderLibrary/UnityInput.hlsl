// Unity 标准输入库
#ifndef CUSTOM_UNITY_Input_INCLUDED
#define CUSTOM_UNITY_Input_INCLUDED

CBUFFER_START(UnityPerDraw)
// 定义一个从模型空间转换到世界空间的转换矩阵
float4x4 unity_ObjectToWorld;
float4x4 unity_WorldToObject;
float4 unity_LODFade;
real4 unity_WorldTransformParams;

CBUFFER_END

// 定义一个从世界空间转换到裁剪空间的矩阵
float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 unity_MatrixInvV;
float4x4 unity_prev_MatrixM;
float4x4 unity_prev_MatrixIM;
float4x4 glstate_matrix_projection;

#endif

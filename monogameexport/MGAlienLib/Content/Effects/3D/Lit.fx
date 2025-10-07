#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

cbuffer Data : register(b0)
{
    // WVP 매트릭스 선언
    float4x4 World;
    float4x4 ViewProjection;
    Texture2D _MainTex;
    float _Sharpness;
    sampler2D MainTextureSampler = sampler_state
    {
        Texture = <_MainTex>;
        AddressU = CLAMP;
        AddressV = CLAMP;
    };
}


struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 WorldPosition : TEXCOORD1;
	float4 Color : COLOR0;
};

float3 test_Lighting(float3 modelNormal)
{
    float3 worldNormal = mul(float4(modelNormal, 0), World).xyz;
    float3 fake_directionalLight = normalize(float3(1, 1, 1));
    float half_lambert = dot(worldNormal, fake_directionalLight) * .5 + .5;
    return float3(half_lambert, half_lambert, half_lambert);
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProjection);
    
    float3 L = test_Lighting(input.Normal);
    output.Color.rgb = input.Color.rgb * L;
    output.Color.a = 1;
    
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return input.Color;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
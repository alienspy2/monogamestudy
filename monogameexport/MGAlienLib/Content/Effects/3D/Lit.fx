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
    float4 _BaseColor;

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
    float2 TextureCoordinates : TEXCOORD0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 tc0 : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
	float4 Color : COLOR0;
};

float3 test_Lighting(float3 modelNormal)
{
    float3 worldNormal = normalize(mul(float4(modelNormal, 0), World).xyz);
    float3 fake_directionalLight = normalize(float3(1, 1, 1));
    float half_lambert = dot(worldNormal, fake_directionalLight) * .5 + .5;
    return float3(half_lambert, half_lambert, half_lambert);
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProjection);
    output.tc0 = input.TextureCoordinates;

    float3 L = test_Lighting(input.Normal);
    output.Color.rgb = input.Color.rgb * L * _BaseColor.rgb;
    output.Color.a = 1 * _BaseColor.a;
    
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 tex = tex2D(MainTextureSampler, input.tc0);
    float3 color = tex.rgb * input.Color.rgb;
    float alpha = tex.a * input.Color.a;

    return float4(color, alpha);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
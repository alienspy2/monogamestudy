// WVP 매트릭스 선언
float4x4 World;
float4x4 View;
float4x4 Projection;

Texture2D _MainTex;

sampler2D TextureSampler = sampler_state
{
 	Texture = <_MainTex>;
};

struct VS_INPUT
{
    float4 Position : POSITION;
    float4 Color : COLOR;
};

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
};

// 정점 셰이더: WVP 매트릭스 적용
PS_INPUT VS_Main(VS_INPUT input)
{
    PS_INPUT output;
    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    output.Color = input.Color; // 색상 유지
    return output;
}

// 픽셀 셰이더: 입력된 색상 그대로 출력
float4 PS_Main(PS_INPUT input) : SV_Target
{
	return tex2D(TextureSampler,input.TextureCoordinates) * input.Color;
}

technique BasicTech
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader  = compile ps_3_0 PS_Main();
    }
}

// #if OPENGL
// 	#define SV_POSITION POSITION
// 	#define VS_SHADERMODEL vs_3_0
// 	#define PS_SHADERMODEL ps_3_0
// #else
// 	#define VS_SHADERMODEL vs_4_0_level_9_1
// 	#define PS_SHADERMODEL ps_4_0_level_9_1
// #endif

// Texture2D SpriteTexture;

// sampler2D SpriteTextureSampler = sampler_state
// {
// 	Texture = <SpriteTexture>;
// };

// struct VertexShaderOutput
// {
// 	float4 Position : SV_POSITION;
// 	float4 Color : COLOR0;
// 	float2 TextureCoordinates : TEXCOORD0;
// };

// float4 MainPS(VertexShaderOutput input) : COLOR
// {
// 	return tex2D(SpriteTextureSampler,input.TextureCoordinates) * input.Color;
// }

// technique SpriteDrawing
// {
// 	pass P0
// 	{
// 		PixelShader = compile PS_SHADERMODEL MainPS();
// 	}
// };
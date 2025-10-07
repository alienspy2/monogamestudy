cbuffer Data : register(b0)
{
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

cbuffer ScissorData : register(b1) // Scissor Test용 데이터
{
    float4 _Scissors[16]; // x, y, width, height
};



struct VS_INPUT
{
    float4 Position : POSITION;
    float4 Color : COLOR;
    float2 TextureCoordinates : TEXCOORD0;
    float4 extData : TEXCOORD2;
};

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
    float2 TextureCoordinates : TEXCOORD0;
    float2 _2dPos : TEXCOORD1;
    float4 extData : TEXCOORD2;
};

// 정점 셰이더: WVP 매트릭스 적용
PS_INPUT VS_Main(VS_INPUT input)
{
    PS_INPUT output;
    output.Position = input.Position;
    output.Position = mul(output.Position, ViewProjection);
    output.Color = input.Color; // 색상 유지
    output.TextureCoordinates = input.TextureCoordinates;
    output._2dPos = input.Position.xy;
    output.extData = input.extData;
    
    return output;
}

// 픽셀 셰이더: 입력된 색상 그대로 출력
float4 PS_Main(PS_INPUT input) : SV_Target
{
    float4 _mainTex = tex2D(MainTextureSampler,input.TextureCoordinates);
    if (_mainTex.r < 1.5/255) discard;
    float a = pow(_mainTex.r, _Sharpness);
    
    int scissorIndex = (int)round(input.extData.x);
    if (scissorIndex >= 0)
    {
        float4 rect = _Scissors[scissorIndex];
        if (input._2dPos.x < rect.x || input._2dPos.x > rect.x + rect.z || input._2dPos.y < rect.y || input._2dPos.y > rect.y + rect.w)
            //return float4(1, 0, 0, 1);
            discard;
    }
    
    // default
    return float4(input.Color.rgb, a * input.Color.a);
}

technique BasicTech
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader  = compile ps_3_0 PS_Main();
    }
}

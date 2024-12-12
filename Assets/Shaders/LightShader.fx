#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
sampler LightMap : register(s1);
sampler BloomMap : register(s2);

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{	
    float4 base = tex2D(SpriteTextureSampler, input.TextureCoordinates);
    base.rgb *= tex2D(LightMap, input.TextureCoordinates).rgb;
    base.rgb += tex2D(BloomMap, input.TextureCoordinates).rgb;
	
	
    return saturate(base);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
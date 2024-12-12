#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

static const int LightMax = 10000;

int LightCount;
float2 screenSize;
float2 camPos;
float4 ambientLight;

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 base = ambientLight;
    float2 mypos = input.Position.xy * screenSize + camPos;
	
	[Unroll(LightMax)]
    for (int i = 0; i < LightCount; i++)
    {
        float4 posRad = tex2D(SpriteTextureSampler, float2(0, (float) i / (LightCount - 1)));
        float3 col = tex2D(SpriteTextureSampler, float2(1.1 / 4, (float) i / (LightCount - 1))).xyz;
        float3 dec = tex2D(SpriteTextureSampler, float2(2.1 / 4, (float) i / (LightCount - 1))).xyz;
        float3 extra = tex2D(SpriteTextureSampler, float2(3.1 / 4, (float) i / (LightCount - 1))).xyz;
		
        //Point Light
        if (posRad.w == 0)
        {
            float dist = sqrt(pow(posRad.x - mypos.x, 2.0) + pow(posRad.y - mypos.y, 2.0));
            float mult = saturate((posRad.z - dist) / posRad.z);
            base.xyz += (col * lerp(dec, col, mult)) * mult;
        }
        //Line Light
        else if (posRad.w == 1)
        {
            float2 start = posRad.xy;
            float2 end = extra.xy;
            
            float a = distance(start, end);
            float b = distance(start, mypos);
            float c = distance(end, mypos);
            
            float C = atan2(mypos.y - start.y, mypos.x - start.x) - atan2(end.y - start.y, end.x - start.x);
            float B = atan2(start.y - end.y, start.x - end.x) - atan2(mypos.y - end.y, mypos.x - end.x);
            float A = 3.1416 - (B + C);
            
            float d = cos(C) * b;
            float e = cos(B) * c;
            float f = sqrt(pow(b, 2) - pow(d, 2));
            
            float mult = saturate(((posRad.z / 2) - f) / (posRad.z / 2));
            if (f < (posRad.z / 2) && abs(d) < a && abs(e) < a) 
                base.xyz += (col * lerp(dec, col, mult)) * mult;
        }
        //Cone Light
        else if (posRad.w == 2 && posRad.z > 0)
        {
            float2 lightPos = posRad.xy;
            float ang = atan2(mypos.y - lightPos.y, mypos.x - lightPos.x);
            float dif = ang - extra.x;
            dif += dif > 3.14159 ? -6.28318 : dif < -3.14159 ? 6.28318 : 0;
            
            if (abs(dif) <= posRad.z)
            {
                float mult = (posRad.z - abs(dif)) / posRad.z;
                base.xyz += (col * lerp(dec, col, pow(mult, 0.5))) * pow(mult, 0.5);
            }
        }
    }
	
    return saturate(base);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
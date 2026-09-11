#include "Macros.fxh"

DECLARE_TEXTURE(Texture, 0);
DECLARE_TEXTURE(LightMap, 1);

BEGIN_CONSTANTS
MATRIX_CONSTANTS
    float4x4 WorldViewProj _vs(c0) _cb(c0);
END_CONSTANTS

struct VSOutput
{
    float4 position : SV_Position;
    float4 color    : COLOR0;
    float2 texCoord : TEXCOORD0;
};

VSOutput MainVS(float4 position : POSITION0, float4 color : COLOR0, float2 texCoord : TEXCOORD0)
{
    VSOutput output;
    output.position = mul(position, WorldViewProj);
    output.color = color;
    output.texCoord = texCoord;
    return output;
}

float4 MainPS(VSOutput input) : SV_Target0
{
    return SAMPLE_TEXTURE(Texture, input.texCoord) * input.color;
}

TECHNIQUE(Default, MainVS, MainPS);

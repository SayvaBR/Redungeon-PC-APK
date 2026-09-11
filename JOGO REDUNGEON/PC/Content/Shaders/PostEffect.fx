#include "Macros.fxh"

DECLARE_TEXTURE(Image, 0);
DECLARE_TEXTURE(AuxImage, 2);

BEGIN_CONSTANTS
MATRIX_CONSTANTS
    float4x4 WorldViewProj _vs(c0) _cb(c0);
    float Time;
    float DrunkF;
    float DrunkA;
    float DrunkDoublingA;
    float InnerVignette;
    float OuterVignette;
    float4 VignetteTint;
    float2 Resolution;
    float2 SpotlightCenter;
    float SpotlightRadius;
    float SpotlightOpacity;
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

float4 DrunkPS(VSOutput input) : SV_Target0
{
    // Reconstruido a partir do assembly do shader original (Android): o efeito
    // real combina uma distorcao em espiral (redemoinho, mais forte longe do
    // centro, animada no tempo) com uma media de 5 amostras em cruz por cima
    // do resultado distorcido -- nao e so um "ghost" simples.
    float aspect = Resolution.x / max(Resolution.y, 1.0);
    float2 centered = input.texCoord - 0.5;
    centered.x *= aspect;
    float dist = length(centered);

    float angle = dist * 12.0 - Time * DrunkF;
    float s = sin(angle);
    float c = cos(angle);

    float2 perp = float2(-centered.y, centered.x) / aspect;
    float2 swirled = input.texCoord + perp * c * DrunkA;

    float4 cCenter = SAMPLE_TEXTURE(Image, swirled);
    float4 cUp    = SAMPLE_TEXTURE(Image, swirled + float2(0.0, -DrunkDoublingA));
    float4 cDown  = SAMPLE_TEXTURE(Image, swirled + float2(0.0,  DrunkDoublingA));
    float4 cLeft  = SAMPLE_TEXTURE(Image, swirled + float2(-DrunkDoublingA, 0.0));
    float4 cRight = SAMPLE_TEXTURE(Image, swirled + float2( DrunkDoublingA, 0.0));

    float4 result = (cCenter + cUp + cDown + cLeft + cRight) * 0.2;
    return result * input.color;
}

float4 VignettePS(VSOutput input) : SV_Target0
{
    float4 baseColor = SAMPLE_TEXTURE(Image, input.texCoord) * input.color;
    float dist = distance(input.texCoord, float2(0.5, 0.5)) * 2.0;
    float vig = saturate(1.0 - smoothstep(InnerVignette, OuterVignette, dist));
    baseColor.rgb = lerp(VignetteTint.rgb, baseColor.rgb, vig);
    return baseColor;
}

// Reconstructed from the original MGFX GLSL: two drifting cloud layers,
// stepped purple palette and a rippling opening. UVs use the short axis so
// portrait, landscape and window resizing preserve a circular player area.
float4 SpotlightPS(VSOutput input) : SV_Target0
{
    float4 scene = SAMPLE_TEXTURE(Image, input.texCoord) * input.color;
    float2 aspect = Resolution / max(min(Resolution.x, Resolution.y), 1.0);
    float2 uv = input.texCoord;
    float2 offset = (uv - SpotlightCenter) * aspect;
    float distanceToPlayer = length(offset);
    float ripple = sin(offset.x / max(distanceToPlayer, 0.001) * 10.0 + Time * 5.0) * 0.01;
    float dist = distanceToPlayer + ripple;
    float radius = max(SpotlightRadius, 0.01);
    float cloudA = SAMPLE_TEXTURE(AuxImage, uv * aspect * 0.5 + Time * float2(0.1, -0.1)).r;
    float cloudB = SAMPLE_TEXTURE(AuxImage, uv * aspect + Time * 0.1).r;
    float cloud = (cloudA + cloudB) * 0.5;
    float rim = 1.0 - smoothstep(radius, radius * 1.66, dist);
    float density = saturate(cloud + rim);
    float band = step(0.6, density) * 0.2 + step(0.8, density) * 0.2 + step(0.9, density) * 0.2;
    float3 smoke = float3(0.89, 0.0, 0.69) * band;
    float3 outside = lerp(scene.rgb * 0.5, smoke, step(radius * 1.25, dist));
    float3 result = lerp(scene.rgb, outside, step(radius, dist));
    return float4(lerp(scene.rgb, result, SpotlightOpacity), scene.a);
}
TECHNIQUE(Drunk, MainVS, DrunkPS);
TECHNIQUE(Vignette, MainVS, VignettePS);
TECHNIQUE(Spotlight, MainVS, SpotlightPS);

Shader "UI Toolkit/Filters/CrtScanlines_4Params"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}

        _ScanlineStrength ("Scanline Strength", Range(0,1)) = 0.35
        _ScanlineFrequency ("Scanline Period (Pixels)", Float) = 8.0
        _ChromaticOffset ("Chromatic Offset (Unit UV)", Float) = 0.0015
        _Curvature ("Curvature", Float) = 0.08
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off
        ZWrite Off
        ZTest Always
        Blend Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram
            #pragma multi_compile _ _UIE_OUTPUT_LINEAR

            #include "UnityCG.cginc"
            #include "UnityUIEFilter.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize; // x=1/width, y=1/height, z=width, w=height

            float _ScanlineStrength;
            float _ScanlineFrequency;   // treated as period in pixels
            float _ChromaticOffset;
            float _Curvature;

            // ********************************
            // FIXED LOOK CONSTANTS
            // ********************************
            static const float FixedVignetteStrength = 0.25;
            static const float FixedNoiseStrength = 0.03;

            struct VertexToFragment
            {
                float4 clipSpacePosition : SV_POSITION;
                float2 atlasUv : TEXCOORD0;
                uint atlasUvRectIndex : TEXCOORD1;
            };

            float2 NormalizeAtlasUvToUnitUv(float2 atlasUv, float4 atlasUvRect)
            {
                return float2(
                    (atlasUv.x - atlasUvRect.x) / atlasUvRect.z,
                    (atlasUv.y - atlasUvRect.y) / atlasUvRect.w
                );
            }

            float2 MapUnitUvToAtlasUv(float2 unitUv, float4 atlasUvRect)
            {
                return float2(
                    unitUv.x * atlasUvRect.z + atlasUvRect.x,
                    unitUv.y * atlasUvRect.w + atlasUvRect.y
                );
            }

            float HashFromUnitUv(float2 unitUv)
            {
                float hashedValue = frac(sin(dot(unitUv, float2(12.9898, 78.233))) * 43758.5453);
                return hashedValue;
            }

            half3 UnpremultiplyRgb(half4 premultipliedColor)
            {
                half inverseAlpha = (premultipliedColor.a > 1e-5h) ? (1.0h / premultipliedColor.a) : 0.0h;
                return premultipliedColor.rgb * inverseAlpha;
            }

            VertexToFragment VertexProgram(FilterVertexInput vertexInput)
            {
                VertexToFragment output;
                output.clipSpacePosition = UnityObjectToClipPos(vertexInput.vertex);
                output.atlasUv = TRANSFORM_TEX(vertexInput.uv, _MainTex);
                output.atlasUvRectIndex = GetFilterRectIndex(vertexInput);
                return output;
            }
// ********************************
// AVALANCHE HASH (MurmurHash3 fmix32)
// ********************************
uint MurmurFmix32(uint hashValue)
{
    hashValue ^= (hashValue >> 16);
    hashValue *= 0x85ebca6bu;
    hashValue ^= (hashValue >> 13);
    hashValue *= 0xc2b2ae35u;
    hashValue ^= (hashValue >> 16);
    return hashValue;
}

uint Hash2DTo1D(uint2 integerCoordinates, uint seed)
{
    // Mix coordinates and seed, then avalanche.
    uint hashValue = integerCoordinates.x * 0x9E3779B9u;
    hashValue ^= integerCoordinates.y * 0x85EBCA6Bu;
    hashValue ^= seed;
    return MurmurFmix32(hashValue);
}

float Hash2DToFloat01(uint2 integerCoordinates, uint seed)
{
    uint hashValue = Hash2DTo1D(integerCoordinates, seed);
    return (float)hashValue * (1.0 / 4294967296.0); // [0,1)
}

// ********************************
// SMOOTH VALUE NOISE IN PIXEL SPACE
// ********************************
float GetValueNoise01FromPixelCoordinates(float2 pixelCoordinates, float cellSizePixels, uint seed)
{
    float2 gridCoordinates = pixelCoordinates / max(1.0, cellSizePixels);

    uint2 cellInteger = (uint2)floor(gridCoordinates);
    float2 cellFraction = frac(gridCoordinates);

    // Smoothstep interpolation to band-limit.
    float2 smoothFraction = cellFraction * cellFraction * (3.0 - 2.0 * cellFraction);

    float v00 = Hash2DToFloat01(cellInteger + uint2(0, 0), seed);
    float v10 = Hash2DToFloat01(cellInteger + uint2(1, 0), seed);
    float v01 = Hash2DToFloat01(cellInteger + uint2(0, 1), seed);
    float v11 = Hash2DToFloat01(cellInteger + uint2(1, 1), seed);

    float vx0 = lerp(v00, v10, smoothFraction.x);
    float vx1 = lerp(v01, v11, smoothFraction.x);

    return lerp(vx0, vx1, smoothFraction.y); // 0..1
}
static const float FixedBloomStrength = 0.35;
static const float FixedBloomThreshold = 0.70;
static const float FixedBloomRadiusPixels = 1.5;

half3 SampleStraightRgbAtUnitUv(float2 unitUv, float4 atlasUvRect)
{
    float2 atlasUv = MapUnitUvToAtlasUv(unitUv, atlasUvRect);
    half4 premultipliedColor = tex2D(_MainTex, atlasUv);
    return UnpremultiplyRgb(premultipliedColor);
}

float GetLuminance(half3 straightRgb)
{
    return dot((float3)straightRgb, float3(0.2126, 0.7152, 0.0722));
}

half3 ComputeBloomStraightRgb(float2 curvedUnitUv, float rectPixelWidth, float rectPixelHeight, float4 atlasUvRect)
{
    float2 unitUvStep =
        float2(FixedBloomRadiusPixels / max(1.0, rectPixelWidth),
               FixedBloomRadiusPixels / max(1.0, rectPixelHeight));

    float2 unitUvCenter = curvedUnitUv;
    float2 unitUvLeft = saturate(curvedUnitUv + float2(-unitUvStep.x, 0.0));
    float2 unitUvRight = saturate(curvedUnitUv + float2(unitUvStep.x, 0.0));
    float2 unitUvDown = saturate(curvedUnitUv + float2(0.0, -unitUvStep.y));
    float2 unitUvUp = saturate(curvedUnitUv + float2(0.0, unitUvStep.y));

    half3 centerStraightRgb = SampleStraightRgbAtUnitUv(unitUvCenter, atlasUvRect);
    half3 leftStraightRgb = SampleStraightRgbAtUnitUv(unitUvLeft, atlasUvRect);
    half3 rightStraightRgb = SampleStraightRgbAtUnitUv(unitUvRight, atlasUvRect);
    half3 downStraightRgb = SampleStraightRgbAtUnitUv(unitUvDown, atlasUvRect);
    half3 upStraightRgb = SampleStraightRgbAtUnitUv(unitUvUp, atlasUvRect);

    float centerLum = GetLuminance(centerStraightRgb);
    float leftLum = GetLuminance(leftStraightRgb);
    float rightLum = GetLuminance(rightStraightRgb);
    float downLum = GetLuminance(downStraightRgb);
    float upLum = GetLuminance(upStraightRgb);

    float centerMask = max(0.0, centerLum - FixedBloomThreshold);
    float leftMask = max(0.0, leftLum - FixedBloomThreshold);
    float rightMask = max(0.0, rightLum - FixedBloomThreshold);
    float downMask = max(0.0, downLum - FixedBloomThreshold);
    float upMask = max(0.0, upLum - FixedBloomThreshold);

    float centerWeight = 0.45;
    float sideWeight = 0.1375;

    half3 bloomAccumulation =
        centerStraightRgb * (half)(centerMask * centerWeight) +
        leftStraightRgb * (half)(leftMask * sideWeight) +
        rightStraightRgb * (half)(rightMask * sideWeight) +
        downStraightRgb * (half)(downMask * sideWeight) +
        upStraightRgb * (half)(upMask * sideWeight);

    float weightSum =
        centerMask * centerWeight +
        leftMask * sideWeight +
        rightMask * sideWeight +
        downMask * sideWeight +
        upMask * sideWeight;

    if (weightSum <= 1e-5)
        return half3(0.0h, 0.0h, 0.0h);

    return bloomAccumulation / (half)weightSum;
}

half4 FragmentProgram(VertexToFragment input) : SV_Target
{
    float4 atlasUvRect = GetFilterUVRect(input.atlasUvRectIndex);
    float2 unitUv = NormalizeAtlasUvToUnitUv(input.atlasUv, atlasUvRect);

    float2 centeredUnitUv = unitUv - 0.5;

    // Curvature in normalized space.
    float2 curvatureOffset = centeredUnitUv * (centeredUnitUv * centeredUnitUv) * _Curvature;
    float2 curvedUnitUv = unitUv + curvatureOffset;

    float isInside =
        step(0.0, curvedUnitUv.x) * step(0.0, curvedUnitUv.y) *
        step(curvedUnitUv.x, 1.0) * step(curvedUnitUv.y, 1.0);

    curvedUnitUv = saturate(curvedUnitUv);

    float2 centerAtlasUv = MapUnitUvToAtlasUv(curvedUnitUv, atlasUvRect);

    // Chromatic offset in normalized space.
    float2 chromaticOffsetUnitUv = float2(_ChromaticOffset, 0.0);
    float2 redAtlasUv = MapUnitUvToAtlasUv(saturate(curvedUnitUv + chromaticOffsetUnitUv), atlasUvRect);
    float2 blueAtlasUv = MapUnitUvToAtlasUv(saturate(curvedUnitUv - chromaticOffsetUnitUv), atlasUvRect);

    half4 centerPremultiplied = tex2D(_MainTex, centerAtlasUv);
    half4 redPremultiplied = tex2D(_MainTex, redAtlasUv);
    half4 bluePremultiplied = tex2D(_MainTex, blueAtlasUv);

    half alpha = centerPremultiplied.a;

    half3 centerStraightRgb = UnpremultiplyRgb(centerPremultiplied);
    half3 redStraightRgb = UnpremultiplyRgb(redPremultiplied);
    half3 blueStraightRgb = UnpremultiplyRgb(bluePremultiplied);

    half3 straightRgb = half3(redStraightRgb.r, centerStraightRgb.g, blueStraightRgb.b);

    // ********************************
    // PIXEL-BASED SCANLINES (stable)
    // ********************************
    float rectPixelHeight = atlasUvRect.w * _MainTex_TexelSize.w;
    float localPixelY = curvedUnitUv.y * rectPixelHeight;

    float scanlinePeriodPixels = max(1.0, _ScanlineFrequency);
    float scanlinePhase = (localPixelY / scanlinePeriodPixels) + (_Time.y * 6.0);
    float scanlineWave = 0.5 + 0.5 * sin(scanlinePhase * 6.2831853);
    float scanlineMultiplier = 1.0 - _ScanlineStrength * (1.0 - scanlineWave);
    straightRgb *= (half)scanlineMultiplier;

    // Vignette (fixed).
    float vignetteFactor = 1.0 - FixedVignetteStrength * saturate(dot(centeredUnitUv, centeredUnitUv) * 2.0);
    straightRgb *= (half)vignetteFactor;

    // ********************************
    // STATIC NOISE (smooth value noise + avalanche hash)
    // ********************************
    float rectPixelWidth = atlasUvRect.z * _MainTex_TexelSize.z;
    float localPixelX = curvedUnitUv.x * rectPixelWidth;

    float2 localPixelCoordinates = float2(localPixelX, localPixelY);

    float frameIndex = floor(_Time.y * 30.0);
    uint frameSeed = (uint)frameIndex * 747796405u + 2891336453u;

    // Key knob for killing moiré: increase this.
    // 1.0 = harsh “snow”, 4–12 = grain that survives warping without moiré.
    const float NoiseCellSizePixels = 0.5;

    float noise01 = GetValueNoise01FromPixelCoordinates(localPixelCoordinates, NoiseCellSizePixels, frameSeed);
    float noiseSigned = (noise01 - 0.5) * 0.5;

    straightRgb += (half)(noiseSigned * FixedNoiseStrength);

    // Re-premultiply and respect bounds.
    half3 premultipliedRgb = straightRgb * alpha;
    half4 outputColor = half4(premultipliedRgb, alpha) * (half)isInside;

    #if UIE_OUTPUT_LINEAR
    outputColor.rgb = GammaToLinearSpace(outputColor.rgb);
    #endif

    return outputColor;
}

            ENDHLSL
        }
    }
}

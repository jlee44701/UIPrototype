Shader "UI Toolkit/Filters/PixelGlitchSweep"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}

        // 0..1
        _Amount ("Amount", Float) = 0

        // Interpreted as pixels (float); in USS you can pass 8px etc (converted to float).
        _PixelSize ("Pixel Size (px)", Float) = 5

        // Interpreted as pixels (float)
        _Amplitude ("Amplitude (px)", Float) = 0.1

        // Interpreted as degrees (float); in USS you can pass 45deg etc (converted to float).
        _Direction ("Direction (deg)", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
 
        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off

            // UI Toolkit filter inputs are premultiplied alpha.
            Blend One OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _UIE_OUTPUT_LINEAR

            #include "UnityCG.cginc"
            #include "UnityUIEFilter.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            float _Amount;
            float _PixelSize;
            float _Amplitude;
            float _Direction;

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                uint rectIndex  : TEXCOORD1;
            };

            v2f vert(FilterVertexInput v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.rectIndex = GetFilterRectIndex(v);
                return o;
            }

            float2 NormalizeUVs(float2 uv, float4 uvRect)
            {
                return float2(
                    (uv.x - uvRect.x) / uvRect.z,
                    (uv.y - uvRect.y) / uvRect.w
                );
            }

            float2 MapToUVRect(float2 uv, float4 uvRect)
            {
                return float2(
                    uv.x * uvRect.z + uvRect.x,
                    uv.y * uvRect.w + uvRect.y
                );
            }

            float Hash12(float2 p)
            {
                // Cheap stable hash for per-cell randomness.
                float h = dot(p, float2(127.1, 311.7));
                return frac(sin(h) * 43758.5453123);
            }

            half3 Unpremultiply(half4 c)
            {
                return (c.a > 1e-5h) ? (c.rgb / c.a) : half3(0, 0, 0);
            }

            half4 frag(v2f i) : SV_Target
            {
                float amount = saturate(_Amount);

                float4 uvRect = GetFilterUVRect(i.rectIndex);

                // Convert atlas UVs to element-local 0..1
                float2 uv = NormalizeUVs(i.uv, uvRect);

                // Approx element size in pixels (based on atlas size + rect size).
                float2 atlasSizePx = _MainTex_TexelSize.zw;
                float2 elementSizePx = max(uvRect.zw * atlasSizePx, 1.0);

                // Pixelation ramps from 1px (no pixelation) to _PixelSize (max pixelation).
                float pixelSizePx = lerp(1.0, max(_PixelSize, 1.0), amount);

                // Grid in cells (integer-ish)
                float2 grid = max(floor(elementSizePx / pixelSizePx), 1.0);

                float2 cell = floor(uv * grid);
                float2 cellCenter = (cell + 0.5) / grid;

                // Direction in degrees -> radians for sin/cos.
                float dirRad = radians(_Direction);
                float2 dir = normalize(float2(cos(dirRad), sin(dirRad)));

                // Sweep factor along direction (0..1 across the rect).
                float2 corners[4] = { float2(0,0), float2(1,0), float2(0,1), float2(1,1) };
                float d0 = dot(corners[0], dir);
                float d1 = dot(corners[1], dir);
                float d2 = dot(corners[2], dir);
                float d3 = dot(corners[3], dir);
                float dMin = min(min(d0, d1), min(d2, d3));
                float dMax = max(max(d0, d1), max(d2, d3));

                float proj01 = (dot(cellCenter, dir) - dMin) / max(dMax - dMin, 1e-5);

                // Feather edge about ~2 cells.
                float feather = max(2.0 / max(grid.x, grid.y), 1e-3);
                float local = saturate((amount - proj01) / feather);

                // Random breakup so it feels “blocky” rather than a perfect wipe.
                float rnd = Hash12(cell);
                local = saturate(local + (rnd - 0.5) * 0.35);

                // Pixelate only where local > 0
                float2 sampleUV = lerp(uv, cellCenter, local);

                // Per-block jitter in pixels -> UV
                float jitterPx = (rnd - 0.5) * 2.0 * _Amplitude * local;
                float2 jitterUV = (dir * jitterPx) / elementSizePx;
                sampleUV = saturate(sampleUV + jitterUV);

                // Chromatic aberration (in px -> UV), tied to local and amplitude.
                float chromaPx = _Amplitude * 0.5 * local;
                float2 chromaUV = (dir * chromaPx) / elementSizePx;

                float2 uvR = saturate(sampleUV + chromaUV);
                float2 uvG = sampleUV;
                float2 uvB = saturate(sampleUV - chromaUV);

                // Map back into atlas UV rect
                uvR = MapToUVRect(uvR, uvRect);
                uvG = MapToUVRect(uvG, uvRect);
                uvB = MapToUVRect(uvB, uvRect);

                half4 cR = tex2D(_MainTex, uvR);
                half4 cG = tex2D(_MainTex, uvG);
                half4 cB = tex2D(_MainTex, uvB);

                // Combine in straight-alpha space, then premultiply for output.
                half3 rU = Unpremultiply(cR);
                half3 gU = Unpremultiply(cG);
                half3 bU = Unpremultiply(cB);

                half a = max(cR.a, max(cG.a, cB.a));
                half3 rgbU = half3(rU.r, gU.g, bU.b);

                half4 outCol;
                outCol.a = a;
                outCol.rgb = rgbU * a;

                // Force-gamma workflow: last pass must linearize.
                #if UIE_OUTPUT_LINEAR
                outCol.rgb = GammaToLinearSpace(outCol.rgb);
                #endif

                return outCol;
            }
            ENDHLSL
        }
    }

    Fallback Off
}

Shader "Custom/SilhouetteShader"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
        _Resolution ("Resolution", Float) = 0.001
        _OutlineThickness ("Outline Thickness", Float) = 0.5
        _FadeThickness ("Fade Thickness", Range(0, 20)) = 20
        _AlphaFadeThickness ("Alpha Fade Thickness", Float) = 75
        _CenterColor ("Center Color", Color) = (1, 1, 1, 1)
        [HDR]_EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {
            "RenderType"="Transparent" 
        }

        Blend SrcAlpha OneMinusSrcAlpha

        // ECHO (LEFT) ------

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define DIV_SQRT_2 0.70710678118
            #define DIV16_LONG_SIDE 0.8944272
            #define DIV16_SHORT_SIDE 0.4472136

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Resolution;
            int _FadeThickness;
            float _AlphaFadeThickness;
            float _OutlineThickness;
            float4 _EdgeColor;
            float4 _CenterColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(float4(v.vertex.x - _OutlineThickness, v.vertex.yzw));
                o.uv = TRANSFORM_TEX(v.uv0, _MainTex);

                return o;
            }

            float InverseLerp(float a, float b, float v) {
                return (v - a) / (b - a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 directions[16] = {float2(1, 0), float2(0, 1), float2(-1, 0), float2(0, -1),
                    float2(DIV_SQRT_2, DIV_SQRT_2), float2(-DIV_SQRT_2, DIV_SQRT_2),
                    float2(-DIV_SQRT_2, -DIV_SQRT_2), float2(DIV_SQRT_2, -DIV_SQRT_2),
                    float2(DIV16_LONG_SIDE, DIV16_SHORT_SIDE), float2(DIV16_SHORT_SIDE, DIV16_LONG_SIDE),
                    float2(-DIV16_LONG_SIDE, DIV16_SHORT_SIDE), float2(-DIV16_SHORT_SIDE, DIV16_LONG_SIDE),
                    float2(DIV16_LONG_SIDE, -DIV16_SHORT_SIDE), float2(DIV16_SHORT_SIDE, -DIV16_LONG_SIDE),
                    float2(-DIV16_LONG_SIDE, -DIV16_SHORT_SIDE), float2(-DIV16_SHORT_SIDE, -DIV16_LONG_SIDE)
                };

                float loopsToEdge = _FadeThickness;

                for (uint loopCount = 0; loopCount < _FadeThickness; loopCount++) {
                    for (uint index = 0; index < 16; index++) {
                        float2 sampleUV = i.uv + directions[index] * (loopCount + 1) * _Resolution;
                        if (tex2D(_MainTex, sampleUV).a <= 0) {
                            loopsToEdge = min(loopsToEdge, loopCount);
                        }
                    }
                }

                // NOTE: calculating the alpha depends on how the single-frame UV is laid out:
                // the number of columns in used to locate the frame's uv within the atlas's uv

                float columnWidth = 0.25;

                uint column = floor(i.uv.x / columnWidth);
                float xPosInColumn = (i.uv.x - column * columnWidth) / columnWidth;
                float columnMaxX = i.uv.x - (i.uv.x - column * columnWidth) + columnWidth;

                float farthestTransparentSample = 0;
                float targetEdgeX = 0;
                for (float loop = 0; loop < columnWidth / _Resolution; loop++) {
                    float2 sampleUV = float2(columnMaxX - loop * _Resolution, i.uv.y);
                    float sampleAlpha = tex2D(_MainTex, sampleUV).a;

                    farthestTransparentSample += (sampleAlpha <= 0) * (sampleUV.x - farthestTransparentSample);
                    targetEdgeX += (sampleAlpha > 0) * (farthestTransparentSample > i.uv.x) * (sampleUV.x - targetEdgeX);
                }

                float alpha = saturate(tex2D(_MainTex, i.uv).a - abs(targetEdgeX - i.uv.x) * _AlphaFadeThickness);

                float distToEdge = InverseLerp(_FadeThickness, 0, loopsToEdge);
                float4 color = lerp(_CenterColor, _EdgeColor, distToEdge);

                return float4(distToEdge.xxx * color, alpha);
            }
            ENDCG
        }

        // ECHO (RIGHT) ------

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define DIV_SQRT_2 0.70710678118
            #define DIV16_LONG_SIDE 0.8944272
            #define DIV16_SHORT_SIDE 0.4472136

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Resolution;
            int _FadeThickness;
            float _AlphaFadeThickness;
            float _OutlineThickness;
            float4 _EdgeColor;
            float4 _CenterColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(float4(v.vertex.x + _OutlineThickness, v.vertex.yzw));
                o.uv = TRANSFORM_TEX(v.uv0, _MainTex);
                
                return o;
            }

            float InverseLerp(float a, float b, float v) {
                return (v - a) / (b - a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 directions[16] = {float2(1, 0), float2(0, 1), float2(-1, 0), float2(0, -1),
                    float2(DIV_SQRT_2, DIV_SQRT_2), float2(-DIV_SQRT_2, DIV_SQRT_2),
                    float2(-DIV_SQRT_2, -DIV_SQRT_2), float2(DIV_SQRT_2, -DIV_SQRT_2),
                    float2(DIV16_LONG_SIDE, DIV16_SHORT_SIDE), float2(DIV16_SHORT_SIDE, DIV16_LONG_SIDE),
                    float2(-DIV16_LONG_SIDE, DIV16_SHORT_SIDE), float2(-DIV16_SHORT_SIDE, DIV16_LONG_SIDE),
                    float2(DIV16_LONG_SIDE, -DIV16_SHORT_SIDE), float2(DIV16_SHORT_SIDE, -DIV16_LONG_SIDE),
                    float2(-DIV16_LONG_SIDE, -DIV16_SHORT_SIDE), float2(-DIV16_SHORT_SIDE, -DIV16_LONG_SIDE)
                };

                float loopsToEdge = _FadeThickness;

                for (uint loopCount = 0; loopCount < _FadeThickness; loopCount++) {
                    for (uint index = 0; index < 16; index++) {
                        float2 sampleUV = i.uv + directions[index] * (loopCount + 1) * _Resolution;
                        if (tex2D(_MainTex, sampleUV).a <= 0) {
                            loopsToEdge = min(loopsToEdge, loopCount);
                        }
                    }
                }

                // NOTE: calculating the alpha depends on how the single-frame UV is laid out:
                // the number of columns in used to locate the frame's uv within the atlas's uv

                float columnWidth = 0.25;
                
                uint column = floor(i.uv.x / columnWidth);
                float xPosInColumn = (i.uv.x - column * columnWidth) / columnWidth;
                float columnMinX = i.uv.x - (i.uv.x - column * columnWidth);

                float farthestTransparentSample = 0;
                float targetEdgeX = 0;
                for (float loop = 0; loop < columnWidth / _Resolution; loop++) {
                    float2 sampleUV = float2(columnMinX + loop * _Resolution, i.uv.y);
                    float sampleAlpha = tex2D(_MainTex, sampleUV).a;

                    farthestTransparentSample += (sampleAlpha <= 0) * (sampleUV.x - farthestTransparentSample);
                    targetEdgeX += (sampleAlpha > 0) * (farthestTransparentSample < i.uv.x) * (sampleUV.x - targetEdgeX);
                }
                
                float alpha = saturate(tex2D(_MainTex, i.uv).a - abs(targetEdgeX - i.uv.x) * _AlphaFadeThickness);

                float distToEdge = InverseLerp(_FadeThickness, 0, loopsToEdge);
                float4 color = lerp(_CenterColor, _EdgeColor, distToEdge);

                return float4(distToEdge.xxx * color, alpha);
            }
            ENDCG
        }

        // ----- TEXTURE PASS ------

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}

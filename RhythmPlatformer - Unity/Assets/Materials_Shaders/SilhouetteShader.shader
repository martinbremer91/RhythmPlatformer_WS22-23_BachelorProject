Shader "Custom/SilhouetteShader"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset]_UVTex ("SpriteUV", 2D) = "white" {}
        _Resolution ("Resolution", Float) = 0.001
        _OutlineThickness ("Outline Thickness", Float) = 0.5
        _FadeThickness ("Fade Thickness", Range(0, 20)) = 20
        _CenterColor ("Center Color", Color) = (1, 1, 1, 1)
        [HDR]_EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {
            "RenderType"="Transparent" 
        }

        Blend SrcAlpha OneMinusSrcAlpha

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
            sampler2D _UVTex;
            //float4 _UVTex_ST;
            float _Resolution;
            int _FadeThickness;
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

                uint hitCount;

                for (uint loopCount = 0; loopCount < 20; loopCount++) {
                    for (uint index = 0; index < 16; index++) {
                        float2 sampleUV = i.uv + directions[index] * (loopCount + 1 / 20) * _OutlineThickness;
                        if (tex2D(_MainTex, sampleUV).a > 0) {
                            hitCount++;
                        }
                    }
                }

                for (uint loopCount = 0; loopCount < _FadeThickness; loopCount++) {
                    for (uint index = 0; index < 16; index++) {
                        float2 sampleUV = i.uv + directions[index] * (loopCount + 1) * _Resolution;
                        if (tex2D(_MainTex, sampleUV).a <= 0) {
                            loopsToEdge = min(loopsToEdge, loopCount);
                        }
                    }
                }

                float distToEdge = InverseLerp(_FadeThickness, 0, loopsToEdge);
                float alpha = min(tex2D(_MainTex, i.uv).a, 4);
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

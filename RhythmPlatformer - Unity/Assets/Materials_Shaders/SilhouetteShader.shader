Shader "Custom/SilhouetteShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Resolution ("Resolution", Float) = 0.001
        _FadeThickness ("Fade Thickness", Integer) = 20
        _CenterColor ("Center Color", Color) = (1, 1, 1, 1)
        [HDR]_EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
        }

        Blend SrcAlpha OneMinusSrcAlpha

        // EXPAND PASS -------------------------------------------------------

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define DIV_SQRT_2 0.70710678118

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
            float _Resolution;
            uint _FadeThickness;
            float4 _EdgeColor;
            float4 _CenterColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            float InverseLerp(float a, float b, float v) {
                return (v - a) / (b - a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 directions[8] = {float2(1, 0), float2(0, 1), float2(-1, 0), float2(0, -1),
                    float2(DIV_SQRT_2, DIV_SQRT_2), float2(-DIV_SQRT_2, DIV_SQRT_2),
                    float2(-DIV_SQRT_2, -DIV_SQRT_2), float2(DIV_SQRT_2, -DIV_SQRT_2)
                };

                float loopsToEdge = _FadeThickness;
                float alpha = tex2D(_MainTex, i.uv).a;

                for (uint loopCount = 0; loopCount < _FadeThickness; loopCount++) {
                    for (uint index = 0; index < 8; index++) {
                        float2 sampleUV = i.uv + directions[index] * (loopCount + 1) * _Resolution;
                        if (tex2D(_MainTex, sampleUV).a <= 0) {
                            loopsToEdge = min(loopsToEdge, loopCount);
                        }
                    }
                }

                float distToEdge = InverseLerp(_FadeThickness, 0, loopsToEdge);
                float4 color = lerp(_CenterColor, _EdgeColor, distToEdge);
                return float4(distToEdge.xxx * color, alpha);
            }
            ENDCG
        }
    }
}

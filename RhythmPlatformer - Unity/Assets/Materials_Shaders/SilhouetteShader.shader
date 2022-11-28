Shader "Custom/SilhouetteShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Resolution ("Resolution", Float) = 0.001
        _FadeThickness ("Fade Thickness", Integer) = 20
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float InverseLerp(float a, float b, float v) {
                return (v - a) / (b - a);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 directions[8] = {float2(1, 0), float2(0, 1), float2(-1, 0), float2(0, -1),
                    float2(DIV_SQRT_2, DIV_SQRT_2), float2(-DIV_SQRT_2, DIV_SQRT_2),
                    float2(-DIV_SQRT_2, -DIV_SQRT_2), float2(DIV_SQRT_2, -DIV_SQRT_2)
                };

                uint loopsToTransparency = 0;
                bool hitTransparency = false;

                for (uint loopCount = 0; loopCount < _FadeThickness; loopCount++) {
                    for (uint index = 0; index < 8; index++) {
                        float2 sampleUV = i.uv + directions[index] * 0.001 + loopCount * _Resolution;
                        if (tex2D(_MainTex, sampleUV).a == 0) {
                            loopsToTransparency = hitTransparency ? loopsToTransparency : loopCount;
                            hitTransparency = true;
                        }
                    }
                }

                float greyscale = InverseLerp(0, _FadeThickness, loopsToTransparency);
                return float4 (greyscale.xxx, tex2D(_MainTex, i.uv).a);

                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}

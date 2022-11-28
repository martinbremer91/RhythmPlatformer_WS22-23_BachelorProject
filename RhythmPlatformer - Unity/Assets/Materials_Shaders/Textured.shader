Shader "Unlit/Textured"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Pattern ("Pattern", 2D) = "white" {}
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }

        Pass
        {
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define TAU 6.28318530718

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _Pattern;
            float4 _MainTex_ST;

            float GetWave(float coord){
                float wave = cos((coord + _Time.y * 0.1) * TAU * 3) * 0.5 + 0.5;
                wave *= coord;
                return wave;
            }

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                float2 topDownProjection = i.worldPos.xz;
                fixed4 noise = tex2D(_MainTex, topDownProjection);
                float pattern = tex2D(_Pattern, i.uv);

                float wave = GetWave(pattern);

                return float4(wave.xxx, wave > 0.1);
            }
            ENDCG
        }
    }
}

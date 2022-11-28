Shader "Custom/vertexDisplacer"
{
    Properties
    {
        _ColorA ("ColorA", Color) = (1,1,1,1)
        _ColorB ("ColorB", Color) = (1,1,1,1)
        _ColorStart ("ColorStart", Range(0, 1)) = 0
        _WaveAmp ("Wave Amp", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { 
            "RenderType"="Opaque"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define TAU 6.28318530718

            float4 _ColorA;
            float4 _ColorB;

            float _ColorStart;
            float _ColorEnd;

            float _WaveAmp;

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal :NORMAL;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            float GetWave(float2 uv){
                float2 uvsCentered = uv * 2 - 1;
                float radialDistance = length(uvsCentered);

                float wave = cos((radialDistance - _Time.y * 0.1) * TAU * 5) * 0.5 + 0.5;
                wave *= 1 - radialDistance;
                return wave;
            }

            Interpolators vert (MeshData v)
            {
                Interpolators o;

                v.vertex.y = GetWave(v.uv) * _WaveAmp;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                return GetWave(i.uv);
            }
            ENDCG
        }
    }
}

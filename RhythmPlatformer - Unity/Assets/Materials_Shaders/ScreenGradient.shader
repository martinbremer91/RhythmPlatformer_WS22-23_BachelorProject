Shader "Custom/ScreenGradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorSW ("Color SW", Color) = (1,1,1,1)
        _ColorNE ("Color NE", Color) = (1,1,1,1)
        _GradientIntensity ("Intensity", Float) = 0.5
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _ColorSW;
            float4 _ColorNE;
            float _GradientIntensity;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float xPos = i.uv.x;
                float yPos = i.uv.y;

                float value = (xPos + yPos) * .5;

                float4 gradientColor = lerp(_ColorSW, _ColorNE, value);

                col.rgb = col.rgb + gradientColor * _GradientIntensity;

                return col;
            }
            ENDCG
        }
    }
}

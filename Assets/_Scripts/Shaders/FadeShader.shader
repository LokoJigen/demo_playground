Shader "Shaders/FadeToBlack"
{
    Properties
    {
        _FadeAmount ("Fade Amount", Range(0,1)) = 0.95
        _Threshold ("Zero Threshold", Range(0, 1)) = .5
        _MainTex ("Base (RGB)", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Name "Fade"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _FadeAmount;
            float _Threshold;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Fade lineare verso nero
                col.rgb = lerp(col.rgb, 0, 1.0 - _FadeAmount);

                // Se sotto la soglia, forziamo a zero
                if (col.r < _Threshold && col.g < _Threshold && col.b < _Threshold)
                {
                    col.rgb = 0;
                }

                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}

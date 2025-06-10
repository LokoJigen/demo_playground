Shader "Shaders/DrawCircle"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _BrushPos("Brush Position", Vector) = (.5, .5, 0, 0)
        _BrushRadius("Brush Radius", Float) = 0.2
        _BrushStrength("Brush Strength", Float) = 1.0
        _FadeSpeed ("Fade Speed", Range(0,1)) = 0.95
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Name "DrawCircle"

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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _MainTex_ST;
            float2 _BrushPos;
            float _BrushRadius;
            float _BrushStrength;
            float _FadeSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = distance(i.uv, _BrushPos);
                float alpha = smoothstep(_BrushRadius, 0.0, dist) * _BrushStrength;

                fixed4 prevColor = tex2D(_MainTex, i.uv); // Use render texture color
                prevColor.rgb *= _FadeSpeed; 

                fixed4 newColor = fixed4(alpha, alpha, alpha, 1);
                fixed4 finalColor = max(prevColor, newColor);

                return finalColor;
            }
            ENDCG
        }
    }
    FallBack Off
}

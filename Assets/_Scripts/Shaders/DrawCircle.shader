Shader "Shaders/DrawCircle"
{
    Properties
    {
        _BrushPos("Brush Position", Vector) = (-1, -1, 0, 0)
        _BrushRadius("Brush Radius", Float) = 0.2
        _BrushStrength("Brush Strength", Float) = 1.0
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

            float2 _BrushPos;
            float _BrushRadius;
            float _BrushStrength;

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
                return fixed4(alpha, alpha, alpha, 1);
            }
            ENDCG
        }
    }
    FallBack Off
}

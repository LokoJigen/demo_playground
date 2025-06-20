Shader "Shaders/FresnelLitToon"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _FresnelToggle("Enable Fresnel", Float) = 1
        _FresnelThreshold("Fresnel Threshold", Range(0,1)) = 0.5
        _FresnelWidth("Fresnel Width", Range(0.01, 1)) = 0.2
        _FresnelColor("Fresnel Color", Color) = (0.3, 1, 1, 1)
        _FresnelIntensity("Fresnel Intensity", Range(0, 5)) = 1
        _LightColorIntensity("Light Color Intensity", Range(0, 1)) = 1
        _EnableCartoon("Enable Cartoon", Float) = 0
        _EnableOutline("Enable Outline", Float) = 0
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", Range(0, 0.05)) = 0.01

        _UseFresnelPulse("Use Fresnel Pulse", Float) = 0
        _FresnelPulseSpeed("Fresnel Pulse Speed", Range(.1, 100)) = 2
        _FresnelPulseAmplitude("Fresnel Pulse Amplitude", Range(.1, 100)) = 1

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseColor;
            float _FresnelToggle;
            float _FresnelThreshold;
            float _FresnelWidth;
            float4 _FresnelColor;
            float _FresnelIntensity;
            float _LightColorIntensity;
            float _EnableCartoon;
            float _EnableOutline;
            float4 _OutlineColor;
            float _OutlineWidth;

            float _UseFresnelPulse;
            float _FresnelPulseSpeed;
            float _FresnelPulseAmplitude;


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS);
                return OUT;
            }

            float3 ApplyFresnel(float3 color, float3 normal, float3 viewDir)
            {
                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                float edge = smoothstep(_FresnelThreshold, _FresnelThreshold + _FresnelWidth, fresnel);

                // Sin wave
                float time = _Time.y; 
                float pulse = (_UseFresnelPulse > 0.5) ? (sin(time * _FresnelPulseSpeed) * 0.5 + 0.5) * _FresnelPulseAmplitude : 1.0;

                float3 fresnelColor = _FresnelColor.rgb * edge * _FresnelIntensity * pulse;
                return color + fresnelColor;
            }


            float3 ToonRamp(float NdotL)
            {
                if (NdotL > 0.66) return float3(1,1,1);
                else if (NdotL > 0.33) return float3(0.7,0.7,0.7);
                else return float3(0.4,0.4,0.4);
            }

            float3 LightingMainLight(float3 normal, float3 viewDir, float3 baseColor)
            {
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);

                float NdotL = saturate(1.0 - dot(normal, -lightDir));
                float3 lightColor = mainLight.color.rgb * _LightColorIntensity;

                float3 diffuse;
                if (_EnableCartoon > 0.5)
                    diffuse = baseColor * lightColor * ToonRamp(NdotL);
                else
                    diffuse = baseColor * lightColor * NdotL;

                float3 halfDir = normalize(-viewDir - lightDir);
                float NdotH = saturate(dot(normal, halfDir));
                float spec = pow(NdotH, 16.0);
                float3 specular = lightColor * spec;

                float3 ambient = baseColor * SampleSH(normal);
                return saturate(ambient + diffuse + specular);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float3 baseColor = baseMap.rgb * _BaseColor.rgb;

                if (all(baseColor == float3(0.0, 0.0, 0.0)))
                    discard;

                float3 normal = normalize(IN.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);

                float3 litColor = LightingMainLight(normal, viewDir, baseColor);

                if (_FresnelToggle > 0.5)
                    litColor = ApplyFresnel(litColor, normal, viewDir);

                return float4(litColor, 1.0);
            }
            ENDHLSL
        }

        // Outline pass (second pass)
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front // Invert culling to draw the outline in front of the main pass
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float _EnableOutline;
            float _OutlineWidth;
            float4 _OutlineColor;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 norm = normalize(IN.normalOS);
                float3 offset = norm * _OutlineWidth;
                float4 pos = IN.positionOS + float4(offset, 0);

                OUT.positionHCS = TransformObjectToHClip(pos);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                return (_EnableOutline > 0.5) ? _OutlineColor : float4(0,0,0,0);
            }
            ENDHLSL
        }

        // ShadowCaster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowPassVertex(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(positionWS); // Niente bias custom
                return OUT;
            }

            float4 ShadowPassFragment(Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }


    }
}

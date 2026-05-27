Shader "Custom/URP_LiquidFill_FrontOpaque_Bright"
{
    Properties
    {
        [Header(Liquid)]
        _FillAmount     ("Fill Amount (Local Y)", Range(-1, 1)) = 0.0
        _LiquidColor    ("Liquid Color", Color) = (0, 0.6, 1, 1)
        _Alpha          ("Alpha", Range(0, 1)) = 1.0
        _AmbientBoost   ("Ambient Light Boost", Range(0, 1)) = 0.4 // Controla qué tan brillante es en la sombra
        
        [Header(Waves)]
        _WaveStrength   ("Wave Strength", Range(0, 0.15)) = 0.03
        _WaveSpeed      ("Wave Speed", Range(0, 10)) = 4.0
        _WaveScale      ("Wave Scale", Range(1, 50)) = 12.0
        
        [Header(Surface Edge)]
        _SurfaceColor   ("Surface Edge Color", Color) = (1, 1, 1, 1)
        _SurfaceThickness ("Surface Thickness", Range(0.001, 0.1)) = 0.02
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "LiquidFront"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On       // Bloquea los objetos de atrás
            Cull Back       // Solo render frontal, sin efecto billboard

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionOS  : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _FillAmount;
            float4 _LiquidColor;
            float _Alpha;
            float _AmbientBoost;
            float _WaveStrength;
            float _WaveSpeed;
            float _WaveScale;
            float4 _SurfaceColor;
            float _SurfaceThickness;

            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionOS = input.positionOS.xyz;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Ondas locales estables
                float time = _Time.y * _WaveSpeed;
                float wave = sin(i.positionOS.x * _WaveScale + time) * cos(i.positionOS.z * _WaveScale * 0.8 + time) * _WaveStrength;
                
                float currentFillLevel = _FillAmount + wave;

                // Corte del líquido
                clip(currentFillLevel - i.positionOS.y);

                // Iluminación
                Light mainLight = GetMainLight();
                half3 normalWS = normalize(i.normalWS);
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                
                // MEJORA: Escalamos la luz difusa para que nunca caiga a negro absoluto usando el _AmbientBoost
                half3 diffuse = mainLight.color * lerp(_AmbientBoost, 1.0, NdotL);

                // Efecto Fresnel para darle volumen translúcido en los bordes
                half3 viewDir = normalize(_WorldSpaceCameraPos - i.positionWS);
                half fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), 3.0);

                // Color base del líquido iluminado
                half3 color = _LiquidColor.rgb * diffuse;
                
                // Añadimos un extra de emisión basado en el propio color del líquido para que "brille" por sí mismo
                color += _LiquidColor.rgb * _AmbientBoost * 0.5;
                
                // Añadimos el reflejo del Fresnel
                color += fresnel * 0.2 * _LiquidColor.rgb;

                // Línea de la superficie (borde superior nítido)
                float distanceToSurface = currentFillLevel - i.positionOS.y;
                float edgeMask = smoothstep(_SurfaceThickness, 0.0, distanceToSurface);
                
                // Forzamos a que el borde superior ignore la sombra del escenario para mantener el "look" de poción/líquido
                color = lerp(color, _SurfaceColor.rgb, edgeMask);

                return half4(color, _LiquidColor.a * _Alpha);
            }
            ENDHLSL
        }
    }
}
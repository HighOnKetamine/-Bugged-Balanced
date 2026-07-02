Shader "Custom/FogOfWar"
{
    Properties
    {
        _VisionTex ("Vision Texture", 2D) = "black" {}
        _FogColor  ("Fog Color",       Color) = (0, 0, 0.05, 0.88)
    }
    SubShader
    {
        // Render after all opaque geometry so alpha-blending darkens the scene.
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent+100"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        // Visible from both sides so the camera angle doesn't matter.
        Cull Off

        Pass
        {
            Name "FogOfWar"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 posOS : POSITION;
                float2 uv    : TEXCOORD0;
            };

            struct Varyings
            {
                float4 posCS : SV_POSITION;
                float2 uv    : TEXCOORD0;
            };

            TEXTURE2D(_VisionTex);
            SAMPLER(sampler_VisionTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _VisionTex_ST;
                half4  _FogColor;
            CBUFFER_END

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS = TransformObjectToHClip(IN.posOS.xyz);
                // UVs are pre-mapped to the map bounds by FogOfWarManager.
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                // 1 = fully visible, 0 = fully fogged.
                half vision = SAMPLE_TEXTURE2D(_VisionTex, sampler_VisionTex, IN.uv).r;
                half4 col   = _FogColor;
                col.a      *= 1.0h - vision;
                return col;
            }
            ENDHLSL
        }
    }
}

Shader "UNSLOW/ShaderBasics/UIBackgroundBoxBlur"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #define USE_FULL_PRECISION_BLIT_TEXTURE
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        float4 _SourceTexelSize;

        float4 Copy(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
        }

        float4 BoxBlur(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float4 color = 0;
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    float2 offset = float2(x, y) * _SourceTexelSize.xy;
                    color += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord + offset);
                }
            }
            return color / 9;
        }
        ENDHLSL

        Pass
        {
            Name "Copy"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Copy
            ENDHLSL
        }
        Pass
        {
            Name "Box 3x3"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment BoxBlur
            ENDHLSL
        }
    }
}

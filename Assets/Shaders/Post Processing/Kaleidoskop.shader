Shader "Hidden/Shader/Kaleidoskop"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    // List of properties to control your post process effect
    float _Intensity;
    float _Segments=10;
    TEXTURE2D_X(_InputTexture);

    #define PI2 3.1415926535897932384626433832795*2
    float2 scene(float2 i)
    {
        // Convert to polar coordinates.
        float2 shiftUV = i - 0.5;
        float radius = sqrt(dot(shiftUV, shiftUV));
        float angle = atan2(shiftUV.y, shiftUV.x);
        // Calculate segment angle amount.
        float segmentAngle = PI2 / _Segments;
        // Calculate which segment this angle is in.
        angle -= segmentAngle * floor(angle / segmentAngle);
        // Each segment contains one reflection.
        angle = min(angle, segmentAngle - angle);
        // Convert back to UV coordinates.
        float2 uv = float2(cos(angle), sin(angle)) * radius + 0.5f;
        // Reflect outside the inner circle boundary.
        uv = max(min(uv, 2.0 - uv), -uv);
        return uv;
        //return tex2D(_MainTex, uv).xyz;

	    //return texture(iChannel3, transform(at) * 2.0);
    }

   //void mainImage( out float3 fragColor, in float2 fragCoord )
   //{
	//    float2 uv = fragCoord.xy / _ScreenSize.xy;	
	//    uv.x = lerp(-1.0, 1.0, uv.x);
	//    uv.y = lerp(-1.0, 1.0, uv.y);
	//    uv.y *= _ScreenSize.y / _ScreenSize.x;
	//    fragColor = scene(kaleido(uv));
   //}

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        
	    float2 uv = input.texcoord.xy;	
	    //uv.y *= _ScreenSize.y / _ScreenSize.x;
        float2 nuv=lerp(uv,scene(uv),_Intensity);
        return LOAD_TEXTURE2D_X(_InputTexture, nuv*_ScreenSize);

        //return float4(outColor, 1);
    }
    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Kaleidoskop"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}

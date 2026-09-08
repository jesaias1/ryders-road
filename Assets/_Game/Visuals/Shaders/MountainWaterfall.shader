Shader "RYDERS ROAD/Mountain Waterfall"
{
    Properties
    {
        _BaseColor("Water tint", Color) = (0.17,0.67,0.74,1)
        _FlowSpeed("Flow speed", Float) = 2
        _StreakScale("Streak spacing", Float) = 4
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Back ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _FlowSpeed;
                float _StreakScale;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half fog:TEXCOORD1; half3 normalWS:TEXCOORD2; };
            Varyings Vert(Attributes a)
            {
                Varyings o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.uv=a.uv;o.normalWS=TransformObjectToWorldNormal(a.normalOS);o.fog=ComputeFogFactor(o.positionCS.z);return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float flow=i.uv.y+_Time.y*_FlowSpeed;
                half streak=pow(saturate(.5+.5*sin(flow*_StreakScale+i.uv.x*13)),6);
                half3 color=lerp(_BaseColor.rgb,half3(.8,.95,.96),streak*.24);
                Light sun=GetMainLight();
                half diffuse=.55+.45*abs(dot(normalize(i.normalWS),sun.direction));
                color*=SampleSH(normalize(i.normalWS)) + sun.color*diffuse;
                return half4(MixFog(color,i.fog),1);
            }
            ENDHLSL
        }
    }
}

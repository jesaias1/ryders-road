Shader "RydersRoad/World Sky"
{
    Properties
    {
        _Zenith ("Zenith", Color) = (.06,.24,.5,1)
        _Horizon ("Horizon", Color) = (.65,.8,.88,1)
        _Nadir ("Depth", Color) = (.1,.22,.34,1)
        _Cloud ("Cloud light", Color) = (.85,.88,.9,1)
        _CloudAmount ("Cloud coverage", Range(0,1)) = .45
        _CloudOpacity ("Cloud opacity", Range(0,1)) = .28
        _CloudCeiling ("Cloud ceiling", Range(.2,1)) = .45
        _SunDirection ("Sun direction", Vector) = (0,.3,1,0)
        _SunColor ("Sun glow", Color) = (1,.8,.5,1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 position : SV_POSITION; float3 direction : TEXCOORD0; };
            float4 _Zenith, _Horizon, _Nadir, _Cloud, _SunDirection, _SunColor;
            float _CloudAmount, _CloudOpacity, _CloudCeiling;
            v2f vert(appdata v) { v2f o; o.position=UnityObjectToClipPos(v.vertex); o.direction=v.vertex.xyz; return o; }
            float hash(float3 p) { p=frac(p*.1031); p+=dot(p,p.yzx+33.33); return frac((p.x+p.y)*p.z); }
            float noise(float3 p)
            {
                float3 i=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+1),f.x),f.y),f.z);
            }
            half4 frag(v2f i) : SV_Target
            {
                // Continuous unit direction: no longitude UV wrap, cube-face texture or horizon plane.
                float3 d=normalize(i.direction);
                float3 sky=lerp(_Horizon.rgb,_Zenith.rgb,smoothstep(-.04, .48, d.y));
                sky=lerp(sky,_Nadir.rgb,smoothstep(0,1,-d.y));
                float3 p=d*float3(5,13,5);
                float n=noise(p)*.65+noise(p*2.03+17)*.25+noise(p*4.01+31)*.10;
                float cloud=smoothstep(.62-_CloudAmount*.25,.92-_CloudAmount*.25,n);
                cloud*=smoothstep(-.65,-.08,d.y)*(1-smoothstep(.12,_CloudCeiling,d.y));
                sky=lerp(sky,_Cloud.rgb,cloud*_CloudOpacity);
                float sun=saturate(dot(d,normalize(_SunDirection.xyz)));
                sky+=_SunColor.rgb*(pow(sun,32)*.12+smoothstep(.9993,.9998,sun)*.5);
                return half4(sky,1);
            }
            ENDHLSL
        }
    }
}

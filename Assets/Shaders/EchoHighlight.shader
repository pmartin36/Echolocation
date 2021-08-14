Shader "Unlit/EchoHighlight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _MaxRadius("Max Radius", Range(-0.5,1.5)) = 1.5
        _Shrinking("Shrinking", Range(-1,1)) = -1

        _StencilRef("_StencilRef", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("_StencilComp (default = Disable) _____Set to NotEqual if you want to mask by specific _StencilRef value, else set to Disable", Float) = 0 //0 = disable
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPass("_StencilPass (default = Keep)", Float) = 0 //0 = Keep
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1"}
        ZWrite Off
        ColorMask 0

        Pass
        {
            Name "StencilWrite"
            Tags{"LightMode" = "SRPDefaultUnlit"}
            Cull Back
            Stencil
            {
                Ref[_StencilRef]
                Comp[_StencilComp]
                Pass[_StencilPass]
                ZFail Keep
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/LitShader/DarkFunctions.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2: TEXCOORD1;
                float2 uv3: TEXCOORD2;
                float4 normal: NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2: TEXCOORD1;
                float4 normal: TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MaxRadius;
            float _Shrinking;

            v2f vert(appdata v)
            {
                v2f o;
                float4 vert = v.vertex;
                vert.xy *= 0.8;
                vert.z += abs(v.uv3.x) / length(float3(unity_ObjectToWorld._m02, unity_ObjectToWorld._m12, unity_ObjectToWorld._m22));
                o.vertex = UnityObjectToClipPos(vert);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = v.uv2;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //return float4(i.uv2.xy, 1, 1);
                clip(i.uv2.x + 0.5);
                float2 uvn = i.uv2 * 2 - 1;

                float r = length(uvn);
                clip(1 - r);
                float rMin = _MaxRadius;

                float amp = sin(_Time.y * 2) * 0.25;
                float2 v = float2(cos(_Time.y * 3), sin(_Time.y * 3));

                float d = abs(dot(normalize(uvn), normalize(v)));
                float rf = r - r * amp * smoothstep(0, 1, d);

                clip(rMin - rf);

                float dsd = DefaultDoubleSampleDark(i.uv2*2).r;
                clip(saturate(rMin/3) - dsd * dsd * _Shrinking);
                return 1;
            }
            ENDHLSL
        }
    }
}

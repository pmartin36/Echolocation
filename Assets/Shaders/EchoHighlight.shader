Shader "Unlit/EchoHighlight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _MaxRadius("Max Radius", Range(0,1.25)) = 1.25

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
                ZFail[_StencilPass]
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/LitShader/DarkFunctions.hlsl"

            float _MaxRadius;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2: TEXCOORD1;
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = v.uv2;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
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
                return 1;
            }
            ENDHLSL
        }
    }
}

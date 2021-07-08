Shader "Unlit/AxisMarker"
{
    Properties
    {
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
        _RimPower("Power", Range(0.0,10.0)) = 1.0
        _RimIntensity("Intensity", Range(0.0,10.0)) = 1.0
        _Color("Rim Color", Color) = (1,1,1,1)
    }
        SubShader
        {
            Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

            ZWrite On
            Blend SrcAlpha Zero

            Pass
            {
                CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma target 2.0
                    #pragma multi_compile_fog

                    #include "UnityCG.cginc"
                    struct appdata_t {
                        float4 vertex : POSITION;
                        float2 texcoord : TEXCOORD0;
                        float3 normal : NORMAL;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                    };
                    struct v2f {
                        float4 vertex : SV_POSITION;
                        float2 texcoord : TEXCOORD0;
                        float3 normal : TEXCOORD1;
                        float3 positionOS : TEXCOORD2;
                    };

                    sampler2D _MainTex;
                    float4 _MainTex_ST;
                    float3 viewDir;
                    fixed4 _Color;
                    half _RimPower;
                    half _RimIntensity;

                    v2f vert(appdata_t v)
                    {
                        v2f o;
                        o.positionOS = normalize(v.vertex.xyz);
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                        o.normal = UnityObjectToWorldNormal(v.normal);
                        return o;
                    }

                    fixed4 frag(v2f i) : SV_Target
                    {
                        half rim = 1 - saturate(dot(float4(0,0,-1,0), i.normal));
                        fixed4 color = float4(i.positionOS.xyz, 1);
                        fixed4 col =  color * pow(rim, _RimPower) * _RimIntensity;
                        col.a *= tex2D(_MainTex, i.texcoord).a;
                        return col;
                    }
                ENDCG
            }
        }
}
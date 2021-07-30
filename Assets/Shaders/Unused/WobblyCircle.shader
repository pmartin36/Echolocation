Shader "Unlit/WobblyCircle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            static const float pi = 3.14159265;

            float3 HUEtoRGB(in float H)
            {
                float R = abs(H * 6 - 3) - 1;
                float G = 2 - abs(H * 6 - 2);
                float B = 2 - abs(H * 6 - 4);
                return saturate(float3(R, G, B));
            }

            float3 HSVtoRGB(in float3 HSV)
            {
                float3 RGB = HUEtoRGB(HSV.x);
                return ((RGB - 1) * HSV.y + 1) * HSV.z;
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = (i.uv - 0.5) * 2;
                float r = length(uv);
                float rMin = (1 - abs(sin(_Time.x*3))) * 0.85;
                
                float amp = sin(_Time.y * 3) * 0.15;
                //float amp = sin(_Time.y * 3) * 0.3;
                float2 v = float2(cos(_Time.y*2), sin(_Time.y*2));

                float d = abs(dot(normalize(uv), normalize(v)));
                float rf = r - r * amp * smoothstep(0, 1, d);

                fixed f = smoothstep(rMin + 0.01, rMin - 0.01, rf);
                fixed4 col = float4(f.xxx, 1);

                //fixed3 f = saturate(smoothstep(rMin + 0.01, rMin - 0.01, rf) * HSVtoRGB(float3(frac(d*2), 1, 1)) + smoothstep(rMin + 0.01, rMin - 0.01, r));
                //fixed4 col = float4(f, 1);
                return col;
            }
            ENDCG
        }
    }
}

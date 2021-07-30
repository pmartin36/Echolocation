//Shader "Unlit/EchoHighlight"
//{
//    Properties
//    {
//        _MainTex("Texture", 2D) = "white" {}
//
//        _StencilRef("_StencilRef", Float) = 0
//        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("_StencilComp (default = Disable) _____Set to NotEqual if you want to mask by specific _StencilRef value, else set to Disable", Float) = 0 //0 = disable
//        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPass("_StencilPass (default = Keep)", Float) = 0 //0 = Keep
//
//        _ProjectionAngleDiscardThreshold("_ProjectionAngleDiscardThreshold (default = 0)", range(-1,1)) = 0
//    }
//    SubShader
//    {
//        Tags { "RenderType" = "Overlay" "Queue" = "Transparent-499" "DisableBatching" = "True" }
//        ZTest Off
//        ZWrite Off
//        ColorMask 0
//
//        Stencil
//        {
//            Ref[_StencilRef]
//            Comp[_StencilComp]
//            Pass[_StencilPass]
//        }
//
//        Pass
//        {
//            HLSLPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #pragma target 3.0
//
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//
//            struct appdata
//            {
//                float3 positionOS : POSITION;
//            };
//
//            struct v2f
//            {
//                float4 positionCS : SV_POSITION;
//                float4 screenPos : TEXCOORD0;
//                float4 viewRayOS : TEXCOORD1; // xyz: viewRayOS, w: extra copy of positionVS.z 
//                float4 cameraPosOSAndFogFactor : TEXCOORD2;
//            };
//
//            sampler2D _MainTex;
//            float4 _MainTex_ST;
//
//            sampler2D _CameraDepthTexture;
//            float _ProjectionAngleDiscardThreshold;
//
//            v2f vert(appdata v)
//            {
//                v2f o;
//                VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(v.positionOS);
//                o.positionCS = vertexPositionInput.positionCS;
//                o.screenPos = ComputeScreenPos(o.positionCS);
//
//                float3 viewRay = vertexPositionInput.positionVS;
//                o.viewRayOS.w = viewRay.z;
//                viewRay *= -1;
//                float4x4 ViewToObjectMatrix = mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V);
//                o.viewRayOS.xyz = mul((float3x3)ViewToObjectMatrix, viewRay);
//                o.cameraPosOSAndFogFactor.xyz = mul(ViewToObjectMatrix, float4(0, 0, 0, 1)).xyz;
//                    
//                return o;
//            }
//
//            half4 frag(v2f i) : SV_Target
//            {
//                i.viewRayOS.xyz /= i.viewRayOS.w;
//
//                float2 screenSpaceUV = i.screenPos.xy / i.screenPos.w;
//                float sceneRawDepth = tex2D(_CameraDepthTexture, screenSpaceUV).r;
//
//                float sceneDepthVS = LinearEyeDepth(sceneRawDepth, _ZBufferParams);
//                float3 decalSpaceScenePos = i.cameraPosOSAndFogFactor.xyz + i.viewRayOS.xyz * sceneDepthVS;
//
//                float3 decalSpaceHardNormal = normalize(cross(ddx(decalSpaceScenePos), ddy(decalSpaceScenePos)));//reconstruct scene hard normal using scene pos ddx&ddy
//
//                // compare scene hard normal with decal projector's dir, decalSpaceHardNormal.z equals dot(decalForwardDir,sceneHardNormalDir)
//                float shouldClip = ceil(_ProjectionAngleDiscardThreshold - decalSpaceHardNormal.z);
//
//                clip(0.5 - abs(decalSpaceScenePos) - shouldClip);
//                return 1;
//            }
//            ENDHLSL
//        }
//    }
//}

Shader "Unlit/EchoHighlight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

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

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2: TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return 1;
            }
            ENDCG
        }
    }
}

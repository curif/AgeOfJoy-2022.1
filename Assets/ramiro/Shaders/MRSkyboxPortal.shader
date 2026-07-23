/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

// Built-in RP: draws an equirectangular skybox only where ramiro/SkyboxPortalMask wrote stencil.
// Opaque (alpha 1) so Quest underlay passthrough does not show through the sky.
// Put this on a large inverted sphere around the camera (Cull Front), or draw via CommandBuffer.
Shader "ramiro/SkyboxPortal"
{
    Properties
    {
        _MainTex ("Equirect Sky (HDRI)", 2D) = "gray" {}
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Exposure ("Exposure", Range(0, 8)) = 1
        _Rotation ("Rotation", Range(0, 360)) = 0
        [IntRange] _StencilRef ("Stencil Ref", Range(0, 255)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry+11"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "SkyboxPortal"
            Cull Front
            ZWrite On
            ZTest LEqual
            // Fully opaque — required for underlay passthrough compositing.
            Blend One Zero

            Stencil
            {
                Ref [_StencilRef]
                Comp Equal
                Pass Keep
                Fail Keep
                ZFail Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Tint;
            half _Exposure;
            float _Rotation;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            float3 RotateAroundY(float3 v, float degrees)
            {
                float rad = degrees * UNITY_PI / 180.0;
                float s, c;
                sincos(rad, s, c);
                return float3(c * v.x + s * v.z, v.y, -s * v.x + c * v.z);
            }

            float2 DirectionToEquirectUv(float3 dir)
            {
                dir = normalize(dir);
                float2 uv;
                uv.x = atan2(dir.x, dir.z) * (0.5 / UNITY_PI) + 0.5;
                uv.y = asin(clamp(dir.y, -1.0, 1.0)) * (1.0 / UNITY_PI) + 0.5;
                return uv;
            }

            v2f vert(appdata v)
            {
                v2f o;
                // Sky at far plane (same trick as Unity skybox).
                float4 clipPos = UnityObjectToClipPos(v.vertex);
#if defined(UNITY_REVERSED_Z)
                clipPos.z = 0.000001 * clipPos.w;
#else
                clipPos.z = clipPos.w;
#endif
                o.pos = clipPos;
                // Direction from camera through the cube vertex (cube is centered on camera).
                o.viewDir = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 dir = RotateAroundY(normalize(i.viewDir), _Rotation);
                float2 uv = TRANSFORM_TEX(DirectionToEquirectUv(dir), _MainTex);
                // Force mip 0 — sky at infinity otherwise picks blurry mips (looks very soft).
                half3 col = tex2Dlod(_MainTex, float4(uv, 0, 0)).rgb;
                col = col * _Tint.rgb * unity_ColorSpaceDouble.rgb * _Exposure;
                // Alpha 1 — solid sky over passthrough underlay.
                return half4(col, 1);
            }
            ENDCG
        }
    }

    Fallback Off
}

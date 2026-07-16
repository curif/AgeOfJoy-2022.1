/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

// Built-in RP: invisible depth occluder + point/spot light tint + shadow receive (no cast).
// Directional lights only cast/receive shadows here — tinting a whole flat face looks painted.
// Default intensities 0 = fully invisible except depth occlusion.
Shader "AgeOfJoy/MR/OccluderAndShadow"
{
    Properties
    {
        _LightIntensity ("Light Intensity", Range(0, 1)) = 0
        _LightFalloff ("Light Falloff", Range(1, 8)) = 2.5
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 1)
        _TintEnabled ("Tint Enabled", Float) = 0
        _TintColor ("Tint Color", Color) = (0.25, 0.75, 0.95, 0.28)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Geometry-1"
        }

        Pass
        {
            Name "DepthOccluder"
            ColorMask 0
            ZWrite On
            ZTest LEqual
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }

        Pass
        {
            Name "ColorTint"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _TintEnabled;
            fixed4 _TintColor;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_TintEnabled < 0.5)
                    return 0;

                return _TintColor;
            }
            ENDCG
        }

        Pass
        {
            // Directional: shadows only (no light tint — flat faces would look solid-painted).
            Name "DirectionalShadowReceive"
            Tags { "LightMode" = "ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma skip_variants LIGHTMAP_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            float _ShadowIntensity;
            fixed4 _ShadowColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                SHADOW_COORDS(2)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_ShadowIntensity <= 0.001)
                    return 0;

                if (_WorldSpaceLightPos0.w > 0.0)
                    return 0;

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ndotl = saturate(dot(normalize(i.worldNormal), lightDir));
                if (ndotl <= 0.0)
                    return 0;

                fixed shadow = UNITY_SHADOW_ATTENUATION(i, i.worldPos);
                float alpha = (1.0 - shadow) * _ShadowIntensity * ndotl;
                alpha *= _ShadowColor.a;
                return fixed4(_ShadowColor.rgb, alpha);
            }
            ENDCG
        }

        Pass
        {
            // Point/Spot: light-colour hotspot with distance falloff + optional shadow darkening.
            Name "PointLightAndShadow"
            Tags { "LightMode" = "ForwardAdd" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd_fullshadows
            #pragma skip_variants DIRECTIONAL DIRECTIONAL_COOKIE
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            float _LightIntensity;
            float _LightFalloff;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                LIGHTING_COORDS(2, 3)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_LightIntensity <= 0.001)
                    return 0;

#if defined(DIRECTIONAL) || defined(DIRECTIONAL_COOKIE)
                return 0;
#else
                float3 toLight = _WorldSpaceLightPos0.xyz - i.worldPos;
                float dist = length(toLight);
                float3 lightDir = toLight / max(dist, 0.0001);
                float ndotl = saturate(dot(normalize(i.worldNormal), lightDir));
                if (ndotl <= 0.0)
                    return 0;

                half distAtten = 1.0h;
#if defined(POINT)
                unityShadowCoord3 worldLightCoord = mul(unity_WorldToLight, unityShadowCoord4(i.worldPos, 1)).xyz;
                distAtten = tex2D(_LightTexture0, dot(worldLightCoord, worldLightCoord).rr).r;
#elif defined(POINT_COOKIE)
                unityShadowCoord3 worldLightCoord = mul(unity_WorldToLight, unityShadowCoord4(i.worldPos, 1)).xyz;
                distAtten = tex2D(_LightTextureB0, dot(worldLightCoord, worldLightCoord).rr).r
                    * texCUBE(_LightTexture0, worldLightCoord).w;
#elif defined(SPOT) || defined(SPOT_COOKIE)
                unityShadowCoord4 worldLightCoord = mul(unity_WorldToLight, unityShadowCoord4(i.worldPos, 1));
                if (worldLightCoord.z <= 0.0)
                    return 0;
                distAtten = tex2D(_LightTextureB0, dot(worldLightCoord.xyz, worldLightCoord.xyz).xx).r;
#if defined(SPOT_COOKIE)
                distAtten *= tex2D(_LightTexture0, worldLightCoord.xy / worldLightCoord.w + 0.5).w;
#else
                // Spot cookie uses the same projective UVs in the non-cookie variant on Built-in.
                distAtten *= tex2D(_LightTexture0, worldLightCoord.xy / worldLightCoord.w + 0.5).w;
#endif
#endif

                if (distAtten <= 0.001h)
                    return 0;

                // Concentrate the hotspot: range falloff + soft angle (not a flat Lambert wash).
                float falloff = max(_LightFalloff, 1.0);
                float radial = pow(saturate((float)distAtten), falloff);
                float angled = pow(ndotl, falloff);
                float hotspot = radial * angled;

                // Occlusion only removes coloured light (object silhouette), never paints
                // an extra black disc — (1-shadow)*hotspot looked like a "spot-shaped shadow".
                half shadow = saturate((half)UNITY_SHADOW_ATTENUATION(i, i.worldPos));
                float litAmt = _LightIntensity * hotspot * shadow;
                if (litAmt <= 0.001)
                    return 0;

                return fixed4(_LightColor0.rgb * litAmt, litAmt);
#endif
            }
            ENDCG
        }
    }

    Fallback Off
}

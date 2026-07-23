/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

// Built-in RP: marks a stencil mask on the window opening mesh (no colour).
// Pair with ramiro/SkyboxPortal — sky draws only where stencil matches.
// Apply to the glass/opening mesh only (not the frame).
Shader "ramiro/SkyboxPortalMask"
{
    Properties
    {
        [IntRange] _StencilRef ("Stencil Ref", Range(0, 255)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry+10"
            "IgnoreProjector" = "True"
        }

        // Pass 1 — write stencil on the window opening (invisible).
        Pass
        {
            Name "WriteStencil"
            ColorMask 0
            ZWrite Off
            ZTest LEqual
            Cull Off

            Stencil
            {
                Ref [_StencilRef]
                Comp Always
                Pass Replace
                Fail Keep
                ZFail Keep
            }

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

        // Pass 2 — clear depth only where stencil was marked so the skybox can draw “behind”.
        Pass
        {
            Name "ClearDepth"
            ColorMask 0
            ZWrite On
            ZTest Always
            Cull Off

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
                // Push depth to the far plane so a subsequent skybox pass can win ZTest.
#if defined(UNITY_REVERSED_Z)
                o.pos.z = 0.000001 * o.pos.w;
#else
                o.pos.z = o.pos.w;
#endif
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }
    }

    Fallback Off
}

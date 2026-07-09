/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

// Duas texturas + Simple Deform (Blender) no vertex.
// Page mesh: plano XZ, curl em Y. Pivot no lombo (_PivotX = mesh.bounds.min.x).
Shader "ramiro/PageConeBend"
{
    Properties
    {
        _PageFront ("Page Front", 2D) = "white" {}
        _PageBack ("Page Back", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Toggle] _FlipFrontU ("Flip Front U", Float) = 1
        [Toggle] _FlipFrontV ("Flip Front V", Float) = 1
        [Toggle] _FlipBackU ("Flip Back U", Float) = 0
        [Toggle] _FlipBackV ("Flip Back V", Float) = 1

        [Header(Simple Deform)]
        _BendAmount ("Bend Amount", Float) = 0
        _BendScale ("Bend Scale", Float) = 1
        _PivotX ("Pivot X", Float) = -20.767
        _PageWidth ("Page Width", Float) = 20.769
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _PageFront;
            sampler2D _PageBack;
            float4 _PageFront_ST;
            fixed4 _Color;

            float _FlipFrontU;
            float _FlipFrontV;
            float _FlipBackU;
            float _FlipBackV;

            float _BendAmount;
            float _BendScale;
            float _PivotX;
            float _PageWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            float2 FlipUV(float2 uv, float flipU, float flipV)
            {
                if (flipU > 0.5)
                    uv.x = 1.0 - uv.x;
                if (flipV > 0.5)
                    uv.y = 1.0 - uv.y;
                return uv;
            }

            float3 BendPositionOS(float3 positionOS)
            {
                float localX = positionOS.x - _PivotX;

                // Lado do pivot (lombo) fica fixo.
                if (localX <= 0.0001)
                    return positionOS;

                float width = max(_PageWidth, 0.0001);
                float bend = _BendAmount * _BendScale;
                if (abs(bend) <= 0.0001)
                    return positionOS;

                // Roda em torno do eixo Z (lombo): curl sobe em Y, não só desliza em X.
                float angle = (localX / width) * bend;
                float s = sin(angle);
                float c = cos(angle);

                positionOS.x = _PivotX + localX * c;
                positionOS.y += localX * s * sign(bend);
                return positionOS;
            }

            float3 BendNormalOS(float3 positionOS, float3 normalOS)
            {
                float localX = positionOS.x - _PivotX;
                if (localX <= 0.0001)
                    return normalOS;

                float width = max(_PageWidth, 0.0001);
                float bend = _BendAmount * _BendScale;
                if (abs(bend) <= 0.0001)
                    return normalOS;

                float angle = (localX / width) * bend;
                float s = sin(angle);
                float c = cos(angle);

                return float3(
                    normalOS.x * c - normalOS.y * s,
                    normalOS.x * s + normalOS.y * c,
                    normalOS.z);
            }

            v2f vert(appdata v)
            {
                v2f o;

                float3 bentPos = BendPositionOS(v.vertex.xyz);
                float3 bentNormal = BendNormalOS(v.vertex.xyz, v.normal);

                o.vertex = UnityObjectToClipPos(float4(bentPos, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _PageFront);
                o.worldPos = mul(unity_ObjectToWorld, float4(bentPos, 1.0)).xyz;
                o.worldNormal = UnityObjectToWorldNormal(bentNormal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float facing = dot(normalize(i.worldNormal), viewDir);

                if (facing > 0.0)
                {
                    float2 uv = FlipUV(i.uv, _FlipFrontU, _FlipFrontV);
                    return tex2D(_PageFront, uv) * _Color;
                }

                float2 uvBack = FlipUV(i.uv, _FlipBackU, _FlipBackV);
                return tex2D(_PageBack, uvBack) * _Color;
            }

            ENDCG
        }
    }
}

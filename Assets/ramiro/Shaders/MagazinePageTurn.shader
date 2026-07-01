/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

Shader "ramiro/MagazinePageTurn"
{
    Properties
    {
        _PageFront ("Page Front", 2D) = "white" {}
        _PageBack ("Page Back", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Curl geometry  object space)]
        // _FlipProgress drives the bow amount (0 = flat). The big page rotation is
        // done by the transform in Magazine.cs; this shader only adds the curve.
        _FlipProgress ("Flip Progress", Range(0, 1)) = 0
        _CurlPower ("Curl Power", Range(0.5, 3)) = 1.35
        _CurlAngle ("Max Curl Angle (deg)", Range(0, 160)) = 70
        _SpinePoint ("Spine Point", Vector) = (0, 0, 0, 0)
        _PageDir ("Page Direction (spine -> edge)", Vector) = (1, 0, 0, 0)
        _CurlAxis ("Curl Axis (rotation axis)", Vector) = (0, 0, 1, 0)
        _PageWidth ("Page Width (along Page Direction)", Float) = 1
        [Toggle] _InvertBend ("Invert Bend", Float) = 0
        [Toggle] _CurlFromBottom ("Curl From Bottom Edge", Float) = 1
        _PageHeight ("Page Height", Float) = 1
        _PageHeightMin ("Page Height Min (object space)", Float) = 0
        _PageHeightAxis ("Page Height Axis", Vector) = (0, 1, 0, 0)

        [Header(UV)]
        _FlipFrontU ("Flip Front U", Float) = 0
        _FlipBackU ("Flip Back U", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
        LOD 100
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
            float _FlipProgress;
            float _CurlPower;
            float _CurlAngle;
            float4 _SpinePoint;
            float4 _PageDir;
            float4 _CurlAxis;
            float _PageWidth;
            float _InvertBend;
            float _CurlFromBottom;
            float _PageHeight;
            float _PageHeightMin;
            float4 _PageHeightAxis;
            float _FlipFrontU;
            float _FlipBackU;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            float3 SafeNormalize(float3 v, float3 fallback)
            {
                float lenSq = dot(v, v);
                return lenSq > 1e-6 ? v * rsqrt(lenSq) : fallback;
            }

            // Rodrigues rotation of v around unit axis by angle.
            float3 RotateAroundAxis(float3 v, float3 axis, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return v * c + cross(axis, v) * s + axis * dot(axis, v) * (1.0 - c);
            }

            void BendPageVertex(float3 localPos, float3 localNormal, out float3 bentPos, out float3 bentNormal)
            {
                bentPos = localPos;
                bentNormal = localNormal;

                if (_FlipProgress <= 0.00001)
                    return;

                float3 spine = _SpinePoint.xyz;
                float3 pageDir = SafeNormalize(_PageDir.xyz, float3(1.0, 0.0, 0.0));
                float3 curlAxis = SafeNormalize(_CurlAxis.xyz, float3(0.0, 0.0, 1.0));

                // Normalized distance from the spine along the page (0..1).
                float3 rel = localPos - spine;
                float dist = dot(rel, pageDir);
                float t = saturate(dist / max(_PageWidth, 0.0001));

                if (_CurlFromBottom > 0.5)
                {
                    float3 heightAxis = SafeNormalize(_PageHeightAxis.xyz, float3(0.0, 1.0, 0.0));
                    float h = dot(localPos - spine, heightAxis);
                    float h01 = saturate((h - _PageHeightMin) / max(_PageHeight, 0.0001));
                    t *= 1.0 - h01;
                }

                float bendSign = _InvertBend > 0.5 ? -1.0 : 1.0;
                float maxAngle = radians(_CurlAngle);
                float angle = bendSign * _FlipProgress * maxAngle * pow(t, _CurlPower);

                bentPos = spine + RotateAroundAxis(rel, curlAxis, angle);
                bentNormal = RotateAroundAxis(localNormal, curlAxis, angle);
            }

            v2f vert(appdata v)
            {
                v2f o;
                float3 bentPos;
                float3 bentNormal;
                BendPageVertex(v.vertex.xyz, v.normal, bentPos, bentNormal);

                o.worldPos = mul(unity_ObjectToWorld, float4(bentPos, 1.0)).xyz;
                o.vertex = UnityObjectToClipPos(float4(bentPos, 1.0));
                o.worldNormal = UnityObjectToWorldNormal(bentNormal);
                o.uv = TRANSFORM_TEX(v.uv, _PageFront);
                return o;
            }

            float2 FlipU(float2 uv, float flip)
            {
                return float2(lerp(uv.x, 1.0 - uv.x, flip), uv.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                bool frontFace = dot(normalize(i.worldNormal), viewDir) > 0.0;
                fixed4 col;
                if (frontFace)
                    col = tex2D(_PageFront, FlipU(i.uv, _FlipFrontU)) * _Color;
                else
                    col = tex2D(_PageBack, FlipU(i.uv, _FlipBackU)) * _Color;
                return col;
            }
            ENDCG
        }
    }
}

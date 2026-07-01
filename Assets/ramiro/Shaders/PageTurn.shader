/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

// Corner-grab page turn. The bound edge (Spine Edge) stays fixed; the opposite free
// edge lifts out of the page plane and rolls back over the spine, like a hand peeling
// a page. Corner Bias makes one corner lead so it reads as a diagonal grab.
//
// Test on a Unity Quad (1x1, UV 0..1):
//   - Spine Edge = Top to reproduce a top-bound pad (hand lifts from the top).
//   - Spine Edge = Left for a left-bound magazine page.
//   - Drag Flip Progress 0 -> 1. Use Invert Bend if it lifts the wrong way.
Shader "ramiro/PageTurn"
{
    Properties
    {
        _PageFront ("Page Front", 2D) = "white" {}
        _PageBack ("Page Back", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Curl)]
        _FlipProgress ("Flip Progress", Range(0, 1)) = 0
        _CurlAngle ("Max Curl Angle (deg)", Range(0, 300)) = 50
        _CurlPower ("Corner Falloff", Range(0.5, 4)) = 4
        _CornerBias ("Corner Bias (0 whole page  1 corner)", Range(0, 1)) = 0.35
        _TipLift ("Tip Lift", Range(0, 0.5)) = 0.05
        [Enum(Left,0,Right,1,Bottom,2,Top,3)] _SpineEdge ("Spine Edge", Float) = 0
        [Toggle] _GrabFarCorner ("Grab Far Corner (flips leading corner)", Float) = 0
        [Toggle] _InvertBend ("Invert Bend (which way it lifts)", Float) = 0

        [Header(Page Frame)]
        [Toggle] _AutoFrame ("Auto Frame (use mesh tangents)", Float) = 1
        [Toggle] _CurlFromUv ("Curl From UV", Float) = 1
        _PageWidth ("Page Width (X)", Float) = 5
        _PageHeight ("Page Height (Y)", Float) = 1
        [Header(Manual Frame  only if Auto Frame is off)]
        _PageDir ("Page X Axis (object space)", Vector) = (1, 0, 0, 0)
        _PageHeightAxis ("Page Y Axis (object space)", Vector) = (0, 1, 0, 0)
        _SpinePoint ("Spine Point (object space)", Vector) = (0, 0, 0, 0)

        [Header(UV Flip)]
        _FlipFrontU ("Flip Front U", Float) = 0
        _FlipBackU ("Flip Back U", Float) = 0

        [HideInInspector] _GrabTopCorner ("legacy", Float) = 0
        [HideInInspector] _SpineAtUvX0 ("legacy", Float) = 1
        [HideInInspector] _CurlAxis ("legacy", Vector) = (0, 0, 1, 0)
        [HideInInspector] _PageHeightMin ("legacy", Float) = 0
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
            float _CurlAngle;
            float _CurlPower;
            float _CornerBias;
            float _TipLift;
            float _SpineEdge;
            float _GrabFarCorner;
            float _InvertBend;
            float _AutoFrame;
            float _CurlFromUv;
            float4 _PageDir;
            float4 _PageHeightAxis;
            float _PageWidth;
            float _PageHeight;
            float4 _SpinePoint;
            float _FlipFrontU;
            float _FlipBackU;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
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

            float3 RotateAroundAxis(float3 v, float3 axis, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return v * c + cross(axis, v) * s + axis * dot(axis, v) * (1.0 - c);
            }

            // Resolve the page frame for this vertex.
            //  along : dir from spine toward the free edge (unit)
            //  spineAxis : dir along the spine / hinge (unit)
            //  p : normalized 0 (spine) .. 1 (free edge)
            //  q : normalized coordinate along the spine (for the leading corner)
            //  width : object-space page length from spine to free edge
            void ResolveFrame(float3 localPos, float2 uv, float3 nrm, float4 tan,
                out float3 along, out float3 spineAxis, out float p, out float q, out float width)
            {
                float3 dirX, dirY;
                if (_AutoFrame > 0.5)
                {
                    // Build the page frame straight from the mesh: tangent = +uv.x direction,
                    // bitangent = +uv.y direction, normal = out of page. No manual axes needed.
                    float3 n = SafeNormalize(nrm, float3(0.0, 0.0, 1.0));
                    dirX = SafeNormalize(tan.xyz, float3(1.0, 0.0, 0.0));
                    dirY = SafeNormalize(cross(n, dirX) * tan.w, float3(0.0, 1.0, 0.0));
                }
                else
                {
                    dirX = SafeNormalize(_PageDir.xyz, float3(1.0, 0.0, 0.0));
                    dirY = SafeNormalize(_PageHeightAxis.xyz, float3(0.0, 1.0, 0.0));
                    dirY = SafeNormalize(dirY - dirX * dot(dirX, dirY), float3(0.0, 1.0, 0.0));
                }

                if (_CurlFromUv > 0.5)
                {
                    // Pick along/spine axes and coordinates from the chosen spine edge.
                    if (_SpineEdge < 0.5)        // Left  (spine at u=0)
                    {
                        along = dirX;  spineAxis = dirY;  p = uv.x;        q = uv.y;  width = _PageWidth;
                    }
                    else if (_SpineEdge < 1.5)   // Right (spine at u=1)
                    {
                        along = -dirX; spineAxis = dirY;  p = 1.0 - uv.x;  q = uv.y;  width = _PageWidth;
                    }
                    else if (_SpineEdge < 2.5)   // Bottom (spine at v=0)
                    {
                        along = dirY;  spineAxis = dirX;  p = uv.y;        q = uv.x;  width = _PageHeight;
                    }
                    else                          // Top (spine at v=1)
                    {
                        along = -dirY; spineAxis = dirX;  p = 1.0 - uv.y;  q = uv.x;  width = _PageHeight;
                    }
                }
                else
                {
                    float3 rel = localPos - _SpinePoint.xyz;
                    along = dirX;  spineAxis = dirY;
                    width = max(_PageWidth, 0.0001);
                    p = dot(rel, along) / width;
                    q = dot(rel, spineAxis) / max(_PageHeight, 0.0001);
                }

                p = saturate(p);
                q = saturate(q);
                width = max(width, 0.0001);
            }

            void BendPageVertex(float3 localPos, float3 localNormal, float4 tangent, float2 uv,
                out float3 bentPos, out float3 bentNormal)
            {
                bentPos = localPos;
                bentNormal = localNormal;

                if (_FlipProgress <= 0.00001)
                    return;

                float3 along, spineAxis;
                float p, q, width;
                ResolveFrame(localPos, uv, localNormal, tangent, along, spineAxis, p, q, width);

                float3 outOfPlane = cross(along, spineAxis); // lift direction

                // Leading corner weight along the spine (diagonal grab). Corner Bias 0 = the
                // whole page rolls evenly; 1 = only the leading column rolls.
                float cornerW = _GrabFarCorner > 0.5 ? q : (1.0 - q);
                cornerW = pow(saturate(cornerW), _CurlPower);
                float weight = lerp(1.0, cornerW, saturate(_CornerBias));

                float bendSign = _InvertBend > 0.5 ? -1.0 : 1.0;

                // Cylindrical roll: the whole strip from spine (p=0) to free edge bends into a
                // circular arc. Curvature is constant per column, so the entire page folds
                // smoothly instead of only the tip. Spine stays fixed.
                float totalAngle = bendSign * _FlipProgress * radians(_CurlAngle) * weight;
                float arcLen = p * width;                 // distance from spine along the page
                float3 pivot = localPos - along * arcLen; // point on the spine at this column

                float k = totalAngle / width;             // curvature (rad per object unit)
                float bendAngle = k * arcLen;
                float newAlong, newLift;
                if (abs(k) < 1e-4)
                {
                    newAlong = arcLen;
                    newLift = 0.0;
                }
                else
                {
                    newAlong = sin(bendAngle) / k;
                    newLift = (1.0 - cos(bendAngle)) / k;
                }

                bentPos = pivot + along * newAlong + outOfPlane * newLift;
                bentNormal = RotateAroundAxis(localNormal, spineAxis, bendAngle);

                if (_TipLift > 0.00001)
                    bentPos += outOfPlane * (bendSign * _FlipProgress * _TipLift * weight * p);
            }

            v2f vert(appdata v)
            {
                v2f o;
                float3 bentPos;
                float3 bentNormal;
                BendPageVertex(v.vertex.xyz, v.normal, v.tangent, v.uv, bentPos, bentNormal);

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

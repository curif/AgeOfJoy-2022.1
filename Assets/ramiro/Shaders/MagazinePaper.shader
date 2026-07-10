/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

// Magazine interior leaf: front/back textures, sem deformação no vertex.
Shader "ramiro/MagazinePaper"
{
    Properties
    {
        _FrontTex ("Front Texture", 2D) = "white" {}
        _BackTex ("Back Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Front Flip)]
        [Toggle] _FlipFrontX ("Flip Front X", Float) = 1
        [Toggle] _FlipFrontY ("Flip Front Y", Float) = 1

        [Header(Back Flip)]
        [Toggle] _FlipBackX ("Flip Back X", Float) = 0
        [Toggle] _FlipBackY ("Flip Back Y", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _FrontTex;
            sampler2D _BackTex;
            float4 _FrontTex_ST;
            fixed4 _Color;

            float _FlipFrontX;
            float _FlipFrontY;
            float _FlipBackX;
            float _FlipBackY;

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

            float2 ApplyFlip(float2 uv, float flipX, float flipY)
            {
                if (flipX > 0.5)
                    uv.x = 1.0 - uv.x;
                if (flipY > 0.5)
                    uv.y = 1.0 - uv.y;
                return uv;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _FrontTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float facing = dot(normalize(i.worldNormal), viewDir);

                if (facing > 0)
                {
                    float2 uv = ApplyFlip(i.uv, _FlipFrontX, _FlipFrontY);
                    return tex2D(_FrontTex, uv) * _Color;
                }

                float2 uv = ApplyFlip(i.uv, _FlipBackX, _FlipBackY);
                return tex2D(_BackTex, uv) * _Color;
            }

            ENDCG
        }
    }
}

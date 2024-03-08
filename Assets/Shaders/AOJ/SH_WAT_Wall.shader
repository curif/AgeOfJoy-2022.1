Shader "AgeOfJoy/WorldAligned_WallMaster"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Tiling("Tiling", Float) = 1.0
        _OffsetX("Offset X", Float) = 0.0 // Add offset X property
        _OffsetY("Offset Y", Float) = 0.0 // Add offset Y property
        _Color("Color Multiplier", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0,1)) = 0.5
        _Smoothness("Smoothness", Range(0,1)) = 0.5
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows

            // Add instancing support for efficient GPU instancing.
            #pragma multi_compile_instancing

            struct Input
            {
                float2 uv_MainTex;
                float3 worldNormal;
                float3 worldPos;
            };

            sampler2D _MainTex;
            float _Tiling;
            float _OffsetX; // Declare offset X variable
            float _OffsetY; // Declare offset Y variable
            fixed4 _Color;
            float _Metallic;
            float _Smoothness;

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Scale the world position by the tiling factor
                float3 scaledWorldPos = IN.worldPos * _Tiling;

                // Apply the X and Y offsets
                scaledWorldPos.x += _OffsetX;
                scaledWorldPos.y += _OffsetY;

                // Determine UV coordinates based on world position and normal direction
                float2 uv;
                if (abs(IN.worldNormal.x) > abs(IN.worldNormal.z))
                {
                    uv = scaledWorldPos.zy; // Surface facing X axis
                }
                else
                {
                    uv = scaledWorldPos.xy; // Surface facing Z axis
                }

                // Sample the texture and apply the color multiplier
                fixed4 c = tex2D(_MainTex, uv) * _Color;
                o.Albedo = c.rgb;
                o.Alpha = c.a;

                // Set the metallic and smoothness values
                o.Metallic = _Metallic;
                o.Smoothness = _Smoothness;
            }
            ENDCG
        }
            FallBack "Diffuse"
}

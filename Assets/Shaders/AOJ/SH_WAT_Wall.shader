Shader "AgeOfJoy/WorldAligned_WallMaster"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Tiling("Tiling", Float) = 1.0
        _Color("Color Multiplier", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0,1)) = 0.5 // Add metallic property
        _Smoothness("Smoothness", Range(0,1)) = 0.5 // Add smoothness property
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
            fixed4 _Color;
            float _Metallic; // Declare metallic variable
            float _Smoothness; // Declare smoothness variable

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Scale the world position by the tiling factor
                float3 scaledWorldPos = IN.worldPos * _Tiling;

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
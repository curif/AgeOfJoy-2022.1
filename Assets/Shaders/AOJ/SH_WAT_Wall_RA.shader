Shader "AgeOfJoy/WorldAligned_WallMaster_RoughnessAlpha"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Tiling("Tiling", Float) = 1.0
        _OffsetX("Offset X", Float) = 0.0 // Add offset X property
        _OffsetY("Offset Y", Float) = 0.0 // Add offset Y property
        _Color("Color Multiplier", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0,1)) = 0.5
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
            half2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
        };

        sampler2D _MainTex;
        half _Tiling;
        half _OffsetX; // Declare offset X variable
        half _OffsetY; // Declare offset Y variable
        fixed4 _Color;
        half _Metallic;

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Scale the world position by the tiling factor, remain in float for accuracy
            float3 scaledWorldPos = IN.worldPos * _Tiling;

            // Apply the X and Y offsets
            scaledWorldPos.x += _OffsetX;
            scaledWorldPos.y += _OffsetY;

            // Determine UV coordinates based on world position and normal direction
            half2 uv;
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

            // Set the metallic value
            o.Metallic = _Metallic;

            // Use the alpha channel of the texture for smoothness
            o.Smoothness = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

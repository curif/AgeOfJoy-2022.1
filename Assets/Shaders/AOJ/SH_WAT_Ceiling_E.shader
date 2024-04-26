Shader "Custom/AOJ_WorldAligned_Ceiling_E"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _BumpMap("Normal", 2D) = "bump" {}
        _Tile("Tile Size", Float) = 1
        _TintColor("Tint Color", Color) = (1,1,1,1) // Default to white (no tint)
        _Smoothness("Smoothness", Range(0, 1)) = 0.5 // Add smoothness property, default 0.5
        _Metallic("Metallic", Range(0, 1)) = 0.0 // Add metallic property, default 0
        _EmissionMap("Emission Map", 2D) = "black" {} // Emissive texture
        _EmissionColor("Emission Color", Color) = (0,0,0,1) // Default to black (no emission)
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows

            struct Input
            {
                float3 worldPos; // Keep as float for accuracy in positioning
                float2 uv_MainTex; // UV coordinates for main texture
            };

            sampler2D _MainTex;
            sampler2D _BumpMap;
            sampler2D _EmissionMap; // Declare the emissive texture
            float _Tile;
            half4 _TintColor; // Use half precision for color data
            half _Smoothness; // Use half precision for smoothness
            half _Metallic; // Use half precision for metallic
            half4 _EmissionColor; // Use half precision for emission color

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Adjusted to use X and Z for world aligned texture coordinates
                half2 uv = IN.worldPos.xz * _Tile;

                // Apply textures, tint, and bump map
                half4 c = tex2D(_MainTex, uv) * _TintColor; // Multiply by tint color
                o.Albedo = c.rgb;
                o.Normal = UnpackNormal(tex2D(_BumpMap, uv));

                // Apply user-defined roughness and metallic settings
                o.Smoothness = _Smoothness; // Use the user-defined smoothness
                o.Metallic = _Metallic; // Use the user-defined metallic value

                // Apply emission texture
                half3 emission = tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
                o.Emission = emission;
            }
            ENDCG
        }
            FallBack "Diffuse"
}

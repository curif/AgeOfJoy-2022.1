Shader "Custom/AOJ_WorldAligned_Ceiling_SP"
{
    Properties
    {
        _MainTex("Packed Texture", 2D) = "white" {} // Now a packed texture
        _BumpMap("Normal", 2D) = "bump" {}
        _Tile("Tile Size", Float) = 1
        _TintColor("Tint Color", Color) = (1,1,1,1) // Default to white (no tint)
        _MetallicStrength("Metallic Strength", Range(0, 1)) = 0 // Control for metallic effect strength
        _ColorA("Color A", Color) = (1,0,0,1) // First color for lerp
        _ColorB("Color B", Color) = (0,0,1,1) // Second color for lerp
        _Smoothness("Smoothness", Range(0, 1)) = 0 // Smoothness control
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows

            struct Input
            {
                float3 worldPos; // Keep as float for positioning accuracy
            };

            sampler2D _MainTex;
            sampler2D _BumpMap;
            float _Tile;
            half4 _TintColor; // Use half precision for color data
            half _MetallicStrength; // Use half precision for metallic strength
            half4 _ColorA; // Use half precision for Color A
            half4 _ColorB; // Use half precision for Color B
            half _Smoothness; // Use half precision for smoothness control

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Adjusted to use X and Z for world aligned texture coordinates
                half2 uv = IN.worldPos.xz * _Tile;

                // Apply packed texture
                half4 packedTexture = tex2D(_MainTex, uv);
                half4 lerpColor = lerp(_ColorA, _ColorB, packedTexture.r); // Lerp between two colors using R channel

                half4 c = packedTexture.g * _TintColor * lerpColor; // Use G channel for diffuse and multiply by lerp color
                o.Albedo = c.rgb;
                o.Normal = UnpackNormal(tex2D(_BumpMap, uv));

                // Roughness and metallic settings
                o.Smoothness = _Smoothness; // Directly control smoothness with _Smoothness property
                o.Metallic = (1 - packedTexture.r) * _MetallicStrength; // Inverse of R channel controls metallic, modulated by strength
            }
            ENDCG
        }
            FallBack "Diffuse"
}

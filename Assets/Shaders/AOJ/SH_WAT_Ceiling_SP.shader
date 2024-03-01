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
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows

            struct Input
            {
                float3 worldPos;
            };

            sampler2D _MainTex;
            sampler2D _BumpMap;
            float _Tile;
            fixed4 _TintColor; // Tint color
            float _MetallicStrength; // Control for metallic effect strength
            fixed4 _ColorA; // First color for lerp
            fixed4 _ColorB; // Second color for lerp

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Adjusted to use X and Z for world aligned texture coordinates
                float2 uv = IN.worldPos.xz * _Tile;

                // Apply packed texture
                fixed4 packedTexture = tex2D(_MainTex, uv);
                fixed4 lerpColor = lerp(_ColorA, _ColorB, packedTexture.r); // Lerp between two colors using R channel

                fixed4 c = packedTexture.g * _TintColor * lerpColor; // Use G channel for diffuse and multiply by lerp color
                o.Albedo = c.rgb;
                o.Normal = UnpackNormal(tex2D(_BumpMap, uv));

                // Roughness and metallic settings
                o.Smoothness = 1 - packedTexture.b; // Use B channel for roughness
                o.Metallic = (1 - packedTexture.r) * _MetallicStrength; // Inverse of R channel controls metallic, modulated by strength
            }
            ENDCG
        }
            FallBack "Diffuse"
}

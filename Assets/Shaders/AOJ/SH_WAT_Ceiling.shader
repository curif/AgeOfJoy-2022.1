Shader "Custom/AOJ_WorldAligned_Ceiling"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _BumpMap("Normal", 2D) = "bump" {}
        _Tile("Tile Size", Float) = 1
        _TintColor("Tint Color", Color) = (1,1,1,1) // Default to white (no tint)
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

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Adjusted to use X and Z for world aligned texture coordinates
                float2 uv = IN.worldPos.xz * _Tile;

                // Apply textures and tint
                fixed4 c = tex2D(_MainTex, uv) * _TintColor; // Multiply by tint color
                o.Albedo = c.rgb;
                o.Normal = UnpackNormal(tex2D(_BumpMap, uv));

                // Roughness and metallic settings
                o.Smoothness = 0; // Roughness of 1 means smoothness is 0
                o.Metallic = 0; // Non-metallic surface
            }
            ENDCG
        }
            FallBack "Diffuse"
}

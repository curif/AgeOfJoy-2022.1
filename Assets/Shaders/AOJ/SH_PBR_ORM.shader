Shader "Custom/AOJ_PBR_ORM"
{
    Properties
    {
        _MainTex("Diffuse Texture", 2D) = "white" {}
        _PackedTex("Packed Texture", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_PackedTex;
            float2 uv_NormalMap;
        };

        sampler2D _MainTex;
        sampler2D _PackedTex;
        sampler2D _NormalMap;

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 packedData = tex2D(_PackedTex, IN.uv_PackedTex);
            fixed3 normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));

            // Use the G channel for roughness and the B channel for metallic
            o.Albedo = c.rgb;
            o.Smoothness = 1.0 - packedData.g; // Roughness is the inverse of smoothness
            o.Metallic = packedData.b; // Metallic from the B channel
            o.Normal = normal;
        }
        ENDCG
    }
        FallBack "Diffuse"
}

Shader "Shader Forge/AOJ_PBR_Emissive" {
    Properties{
        _Color("Main Color", Color) = (1, 1, 1, 1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0.0
        _EmissionColor("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionMap("Emission (RGB)", 2D) = "white" {}
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows
            #pragma target 3.0

            struct Input {
                float2 uv_MainTex;
                float2 uv_EmissionMap;
            };

            sampler2D _MainTex;
            sampler2D _EmissionMap;
            fixed4 _Color;
            fixed4 _EmissionColor;
            half _Glossiness;
            half _Metallic;

            void surf(Input IN, inout SurfaceOutputStandard o) {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = c.a;

                fixed4 emission = tex2D(_EmissionMap, IN.uv_EmissionMap) * _EmissionColor;
                o.Emission = emission.rgb;
            }
            ENDCG
    }
        FallBack "Diffuse"
}

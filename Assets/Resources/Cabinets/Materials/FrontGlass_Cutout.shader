Shader "Custom/FrontGlass_Cutout"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5 // Added alpha cutoff property
    }

    SubShader
    {
        Tags {"Queue" = "AlphaTest" "RenderType" = "TransparentCutout"}
        LOD 200

        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows alphaTest:_Cutoff
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        fixed _Cutoff; // Use fixed type for _Cutoff to avoid cbuffer issues

        struct Input {
            half2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // Alpha test based on the _Cutoff value
            clip(c.a - _Cutoff);

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }

        ENDCG
    }
    
    FallBack "Standard"
}

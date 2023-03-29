Shader "Custom/BacklitImage" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _BacklightColor ("Backlight Color", Color) = (1,1,1,1)
        _BacklightIntensity ("Backlight Intensity", Range(0,1)) = 0.5
        _EmissionTex ("Emission Texture", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionIntensity ("Emission Intensity", Range(0,1)) = 0.5
    }
 
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        CGPROGRAM
        #pragma surface surf Standard

        sampler2D _MainTex;
        float4 _Color;
        float4 _BacklightColor;
        float _BacklightIntensity;
        sampler2D _EmissionTex;
        float4 _EmissionColor;
        float _EmissionIntensity;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            float4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = _Color.rgb * tex.rgb;
            o.Alpha = _Color.a * tex.a;


            // Backlight effect
            float4 backlight = float4(_BacklightColor.rgb * pow(tex2D(_MainTex, IN.uv_MainTex + float2(0, _BacklightIntensity)).rgb, 2.0), 1.0);
            backlight += float4(_BacklightColor.rgb * pow(tex2D(_MainTex, IN.uv_MainTex - float2(0, _BacklightIntensity)).rgb, 2.0), 1.0);
            // Emission effect
            float4 emission = tex2D(_EmissionTex, IN.uv_MainTex);
            emission *= float4(_EmissionColor.rgb * _EmissionIntensity, 1.0);
            o.Emission = backlight.rgb + emission.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}


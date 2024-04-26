Shader "Custom/BacklitImage" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _BacklightColor("Backlight Color", Color) = (1,1,1,1)
        _BacklightIntensity("Backlight Intensity", Range(0,1)) = 0.5
        _EmissionTex("Emission Texture", 2D) = "white" {}
        _EmissionColor("Emission Color", Color) = (1,1,1,1)
        _EmissionIntensity("Emission Intensity", Range(0,1)) = 0.5
    }

        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 100

            CGPROGRAM
            #pragma surface surf Standard half

            sampler2D _MainTex;
            half4 _Color;
            half4 _BacklightColor;
            half _BacklightIntensity;
            sampler2D _EmissionTex;
            half4 _EmissionColor;
            half _EmissionIntensity;

            struct Input {
                half2 uv_MainTex;
            };

            void surf(Input IN, inout SurfaceOutputStandard o) {
                half4 tex = tex2D(_MainTex, IN.uv_MainTex);
                o.Albedo = _Color.rgb * tex.rgb;
                o.Alpha = _Color.a * tex.a;

                // Backlight effect
                half4 backlight = half4(_BacklightColor.rgb * pow(tex2D(_MainTex, IN.uv_MainTex + half2(0, _BacklightIntensity)).rgb, 2.0), 1.0);
                backlight += half4(_BacklightColor.rgb * pow(tex2D(_MainTex, IN.uv_MainTex - half2(0, _BacklightIntensity)).rgb, 2.0), 1.0);
                // Emission effect
                half4 emission = tex2D(_EmissionTex, IN.uv_MainTex);
                emission *= half4(_EmissionColor.rgb * _EmissionIntensity, 1.0);
                o.Emission = backlight.rgb + emission.rgb;
            }
            ENDCG
        }
            FallBack "Diffuse"
}

Shader "AgeOfJoy/WorldAligned_Wall_Specular_v2"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Tiling("Tiling", Float) = 1.0
        _Color("Color Multiplier", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0,1)) = 0.5
        _LightPosition("Light Position", Vector) = (0,10,0)
        _SpecularFalloff("Specular Falloff", Float) = 8.0 // Increase to sharpen
        _LightColor("Light Color", Color) = (1,1,1,1)
        _SmoothnessMultiplier("Smoothness Influence", Float) = 2.0 // Add control for smoothness influence
        _LightIntensity("Light Intensity", Float) = 2.0 // New property to control light color intensity
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Add instancing support for efficient GPU instancing.
        #pragma multi_compile_instancing

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
        };

        sampler2D _MainTex;
        half _Tiling;
        fixed4 _Color;
        half _Metallic;
        half3 _LightPosition;
        half _SpecularFalloff;
        half _SmoothnessMultiplier;
        fixed4 _LightColor;
        half _LightIntensity;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            half3 scaledWorldPos = IN.worldPos * _Tiling;

            half2 uv;
            if (abs(IN.worldNormal.x) > abs(IN.worldNormal.z))
            {
                uv = scaledWorldPos.zy;
            }
            else
            {
                uv = scaledWorldPos.xy;
            }

            fixed4 c = tex2D(_MainTex, uv) * _Color;
            o.Albedo = c.rgb;

            o.Smoothness = c.a;
            o.Metallic = _Metallic;

            half3 lightDir = normalize(_LightPosition - IN.worldPos);
            half3 viewDir = normalize(UnityWorldSpaceViewDir(IN.worldPos));
            half3 reflectDir = reflect(-lightDir, IN.worldNormal);

            // Fix 1: clamp exponent so it's never 0 (pow(x,0)=1 everywhere on rough surfaces);
            // multiply spec by smoothness so rough surfaces are dim, not just broad
            half smoothness = saturate(c.a * _SmoothnessMultiplier);
            half exponent = max(1.0, _SpecularFalloff * smoothness);
            half spec = pow(max(dot(viewDir, reflectDir), 0.0), exponent) * smoothness;

            // Fix 3: kill spec only on back-facing surfaces; don't gate by lambertian so
            // grazing-angle highlights still fire on front-facing geometry
            spec *= smoothstep(-0.1, 0.1, dot(IN.worldNormal, lightDir));

            // Fix 2: spec is the light color — remove albedo tint so the highlight can pop
            o.Emission = spec * _LightColor.rgb * _LightIntensity;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

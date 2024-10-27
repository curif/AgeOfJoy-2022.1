Shader "AgeOfJoy/AOJ_Standard_Specular"
{
    Properties
    {
        _MainTex("Albedo", 2D) = "white" {}
        _Normal("Normal Map", 2D) = "bump" {}
        _Color("Color Multiplier", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0,1)) = 0.5
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        _LightPosition("Light Position", Vector) = (0,10,0)
        _SpecularFalloff("Specular Falloff", Float) = 8.0
        _LightColor("Light Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Normal;
            float3 worldNormal;
            float3 worldPos;
            half3 lightDir;
            half3 viewDir;
            INTERNAL_DATA
        };

        sampler2D _MainTex;
        sampler2D _Normal;
        fixed4 _Color;
        half _Metallic;
        half _Smoothness;
        half3 _LightPosition;
        half _SpecularFalloff;
        fixed4 _LightColor;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
            o.lightDir = normalize(_LightPosition - o.worldPos);
            o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Sample the albedo and normal map
            fixed4 albedoTex = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = albedoTex.rgb;

            fixed4 normalTex = tex2D(_Normal, IN.uv_Normal);
            o.Normal = UnpackNormal(normalTex);

            // Set metallic and smoothness values directly
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;

            // Calculate Lambertian diffuse component
            half lambertian = max(dot(o.Normal, IN.lightDir), 0.0);

            // Specular calculation with view direction
            half3 reflectDir = reflect(-IN.lightDir, o.Normal);
            half specIntensity = pow(max(dot(IN.viewDir, reflectDir), 0.0), _SpecularFalloff / (1.0 - _Smoothness));

            // Specular highlight color
            fixed3 specularHighlight = lambertian * specIntensity * _LightColor.rgb;

            // Combine specular with albedo for emission output
            o.Emission = specularHighlight * albedoTex.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
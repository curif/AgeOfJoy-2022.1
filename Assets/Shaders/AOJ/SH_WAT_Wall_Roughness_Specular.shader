Shader "AgeOfJoy/WorldAligned_Wall_Roughness_Specular"
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
        half _SmoothnessMultiplier; // Add smoothness multiplier to sharpen specular
        fixed4 _LightColor;
        half _LightIntensity; // New light intensity multiplier

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            
            // Pass world position to fragment shader
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            
            // Pass world normal to fragment shader
            o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Scale the world position by the tiling factor
            half3 scaledWorldPos = IN.worldPos * _Tiling;

            // Determine UV coordinates based on world position and normal direction
            half2 uv;
            if (abs(IN.worldNormal.x) > abs(IN.worldNormal.z))
            {
                uv = scaledWorldPos.zy;
            }
            else
            {
                uv = scaledWorldPos.xy;
            }

            // Sample the texture and apply the color multiplier
            fixed4 c = tex2D(_MainTex, uv) * _Color;
            o.Albedo = c.rgb;

            // Use the alpha channel of the texture for smoothness
            o.Smoothness = c.a;

            // Set the metallic value
            o.Metallic = _Metallic;

            // Lambertian reflection (diffuse term)
            half3 lightDir = normalize(_LightPosition - IN.worldPos);
            half lambertian = max(dot(IN.worldNormal, lightDir), 0.0);

            // Calculate the fake specular in the fragment shader
            half3 reflectDir = reflect(-lightDir, IN.worldNormal);

            // Calculate per-pixel view direction in fragment shader
            half3 viewDir = normalize(UnityWorldSpaceViewDir(IN.worldPos));

            // Use the smoothness value from the alpha channel to modulate the specular sharpness
            half smoothness = c.a * _SmoothnessMultiplier; // Scale smoothness influence

            // Modify the specular power with smoothness to control the sharpness of the highlight
            half spec = pow(max(dot(viewDir, reflectDir), 0.0), _SpecularFalloff * smoothness);

            // Clamp the specular value to keep it sharp
            spec = clamp(spec, 0.0, 1.0);

            // Apply light color and intensity to the specular, modulated by diffuse (Lambertian)
            fixed3 specularHighlight = lambertian * spec * _LightColor.rgb * _LightIntensity;

            // Blend the specular highlight with the diffuse texture color
            fixed3 finalSpecular = specularHighlight * c.rgb;

            // Add the blended specular highlight to the emissive output
            o.Emission = finalSpecular;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

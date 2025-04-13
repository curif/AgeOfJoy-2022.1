// Unlit shader that passes through texture color,
// converting from Linear back to sRGB before output.
Shader "Unlit/Rgb565PassThroughLinearCorrected"
{
    Properties
    {
        _MainTex ("Texture (sRGB)", 2D) = "white" {}
    }
    SubShader
    {
        // Standard tags for opaque geometry.
        // Queue=Geometry is default but good practice.
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        // Ensure standard opaque render states
        ZWrite On  // Write to depth buffer
        Blend Off // No alpha blending

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0 // Needed for color space conversions

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // Quantization function using float precision
            // value: Input color component [0,1]
            // maxLevelValue: The maximum integer value for the bit depth (e.g., 31 for 5 bits)
            float Quantize(float value, float maxLevelValue)
            {
                // Scale to [0, maxLevelValue], round to nearest integer, scale back to [0, 1]
                return floor(value * maxLevelValue + 0.5) / maxLevelValue;
            }

            float4 frag (v2f i) : SV_Target
            {
                // 1. Sample Texture (gets Linear color)
                float4 col_linear = tex2D(_MainTex, i.uv);

                // 2. Convert to Gamma Space for perceptually relevant quantization
                float3 col_gamma = LinearToGammaSpace(col_linear.rgb);

                // 3. Quantize R(5-bit), G(6-bit), B(5-bit) in Gamma Space
                float r_quantized = Quantize(col_gamma.r, 31.0); // 2^5 - 1 = 31
                float g_quantized = Quantize(col_gamma.g, 63.0); // 2^6 - 1 = 63
                float b_quantized = Quantize(col_gamma.b, 31.0); // 2^5 - 1 = 31

                float3 col_gamma_quantized = float3(r_quantized, g_quantized, b_quantized);

                // Clamp just in case rounding pushes slightly > 1.0 (good practice)
                col_gamma_quantized = saturate(col_gamma_quantized);

                // 4. Convert quantized Gamma color back to Linear Space for pipeline output
                float3 col_linear_quantized = GammaToLinearSpace(col_gamma_quantized);

                // 5. Output: Return quantized Linear RGB and set Alpha to 1.0 (fully opaque)
                //          This simulates the lack of an alpha channel in the RGB565 format.
                return float4(col_linear_quantized, 1.0);
            }
            ENDCG
        }
    }
}
// Shader "Hidden/CheckAlpha"
Shader "Hidden/CheckAlpha"
{
    Properties { _MainTex ("Texture", 2D) = "white" {} _AlphaThreshold ("Alpha Threshold", Range(0.0, 1.0)) = 1.0 }
    SubShader {
        Cull Off ZWrite Off ZTest Always Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _AlphaThreshold; // Set to 1.0 by C#

            // Using fixed4 here, but values might exceed typical 0-1 range before clamping by RT format.
            // Using float4 might be slightly safer conceptually if intermediate values could be high.
            float4 frag (v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float epsilon = 0.001; // Keep epsilon for precision near 1.0

                // Check if alpha is significantly less than 1.0
                if (col.a < (_AlphaThreshold - epsilon))
                {
                    // Alpha is used -> Output a STRONG signal (e.g., bright red with high magnitude)
                    // Even if the RT clamps this to 1.0, the intention is clear.
                    // Using float4 return type might prevent premature clamping if RT format allows HDR.
                    return float4(100.0, 0.0, 0.0, 1.0);
                }
                else
                {
                    // Alpha is effectively 1.0 -> Output black
                    return float4(0.0, 0.0, 0.0, 1.0);
                }
            }
            ENDCG
        }
    }
}
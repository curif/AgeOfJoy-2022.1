// Shader to simply copy RGB values from source to destination
Shader "Hidden/CopyRGB"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img // Use built-in vertex shader for image effects
            #pragma fragment frag
            #include "UnityCG.cginc" // Include common helper functions

            sampler2D _MainTex; // Input texture

            // Fragment shader: Takes texture coordinates, returns color
            float4 frag (v2f_img i) : SV_Target
            {
                // Sample the source texture at the given UV coordinates
                float4 color = tex2D(_MainTex, i.uv);
                // Return the RGB color, alpha will be ignored by RGB24 target.
                // Set alpha to 1.0 just in case, though it shouldn't matter.
                return float4(color.rgb, 1.0);
            }
            ENDCG
        }
    }
}
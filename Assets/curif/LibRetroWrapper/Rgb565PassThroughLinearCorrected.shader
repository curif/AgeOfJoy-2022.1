// Unlit shader that passes through texture color,
// converting from Linear back to sRGB before output.
Shader "Unlit/Rgb565PassThroughLinearCorrected"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Ensure a minimum shader model that supports color space conversions
            #pragma target 3.0

            // ***** MAKE SURE THIS IS HERE *****
            #include "UnityCG.cginc"
            // *********************************

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

                        // Line 54 should be around here
            // In the fragment shader:
            // Change this:
            // fixed4 frag (v2f i) : SV_Target
            // {
            //     fixed4 col = tex2D(_MainTex, i.uv);
            //     // ...
            // }
            // To this:
            float4 frag (v2f i) : SV_Target // Or half4
            {
                float4 col = tex2D(_MainTex, i.uv); // Or half4
                col.rgb = LinearToGammaSpace(col.rgb);
                return col;
            }
            ENDCG
        }
    }
}
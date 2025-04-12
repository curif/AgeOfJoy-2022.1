// Unlit shader that just passes through the texture color.
// The conversion happens implicitly when writing to the RGB565 Render Target.
Shader "Unlit/Rgb565PassThrough"
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
            float4 _MainTex_ST; // For tiling/offset

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the source texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // Simply return the sampled color. The GPU handles the conversion
                // to the RGB565 format of the render target during the write.
                return col;
            }
            ENDCG
        }
    }
}
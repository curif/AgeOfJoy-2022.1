Shader "Custom/VertexColorEmissiveGradient" {
    Properties{
        _NumberColorA("Number Color A", Color) = (1,1,1,1) // Default to white
        _NumberColorB("Number Color B", Color) = (1,1,1,1) // Default to white, change as needed
        _MarkerColor("Marker Color", Color) = (1,0,0,1) // Default to red, keeping red as marker
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                // Use this to access UV coordinates
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float4 color : COLOR;
                    float2 uv : TEXCOORD0;
                };

                struct v2f {
                    float4 pos : SV_POSITION;
                    fixed4 color : COLOR0;
                    float2 uv : TEXCOORD0;
                };

                // Define uniforms for the colors
                fixed4 _NumberColorA;
                fixed4 _NumberColorB;
                fixed4 _MarkerColor;

                v2f vert(appdata v) {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    // Determine if the vertex color is red
                    float isRed = step(0.9, i.color.r) * (1.0 - step(0.1, i.color.g)) * (1.0 - step(0.1, i.color.b));
                // Use modified _MarkerColor if the vertex is predominantly red
                if (isRed > 0.5) {
                    return _MarkerColor * i.uv.x; // Multiply _MarkerColor by U coordinate for gradient effect
                }
 else {
                    // Otherwise, lerp between _NumberColorA and _NumberColorB based on the blue component of the vertex color
                    float blueLerpFactor = i.color.b; // Directly use the blue component for lerp factor
                    return lerp(_NumberColorA, _NumberColorB, blueLerpFactor);
                }
            }
            ENDCG
        }
    }
        FallBack "Diffuse"
}

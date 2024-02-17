Shader "Custom/SimpleCubemap" {
    Properties{
        _CubeMap("Cube Map", Cube) = "" {}
    }
        SubShader{
            Tags { "Queue" = "Background" }
            LOD 100

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t {
                    float4 vertex : POSITION;
                };

                struct v2f {
                    float3 texCoord : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                samplerCUBE _CubeMap;

                v2f vert(appdata_t v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    // Transform vertex position to world space and use as direction for cubemap
                    o.texCoord = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    // Normalize direction vector since it's used as a direction, not a position
                    float3 viewDir = normalize(i.texCoord);
                    // Sample the cube map using the view direction
                    fixed4 cubemapColor = texCUBE(_CubeMap, viewDir);
                    return cubemapColor;
                }
                ENDCG
            }
    }
        FallBack "Off"
}

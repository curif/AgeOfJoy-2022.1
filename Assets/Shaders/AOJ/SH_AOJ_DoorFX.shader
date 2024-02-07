Shader "Custom/CubemapViewWithPanningSmoke" {
    Properties{
        _CubeMap("Cube Map", Cube) = "" {}
        _SmokeTex("Smoke Texture", 2D) = "white" {}
        _SmokeColor("Smoke Color", Color) = (1,1,1,1)
        _PanningSpeed("Panning Speed", Vector) = (0.1,0,0,0)
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f {
                    float3 viewDir : TEXCOORD0;
                    float2 uv : TEXCOORD1;
                    float4 vertex : SV_POSITION;
                };

                uniform samplerCUBE _CubeMap;
                sampler2D _SmokeTex;
                float4 _SmokeColor;
                float4 _PanningSpeed;

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.viewDir = _WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz;
                    // Pass through UV and apply panning based on _Time and _PanningSpeed
                    o.uv = v.uv + _Time.xy * _PanningSpeed.xy;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    // Sample the cube map
                    float3 viewDirectionNormalized = normalize(i.viewDir);
                    viewDirectionNormalized.y = -viewDirectionNormalized.y; // Correcting orientation
                    fixed4 cubemapTex = texCUBE(_CubeMap, viewDirectionNormalized);

                    // Sample the smoke texture and apply the smoke color
                    fixed4 smokeTex = tex2D(_SmokeTex, i.uv) * _SmokeColor;

                    // Add the smoke effect on top of the cubemap
                    return cubemapTex + smokeTex;
                }
                ENDCG
            }
        }
            FallBack "Diffuse"
}

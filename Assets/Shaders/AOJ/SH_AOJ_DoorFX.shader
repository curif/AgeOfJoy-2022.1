Shader "Custom/CubemapViewWithPanningSmoke" {
    Properties{
        _CubeMap("Cube Map", Cube) = "" {}
        _SmokeTex("Smoke Texture", 2D) = "white" {}
        _SmokeColor("Smoke Color", Color) = (1,1,1,1)
        _PanningSpeed("Panning Speed", Vector) = (0.1,0,0,0)
        _Transparency("Transparency", Range(0, 1)) = 0.5
        _Tiling("Tiling", Vector) = (1,1,0,0)
    }
        SubShader{
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 100

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog // Support different fog modes
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f {
                    float3 viewDir : TEXCOORD0;
                    float2 uv : TEXCOORD1;
                    float2 originalUv : TEXCOORD2; // For stationary sampling
                    float4 vertex : SV_POSITION;
                    UNITY_FOG_COORDS(3) // Declare fog coordinates
                };

                uniform samplerCUBE _CubeMap;
                sampler2D _SmokeTex;
                float4 _SmokeColor;
                float4 _PanningSpeed;
                float _Transparency;
                float2 _Tiling;

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.viewDir = _WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz;
                    // Apply panning and tiling for animated UVs
                    o.uv = (v.uv + _Time.xy * _PanningSpeed.xy) * _Tiling.xy;
                    // Pass through original UVs for stationary sampling
                    o.originalUv = v.uv * _Tiling.xy;
                    UNITY_TRANSFER_FOG(o,o.vertex); // Assign fog coordinates
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    // Sample the cube map
                    float3 viewDirectionNormalized = normalize(i.viewDir);
                    viewDirectionNormalized.y = -viewDirectionNormalized.y;
                    fixed4 cubemapTex = texCUBE(_CubeMap, viewDirectionNormalized);

                    // Sample the smoke texture with panning UVs, using G channel
                    float smokeIntensityG = tex2D(_SmokeTex, i.uv).g;
                    fixed4 smokeTexG = fixed4(smokeIntensityG, smokeIntensityG, smokeIntensityG, 1) * _SmokeColor;

                    // Sample the smoke texture with stationary UVs, using B channel
                    float smokeIntensityB = tex2D(_SmokeTex, i.originalUv).b;

                    // Modify only the color components based on blue channel intensity
                    fixed3 colorModifier = fixed3(smokeIntensityB, smokeIntensityB, smokeIntensityB);

                    // Combine cubemap and modified smoke texture using the color modifier
                    fixed4 result = fixed4((cubemapTex.rgb * colorModifier) + (smokeTexG.rgb * colorModifier), 1.0);

                    // Apply global transparency correctly
                    result.a = _Transparency;

                    // Apply fog
                    UNITY_APPLY_FOG(i.fogCoord, result); // Apply fog to the result color

                    return result;
                }
                ENDCG
            }
        }
            FallBack "Transparent/Diffuse"
}

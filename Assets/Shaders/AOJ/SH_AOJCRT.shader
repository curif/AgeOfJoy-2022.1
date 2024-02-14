Shader "Custom/CRTShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _MaskBrightness("Mask Brightness", Float) = 0.7
        _ScanlineWeight("Scanline Weight", Float) = 6.0
        _ScanlineGapBrightness("Scanline Gap Brightness", Float) = 0.12
        _ScanlineDensity("Scanline Density", Float) = 1.0
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
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
                float _MaskBrightness;
                float _ScanlineWeight;
                float _ScanlineGapBrightness;
                float _ScanlineDensity;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                float CalcScanLineWeight(float dist)
                {
                    return max(1.0 - dist * dist * _ScanlineWeight, _ScanlineGapBrightness);
                }

                float CalcScanLine(float dy)
                {
                    dy *= _ScanlineDensity;
                    float scanLineWeight = CalcScanLineWeight(dy);
                    scanLineWeight += CalcScanLineWeight(dy - (1.0 / _ScanlineDensity));
                    scanLineWeight += CalcScanLineWeight(dy + (1.0 / _ScanlineDensity));
                    scanLineWeight *= 0.3333333;
                    return scanLineWeight;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 texcoord = i.uv;
                    float2 texcoordInPixels = texcoord * _ScreenParams.xy;
                    float tempY = floor(texcoordInPixels.y) + 0.5;
                    float yCoord = tempY / _ScreenParams.y;
                    float dy = texcoordInPixels.y - tempY;
                    float scanLineWeight = CalcScanLine(dy);
                    float signY = sign(dy);
                    dy = dy * dy;
                    dy = dy * dy;
                    dy *= 8.0;
                    dy /= _ScreenParams.y;
                    dy *= signY;
                    float2 tc = float2(texcoord.x, yCoord + dy);

                    float3 colour = tex2D(_MainTex, tc).rgb;

                    // Apply scanline weight
                    scanLineWeight *= 1.0;
                    colour *= scanLineWeight;

                    // Apply mask
                    float3 mask;
                    float whichMask = frac(i.vertex.x * 0.5);
                    if (whichMask < 0.5)
                        mask = float3(_MaskBrightness, 1.0, _MaskBrightness);
                    else
                        mask = float3(1.0, _MaskBrightness, 1.0);

                    return float4(colour * mask, 1.0);
                }
                ENDCG
            }
        }
}

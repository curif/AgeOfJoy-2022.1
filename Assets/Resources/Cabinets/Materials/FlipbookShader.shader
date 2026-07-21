//This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

Shader "Custom/FlipbookShader"
{
    Properties
    {
        _MainTex ("Flipbook Atlas (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Columns ("Columns", Range(1, 16)) = 4
        _Rows ("Rows", Range(1, 16)) = 4
        _Speed ("Speed (fps)", Range(0.5, 60)) = 12
        _Pingpong ("Ping-Pong (0=off 1=on)", Range(0, 1)) = 0
        _Glossiness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metallic", Range(0, 1)) = 0
        _EmissionStrength ("Emission Strength", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        CGPROGRAM
        #pragma surface surf Standard

        sampler2D _MainTex;
        half4 _Color;
        float _Columns;
        float _Rows;
        float _Speed;
        float _Pingpong;
        half _Glossiness;
        half _Metallic;
        half _EmissionStrength;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float totalFrames = _Columns * _Rows;
            float frame;
            if (_Pingpong > 0.5)
            {
                // cycle: 0…N-1…0, length = 2*(N-1) to avoid repeating end frames
                float ppLen = 2.0 * totalFrames - 2.0;
                float t = fmod(_Time.y, ppLen / _Speed);
                float fi = floor(t * _Speed);
                frame = totalFrames - 1.0 - abs(fi - (totalFrames - 1.0));
            }
            else
            {
                float t = fmod(_Time.y, totalFrames / _Speed);
                frame = floor(t * _Speed);
            }

            float col = fmod(frame, _Columns);
            float row = floor(frame / _Columns);

            float2 cellSize = float2(1.0 / _Columns, 1.0 / _Rows);
            float2 uv = IN.uv_MainTex * cellSize;
            // row 0 at top: invert the row axis
            uv += float2(col * cellSize.x, (_Rows - 1.0 - row) * cellSize.y);

            half4 c = tex2D(_MainTex, uv) * _Color;
            // crossfade: strength 1 = fully self-lit (unlit look), 0 = ordinary lit surface
            o.Albedo = c.rgb * (1.0 - _EmissionStrength);
            o.Emission = c.rgb * _EmissionStrength;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

//This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

Shader "Custom/MoviePoster"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _PaperMap ("Paper Map", 2D) = "white" {}
        _Blend ("Texture Blend", Range(0,1)) = 0.0

        //[PowerSlider(5.0)] _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
        //[HDR] _EmissionColor ("Emission Color", Color) = (0,0,0)
    }
    SubShader
    {
        //Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _PaperMap;

        half _Glossiness;
        half _Metallic;
        half _Shininess;
        half _Blend;

        struct Input
        {
            float2 uv_MainTex;
        };

        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 mainCol = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 texTwoCol = tex2D(_PaperMap, IN.uv_MainTex);                           
            fixed4 mainOutput = mainCol.rgba * (1.0 - (texTwoCol.a * _Blend));
            fixed4 blendOutput = texTwoCol.rgba * texTwoCol.a * _Blend;         

            o.Albedo = mainOutput.rgb + blendOutput.rgb;
            o.Alpha = mainOutput.a + blendOutput.a;

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

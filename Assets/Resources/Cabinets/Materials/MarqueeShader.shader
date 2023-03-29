//This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

Shader "Custom/MarqueeShader"
{
    Properties
    {
       // _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _EmissionMap ("Emission Map", 2D) = "black" {}
        //[PowerSlider(5.0)] _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0)
        _MainTex_TilingX ("Tiling X", Range(1, 10)) = 1
        _MainTex_TilingY ("Tiling Y", Range(1, 10)) = 1
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
        sampler2D _EmissionMap;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        //fixed4 _Color;
        fixed4 _EmissionColor;
        //half _Shininess;
        float _MainTex_TilingX;
        float _MainTex_TilingY;
        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        //UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        //UNITY_INSTANCING_BUFFER_END(Props)


        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            //fixed4 c = tex2D(_MainTex, IN.uv_MainTex); //* _Color;
            //o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            //o.Metallic = _Metallic;
            //o.Smoothness = _Glossiness;
            //o.Alpha = c.a;

            float2 uv = IN.uv_MainTex;
            uv *= float2(_MainTex_TilingX, _MainTex_TilingY);
            uv = frac(uv);
            fixed4 c = tex2D(_MainTex, uv);
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Emission = c.rgb  * tex2D(_EmissionMap, IN.uv_MainTex) * _EmissionColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

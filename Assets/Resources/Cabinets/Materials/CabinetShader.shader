//This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

Shader "Custom/CabinetShader" {
    //https://docs.unity3d.com/2020.1/Documentation/Manual/SL-SetTexture.html
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _SpecColor ("Spec Color", Color) = (1,1,1,1)

        _Emission ("Emmisive Color", Color) = (0,0,0,0)
        _Shininess ("Shininess", Range (0.01, 1)) = 0.7
        _MainTex ("Base (RGB)", 2D) = "white" {}

        _Sticker ("Sticker (RGBA) ", 2D) =  "(0,0,0,0)" {}
    }
    SubShader {
        //Tags { "RenderType"="Opaque" }
        //LOD 200

        Pass {
             // Set up basic vertex lighting
            Material {
                Diffuse [_Color]
                Ambient [_Color]
                Shininess [_Shininess]
                Specular [_SpecColor]
                Emission [_Emission]
            }
            Lighting On

            // Apply base texture
            SetTexture [_MainTex] {
                combine texture
            }
            // Blend in the sticker
            SetTexture [_Sticker] {
                combine texture lerp(texture) previous
            }
            // Multiply in texture
            SetTexture [_MainTex] {
                combine previous * texture
            }
        }
    }
}

//            CGPROGRAM
//            
//            // Physically based Standard lighting model, and enable shadows on all light types
//            #pragma surface surf Standard fullforwardshadows
//
//            // Use shader model 3.0 target, to get nicer looking lighting
//            #pragma target 3.0
//    
//            sampler2D _MainTex;
//
//            half _Glossiness;
//            half _Metallic;
//            fixed4 _Color;
//
//            struct Input {
//                float2 uv_MainTex;
//            };
//    
//            void surf (Input IN, inout SurfaceOutputStandard o) {
//                // Albedo comes from a texture tinted by color
//                fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
//                o.Albedo = c.rgb;
//                // Metallic and smoothness come from slider variables
//                o.Metallic = _Metallic;
//                o.Smoothness = _Glossiness;
//                o.Alpha = c.a;
//            }
//           ENDCG
//        }
//    }
// /}

//    Properties {
//        _Color ("Color", Color) = (1,1,1,1)
//        _Blend ("Texture Blend", Range(0,1)) = 0.0
//        _MainTex ("Albedo (RGB)", 2D) = "white" {}
//        _MainTex2 ("Albedo 2 (RGB)", 2D) = "white" {}
//        _Glossiness ("Smoothness", Range(0,1)) = 0.5
//        _Metallic ("Metallic", Range(0,1)) = 0.0
//    }
//    SubShader {
//        Tags { "RenderType"="Opaque" }
//        LOD 200
//    
//        CGPROGRAM
//        // Physically based Standard lighting model, and enable shadows on all light types
//        #pragma surface surf Standard fullforwardshadows
//
//        // Use shader model 3.0 target, to get nicer looking lighting
//        #pragma target 3.0
//
//        sampler2D _MainTex;
//        sampler2D _MainTex2;
//
//        struct Input {
//            float2 uv_MainTex;
//            float2 uv_MainTex2;
//        };
//
//        half _Blend;
//        half _Glossiness;
//        half _Metallic;
//        fixed4 _Color;
//
//        void surf (Input IN, inout SurfaceOutputStandard o) {
//            // Albedo comes from a texture tinted by color
//            fixed4 c = lerp (tex2D (_MainTex, IN.uv_MainTex), tex2D (_MainTex2, IN.uv_MainTex2), _Blend) * _Color;
//            o.Albedo = c.rgb;
//            // Metallic and smoothness come from slider variables
//            o.Metallic = _Metallic;
//            o.Smoothness = _Glossiness;
//            o.Alpha = c.a;
//        }
//        ENDCG
//    }
//    FallBack "Diffuse"
//}
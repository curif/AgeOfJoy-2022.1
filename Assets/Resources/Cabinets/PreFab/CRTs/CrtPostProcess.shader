//This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.


// Based on the excelent shader of @luka712
// https://github.com/luka712/Unity-Effects/tree/master/CRT

Shader "Custom/CrtPostProcess"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		u_time ("Time (change with each frame)", Float) = 0.01
		u_bend ("Bend", Float) = 0
		u_scanline_size_1("scanline_size_1", Float) = 9.49
		u_scanline_speed_1("scanline_speed_1", Float) = -10
		u_scanline_size_2("scanline_size_2", Float) = 175
		u_scanline_speed_2("scanline_speed_2", Float) = 2.66
		u_scanline_amount("scanline_amount", Float) = 0.005
		u_vignette_size("vignette_size", Float) = 0.01
		u_vignette_smoothness("vignette_smoothness", Float) = 0.6
		u_vignette_edge_round("vignette_edge_round", Float) = 7
		u_noise_size("noise_size", Float) = 2.8
		u_noise_amount("noise_amount", Float) = -0.1
		u_redOffset_x("redOffset_x", Float) = 0.002
		u_redOffset_y("redOffset_y", Float) = 0
		u_blueOffset_x("blueOffset_x", Float) = 0
		u_blueOffset_y("blueOffset_y", Float) = 0
		u_greenOffset_x("greenOffset_x", Float) = 0
		u_greenOffset_y("greenOffset_y", Float) = -0.002
		// [HDR] _EmissionColor("Color", Color) = (0,0,0)
		[Toggle] MirrorX("Mirror X", Float) = 0
		[Toggle] MirrorY("Mirror Y", Float) = 0
	}

	SubShader { 
		// culling or depth https://docs.unity3d.com/es/2018.4/Manual/SL-CullAndDepth.html
		Cull Back ZWrite On ZTest LEqual

		// Pass {
		// 	Name "META"
		// 	Tags { "LightMode"="Meta" }
			
		// 	Cull Off

		// 	CGPROGRAM
		// 	#pragma target 3.0
		// 	#pragma vertex vert_meta
		// 	#pragma fragment frag_meta

		// 	#pragma shader_feature _EMISSION
		// 	#pragma shader_feature _METALLICGLOSSMAP
		// 	#pragma shader_feature ___ _DETAIL_MULX2

		// 	#include "UnityStandardMeta.cginc"
		// 	ENDCG
    // }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// #pragma shader_feature MIRROR_X
			// #pragma shader_feature MIRROR_Y
			
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
			uniform float u_time;
			uniform float u_bend;
			uniform float u_scanline_size_1;
			uniform float u_scanline_speed_1;
			uniform float u_scanline_size_2;
			uniform float u_scanline_speed_2;
			uniform float u_scanline_amount;
			uniform float u_vignette_size;
			uniform float u_vignette_smoothness;
			uniform float u_vignette_edge_round;
			uniform float u_noise_size;
			uniform float u_noise_amount;
			uniform float u_redOffset_x;
			uniform float u_redOffset_y;
			uniform half2 u_red_offset ; //= half2(u_redOffset_x, u_redOffset_y);
			uniform float u_greenOffset_x;
			uniform float u_greenOffset_y;
			uniform half2 u_green_offset;
			uniform float u_blueOffset_x;
			uniform float u_blueOffset_y;
			uniform half2 u_blue_offset;
			float MirrorX;
			float MirrorY;


			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				if (MirrorX == 1) {
					o.uv.x = 1.0 - o.uv.x;
				}
				
				if (MirrorY == 1) {
					o.uv.y = 1.0 - o.uv.y;
				}
				
				return o;
			}
			

			half2 crt_coords(half2 uv, float bend)
			{
				uv -= 0.5;
				uv *= 2.;
				uv.x *= 1. + pow(abs(uv.y) / bend, 2.);
				uv.y *= 1. + pow(abs(uv.x) / bend, 2.);

				uv /= 2.5;
				return uv + 0.5;
			}

			float vignette(half2 uv, float size, float smoothness, float edgeRounding)
			{
				if (size == 0.0) {
					return 0.0;
				}
				uv -= .5;
				uv *= size;
				float amount = sqrt(pow(abs(uv.x), edgeRounding) + pow(abs(uv.y), edgeRounding));
				amount = 1. - amount;
				return smoothstep(0, smoothness, amount);
			}

			float scanline(half2 uv, float lines, float speed)
			{
				return sin(uv.y * lines + u_time * speed);
			}

			float random(half2 uv)
			{
				return frac(sin(dot(uv, half2(15.1511, 42.5225))) * 12341.51611 * sin(u_time * 0.03));
			}

			float noise(half2 uv)
			{
				half2 i = floor(uv);
				half2 f = frac(uv);

				float a = random(i);
				float b = random(i + half2(1., 0.));
				float c = random(i + half2(0, 1.));
				float d = random(i + half2(1., 1.));

				half2 u = smoothstep(0., 1., f);

				return lerp(a, b, u.x) + (c - a) * u.y * (1. - u.x) + (d - b) * u.x * u.y;
			}


			fixed4 frag (v2f i) : SV_Target
			{
				u_red_offset = half2(u_redOffset_x, u_redOffset_y);
				u_green_offset = half2(u_greenOffset_x, u_greenOffset_y);
				u_blue_offset = half2(u_blueOffset_x, u_blueOffset_y);

				half2 crt_uv = (u_bend != 0)? crt_coords(i.uv, u_bend) : i.uv;

				fixed4 col;
				col.r = tex2D(_MainTex, crt_uv + u_red_offset).r;
				col.g = tex2D(_MainTex, crt_uv + u_green_offset).g;
				col.b = tex2D(_MainTex, crt_uv + u_blue_offset).b;
				col.a = tex2D(_MainTex, crt_uv).a;

				float s1 = scanline(i.uv, u_scanline_size_1, u_scanline_speed_1);
				float s2 = scanline(i.uv, u_scanline_size_2, u_scanline_speed_2);

				col = lerp(col, fixed(s1 + s2), u_scanline_amount);
				col = lerp(col, fixed(noise(i.uv * u_noise_size)), u_noise_amount) * vignette(i.uv, u_vignette_size, u_vignette_smoothness, u_vignette_edge_round);

				return col;
			}

			ENDCG
		}
		// https://github.com/QianMo/Unity-Shader-From-Scratch/blob/master/Assets/Shaders/Unity%20Built-in%20Shaders/builtin_shaders-5.2.5f1/DefaultResourcesExtra/Standard.shader
		
		
    // FallBack "Diffuse"
	}
}

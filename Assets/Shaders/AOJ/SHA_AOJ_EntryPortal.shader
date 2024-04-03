// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/EntryPortal"
{
	Properties
	{
		_Texture0("Texture 0", 2D) = "white" {}
		_TextureSample2("Texture Sample 2", 2D) = "bump" {}
		_Color0("Color 0", Color) = (0.9764706,0.7725491,0.003921569,1)
		_Color1("Color 1", Color) = (0.2578616,0.09261715,0,0)
		_TransitionDistance("TransitionDistance", Float) = 3
		_POwer("POwer", Float) = 5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			half2 vertexToFrag53;
			float2 uv_texcoord;
			half2 vertexToFrag52;
			float3 worldPos;
		};

		uniform half4 _Color0;
		uniform half4 _Color1;
		uniform sampler2D _Texture0;
		uniform sampler2D _TextureSample2;
		uniform half _TransitionDistance;
		uniform half _POwer;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 uv_TexCoord3 = v.texcoord.xy * float2( 0.5,1 );
			half2 panner9 = ( 1.0 * _Time.y * float2( -0.19,-0.03 ) + uv_TexCoord3);
			o.vertexToFrag53 = panner9;
			half2 panner2 = ( 1.0 * _Time.y * float2( 0.15,-0.02 ) + uv_TexCoord3);
			o.vertexToFrag52 = panner2;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_TexCoord3 = i.uv_texcoord * float2( 0.5,1 );
			half2 panner22 = ( 1.0 * _Time.y * float2( 0,-0.3 ) + uv_TexCoord3);
			half3 temp_output_19_0 = ( UnpackNormal( tex2D( _TextureSample2, panner22 ) ) * half3( half2( 0.2,0.2 ) ,  0.0 ) );
			half temp_output_10_0 = max( tex2D( _Texture0, ( half3( i.vertexToFrag53 ,  0.0 ) + temp_output_19_0 ).xy ).r , tex2D( _Texture0, ( half3( i.vertexToFrag52 ,  0.0 ) + temp_output_19_0 ).xy ).g );
			half4 lerpResult11 = lerp( _Color0 , _Color1 , temp_output_10_0);
			half clampResult46 = clamp( pow( i.uv_texcoord.y , 2.0 ) , 0.0 , 1.0 );
			o.Emission = ( lerpResult11 * clampResult46 ).rgb;
			float3 ase_worldPos = i.worldPos;
			half clampResult34 = clamp( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / _TransitionDistance ) , _POwer ) , 0.0 , 1.0 );
			o.Alpha = ( ( clampResult34 * 2.0 ) - temp_output_10_0 );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float2 customPack2 : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.vertexToFrag53;
				o.customPack1.zw = customInputData.uv_texcoord;
				o.customPack1.zw = v.texcoord;
				o.customPack2.xy = customInputData.vertexToFrag52;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.vertexToFrag53 = IN.customPack1.xy;
				surfIN.uv_texcoord = IN.customPack1.zw;
				surfIN.vertexToFrag52 = IN.customPack2.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-2087.667,22.49997;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.5,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;22;-1933.057,426.1501;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-0.3;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;16;-1342.058,405.15;Inherit;True;Property;_TextureSample2;Texture Sample 2;1;0;Create;True;0;0;0;False;0;False;-1;2e7df1fb15fc2af439af22ebf06d4591;2e7df1fb15fc2af439af22ebf06d4591;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;20;-1348.057,620.4833;Inherit;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;0;False;0;False;0.2,0.2;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.WorldPosInputsNode;30;351.9427,641.1501;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;31;263.9427,818.4836;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PannerNode;2;-1774,-169.1667;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.15,-0.02;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;9;-1797.723,80.8166;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.19,-0.03;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-966.7242,503.8167;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DistanceOpNode;23;597.9426,701.8169;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;725.2761,837.1503;Inherit;False;Property;_TransitionDistance;TransitionDistance;4;0;Create;True;0;0;0;False;0;False;3;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;52;-1358.87,-176.9309;Inherit;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexToFragmentNode;53;-1377.164,45.81868;Inherit;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;5;-820.3335,-257.5;Inherit;True;Property;_Texture0;Texture 0;0;0;Create;True;0;0;0;False;0;False;a11563a4124b96c4db654a9ea7217d2c;a11563a4124b96c4db654a9ea7217d2c;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleDivideOpNode;27;954.6094,751.8168;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;1048.609,1025.15;Inherit;False;Property;_POwer;POwer;5;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;18;-889.7243,-27.84996;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;41;-939.5427,209.3724;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;6;-586.3334,85.16669;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-572.7236,332.4832;Inherit;True;Property;_TextureSample1;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;33;1163.276,785.1501;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;43;386.7908,424.3725;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;48;623.8163,549.7306;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;10;-266.7238,197.8166;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;34;1333.276,785.15;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-196.7238,451.1499;Inherit;False;Property;_Color0;Color 0;2;0;Create;True;0;0;0;False;0;False;0.9764706,0.7725491,0.003921569,1;0.9921568,0.7476662,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;14;-198.0572,657.15;Inherit;False;Property;_Color1;Color 1;3;0;Create;True;0;0;0;False;0;False;0.2578616,0.09261715,0,0;0,0.01702548,0.5566037,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;47;760.483,465.0639;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;1212.074,528.3883;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;46;990.483,389.7304;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;11;92.27609,319.1498;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;40;1440.821,521.67;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;1132.124,273.7058;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;45;355.8164,58.73059;Inherit;False;Constant;_Color3;Color 3;1;0;Create;True;0;0;0;False;0;False;0.9137255,0.5827457,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1764.667,187.3333;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;AgeOfJoy/EntryPortal;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;22;0;3;0
WireConnection;16;1;22;0
WireConnection;2;0;3;0
WireConnection;9;0;3;0
WireConnection;19;0;16;0
WireConnection;19;1;20;0
WireConnection;23;0;30;0
WireConnection;23;1;31;0
WireConnection;52;0;2;0
WireConnection;53;0;9;0
WireConnection;27;0;23;0
WireConnection;27;1;29;0
WireConnection;18;0;52;0
WireConnection;18;1;19;0
WireConnection;41;0;53;0
WireConnection;41;1;19;0
WireConnection;6;0;5;0
WireConnection;6;1;18;0
WireConnection;7;0;5;0
WireConnection;7;1;41;0
WireConnection;33;0;27;0
WireConnection;33;1;35;0
WireConnection;10;0;7;1
WireConnection;10;1;6;2
WireConnection;34;0;33;0
WireConnection;47;0;43;2
WireConnection;47;1;48;0
WireConnection;37;0;34;0
WireConnection;46;0;47;0
WireConnection;11;0;13;0
WireConnection;11;1;14;0
WireConnection;11;2;10;0
WireConnection;40;0;37;0
WireConnection;40;1;10;0
WireConnection;42;0;11;0
WireConnection;42;1;46;0
WireConnection;0;2;42;0
WireConnection;0;9;40;0
ASEEND*/
//CHKSM=DFB7B9D33B89B45A1CAD2E947F2206342C8CB189
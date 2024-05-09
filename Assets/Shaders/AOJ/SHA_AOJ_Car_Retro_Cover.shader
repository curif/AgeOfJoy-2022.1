// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/SHA_AOJ_Car_Retro_Cover"
{
	Properties
	{
		_Metllic("Metllic", Range( 0 , 1)) = 0.642
		_TimeBoxSpeed("TimeBoxSpeed", Range( 0 , 75)) = 2
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_Movement("Movement", Vector) = (0.1,0,0.1,0)
		_MoveClamp("MoveClamp", Range( 0 , 1)) = 1
		_MoveScale("MoveScale", Range( 0 , 50)) = 0
		_CoverColor("CoverColor", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform half _MoveScale;
		uniform half _TimeBoxSpeed;
		uniform sampler2D _TextureSample1;
		uniform half4 _TextureSample1_ST;
		uniform half _MoveClamp;
		uniform half3 _Movement;
		uniform half4 _CoverColor;
		uniform sampler2D _TextureSample0;
		uniform half4 _TextureSample0_ST;
		uniform half _Metllic;
		uniform half _Smoothness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			half mulTime1_g2 = _Time.y * _MoveScale;
			half vertexToFrag5_g2 = ( ( sin( ( fmod( mulTime1_g2 , 120.0 ) * _TimeBoxSpeed ) ) + 1.0 ) / 2.0 );
			float2 uv_TextureSample1 = v.texcoord * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
			float2 uv_TexCoord65 = v.texcoord.xy * float2( 66,66 );
			v.vertex.xyz += ( ( ( vertexToFrag5_g2 * saturate( ( tex2Dlod( _TextureSample1, float4( uv_TextureSample1, 0, 0.0) ).g - _MoveClamp ) ) ) * _Movement ) * sin( uv_TexCoord65.x ) );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			half4 tex2DNode48 = tex2D( _TextureSample0, uv_TextureSample0 );
			o.Albedo = ( _CoverColor * tex2DNode48 ).rgb;
			o.Metallic = _Metllic;
			half temp_output_37_0 = _Smoothness;
			o.Smoothness = temp_output_37_0;
			o.Alpha = _CoverColor.a;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

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
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
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
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
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
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
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
Node;AmplifyShaderEditor.SamplerNode;50;-426.6687,506.2029;Inherit;True;Property;_TextureSample1;Texture Sample 1;6;0;Create;True;0;0;0;False;0;False;-1;29457f7fa74e71c46a70bf45345e8884;3043c7560a917504c8a8d9065a661ea8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;64;-368.002,797.5361;Inherit;False;Property;_MoveClamp;MoveClamp;8;0;Create;True;0;0;0;False;0;False;1;0.655;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;63;-72.66876,624.2028;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-532.0227,345.9102;Inherit;False;Property;_MoveScale;MoveScale;9;0;Create;True;0;0;0;False;0;False;0;0.45;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;61;40.66467,725.536;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;51;-231.3354,391.5361;Inherit;False;SHAF_TimeBox;1;;2;fe42b26378975cf4dbc110668364facf;0;1;10;FLOAT;0.45;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;205.998,643.5363;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;65;77.64882,979.9224;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;66,66;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;53;383.3313,794.2029;Inherit;False;Property;_Movement;Movement;7;0;Create;True;0;0;0;False;0;False;0.1,0,0.1;0.08,0,0.1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;48;-458.6688,-379.1305;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;0;False;0;False;-1;09ff5171bca925544bbb40ad0301b068;09ff5171bca925544bbb40ad0301b068;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;674.6644,649.5363;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SinOpNode;68;447.6487,1013.923;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;80;79.97815,-605.4229;Inherit;False;Property;_CoverColor;CoverColor;11;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.7989943,0.8962264,0.862969,0.09803922;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;78;-369.3558,-81.08969;Inherit;False;Property;_Ruff;Ruff;10;0;Create;True;0;0;0;False;0;False;0;0.357;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;76;-73.35583,-139.0897;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-100.6691,124.8693;Inherit;False;Property;_Smoothness;Smoothness;3;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;77;82.64423,-137.7564;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;230.6642,248.8694;Inherit;False;Property;_Opacity;Opacity;4;0;Create;True;0;0;0;False;0;False;0.2029034;0.146;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;861.3231,647.8392;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-96.6691,30.86935;Inherit;False;Property;_Metllic;Metllic;0;0;Create;True;0;0;0;False;0;False;0.642;0.245;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;237.9979,-114.4639;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;279.3114,-343.4229;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;704.6666,-82;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Custom/SHA_AOJ_Car_Retro_Cover;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;63;0;50;2
WireConnection;63;1;64;0
WireConnection;61;0;63;0
WireConnection;51;10;75;0
WireConnection;52;0;51;0
WireConnection;52;1;61;0
WireConnection;59;0;52;0
WireConnection;59;1;53;0
WireConnection;68;0;65;1
WireConnection;76;0;48;2
WireConnection;76;1;78;0
WireConnection;77;0;76;0
WireConnection;74;0;59;0
WireConnection;74;1;68;0
WireConnection;49;0;77;0
WireConnection;49;1;37;0
WireConnection;79;0;80;0
WireConnection;79;1;48;0
WireConnection;0;0;79;0
WireConnection;0;3;40;0
WireConnection;0;4;37;0
WireConnection;0;9;80;4
WireConnection;0;11;74;0
ASEEND*/
//CHKSM=98349899DFB95192722AAA856C3F43E082E0BA58
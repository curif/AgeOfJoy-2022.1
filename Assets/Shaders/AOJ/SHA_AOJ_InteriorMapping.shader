// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AOJ/AOJ_InteriorMapping"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", CUBE) = "white" {}
		_TimeBoxSpeed("TimeBoxSpeed", Range( 0 , 75)) = 2
		_HashSize("HashSize", Range( 0 , 2)) = 0
		_Wiggle("Wiggle", Range( 0 , 1)) = 0
		_DistortionAmount("DistortionAmount", Range( 0 , 1)) = 0
		_TextureSample2("Texture Sample 2", 2D) = "bump" {}
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			half3 viewDir;
			INTERNAL_DATA
			float2 uv_texcoord;
			float3 worldPos;
			half vertexToFrag5_g18;
		};

		uniform samplerCUBE _TextureSample0;
		uniform half _HashSize;
		uniform half _Wiggle;
		uniform sampler2D _TextureSample2;
		uniform half4 _TextureSample2_ST;
		uniform half _DistortionAmount;
		uniform half _TimeBoxSpeed;
		uniform sampler2D _TextureSample1;
		uniform half4 _TextureSample1_ST;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.vertexToFrag5_g18 = ( ( sin( ( _Time.y * _TimeBoxSpeed ) ) + 0.0 ) / 2.0 );
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			half3 temp_output_28_0_g17 = ( i.viewDir * float3( -1,1,1 ) );
			half3 temp_output_17_0_g17 = ( float3( 1,1,1 ) / temp_output_28_0_g17 );
			half3 appendResult29_g17 = (half3(( ( frac( i.uv_texcoord ) * float2( 2,-2 ) ) - float2( 1,-1 ) ) , -1.0));
			half3 break19_g17 = ( abs( temp_output_17_0_g17 ) - ( temp_output_17_0_g17 * appendResult29_g17 ) );
			half3 temp_output_30_0_g17 = ( ( min( min( break19_g17.x , break19_g17.y ) , break19_g17.z ) * temp_output_28_0_g17 ) + appendResult29_g17 );
			float3 ase_worldPos = i.worldPos;
			half2 appendResult123_g17 = (half2(ase_worldPos.x , ase_worldPos.z));
			half2 break36_g17 = floor( ( ( appendResult123_g17 * _HashSize ) + _Wiggle ) );
			half3 appendResult10_g17 = (half3(break36_g17.x , break36_g17.y , break36_g17.x));
			half dotResult5_g17 = dot( appendResult10_g17 , half3(127.1,311.7,74.7) );
			half3 appendResult3_g17 = (half3(break36_g17.y , break36_g17.x , break36_g17.x));
			half dotResult4_g17 = dot( appendResult3_g17 , half3(269.5,183.3,246.1) );
			half3 appendResult8_g17 = (half3(break36_g17.x , break36_g17.y , break36_g17.y));
			half dotResult7_g17 = dot( appendResult8_g17 , half3(113.5,271.9,124.6) );
			half3 appendResult6_g17 = (half3(dotResult5_g17 , dotResult4_g17 , dotResult7_g17));
			half3 temp_output_39_0_g17 = round( frac( ( sin( appendResult6_g17 ) * 43758.55 ) ) );
			half3 break40_g17 = temp_output_39_0_g17;
			half3 lerpResult42_g17 = lerp( half3(-1,1,1) , half3(1,1,-1) , break40_g17.x);
			half3 lerpResult43_g17 = lerp( half3(1,1,1) , half3(-1,1,1) , break40_g17.y);
			half3 temp_output_89_0_g17 = ( temp_output_30_0_g17 * ( lerpResult42_g17 * lerpResult43_g17 ) );
			half3 lerpResult55_g17 = lerp( temp_output_89_0_g17 , (temp_output_89_0_g17).zyx , break40_g17.z);
			half3 break120_g17 = lerpResult55_g17;
			half3 appendResult121_g17 = (half3(break120_g17.x , ( break120_g17.y * -1.0 ) , break120_g17.z));
			float2 uv_TextureSample2 = i.uv_texcoord * _TextureSample2_ST.xy + _TextureSample2_ST.zw;
			half3 tex2DNode27 = UnpackNormal( tex2D( _TextureSample2, uv_TextureSample2 ) );
			half2 appendResult30 = (half2(tex2DNode27.r , tex2DNode27.g));
			half lerpResult23 = lerp( 1.0 , 0.75 , i.vertexToFrag5_g18);
			float2 uv_TextureSample1 = i.uv_texcoord * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
			half4 lerpResult26 = lerp( ( texCUBE( _TextureSample0, ( appendResult121_g17 + half3( ( appendResult30 * _DistortionAmount ) ,  0.0 ) ) ) * lerpResult23 ) , float4( 0,0,0,0 ) , tex2D( _TextureSample1, uv_TextureSample1 ).g);
			o.Emission = lerpResult26.rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows vertex:vertexDataFunc 

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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack1.z = customInputData.vertexToFrag5_g18;
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
				surfIN.vertexToFrag5_g18 = IN.customPack1.z;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = IN.tSpace0.xyz * worldViewDir.x + IN.tSpace1.xyz * worldViewDir.y + IN.tSpace2.xyz * worldViewDir.z;
				surfIN.worldPos = worldPos;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
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
Node;AmplifyShaderEditor.SamplerNode;27;-1466,212.1674;Inherit;True;Property;_TextureSample2;Texture Sample 2;7;0;Create;True;0;0;0;False;0;False;-1;None;2e7df1fb15fc2af439af22ebf06d4591;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;30;-1121.334,253.5008;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-881.3337,184.8342;Inherit;False;Property;_DistortionAmount;DistortionAmount;6;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;20;-1092.001,-185.8329;Inherit;False;SHAF_InteriorMappingHack;3;;17;0c7b5ba558395c548bda693666577299;1,59,1;0;2;FLOAT3;56;FLOAT3;61
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-881.334,24.16754;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0.2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;22;-666.0007,201.1673;Inherit;False;SHAF_TimeBox;1;;18;fe42b26378975cf4dbc110668364facf;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-669.3341,-99.16583;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;4;-373.334,-45.16644;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;8572496c029995a48b49ab7072afe53b;751e88dc59438c2459adaa6ebecaea8c;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;23;-286.0007,177.1673;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0.75;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;7.999268,91.83392;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;25;-151.3342,-243.1659;Inherit;True;Property;_TextureSample1;Texture Sample 1;8;0;Create;True;0;0;0;False;0;False;-1;None;4e3c6795ff74e4f439b54bdf81417d8f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;24;-50.66736,247.1673;Inherit;False;Constant;_Float1;Float 1;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;26;240.6658,-7.832581;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;549.3333,-14;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;AOJ/AOJ_InteriorMapping;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;30;0;27;1
WireConnection;30;1;27;2
WireConnection;31;0;30;0
WireConnection;31;1;32;0
WireConnection;28;0;20;56
WireConnection;28;1;31;0
WireConnection;4;1;28;0
WireConnection;23;2;22;0
WireConnection;21;0;4;0
WireConnection;21;1;23;0
WireConnection;26;0;21;0
WireConnection;26;2;25;2
WireConnection;0;2;26;0
ASEEND*/
//CHKSM=6C544CED0F008509748B3E7A6DE51B9030291EB1
// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_AOJ_SupportTriangle_Skybox"
{
	Properties
	{
		_SmaskRaidus("SmaskRaidus", Range( 0 , 20)) = 20
		_Bias("Bias", Range( -5 , 5)) = -1
		_ChangeDistance("ChangeDistance", Range( 0 , 20)) = 9.7
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[Toggle]_useTextureMask("useTextureMask", Float) = 0
		[Toggle]_bypassSphereMask("bypassSphereMask", Float) = 0
		_TextureSample2("Texture Sample 2", 2D) = "white" {}
		_TextureSample1("Texture Sample 1", CUBE) = "white" {}
		_OFfset("OFfset", Vector) = (0,-2,0,0)
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
		struct Input
		{
			float3 worldPos;
			float3 viewDir;
			float2 uv_texcoord;
		};

		uniform float3 _OFfset;
		uniform float _bypassSphereMask;
		uniform float _useTextureMask;
		uniform float _SmaskRaidus;
		uniform float _Bias;
		uniform float _ChangeDistance;
		uniform sampler2D _TextureSample0;
		uniform samplerCUBE _TextureSample1;
		uniform sampler2D _TextureSample2;
		uniform float4 _TextureSample2_ST;


		float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
		{
			original -= center;
			float C = cos( angle );
			float S = sin( angle );
			float t = 1 - C;
			float m00 = t * u.x * u.x + C;
			float m01 = t * u.x * u.y - S * u.z;
			float m02 = t * u.x * u.z + S * u.y;
			float m10 = t * u.x * u.y + S * u.z;
			float m11 = t * u.y * u.y + C;
			float m12 = t * u.y * u.z - S * u.x;
			float m20 = t * u.x * u.z - S * u.y;
			float m21 = t * u.y * u.z + S * u.x;
			float m22 = t * u.z * u.z + C;
			float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
			return mul( finalMatrix, original ) + center;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 _Vector0 = float3(7.063,1.434,-9.331);
			float saferPower7_g10 = abs( ( ( distance( _Vector0 , _WorldSpaceCameraPos ) + _Bias ) / _ChangeDistance ) );
			float temp_output_8_0_g10 = saturate( pow( saferPower7_g10 , 3.0 ) );
			float lerpResult12_g10 = lerp( _SmaskRaidus , 0.0 , temp_output_8_0_g10);
			float3 temp_output_5_0_g11 = ( ( ase_worldPos - _Vector0 ) / lerpResult12_g10 );
			float dotResult8_g11 = dot( temp_output_5_0_g11 , temp_output_5_0_g11 );
			float temp_output_14_0_g10 = pow( saturate( dotResult8_g11 ) , 5.0 );
			float3 lerpResult2 = lerp( float3( 0,0,0 ) , _OFfset , (( _bypassSphereMask )?( temp_output_8_0_g10 ):( (( _useTextureMask )?( temp_output_14_0_g10 ):( saturate( ( ( temp_output_14_0_g10 * 2.0 ) - tex2Dlod( _TextureSample0, float4( ( v.texcoord.xy * float2( 4,4 ) ), 0, 0.0) ).g ) ) )) )));
			v.vertex.xyz += lerpResult2;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 normalizeResult101 = normalize( i.viewDir );
			float3 rotatedValue103 = RotateAroundAxis( float3( 0,0,0 ), normalizeResult101, float3(0,0,1), 3.14 );
			float2 uv_TextureSample2 = i.uv_texcoord * _TextureSample2_ST.xy + _TextureSample2_ST.zw;
			o.Emission = ( texCUBE( _TextureSample1, rotatedValue103 ) + ( tex2D( _TextureSample2, uv_TextureSample2 ).g * 0.1 ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc 

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
				surfIN.viewDir = worldViewDir;
				surfIN.worldPos = worldPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
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
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;100;-1434.347,-88.63547;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;101;-1177.014,-79.96872;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;104;-1158.947,-532.7332;Inherit;False;Constant;_Vector0;Vector 0;6;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;105;-1084.28,-298.7332;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;3.14;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;103;-838.2801,-279.3999;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;106;-740.3944,45.99133;Inherit;True;Property;_TextureSample2;Texture Sample 2;8;0;Create;True;0;0;0;False;0;False;-1;None;3b4bc173790b8f34e918d52865e6eae0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;20;-876.355,442.5615;Inherit;False;SHAF_DistanceSwitch;1;;10;093fd74bb1b269c45832e583a601c99c;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;1;-819.2455,279.4573;Inherit;False;Property;_OFfset;OFfset;12;0;Create;True;0;0;0;False;0;False;0,-2,0;0,-2,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;4;-509.1674,-320.4998;Inherit;True;Property;_TextureSample1;Texture Sample 1;11;0;Create;True;0;0;0;False;0;False;-1;784d8a7d322a43baa027462fbd81f226;784d8a7d322a43baa027462fbd81f226;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-419.1824,21.91705;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;5;-3077.674,1135.465;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DistanceOpNode;6;-2629.926,1164.113;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-2563.926,1316.779;Inherit;False;Property;_Bias1;Bias;9;0;Create;True;0;0;0;False;0;False;-1;-1;-5;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-2167.258,1150.78;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-2173.259,1351.446;Inherit;False;Property;_ChangeDistance1;ChangeDistance;10;0;Create;True;0;0;0;False;0;False;9.7;9.1;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;10;-1933.926,1098.113;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;11;-1706.389,1098.811;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;12;-1493.859,1103.927;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-2075.925,711.7792;Inherit;False;Property;_SmaskRaidus1;SmaskRaidus;0;0;Create;True;0;0;0;False;0;False;20;13.9;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;14;-1003.926,531.7792;Inherit;False;SphereMask;-1;;12;988803ee12caf5f4690caee3c8c4a5bb;0;3;15;FLOAT3;0,0,0;False;14;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;15;-2596.997,544.9988;Inherit;False;Constant;_Vector2;Vector 0;1;0;Create;True;0;0;0;False;0;False;7.063,1.434,-9.331;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;16;-1656.592,690.7792;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;17;-1284.806,625.8997;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-1334.547,777.6691;Inherit;False;Constant;_Hard1;Hard;2;0;Create;True;0;0;0;False;0;False;5;0;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;2;-547.2346,302.4815;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;108;-201.6731,9.795776;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_AOJ_SupportTriangle_Skybox;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;101;0;100;0
WireConnection;103;0;104;0
WireConnection;103;1;105;0
WireConnection;103;3;101;0
WireConnection;4;1;103;0
WireConnection;109;0;106;2
WireConnection;6;0;15;0
WireConnection;6;1;5;0
WireConnection;8;0;6;0
WireConnection;8;1;7;0
WireConnection;10;0;8;0
WireConnection;10;1;9;0
WireConnection;11;0;10;0
WireConnection;12;0;11;0
WireConnection;14;15;15;0
WireConnection;14;14;16;0
WireConnection;14;12;18;0
WireConnection;16;0;13;0
WireConnection;16;2;12;0
WireConnection;2;1;1;0
WireConnection;2;2;20;0
WireConnection;108;0;4;0
WireConnection;108;1;109;0
WireConnection;0;2;108;0
WireConnection;0;11;2;0
ASEEND*/
//CHKSM=0006984C4FB80E185D6D12357133244FEF0FC34D
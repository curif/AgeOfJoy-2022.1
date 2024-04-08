// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_WallDecal"
{
	Properties
	{
		_Diffuse("Diffuse", 2D) = "white" {}
		_TextureSample1("Texture Sample 1", CUBE) = "white" {}
		_TimeBoxSpeed("TimeBoxSpeed", Range( 0 , 75)) = 2
		_RotSpeed("RotSpeed", Range( 0 , 6.3)) = 0.25
		_SparkeColorA("SparkeColorA", Color) = (1,0,0,0)
		_SparkeColorB("SparkeColorB", Color) = (0.04078627,1,0,0)
		_SparkeColorC("SparkeColorC", Color) = (0,0.5083904,1,0)
		_Normal("Normal", 2D) = "bump" {}
		_Tiling("Tiling", Vector) = (2,2,0,0)
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float3 worldPos;
			half vertexToFrag41_g1;
			half vertexToFrag43_g1;
			half vertexToFrag44_g1;
			half vertexToFrag42_g1;
			half vertexToFrag5_g2;
		};

		uniform sampler2D _Normal;
		uniform half2 _Tiling;
		uniform sampler2D _Diffuse;
		uniform samplerCUBE _TextureSample1;
		uniform half _RotSpeed;
		uniform half4 _SparkeColorA;
		uniform half4 _SparkeColorB;
		uniform half4 _SparkeColorC;
		uniform half _TimeBoxSpeed;
		uniform half _Metallic;
		uniform half _Smoothness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			half mulTime8_g1 = _Time.y * _RotSpeed;
			half temp_output_12_0_g1 = cos( mulTime8_g1 );
			o.vertexToFrag41_g1 = temp_output_12_0_g1;
			half temp_output_9_0_g1 = sin( mulTime8_g1 );
			o.vertexToFrag43_g1 = temp_output_9_0_g1;
			o.vertexToFrag44_g1 = temp_output_9_0_g1;
			o.vertexToFrag42_g1 = temp_output_12_0_g1;
			o.vertexToFrag5_g2 = ( ( sin( ( _Time.y * _TimeBoxSpeed ) ) + 0.0 ) / 2.0 );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord5 = i.uv_texcoord * _Tiling;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_TexCoord5 ) );
			o.Albedo = ( i.vertexColor * tex2D( _Diffuse, uv_TexCoord5 ) ).rgb;
			float3 ase_worldPos = i.worldPos;
			half3 normalizeResult7_g1 = normalize( ( ase_worldPos - half3(11.686,2.743,5.441) ) );
			half3 break10_g1 = normalizeResult7_g1;
			half3 appendResult25_g1 = (half3(( ( break10_g1.x * i.vertexToFrag41_g1 ) + ( break10_g1.z * i.vertexToFrag43_g1 ) ) , break10_g1.y , ( -( break10_g1.x * i.vertexToFrag44_g1 ) + ( break10_g1.z * i.vertexToFrag42_g1 ) )));
			half4 break30_g1 = texCUBE( _TextureSample1, appendResult25_g1 );
			o.Emission = ( saturate( ( ( break30_g1.r * _SparkeColorA ) + ( break30_g1.g * _SparkeColorB ) + ( ( break30_g1.b * _SparkeColorC ) * i.vertexToFrag5_g2 ) ) ) * 0.18 ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.Vector2Node;6;-1433.334,369.8333;Inherit;False;Property;_Tiling;Tiling;10;0;Create;True;0;0;0;False;0;False;2,2;2,2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-1222.668,357.8334;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-788.0005,111.8334;Inherit;True;Property;_Diffuse;Diffuse;0;0;Create;True;0;0;0;False;0;False;-1;None;d5815716b94cf274ebd1a1414f8ea7bc;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;4;-750.6671,-200.8333;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-774.6672,435.8334;Inherit;True;Property;_Normal;Normal;9;0;Create;True;0;0;0;False;0;False;-1;None;d4bc14ded4ddbae44a59dca87c9ab34a;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-280.6671,29.16672;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-337.334,432.5;Inherit;False;Property;_Metallic;Metallic;11;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-335.334,536.5001;Inherit;False;Property;_Smoothness;Smoothness;12;0;Create;True;0;0;0;False;0;False;0;0.364;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;10;-440.0005,245.1668;Inherit;False;SHAF_DiscoBallProjection;1;;1;a2494b936af1b0c4ebd0102a5ccf3c45;0;0;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_WallDecal;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;6;0
WireConnection;1;1;5;0
WireConnection;2;1;5;0
WireConnection;3;0;4;0
WireConnection;3;1;1;0
WireConnection;0;0;3;0
WireConnection;0;1;2;0
WireConnection;0;2;10;0
WireConnection;0;3;7;0
WireConnection;0;4;9;0
ASEEND*/
//CHKSM=22CAE4D6B6F8F386E7DF16EF7A0DC35D2F694164
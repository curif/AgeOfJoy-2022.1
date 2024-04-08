// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_DiscoBallFX"
{
	Properties
	{
		_TextureSample1("Texture Sample 1", CUBE) = "white" {}
		_RotSpeed("RotSpeed", Range( 0 , 6.3)) = 0.5
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Color0("Color 0", Color) = (1,0,0.5166283,0)
		_Vector0("Vector 0", Vector) = (1,0,0,0)
		_Vector01("Vector 01", Vector) = (0,1,0,0)
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
			float3 worldPos;
			half3 vertexToFrag55;
		};

		uniform sampler2D _TextureSample0;
		uniform half4 _TextureSample0_ST;
		uniform samplerCUBE _TextureSample1;
		uniform half _RotSpeed;
		uniform half3 _Vector0;
		uniform half3 _Vector01;
		uniform half4 _Color0;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			half3 lerpResult53 = lerp( _Vector0 , _Vector01 , ( ( _SinTime.w + 1.0 ) / 2.0 ));
			o.vertexToFrag55 = lerpResult53;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			o.Albedo = tex2D( _TextureSample0, uv_TextureSample0 ).rgb;
			float3 ase_worldPos = i.worldPos;
			half3 normalizeResult13 = normalize( ( ase_worldPos - half3(11.686,2.743,5.441) ) );
			half3 break30 = normalizeResult13;
			half mulTime35 = _Time.y * _RotSpeed;
			half temp_output_20_0 = cos( mulTime35 );
			half temp_output_19_0 = sin( mulTime35 );
			half3 appendResult28 = (half3(( ( break30.x * temp_output_20_0 ) + ( break30.z * temp_output_19_0 ) ) , break30.y , ( -( break30.x * temp_output_19_0 ) + ( break30.z * temp_output_20_0 ) )));
			half4 break47 = ( texCUBE( _TextureSample1, appendResult28 ) * half4( i.vertexToFrag55 , 0.0 ) );
			o.Emission = ( ( ( break47.r + break47.g + break47.b ) * 0.18 ) * _Color0 ).rgb;
			o.Metallic = 0.0;
			o.Smoothness = 0.0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CommentaryNode;31;-2905.63,-2499.676;Inherit;False;750.666;449.2266;Comment;4;16;14;15;13;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;29;-3189.491,-1962.087;Inherit;False;3203.681;1624.872;rotaty;27;49;55;53;52;54;41;51;50;18;39;47;40;10;28;27;26;25;24;23;22;21;20;32;30;19;35;17;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;14;-2855.63,-2449.676;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;16;-2837.825,-2234.116;Inherit;False;Constant;_Vector1;Vector 1;2;0;Create;True;0;0;0;False;0;False;11.686,2.743,5.441;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;15;-2533.63,-2367.009;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-2940.462,-1347.85;Inherit;False;Property;_RotSpeed;RotSpeed;1;0;Create;True;0;0;0;False;0;False;0.5;0.25;0;6.3;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;13;-2332.964,-2412.343;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;35;-2589.914,-1393.032;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;19;-1820.462,-1453.851;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;30;-2390.134,-1925.094;Inherit;True;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WireNode;32;-2082.58,-1524.365;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;20;-1840.462,-1611.849;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-1480.927,-1592.754;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinTimeNode;50;-1911.509,-684.6727;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1541.593,-1912.087;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;23;-1338.259,-1590.754;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1365.593,-1423.421;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-1546.926,-1790.754;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;51;-1670.843,-634.6726;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-1264.926,-1873.42;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;-1131.593,-1504.087;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;41;-1632.176,-1116.673;Inherit;False;Property;_Vector0;Vector 0;4;0;Create;True;0;0;0;False;0;False;1,0,0;0,1,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;54;-1638.843,-958.006;Inherit;False;Property;_Vector01;Vector 01;5;0;Create;True;0;0;0;False;0;False;0,1,0;1,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;52;-1452.843,-633.3393;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;28;-1004.26,-1702.754;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;53;-1290.843,-1094.006;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;10;-691.7838,-1721.101;Inherit;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;0;False;0;False;-1;dd6b53db5e6f0ae4a8e1b535c6d914c2;ea85292abd79bb146ad9c62f15ebd156;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexToFragmentNode;55;-940.0171,-1046.3;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-404.9058,-1328.009;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;47;-273.573,-1217.342;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;39;-254.8329,-614.918;Inherit;False;Constant;_DiscoBallPower;DiscoBallPower;3;0;Create;True;0;0;0;False;0;False;0.18;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;45;-55.57288,-1214.008;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;43;-216.2391,-472.6752;Inherit;False;Property;_Color0;Color 0;3;0;Create;True;0;0;0;False;0;False;1,0,0.5166283,0;1,0.04245278,0.9342039,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;41.56726,-779.5847;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;18;-1987.504,-1840.336;Inherit;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;107.7607,-506.0087;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;56;99.91809,-42.46802;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;193.9181,72.19861;Inherit;False;Constant;_Float1;Float 1;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;37;-380.9164,-230.2233;Inherit;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;0;False;0;False;-1;fcf04129d512d21418f5f4e49789e92e;d5815716b94cf274ebd1a1414f8ea7bc;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;49;-1611.509,-803.3393;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;371.2,-163.2;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_DiscoBallFX;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;15;0;14;0
WireConnection;15;1;16;0
WireConnection;13;0;15;0
WireConnection;35;0;17;0
WireConnection;19;0;35;0
WireConnection;30;0;13;0
WireConnection;32;0;35;0
WireConnection;20;0;32;0
WireConnection;21;0;30;0
WireConnection;21;1;19;0
WireConnection;22;0;30;0
WireConnection;22;1;20;0
WireConnection;23;0;21;0
WireConnection;24;0;30;2
WireConnection;24;1;20;0
WireConnection;25;0;30;2
WireConnection;25;1;19;0
WireConnection;51;0;50;4
WireConnection;26;0;22;0
WireConnection;26;1;25;0
WireConnection;27;0;23;0
WireConnection;27;1;24;0
WireConnection;52;0;51;0
WireConnection;28;0;26;0
WireConnection;28;1;30;1
WireConnection;28;2;27;0
WireConnection;53;0;41;0
WireConnection;53;1;54;0
WireConnection;53;2;52;0
WireConnection;10;1;28;0
WireConnection;55;0;53;0
WireConnection;40;0;10;0
WireConnection;40;1;55;0
WireConnection;47;0;40;0
WireConnection;45;0;47;0
WireConnection;45;1;47;1
WireConnection;45;2;47;2
WireConnection;38;0;45;0
WireConnection;38;1;39;0
WireConnection;42;0;38;0
WireConnection;42;1;43;0
WireConnection;0;0;37;0
WireConnection;0;2;42;0
WireConnection;0;3;56;0
WireConnection;0;4;57;0
ASEEND*/
//CHKSM=B645DE2D4DE802307B9C41F63522CEF15CBACFD1
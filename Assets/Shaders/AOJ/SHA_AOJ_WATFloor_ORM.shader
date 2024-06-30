// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_AOJ_WATFloor_ORM"
{
	Properties
	{
		_Diffuse("Diffuse", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_ORM("ORM", 2D) = "white" {}
		_TileSize("Tile Size", Float) = 1
		_SmoothnessMin("SmoothnessMin", Range( 0 , 1)) = 0
		_SmoothnessMax("SmoothnessMax", Range( 0 , 1)) = 1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform sampler2D _Normal;
		uniform half _TileSize;
		uniform sampler2D _Diffuse;
		uniform sampler2D _ORM;
		uniform half _SmoothnessMin;
		uniform half _SmoothnessMax;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			half4 appendResult2 = (half4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
			half4 temp_output_3_0 = ( appendResult2 / _TileSize );
			o.Normal = UnpackNormal( tex2D( _Normal, temp_output_3_0.xy ) );
			o.Albedo = tex2D( _Diffuse, temp_output_3_0.xy ).rgb;
			half4 tex2DNode7 = tex2D( _ORM, temp_output_3_0.xy );
			o.Metallic = tex2DNode7.b;
			half lerpResult9 = lerp( _SmoothnessMin , _SmoothnessMax , ( 1.0 - tex2DNode7.g ));
			o.Smoothness = lerpResult9;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.WorldPosInputsNode;1;-1060.667,6.833313;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;2;-777.3333,64.83331;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-1024,250.1667;Inherit;False;Property;_TileSize;Tile Size;3;0;Create;True;0;0;0;False;0;False;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;3;-595.3333,111.5;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;7;-378.6666,416.8333;Inherit;True;Property;_ORM;ORM;2;0;Create;True;0;0;0;False;0;False;-1;fa06fcd238ee955499cdd5eea262320b;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;8;22,628.8333;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-638,656.1667;Inherit;False;Property;_SmoothnessMax;SmoothnessMax;5;0;Create;True;0;0;0;False;0;False;1;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-675.9999,527.5;Inherit;False;Property;_SmoothnessMin;SmoothnessMin;4;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-370,21.49997;Inherit;True;Property;_Diffuse;Diffuse;0;0;Create;True;0;0;0;False;0;False;-1;78d0b45696db41f4a8233c413fd97f83;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;6;-383.3331,212.1667;Inherit;True;Property;_Normal;Normal;1;0;Create;True;0;0;0;False;0;False;-1;277ba03a70cef8e49be0a3e1e104ce61;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;9;370,409.5;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;670.6666,-14.00002;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_AOJ_WATFloor_ORM;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;1;1
WireConnection;2;1;1;3
WireConnection;3;0;2;0
WireConnection;3;1;4;0
WireConnection;7;1;3;0
WireConnection;8;0;7;2
WireConnection;5;1;3;0
WireConnection;6;1;3;0
WireConnection;9;0;10;0
WireConnection;9;1;11;0
WireConnection;9;2;8;0
WireConnection;0;0;5;0
WireConnection;0;1;6;0
WireConnection;0;3;7;3
WireConnection;0;4;9;0
ASEEND*/
//CHKSM=5F47BBA371B4A5B1D7E64A2AF8B7DE782AE0995A
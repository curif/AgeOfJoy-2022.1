// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_PolyRoom_Floor"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_tilingsize("tilingsize", Range( 0 , 5)) = 1.2
		_Radius("Radius", Float) = 5
		_Float2("Hard", Float) = 1
		_WaveOffset("WaveOffset", Vector) = (0,0.2,0,0)
		_TimeScale("TimeScale", Range( -3 , 3)) = -0.1
		_ApproachDivid("ApproachDivid", Range( 0 , 50)) = 1
		_Bias("Bias", Range( -50 , 50)) = 0
		_TextureSample1("Texture Sample 1", 2D) = "bump" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_Color0("Color 0", Color) = (0,0,0,0)
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
		};

		uniform float _Radius;
		uniform float _Float2;
		uniform float _TimeScale;
		uniform float3 _WaveOffset;
		uniform float _Bias;
		uniform float _ApproachDivid;
		uniform sampler2D _TextureSample1;
		uniform float _tilingsize;
		uniform sampler2D _TextureSample0;
		uniform float4 _Color0;
		uniform float _Smoothness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 _Vector1 = float3(1.577,0.37,29.045);
			float3 temp_output_5_0_g32 = ( ( ase_worldPos - _Vector1 ) / _Radius );
			float dotResult8_g32 = dot( temp_output_5_0_g32 , temp_output_5_0_g32 );
			float temp_output_7_0_g31 = pow( saturate( dotResult8_g32 ) , _Float2 );
			float mulTime8_g31 = _Time.y * _TimeScale;
			float temp_output_4_0_g31 = ( sin( ( ( temp_output_7_0_g31 + mulTime8_g31 ) * 16.0 ) ) * ( 1.0 - temp_output_7_0_g31 ) );
			float temp_output_30_0_g31 = ( 1.0 - saturate( ( ( distance( _WorldSpaceCameraPos , _Vector1 ) + _Bias ) / _ApproachDivid ) ) );
			v.vertex.xyz += ( ( temp_output_4_0_g31 * _WaveOffset ) * temp_output_30_0_g31 );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float2 appendResult6 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 temp_output_47_0 = ( appendResult6 * _tilingsize );
			o.Normal = UnpackNormal( tex2D( _TextureSample1, temp_output_47_0 ) );
			float3 _Vector1 = float3(1.577,0.37,29.045);
			float3 temp_output_5_0_g32 = ( ( ase_worldPos - _Vector1 ) / _Radius );
			float dotResult8_g32 = dot( temp_output_5_0_g32 , temp_output_5_0_g32 );
			float temp_output_7_0_g31 = pow( saturate( dotResult8_g32 ) , _Float2 );
			float mulTime8_g31 = _Time.y * _TimeScale;
			float temp_output_4_0_g31 = ( sin( ( ( temp_output_7_0_g31 + mulTime8_g31 ) * 16.0 ) ) * ( 1.0 - temp_output_7_0_g31 ) );
			float lerpResult50 = lerp( 0.2 , 1.0 , ( ( temp_output_4_0_g31 + 1.0 ) / 2.0 ));
			float temp_output_30_0_g31 = ( 1.0 - saturate( ( ( distance( _WorldSpaceCameraPos , _Vector1 ) + _Bias ) / _ApproachDivid ) ) );
			float lerpResult56 = lerp( 1.0 , lerpResult50 , temp_output_30_0_g31);
			o.Albedo = ( ( tex2D( _TextureSample0, temp_output_47_0 ) * _Color0 ) * lerpResult56 ).rgb;
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
Node;AmplifyShaderEditor.WorldPosInputsNode;2;-1430.88,-298.1883;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;6;-1207.546,-270.1883;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-1378.214,42.47833;Inherit;False;Property;_tilingsize;tilingsize;1;0;Create;True;0;0;0;False;0;False;1.2;0.4;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-1088.546,106.1275;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;55;-1033.241,289.0732;Inherit;False;SHAF_Polyroom_Distort;2;;31;e902e14d2ca77d547bfdbec6a3a98c30;0;0;3;FLOAT3;0;FLOAT;19;FLOAT;31
Node;AmplifyShaderEditor.SamplerNode;5;-783.5469,-338.855;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;28dafe5e9d6b7ae46a6ebb883cb63a45;f3671cf1b48e1f340a788e6a1462a662;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;43;-529.213,-93.87259;Inherit;False;Property;_Color0;Color 0;11;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.2470586,0.2470586,0.2470586,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;50;-541.8796,422.7942;Inherit;False;3;0;FLOAT;0.2;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-328.5463,-312.5392;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;56;-265.213,60.12747;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-313.8799,382.7941;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;45;-832.5463,38.79407;Inherit;True;Property;_TextureSample1;Texture Sample 1;9;0;Create;True;0;0;0;False;0;False;-1;None;6ecba3b3d03d1ea41a35d98c804e14fa;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;3;-982.8803,-264.1883;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-73.87964,-199.8726;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;44;66.12036,254.1274;Inherit;False;Property;_Smoothness;Smoothness;10;0;Create;True;0;0;0;False;0;False;0;0.256;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;388,45.33337;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_PolyRoom_Floor;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.CommentaryNode;42;-616.5463,-24.53925;Inherit;False;100;100;Comment;0;;1,1,1,1;0;0
WireConnection;6;0;2;1
WireConnection;6;1;2;3
WireConnection;47;0;6;0
WireConnection;47;1;4;0
WireConnection;5;1;47;0
WireConnection;50;2;55;19
WireConnection;41;0;5;0
WireConnection;41;1;43;0
WireConnection;56;1;50;0
WireConnection;56;2;55;31
WireConnection;45;1;47;0
WireConnection;3;0;6;0
WireConnection;3;1;4;0
WireConnection;49;0;41;0
WireConnection;49;1;56;0
WireConnection;0;0;49;0
WireConnection;0;1;45;0
WireConnection;0;4;44;0
WireConnection;0;11;55;0
ASEEND*/
//CHKSM=622CE943772D1739491A52B3BFD442C2A15D34FD
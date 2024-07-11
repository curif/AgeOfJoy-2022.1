// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_AOJ_CurtainMover"
{
	Properties
	{
		_Bias1("Bias", Range( -5 , 5)) = -1
		_ChangeDistance1("ChangeDistance", Range( 0 , 20)) = 2
		_MoverDirection("MoverDirection", Vector) = (0,0,1,0)
		_Color0("Color 0", Color) = (0.1572326,0.06674966,0.06674966,0)
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
			half filler;
		};

		uniform float _Bias1;
		uniform float _ChangeDistance1;
		uniform float3 _MoverDirection;
		uniform float4 _Color0;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float saferPower26 = abs( ( ( distance( float3(-22.301,1.192,-15.69) , _WorldSpaceCameraPos ) + _Bias1 ) / _ChangeDistance1 ) );
			float3 appendResult11 = (float3(v.color.r , v.color.r , v.color.r));
			v.vertex.xyz += ( ( ( 1.0 - saturate( pow( saferPower26 , 3.0 ) ) ) * _MoverDirection ) * appendResult11 );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _Color0.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 0.16;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CommentaryNode;29;-1664.65,935.5623;Inherit;False;1811.815;563.1133;Comment;9;20;21;22;23;24;25;26;27;28;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;28;-1601.973,985.5623;Inherit;False;Constant;_Vector2;Vector 0;1;0;Create;True;0;0;0;False;0;False;-22.301,1.192,-15.69;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;20;-1617.984,1277.362;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DistanceOpNode;21;-1166.902,1198.676;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-1100.902,1351.343;Inherit;False;Property;_Bias1;Bias;0;0;Create;True;0;0;0;False;0;False;-1;-1;-5;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-704.2344,1185.343;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-710.2354,1386.009;Inherit;False;Property;_ChangeDistance1;ChangeDistance;1;0;Create;True;0;0;0;False;0;False;2;1;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;25;-470.9023,1132.676;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;26;-243.3652,1133.375;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;27;-30.83533,1138.49;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;6;-477.9999,613.5001;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;3;-755.3333,335.5;Inherit;False;Property;_MoverDirection;MoverDirection;2;0;Create;True;0;0;0;False;0;False;0,0,1;1,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.OneMinusNode;30;-429.9998,484.8334;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-290.6666,298.8334;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;11;-152.6665,528.8334;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-74,357.5001;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;162.6667,737.5001;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;3.333374,199.5001;Half;False;Constant;_SMoothness;SMoothness;3;0;Create;True;0;0;0;False;0;False;0.16;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;17;209.3334,-65.16658;Inherit;False;Property;_Color0;Color 0;3;0;Create;True;0;0;0;False;0;False;0.1572326,0.06674966,0.06674966,0;0.6792453,0.1121395,0.2511301,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;31;-24.66589,53.83348;Inherit;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;473.3333,70.66669;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_AOJ_CurtainMover;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;21;0;28;0
WireConnection;21;1;20;0
WireConnection;23;0;21;0
WireConnection;23;1;22;0
WireConnection;25;0;23;0
WireConnection;25;1;24;0
WireConnection;26;0;25;0
WireConnection;27;0;26;0
WireConnection;30;0;27;0
WireConnection;2;0;30;0
WireConnection;2;1;3;0
WireConnection;11;0;6;1
WireConnection;11;1;6;1
WireConnection;11;2;6;1
WireConnection;5;0;2;0
WireConnection;5;1;11;0
WireConnection;0;0;17;0
WireConnection;0;3;31;0
WireConnection;0;4;19;0
WireConnection;0;11;5;0
ASEEND*/
//CHKSM=AD6B8848DE3B45510962B7394A09A8841104D688
// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_TV_QD"
{
	Properties
	{
		_T_QD_Overlay("T_QD_Overlay", 2D) = "white" {}
		_T_QD_BG("T_QD_BG", 2D) = "white" {}
		_T_QD_Cursor("T_QD_Cursor", 2D) = "white" {}
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
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _T_QD_BG;
		uniform sampler2D _T_QD_Cursor;
		uniform sampler2D _T_QD_Overlay;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			half2 panner2 = ( 1.0 * _Time.y * float2( 0.02,0 ) + ( i.uv_texcoord * float2( 0.5,1 ) ));
			half lerpResult21 = lerp( -0.4 , 0.4 , ( ( sin( _Time.y ) + 1.0 ) / 2.0 ));
			half mulTime22 = _Time.y * 3.0;
			half lerpResult26 = lerp( -0.3 , 0.3 , ( ( sin( mulTime22 ) + 1.0 ) / 2.0 ));
			half2 appendResult16 = (half2(( i.uv_texcoord.x + lerpResult21 ) , ( lerpResult26 + i.uv_texcoord.y )));
			half4 tex2DNode7 = tex2D( _T_QD_Cursor, appendResult16 );
			half4 lerpResult8 = lerp( tex2D( _T_QD_BG, panner2 ) , tex2DNode7 , tex2DNode7.a);
			half4 tex2DNode1 = tex2D( _T_QD_Overlay, i.uv_texcoord );
			half4 lerpResult5 = lerp( lerpResult8 , tex2DNode1 , tex2DNode1.a);
			o.Albedo = lerpResult5.rgb;
			o.Emission = ( lerpResult5 * 0.5 ).rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.SimpleTimeNode;13;-1677.5,512;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;22;-1771.7,681.8;Inherit;False;1;0;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;14;-1483.5,530;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;23;-1580.7,699.8;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;24;-1444.7,704.8;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-1351.5,573;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;25;-1301.7,753.8;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;20;-1204.5,584;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;3;-1724.5,-9;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexCoordVertexDataNode;9;-1656.5,359;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;21;-1116.5,536;Inherit;False;3;0;FLOAT;-0.4;False;1;FLOAT;0.4;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;26;-1181.7,699.8;Inherit;False;3;0;FLOAT;-0.3;False;1;FLOAT;0.3;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-1243.5,142;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-992.5,390;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-920.5,676;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;2;-1100.5,75;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.02,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;16;-790.5,502;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;33;-1379.5,-218;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-914.5,211;Inherit;True;Property;_T_QD_BG;T_QD_BG;1;0;Create;True;0;0;0;False;0;False;-1;47cff3f7dc0611f4e8ba627580c00585;47cff3f7dc0611f4e8ba627580c00585;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-641.5,437;Inherit;True;Property;_T_QD_Cursor;T_QD_Cursor;2;0;Create;True;0;0;0;False;0;False;-1;d23edf654f12eb14fa0b1acf1f6348c1;d23edf654f12eb14fa0b1acf1f6348c1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-797.5,-153;Inherit;True;Property;_T_QD_Overlay;T_QD_Overlay;0;0;Create;True;0;0;0;False;0;False;-1;3e3b0cdedb4d93f41bdd3b0f3ae7ed59;3e3b0cdedb4d93f41bdd3b0f3ae7ed59;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;8;-236.5,260;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;5;-290.5,-221;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-121.5,18;Inherit;False;Constant;_Float2;Float 2;3;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;100.5,182;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;79.5,-159;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.5660378;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;30;156.5,287;Inherit;False;Constant;_Float1;Float 1;3;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;406,-11;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_TV_QD;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;14;0;13;0
WireConnection;23;0;22;0
WireConnection;24;0;23;0
WireConnection;19;0;14;0
WireConnection;25;0;24;0
WireConnection;20;0;19;0
WireConnection;21;2;20;0
WireConnection;26;2;25;0
WireConnection;6;0;3;0
WireConnection;11;0;9;1
WireConnection;11;1;21;0
WireConnection;28;0;26;0
WireConnection;28;1;9;2
WireConnection;2;0;6;0
WireConnection;16;0;11;0
WireConnection;16;1;28;0
WireConnection;4;1;2;0
WireConnection;7;1;16;0
WireConnection;1;1;33;0
WireConnection;8;0;4;0
WireConnection;8;1;7;0
WireConnection;8;2;7;4
WireConnection;5;0;8;0
WireConnection;5;1;1;0
WireConnection;5;2;1;4
WireConnection;31;0;5;0
WireConnection;31;1;35;0
WireConnection;0;0;5;0
WireConnection;0;2;31;0
WireConnection;0;3;29;0
WireConnection;0;4;30;0
ASEEND*/
//CHKSM=A3AA1E7CD3558DA541EA41951E74204613FE908B
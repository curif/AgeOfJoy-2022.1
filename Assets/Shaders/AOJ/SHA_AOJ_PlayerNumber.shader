// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/PlayerNumber"
{
	Properties
	{
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color8 = IsGammaSpace() ? float4(0,0.5944536,1,0) : float4(0,0.3121113,1,0);
			float4 color7 = IsGammaSpace() ? float4(0,0.08015776,0.08176088,0) : float4(0,0.007214603,0.007421687,0);
			float4 lerpResult9 = lerp( color8 , color7 , i.uv_texcoord.x);
			float4 lerpResult15 = lerp( float4( 0,0,0,0 ) , lerpResult9 , i.vertexColor.r);
			o.Albedo = lerpResult15.rgb;
			float4 color3 = IsGammaSpace() ? float4(1,0.8077339,0,0) : float4(1,0.6170191,0,0);
			float4 color4 = IsGammaSpace() ? float4(1,0,0,0) : float4(1,0,0,0);
			float4 lerpResult5 = lerp( color3 , color4 , i.uv_texcoord.y);
			float4 lerpResult14 = lerp( lerpResult5 , float4( 0,0,0,0 ) , i.vertexColor.r);
			o.Emission = ( ( lerpResult15 * float4( 0.01257855,0.01257855,0.01257855,0 ) ) + lerpResult14 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-1013,-135.1668;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;8;-1005.333,233.8333;Inherit;False;Constant;_BGColorA;BGColorA;0;0;Create;True;0;0;0;False;0;False;0,0.5944536,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;-1010,426.5;Inherit;False;Constant;_BGColorB;BGColorB;0;0;Create;True;0;0;0;False;0;False;0,0.08015776,0.08176088,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-347.0001,635.4999;Inherit;False;Constant;_NumberColorB;NumberColorB;0;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;3;-341.0001,444.1666;Inherit;False;Constant;_NumberColorA;NumberColorA;0;0;Create;True;0;0;0;False;0;False;1,0.8077339,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;9;-654.3335,332.1667;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;1;-407.0001,-75.16675;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;5;-28.33331,563.5;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;15;248.3333,-90.50002;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;14;142.3333,381.5;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;407.6665,27.83324;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0.01257855,0.01257855,0.01257855,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;17;488.3333,218.4999;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;679.3333,-55.33332;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;AgeOfJoy/PlayerNumber;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;8;0
WireConnection;9;1;7;0
WireConnection;9;2;2;1
WireConnection;5;0;3;0
WireConnection;5;1;4;0
WireConnection;5;2;2;2
WireConnection;15;1;9;0
WireConnection;15;2;1;1
WireConnection;14;0;5;0
WireConnection;14;2;1;1
WireConnection;18;0;15;0
WireConnection;17;0;18;0
WireConnection;17;1;14;0
WireConnection;0;0;15;0
WireConnection;0;2;17;0
ASEEND*/
//CHKSM=12F882F55D82B2CF8B1F2F44EE18CD67EE2FBC63
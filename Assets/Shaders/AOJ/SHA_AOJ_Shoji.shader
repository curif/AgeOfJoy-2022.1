// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_AOJ_Shoji"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_TimeBoxSpeed("TimeBoxSpeed", Range( 0 , 75)) = 2
		_TextureSample1("Texture Sample 0", 2D) = "white" {}
		_ShojiColor("ShojiColor", Color) = (0,0,0,0)
		_ShojiColorB("ShojiColorB", Color) = (0,0,0,0)
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
			half vertexToFrag5_g1;
		};

		uniform sampler2D _TextureSample0;
		uniform half4 _TextureSample0_ST;
		uniform sampler2D _TextureSample1;
		uniform half4 _TextureSample1_ST;
		uniform half4 _ShojiColor;
		uniform half4 _ShojiColorB;
		uniform half _TimeBoxSpeed;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			half mulTime1_g1 = _Time.y * 0.5;
			o.vertexToFrag5_g1 = ( ( sin( ( fmod( mulTime1_g1 , 120.0 ) * _TimeBoxSpeed ) ) + 1.0 ) / 2.0 );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			half4 lerpResult3 = lerp( half4(0,0,0,0) , tex2D( _TextureSample0, uv_TextureSample0 ) , i.vertexColor.r);
			o.Albedo = lerpResult3.rgb;
			float2 uv_TextureSample1 = i.uv_texcoord * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
			half4 lerpResult15 = lerp( _ShojiColor , _ShojiColorB , i.vertexToFrag5_g1);
			o.Emission = ( ( tex2D( _TextureSample1, uv_TextureSample1 ) * lerpResult15 ) * ( 1.0 - i.vertexColor.r ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.RangedFloatNode;14;-696.1664,665.1667;Inherit;False;Constant;_GlowSpeed;GlowSpeed;4;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-272.1666,78.83318;Inherit;False;Property;_ShojiColor;ShojiColor;4;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,0.7900136,0.3238992,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;13;-445.1664,630.1667;Inherit;False;SHAF_TimeBox;1;;1;fe42b26378975cf4dbc110668364facf;0;1;10;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;16;-278.1664,247.1667;Inherit;False;Property;_ShojiColorB;ShojiColorB;5;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,0.7900136,0.3238992,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;5;-714,324.1666;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;10;-110,-306.5001;Inherit;True;Property;_TextureSample1;Texture Sample 0;3;0;Create;True;0;0;0;False;0;False;-1;2f754e94d764dad0191edc2e28a951fa;1af53a5364e611f4c8f476da747a63ba;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;15;14.83362,183.1667;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector4Node;1;-661,-1.833374;Inherit;False;Constant;_Vector0;Vector 0;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;9;-377,386.1666;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-613,-228.8334;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;1d462a9be0a0b3a86bfadcb74c4a59d4;1d462a9be0a0b3a86bfadcb74c4a59d4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;62.5,37.49985;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;3;-241,-142.8334;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;97.33325,270.4999;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;396,-56;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_AOJ_Shoji;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;13;10;14;0
WireConnection;15;0;12;0
WireConnection;15;1;16;0
WireConnection;15;2;13;0
WireConnection;9;0;5;1
WireConnection;11;0;10;0
WireConnection;11;1;15;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;3;2;5;1
WireConnection;8;0;11;0
WireConnection;8;1;9;0
WireConnection;0;0;3;0
WireConnection;0;2;8;0
ASEEND*/
//CHKSM=E8963AC9B54DFF63C4DD5064CF8DC19FBDD11F97
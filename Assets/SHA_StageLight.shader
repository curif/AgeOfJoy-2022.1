// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_StageLight"
{
	Properties
	{
		_T_Stagelight_D("T_Stagelight_D", 2D) = "white" {}
		_LightColor("LightColor", Color) = (0,1,0.9965577,0)
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

		uniform sampler2D _T_Stagelight_D;
		uniform half4 _T_Stagelight_D_ST;
		uniform half4 _LightColor;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_T_Stagelight_D = i.uv_texcoord * _T_Stagelight_D_ST.xy + _T_Stagelight_D_ST.zw;
			o.Albedo = tex2D( _T_Stagelight_D, uv_T_Stagelight_D ).rgb;
			o.Emission = ( _LightColor * ( 1.0 - i.vertexColor.r ) ).rgb;
			half lerpResult2 = lerp( 0.0 , 0.7 , i.vertexColor.r);
			o.Metallic = lerpResult2;
			half lerpResult6 = lerp( 1.0 , 1.0 , i.vertexColor.r);
			o.Smoothness = lerpResult6;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.VertexColorNode;5;-779.0001,231.1667;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;-1304.506,-430.3518;Inherit;False;Property;_LightColor;LightColor;1;0;Create;True;0;0;0;False;0;False;0,1,0.9965577,0;1,0.9352571,0.6069182,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;9;-472.1724,36.98163;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-564.7629,-505.5202;Inherit;True;Property;_T_Stagelight_D;T_Stagelight_D;0;0;Create;True;0;0;0;False;0;False;-1;3aa5973d716e07140acb4efbe2520cf5;3aa5973d716e07140acb4efbe2520cf5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-384.839,-187.685;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;6;-358.3335,316.5001;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;2;-444.3334,157.1667;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.7;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_StageLight;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;5;1
WireConnection;8;0;7;0
WireConnection;8;1;9;0
WireConnection;6;2;5;1
WireConnection;2;2;5;1
WireConnection;0;0;1;0
WireConnection;0;2;8;0
WireConnection;0;3;2;0
WireConnection;0;4;6;0
ASEEND*/
//CHKSM=EC447B5B0F290C25C12E119E3771077F4120FD11
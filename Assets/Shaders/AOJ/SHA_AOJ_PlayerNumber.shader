// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/PlayerNumber"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "bump" {}
		_BGColorB("BGColorB", Color) = (0.2238637,0.8911631,0.9245283,0)
		_BGColorA("BGColorA", Color) = (0,0.5944536,1,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform half4 _BGColorB;
		uniform half4 _BGColorA;
		uniform sampler2D _TextureSample0;
		uniform half4 _TextureSample0_ST;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			half4 color3 = IsGammaSpace() ? half4(1,0.8077339,0,0) : half4(1,0.6170191,0,0);
			half4 color4 = IsGammaSpace() ? half4(1,0,0,0) : half4(1,0,0,0);
			half4 lerpResult5 = lerp( color3 , color4 , i.uv_texcoord.y);
			half4 lerpResult9 = lerp( _BGColorB , _BGColorA , i.uv_texcoord.x);
			half4 lerpResult31 = lerp( lerpResult5 , lerpResult9 , i.vertexColor.r);
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			o.Emission = ( lerpResult31 * ( ( UnpackNormal( tex2D( _TextureSample0, uv_TextureSample0 ) ).r + 1.0 ) * 0.5 ) ).rgb;
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
Node;AmplifyShaderEditor.ColorNode;8;-1005.333,239.8333;Inherit;False;Property;_BGColorA;BGColorA;2;0;Create;True;0;0;0;False;0;False;0,0.5944536,1,0;0.4575472,0.8461723,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;-1010,426.5;Inherit;False;Property;_BGColorB;BGColorB;1;0;Create;True;0;0;0;False;0;False;0.2238637,0.8911631,0.9245283,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-347.0001,635.4999;Inherit;False;Constant;_NumberColorB;NumberColorB;0;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;3;-341.0001,444.1666;Inherit;False;Constant;_NumberColorA;NumberColorA;0;0;Create;True;0;0;0;False;0;False;1,0.8077339,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;22;574.0004,417.5;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;b221b7a0e2221a447aa6b5981ad54d54;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;9;-654.3335,332.1667;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;5;-28.33331,564.8333;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;1;-157.6668,25.49991;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;25;878.7021,424.6931;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;31;206.7023,829.6933;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;1122.702,520.6932;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;19;978.0004,-184.8336;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;233.6665,165.1666;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0.01257855,0.01257855,0.01257855,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;14;312.3333,416.1667;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;17;660.3333,67.1666;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;1267.334,24.83331;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0.2389936,0.2389936,0.2389936,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;28;1291.369,204.0265;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;1307.369,468.0265;Inherit;False;Constant;_Float1;Float 1;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;30;1190.702,-208.3067;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;15;110.9997,-253.1667;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;1201.369,829.0266;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1622.667,-59.33332;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;AgeOfJoy/PlayerNumber;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;7;0
WireConnection;9;1;8;0
WireConnection;9;2;2;1
WireConnection;5;0;3;0
WireConnection;5;1;4;0
WireConnection;5;2;2;2
WireConnection;25;0;22;1
WireConnection;31;0;5;0
WireConnection;31;1;9;0
WireConnection;31;2;1;1
WireConnection;27;0;25;0
WireConnection;19;0;5;0
WireConnection;19;1;15;0
WireConnection;19;2;1;1
WireConnection;18;0;15;0
WireConnection;14;0;5;0
WireConnection;14;2;1;1
WireConnection;17;0;18;0
WireConnection;17;1;14;0
WireConnection;23;0;17;0
WireConnection;30;0;19;0
WireConnection;30;1;5;0
WireConnection;30;2;1;1
WireConnection;15;1;9;0
WireConnection;15;2;1;1
WireConnection;32;0;31;0
WireConnection;32;1;27;0
WireConnection;0;2;32;0
ASEEND*/
//CHKSM=DFC91831034EE10B2FB1EC5258A5292F9818EDAD
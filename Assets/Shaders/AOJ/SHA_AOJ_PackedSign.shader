// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_AOJ_PackedSign"
{
	Properties
	{
		_DiffusePacked("DiffusePacked", 2D) = "white" {}
		_Normal("Normal", 2D) = "white" {}
		_ColorA("ColorA", Color) = (1,1,1,0)
		_ColorB("ColorB", Color) = (0,0,0,0)
		_SmoothnessA("SmoothnessA", Range( 0 , 1)) = 0
		_MetalA("MetalA", Range( 0 , 1)) = 0
		_SmoothnessB("SmoothnessB", Range( 0 , 1)) = 0
		_MetalB("MetalB", Range( 0 , 1)) = 0
		_UseChannel("UseChannel", Vector) = (1,1,1,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform half4 _Normal_ST;
		uniform half4 _ColorB;
		uniform half4 _ColorA;
		uniform sampler2D _DiffusePacked;
		uniform half4 _DiffusePacked_ST;
		uniform half4 _UseChannel;
		uniform half _MetalB;
		uniform half _MetalA;
		uniform half _SmoothnessB;
		uniform half _SmoothnessA;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = tex2D( _Normal, uv_Normal ).rgb;
			float2 uv_DiffusePacked = i.uv_texcoord * _DiffusePacked_ST.xy + _DiffusePacked_ST.zw;
			half4 break6 = ( tex2D( _DiffusePacked, uv_DiffusePacked ) * _UseChannel );
			half temp_output_10_0 = saturate( ( break6.r + break6.g + break6.b + break6.a ) );
			half4 lerpResult11 = lerp( _ColorB , _ColorA , temp_output_10_0);
			o.Albedo = lerpResult11.rgb;
			half lerpResult25 = lerp( _MetalB , _MetalA , temp_output_10_0);
			o.Metallic = lerpResult25;
			half lerpResult21 = lerp( _SmoothnessB , _SmoothnessA , temp_output_10_0);
			o.Smoothness = lerpResult21;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.SamplerNode;1;-1044,-71.83337;Inherit;True;Property;_DiffusePacked;DiffusePacked;0;0;Create;True;0;0;0;False;0;False;-1;None;b435bd455f6b8c547891edec9b2376ba;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;26;-846.8518,164.0028;Inherit;False;Property;_UseChannel;UseChannel;8;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-580,-111.8334;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;6;-353.3334,-117.1667;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;9;-164,-100.5001;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;10;-38,-53.16672;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;-193.3332,562.8334;Inherit;False;Property;_ColorA;ColorA;2;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.9097006,0.9496855,0.2299552,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;16;-282.6666,296.8333;Inherit;False;Property;_ColorB;ColorB;3;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.8113207,0.07909087,0.07909087,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;19;299.1473,730.3358;Inherit;False;Property;_SmoothnessA;SmoothnessA;4;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;314.4806,603.0026;Inherit;False;Property;_SmoothnessB;SmoothnessB;6;0;Create;True;0;0;0;False;0;False;0;0.863;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;275.5916,1045.447;Inherit;False;Property;_MetalB;MetalB;7;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;260.2584,1172.78;Inherit;False;Property;_MetalA;MetalA;5;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;11;180,-65.83339;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;21;666.4807,526.3359;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;25;627.5918,968.7803;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;415.814,307.6692;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;-666.6666,359.5;Inherit;True;Property;_Normal;Normal;1;0;Create;True;0;0;0;False;0;False;-1;194a51ad3c0179644abea3f196c5ebe6;194a51ad3c0179644abea3f196c5ebe6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;853.3334,114;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_AOJ_PackedSign;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;0;1;0
WireConnection;3;1;26;0
WireConnection;6;0;3;0
WireConnection;9;0;6;0
WireConnection;9;1;6;1
WireConnection;9;2;6;2
WireConnection;9;3;6;3
WireConnection;10;0;9;0
WireConnection;11;0;16;0
WireConnection;11;1;15;0
WireConnection;11;2;10;0
WireConnection;21;0;22;0
WireConnection;21;1;19;0
WireConnection;21;2;10;0
WireConnection;25;0;24;0
WireConnection;25;1;23;0
WireConnection;25;2;10;0
WireConnection;0;0;11;0
WireConnection;0;1;17;0
WireConnection;0;3;25;0
WireConnection;0;4;21;0
ASEEND*/
//CHKSM=862C1088ED8D66465407DF11FE55065DAB5A91CF
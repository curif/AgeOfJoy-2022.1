// Upgrade NOTE: upgraded instancing buffer 'AgeOfJoyPrintout' to new syntax.

// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/Printout"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_RChannelPower("R Channel Power", Range( 0 , 1)) = 1
		_GChannelPower("G Channel Power", Range( 0 , 1)) = 0
		_BChannelPower("B Channel Power", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _TextureSample1;
		uniform float _Cutoff = 0.5;

		UNITY_INSTANCING_BUFFER_START(AgeOfJoyPrintout)
			UNITY_DEFINE_INSTANCED_PROP(float4, _TextureSample1_ST)
#define _TextureSample1_ST_arr AgeOfJoyPrintout
			UNITY_DEFINE_INSTANCED_PROP(float, _RChannelPower)
#define _RChannelPower_arr AgeOfJoyPrintout
			UNITY_DEFINE_INSTANCED_PROP(float, _GChannelPower)
#define _GChannelPower_arr AgeOfJoyPrintout
			UNITY_DEFINE_INSTANCED_PROP(float, _BChannelPower)
#define _BChannelPower_arr AgeOfJoyPrintout
		UNITY_INSTANCING_BUFFER_END(AgeOfJoyPrintout)

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 _TextureSample1_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_TextureSample1_ST_arr, _TextureSample1_ST);
			float2 uv_TextureSample1 = i.uv_texcoord * _TextureSample1_ST_Instance.xy + _TextureSample1_ST_Instance.zw;
			float4 tex2DNode2 = tex2Dbias( _TextureSample1, float4( uv_TextureSample1, 0, -0.7) );
			float _RChannelPower_Instance = UNITY_ACCESS_INSTANCED_PROP(_RChannelPower_arr, _RChannelPower);
			float _GChannelPower_Instance = UNITY_ACCESS_INSTANCED_PROP(_GChannelPower_arr, _GChannelPower);
			float _BChannelPower_Instance = UNITY_ACCESS_INSTANCED_PROP(_BChannelPower_arr, _BChannelPower);
			float temp_output_5_0 = ( ( tex2DNode2.r * _RChannelPower_Instance ) + ( tex2DNode2.g * _GChannelPower_Instance ) + ( tex2DNode2.b * _BChannelPower_Instance ) );
			float3 temp_cast_0 = (temp_output_5_0).xxx;
			o.Albedo = temp_cast_0;
			float temp_output_3_0 = 0.0;
			o.Metallic = temp_output_3_0;
			o.Smoothness = temp_output_3_0;
			o.Alpha = 1;
			clip( tex2DNode2.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.RangedFloatNode;11;-574.3334,136.5002;Inherit;False;InstancedProperty;_GChannelPower;G Channel Power;3;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-525.3334,232.5003;Inherit;False;InstancedProperty;_BChannelPower;B Channel Power;4;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-576.3334,-123.4997;Inherit;False;InstancedProperty;_RChannelPower;R Channel Power;2;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-932,-191.5001;Inherit;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;0;False;0;False;-1;86647280307750f47b15cd0564f48f0d;86647280307750f47b15cd0564f48f0d;True;0;False;white;Auto;False;Object;-1;MipBias;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;-0.7;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-212.3334,-262.4998;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-181.3334,-140.4997;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-182.3334,29.50024;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;5;166.6666,-161.4997;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TwoSidedSign;13;-169.3329,282.5003;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;181,169.4999;Inherit;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;4;-476.3334,359.5003;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;14;357.6671,-41.49976;Inherit;False;3;0;FLOAT;2;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;579,-86.99998;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;AgeOfJoy/Printout;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;0;2;1
WireConnection;7;1;10;0
WireConnection;8;0;2;2
WireConnection;8;1;11;0
WireConnection;9;0;2;3
WireConnection;9;1;12;0
WireConnection;5;0;7;0
WireConnection;5;1;8;0
WireConnection;5;2;9;0
WireConnection;14;1;5;0
WireConnection;14;2;13;0
WireConnection;0;0;5;0
WireConnection;0;3;3;0
WireConnection;0;4;3;0
WireConnection;0;10;2;4
ASEEND*/
//CHKSM=F1D11D825CF473BEA855B3EA080B275DD5BC6193
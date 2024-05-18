// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_Polyroom_Standard"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Radius("Radius", Float) = 5
		_Float2("Hard", Float) = 1
		_WaveOffset("WaveOffset", Vector) = (0,0.2,0,0)
		_TimeScale("TimeScale", Range( -3 , 3)) = -0.1
		_ApproachDivid("ApproachDivid", Range( 0 , 50)) = 1
		_Bias("Bias", Range( -50 , 50)) = 0
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_Color0("Color 0", Color) = (1,1,1,0)
		_Smooth("Smooth", Float) = 0
		_Metal("Metal", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
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
			float2 uv_texcoord;
		};

		uniform float _Radius;
		uniform float _Float2;
		uniform float _TimeScale;
		uniform float3 _WaveOffset;
		uniform float _Bias;
		uniform float _ApproachDivid;
		uniform sampler2D _TextureSample1;
		uniform float4 _TextureSample1_ST;
		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform float4 _Color0;
		uniform float _Metal;
		uniform float _Smooth;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 _Vector1 = float3(1.577,0.37,29.045);
			float3 temp_output_5_0_g24 = ( ( ase_worldPos - _Vector1 ) / _Radius );
			float dotResult8_g24 = dot( temp_output_5_0_g24 , temp_output_5_0_g24 );
			float temp_output_7_0_g23 = pow( saturate( dotResult8_g24 ) , _Float2 );
			float mulTime8_g23 = _Time.y * _TimeScale;
			float temp_output_4_0_g23 = ( sin( ( ( temp_output_7_0_g23 + mulTime8_g23 ) * 16.0 ) ) * ( 1.0 - temp_output_7_0_g23 ) );
			float temp_output_30_0_g23 = ( 1.0 - saturate( ( ( distance( _WorldSpaceCameraPos , _Vector1 ) + _Bias ) / _ApproachDivid ) ) );
			v.vertex.xyz += ( ( temp_output_4_0_g23 * _WaveOffset ) * temp_output_30_0_g23 );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TextureSample1 = i.uv_texcoord * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
			o.Normal = tex2D( _TextureSample1, uv_TextureSample1 ).rgb;
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			o.Albedo = ( tex2D( _TextureSample0, uv_TextureSample0 ) * _Color0 ).rgb;
			o.Metallic = _Metal;
			o.Smoothness = _Smooth;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.SamplerNode;1;109.0133,-197.2997;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;b79ce5c8b72a7d84e823f8c3283bd02a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-35.97229,33.85257;Inherit;False;Property;_Color0;Color 0;9;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.2941176,0.2196078,0.1372548,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;19;195.0857,126.8953;Inherit;True;Property;_TextureSample1;Texture Sample 1;8;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;500.0277,-140.1474;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;21;9.36108,431.8526;Inherit;False;Property;_Metal;Metal;11;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-55.30566,313.1859;Inherit;False;Property;_Smooth;Smooth;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;32;-36.93738,636.525;Inherit;False;SHAF_Polyroom_Distort;1;;23;e902e14d2ca77d547bfdbec6a3a98c30;0;0;3;FLOAT3;0;FLOAT;19;FLOAT;31
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;727.0697,147.3148;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_Polyroom_Standard;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;22;0;1;0
WireConnection;22;1;23;0
WireConnection;0;0;22;0
WireConnection;0;1;19;0
WireConnection;0;3;21;0
WireConnection;0;4;20;0
WireConnection;0;11;32;0
ASEEND*/
//CHKSM=F26F1CC8ADEBBC7A8BD9A05A80663AD2E8CA7E0A
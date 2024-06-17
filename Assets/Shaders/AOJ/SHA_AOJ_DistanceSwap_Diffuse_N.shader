// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_AOJ_DistanceSwap_Diffuse_N"
{
	Properties
	{
		_Difuse("Difuse", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_SmaskRaidus("SmaskRaidus", Range( 0 , 20)) = 20
		_Bias("Bias", Range( -5 , 5)) = -1
		_ChangeDistance("ChangeDistance", Range( 0 , 20)) = 9.7
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[Toggle]_useTextureMask("useTextureMask", Float) = 0
		[Toggle]_bypassSphereMask("bypassSphereMask", Float) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_Metal("Metal", Range( 0 , 1)) = 0
		_OFfset("OFfset", Vector) = (0,-2,0,0)
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

		uniform float3 _OFfset;
		uniform float _bypassSphereMask;
		uniform float _useTextureMask;
		uniform float _SmaskRaidus;
		uniform float _Bias;
		uniform float _ChangeDistance;
		uniform sampler2D _TextureSample0;
		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _Difuse;
		uniform float4 _Difuse_ST;
		uniform float _Metal;
		uniform float _Smoothness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 _Vector0 = float3(7.063,1.434,-9.331);
			float saferPower7_g7 = abs( ( ( distance( _Vector0 , _WorldSpaceCameraPos ) + _Bias ) / _ChangeDistance ) );
			float temp_output_8_0_g7 = saturate( pow( saferPower7_g7 , 3.0 ) );
			float lerpResult12_g7 = lerp( _SmaskRaidus , 0.0 , temp_output_8_0_g7);
			float3 temp_output_5_0_g8 = ( ( ase_worldPos - _Vector0 ) / lerpResult12_g7 );
			float dotResult8_g8 = dot( temp_output_5_0_g8 , temp_output_5_0_g8 );
			float temp_output_14_0_g7 = pow( saturate( dotResult8_g8 ) , 5.0 );
			float3 lerpResult38 = lerp( float3(0,0,0) , _OFfset , (( _bypassSphereMask )?( temp_output_8_0_g7 ):( (( _useTextureMask )?( temp_output_14_0_g7 ):( saturate( ( ( temp_output_14_0_g7 * 2.0 ) - tex2Dlod( _TextureSample0, float4( ( v.texcoord.xy * float2( 4,4 ) ), 0, 0.0) ).g ) ) )) )));
			v.vertex.xyz += lerpResult38;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Difuse = i.uv_texcoord * _Difuse_ST.xy + _Difuse_ST.zw;
			o.Albedo = tex2D( _Difuse, uv_Difuse ).rgb;
			o.Metallic = _Metal;
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
Node;AmplifyShaderEditor.FunctionNode;41;-734.272,411.6859;Inherit;False;SHAF_DistanceSwitch;2;;7;093fd74bb1b269c45832e583a601c99c;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;39;-795.8293,225.9149;Inherit;False;Property;_OFfset;OFfset;11;0;Create;True;0;0;0;False;0;False;0,-2,0;0,-2,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;48;-850.3964,36.40927;Inherit;False;Constant;_Vector2;Vector 2;6;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;1;-568.7714,-672.2295;Inherit;True;Property;_Difuse;Difuse;0;0;Create;True;0;0;0;False;0;False;-1;None;a0326e11a8a8b4e47b304aef5bd72415;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;40;-528.0488,-440.7384;Inherit;True;Property;_Normal;Normal;1;0;Create;True;0;0;0;False;0;False;-1;None;2141831046412d84aa4959b38b8878f5;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;38;-381.1516,230.9391;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-318.0346,-15.24231;Inherit;False;Property;_Metal;Metal;10;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-339.9091,-129.1402;Inherit;False;Property;_Smoothness;Smoothness;9;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;187.3333,-28.66666;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_AOJ_DistanceSwap_Diffuse_N;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.CommentaryNode;47;-1357.063,-322.9241;Inherit;False;100;100;Comment;0;;1,1,1,1;0;0
WireConnection;38;0;48;0
WireConnection;38;1;39;0
WireConnection;38;2;41;0
WireConnection;0;0;1;0
WireConnection;0;1;40;0
WireConnection;0;3;50;0
WireConnection;0;4;49;0
WireConnection;0;11;38;0
ASEEND*/
//CHKSM=C62C8D351A46DD90A7DD93E12582E80F57251B7D
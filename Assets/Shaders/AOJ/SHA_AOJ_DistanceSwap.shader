// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_AOJ_DistanceSwap"
{
	Properties
	{
		_TextureA("TextureA", 2D) = "white" {}
		_SmaskRaidus("SmaskRaidus", Range( 0 , 20)) = 20
		_Bias("Bias", Range( -5 , 5)) = -1
		_ChangeDistance("ChangeDistance", Range( 0 , 20)) = 9.7
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_TextureB("TextureB", 2D) = "white" {}
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
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _TextureA;
		uniform float4 _TextureA_ST;
		uniform sampler2D _TextureB;
		uniform float4 _TextureB_ST;
		uniform float _SmaskRaidus;
		uniform float _Bias;
		uniform float _ChangeDistance;
		uniform sampler2D _TextureSample0;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TextureA = i.uv_texcoord * _TextureA_ST.xy + _TextureA_ST.zw;
			float2 uv_TextureB = i.uv_texcoord * _TextureB_ST.xy + _TextureB_ST.zw;
			float3 ase_worldPos = i.worldPos;
			float3 _Vector0 = float3(7.063,1.434,-9.331);
			float saferPower7_g5 = abs( ( ( distance( _Vector0 , _WorldSpaceCameraPos ) + _Bias ) / _ChangeDistance ) );
			float lerpResult12_g5 = lerp( _SmaskRaidus , 0.0 , saturate( pow( saferPower7_g5 , 3.0 ) ));
			float3 temp_output_5_0_g6 = ( ( ase_worldPos - _Vector0 ) / lerpResult12_g5 );
			float dotResult8_g6 = dot( temp_output_5_0_g6 , temp_output_5_0_g6 );
			float4 lerpResult6 = lerp( tex2D( _TextureA, uv_TextureA ) , tex2D( _TextureB, uv_TextureB ) , saturate( ( ( pow( saturate( dotResult8_g6 ) , 5.0 ) * 2.0 ) - tex2D( _TextureSample0, ( i.uv_texcoord * float2( 4,4 ) ) ).g ) ));
			o.Albedo = lerpResult6.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.SamplerNode;29;-1018.214,-612.046;Inherit;True;Property;_TextureB;TextureB;6;0;Create;True;0;0;0;False;0;False;-1;None;f5a5902fe5975464e8ff3488819e53a5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-999.3337,-822.1668;Inherit;True;Property;_TextureA;TextureA;0;0;Create;True;0;0;0;False;0;False;-1;None;42fb2a31f50b0f546a160f0a40dbb022;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;37;-637.787,196.6448;Inherit;False;SHAF_DistanceSwitch;1;;5;093fd74bb1b269c45832e583a601c99c;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;4;-619.3334,-195.5001;Inherit;False;Constant;_Color0;Color 0;1;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;5;-676.6666,15.83325;Inherit;False;Constant;_Color1;Color 1;1;0;Create;True;0;0;0;False;0;False;0,0.2902584,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;6;-186,16.49994;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-1565.466,-161.8686;Inherit;False;Property;_FakeRadius;FakeRadius;10;0;Create;True;0;0;0;False;0;False;22;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;7;-1025.252,-249.9892;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;9;-1476.585,-510.6559;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;10;-3322.415,996.8525;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DistanceOpNode;16;-2874.667,1025.5;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-2808.667,1178.167;Inherit;False;Property;_Bias;Bias;8;0;Create;True;0;0;0;False;0;False;-1;-1;-5;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-2411.999,1012.167;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-2418,1212.833;Inherit;False;Property;_ChangeDistance;ChangeDistance;9;0;Create;True;0;0;0;False;0;False;9.7;9.1;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;17;-2178.667,959.5001;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;25;-1951.131,960.1987;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;26;-1738.6,965.314;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-2320.666,573.1667;Inherit;False;Property;_SmaskRaidus;SmaskRaidus;7;0;Create;True;0;0;0;False;0;False;20;13.9;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;31;-966.8809,559.2876;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;36;-888.881,743.9542;Inherit;False;Constant;_Vector1;Vector 1;7;0;Create;True;0;0;0;False;0;False;4,4;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;24;-1901.333,552.1667;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-732.8809,555.2876;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;4,4;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;11;-1248.667,393.1667;Inherit;False;SphereMask;-1;;4;988803ee12caf5f4690caee3c8c4a5bb;0;3;15;FLOAT3;0,0,0;False;14;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;30;-530.2141,497.9542;Inherit;True;Property;_TextureSample0;Texture Sample 0;11;0;Create;True;0;0;0;False;0;False;-1;a11563a4124b96c4db654a9ea7217d2c;a11563a4124b96c4db654a9ea7217d2c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-630.8809,341.9542;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;34;-326.2142,298.6208;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;27;-1529.547,487.2872;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;12;-2841.738,406.3863;Inherit;False;Constant;_Vector0;Vector 0;1;0;Create;True;0;0;0;False;0;False;7.063,1.434,-9.331;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;15;-1579.288,639.0566;Inherit;False;Constant;_Hard;Hard;2;0;Create;True;0;0;0;False;0;False;5;0;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;35;-149.9879,315.5199;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;187.3333,-28.66666;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_AOJ_DistanceSwap;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;1;0
WireConnection;6;1;29;0
WireConnection;6;2;37;0
WireConnection;7;0;9;0
WireConnection;16;0;12;0
WireConnection;16;1;10;0
WireConnection;23;0;16;0
WireConnection;23;1;22;0
WireConnection;17;0;23;0
WireConnection;17;1;18;0
WireConnection;25;0;17;0
WireConnection;26;0;25;0
WireConnection;24;0;14;0
WireConnection;24;2;26;0
WireConnection;32;0;31;0
WireConnection;32;1;36;0
WireConnection;11;15;12;0
WireConnection;11;14;24;0
WireConnection;11;12;15;0
WireConnection;30;1;32;0
WireConnection;33;0;11;0
WireConnection;34;0;33;0
WireConnection;34;1;30;2
WireConnection;35;0;34;0
WireConnection;0;0;6;0
ASEEND*/
//CHKSM=41C4B7F2A0ED0431F4AD9B9439CBC7EFBCB208DD
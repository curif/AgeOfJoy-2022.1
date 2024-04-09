// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/SHA_AOJ_CubeProjector"
{
	Properties
	{
		_Diffuse("Diffuse", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_TextureSample1("Texture Sample 1", CUBE) = "white" {}
		_TimeBoxSpeed("TimeBoxSpeed", Range( 0 , 75)) = 2
		_RotSpeed("RotSpeed", Range( 0 , 6.3)) = 0.25
		_SparkeColorA("SparkeColorA", Color) = (1,0,0,0)
		_SparkeColorB("SparkeColorB", Color) = (0.04078627,1,0,0)
		_SparkeColorC("SparkeColorC", Color) = (0,0.5083904,1,0)
		_Metal("Metal", Range( 0 , 1)) = 0
		_Smoothless("Smoothless", Range( 0 , 1)) = 0
		_DiffuseTint("DiffuseTint", Color) = (1,1,1,0)
		[Toggle(_ISFLOORCEILING_ON)] _isFloorCeiling("isFloorCeiling", Float) = 0
		_Offset("Offset", Vector) = (0,0,0,0)
		_Size("Size", Range( 0.01 , 20)) = 11
		[Toggle(_XYVSZY_ON)] _XYvsZY("XY vs ZY", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _XYVSZY_ON
		#pragma shader_feature_local _ISFLOORCEILING_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			half vertexToFrag41_g1;
			half vertexToFrag43_g1;
			half vertexToFrag44_g1;
			half vertexToFrag42_g1;
			half vertexToFrag5_g2;
		};

		uniform sampler2D _Normal;
		uniform half _Size;
		uniform half2 _Offset;
		uniform half4 _DiffuseTint;
		uniform sampler2D _Diffuse;
		uniform samplerCUBE _TextureSample1;
		uniform half _RotSpeed;
		uniform half4 _SparkeColorA;
		uniform half4 _SparkeColorB;
		uniform half4 _SparkeColorC;
		uniform half _TimeBoxSpeed;
		uniform half _Metal;
		uniform half _Smoothless;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			half mulTime8_g1 = _Time.y * _RotSpeed;
			half temp_output_12_0_g1 = cos( mulTime8_g1 );
			o.vertexToFrag41_g1 = temp_output_12_0_g1;
			half temp_output_9_0_g1 = sin( mulTime8_g1 );
			o.vertexToFrag43_g1 = temp_output_9_0_g1;
			o.vertexToFrag44_g1 = temp_output_9_0_g1;
			o.vertexToFrag42_g1 = temp_output_12_0_g1;
			o.vertexToFrag5_g2 = ( ( sin( ( _Time.y * _TimeBoxSpeed ) ) + 0.0 ) / 2.0 );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			half2 appendResult2 = (half2(ase_worldPos.x , ase_worldPos.y));
			half2 appendResult17 = (half2(ase_worldPos.x , ase_worldPos.z));
			#ifdef _ISFLOORCEILING_ON
				half2 staticSwitch15 = appendResult17;
			#else
				half2 staticSwitch15 = appendResult2;
			#endif
			half2 appendResult12 = (half2(ase_worldPos.z , ase_worldPos.y));
			#ifdef _XYVSZY_ON
				half2 staticSwitch10 = appendResult12;
			#else
				half2 staticSwitch10 = staticSwitch15;
			#endif
			half2 temp_output_6_0 = ( ( staticSwitch10 * _Size ) + _Offset );
			o.Normal = UnpackNormal( tex2D( _Normal, temp_output_6_0 ) );
			o.Albedo = ( _DiffuseTint * tex2D( _Diffuse, temp_output_6_0 ) ).rgb;
			half3 normalizeResult7_g1 = normalize( ( ase_worldPos - half3(11.686,2.743,5.441) ) );
			half3 break10_g1 = normalizeResult7_g1;
			half3 appendResult25_g1 = (half3(( ( break10_g1.x * i.vertexToFrag41_g1 ) + ( break10_g1.z * i.vertexToFrag43_g1 ) ) , break10_g1.y , ( -( break10_g1.x * i.vertexToFrag44_g1 ) + ( break10_g1.z * i.vertexToFrag42_g1 ) )));
			half4 break30_g1 = texCUBE( _TextureSample1, appendResult25_g1 );
			o.Emission = ( saturate( ( ( break30_g1.r * _SparkeColorA ) + ( break30_g1.g * _SparkeColorB ) + ( ( break30_g1.b * _SparkeColorC ) * i.vertexToFrag5_g2 ) ) ) * 0.18 ).rgb;
			o.Metallic = _Metal;
			o.Smoothness = _Smoothless;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.WorldPosInputsNode;1;-1954,-113.1668;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;16;-1949.335,-353.8326;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;11;-1491,149.5003;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;2;-1764,-89.16675;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;17;-1754.001,-311.8327;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;12;-1299.667,173.5003;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;15;-1392.001,-135.8327;Inherit;False;Property;_isFloorCeiling;isFloorCeiling;13;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;10;-1012.668,-84.49968;Inherit;False;Property;_XYvsZY;XY vs ZY;16;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-992.6675,239.5002;Inherit;False;Property;_Size;Size;15;0;Create;True;0;0;0;False;0;False;11;1;0.01;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;7;-682.6675,211.5003;Inherit;False;Property;_Offset;Offset;14;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-774.0009,-6.499756;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;6;-536.0009,2.833557;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;3;-340.6666,-7.166718;Inherit;True;Property;_Diffuse;Diffuse;0;0;Create;True;0;0;0;False;0;False;-1;3f905e2a88606393a83cf48fe0a64b1b;42fb2a31f50b0f546a160f0a40dbb022;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;22;-477.3349,-518.1653;Inherit;False;Property;_DiffuseTint;DiffuseTint;12;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;19;-220.6676,278.1675;Inherit;False;Property;_Metal;Metal;10;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-219.3344,395.5009;Inherit;False;Property;_Smoothless;Smoothless;11;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;20;-468.6682,465.8344;Inherit;True;Property;_Normal;Normal;1;0;Create;True;0;0;0;False;0;False;-1;845ab3559ed627f4aa38cf24bfe7e7e0;f68259b42c9bde044b768e61da8b4cf4;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-128.0016,-350.8321;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;34;-478.0017,254.8338;Inherit;False;SHAF_DiscoBallProjection;2;;1;a2494b936af1b0c4ebd0102a5ccf3c45;0;0;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;121.3334,-2;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Custom/SHA_AOJ_CubeProjector;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;1;1
WireConnection;2;1;1;2
WireConnection;17;0;16;1
WireConnection;17;1;16;3
WireConnection;12;0;11;3
WireConnection;12;1;11;2
WireConnection;15;1;2;0
WireConnection;15;0;17;0
WireConnection;10;1;15;0
WireConnection;10;0;12;0
WireConnection;5;0;10;0
WireConnection;5;1;9;0
WireConnection;6;0;5;0
WireConnection;6;1;7;0
WireConnection;3;1;6;0
WireConnection;20;1;6;0
WireConnection;21;0;22;0
WireConnection;21;1;3;0
WireConnection;0;0;21;0
WireConnection;0;1;20;0
WireConnection;0;2;34;0
WireConnection;0;3;19;0
WireConnection;0;4;18;0
ASEEND*/
//CHKSM=87DD19032F663CD043549852466C204B957124E3
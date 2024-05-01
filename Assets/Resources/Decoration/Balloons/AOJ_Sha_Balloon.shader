// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AOJ_Sha_Balloon"
{
	Properties
	{
		_Divisor("Divisor", Float) = 500
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float4 vertexToFrag23;
			float3 worldPos;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _TextureSample0;
		uniform float _Divisor;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 color1 = IsGammaSpace() ? float4(1,0,0,0) : float4(1,0,0,0);
			float4 color9 = IsGammaSpace() ? float4(0.9014575,1,0,0) : float4(0.7902996,1,0,0);
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float fresnelNdotV6 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode6 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV6, 5.0 ) );
			float4 lerpResult7 = lerp( color1 , color9 , saturate( fresnelNode6 ));
			o.vertexToFrag23 = lerpResult7;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color38 = IsGammaSpace() ? float4(0.3522012,0.3522012,0.3522012,0) : float4(0.1017972,0.1017972,0.1017972,0);
			float3 hsvTorgb4_g1 = RGBToHSV( i.vertexToFrag23.rgb );
			float3 ase_worldPos = i.worldPos;
			float2 appendResult21 = (float2(ase_worldPos.x , ase_worldPos.z));
			float4 tex2DNode20 = tex2D( _TextureSample0, ( appendResult21 / _Divisor ) );
			float3 hsvTorgb8_g1 = HSVToRGB( float3(( hsvTorgb4_g1.x + tex2DNode20.r ),( hsvTorgb4_g1.y + 0.0 ),( hsvTorgb4_g1.z + 0.0 )) );
			float4 lerpResult39 = lerp( color38 , float4( saturate( hsvTorgb8_g1 ) , 0.0 ) , i.vertexColor.b);
			o.Albedo = lerpResult39.rgb;
			o.Metallic = 0.0;
			float lerpResult41 = lerp( 0.0 , 0.82 , i.vertexColor.b);
			o.Smoothness = lerpResult41;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.WorldPosInputsNode;13;-1740.667,142.8334;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FresnelNode;6;-687.3334,442.8334;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-1577.333,477.5001;Inherit;False;Property;_Divisor;Divisor;0;0;Create;True;0;0;0;False;0;False;500;165.9;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;21;-1466.667,169.5002;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;8;-367.9999,543.5001;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-725.3333,-405.1666;Inherit;False;Constant;_Color0;Color 0;0;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;9;-737.9998,-220.5;Inherit;False;Constant;_Color01;Color 01;0;0;Create;True;0;0;0;False;0;False;0.9014575,1,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;14;-1342.667,245.5001;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;7;-435.3332,-293.8333;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;20;-1060,18.83351;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;438e40d0c50cd8742a6f7df5344275a6;438e40d0c50cd8742a6f7df5344275a6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexToFragmentNode;23;-260,-264.4997;Inherit;False;False;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;40;-420.666,210.1672;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;12;-150.6667,-245.1666;Inherit;False;HueShift;-1;;1;9f07e9ddd8ab81c47b3582f22189b65b;0;4;14;COLOR;0,0,0,0;False;15;FLOAT;0;False;16;FLOAT;0;False;17;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;38;-221.3326,-31.83282;Inherit;False;Constant;_Color1;Color 1;4;0;Create;True;0;0;0;False;0;False;0.3522012,0.3522012,0.3522012,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;11;-103.3331,684.1667;Inherit;False;Constant;_Float1;Float 1;0;0;Create;True;0;0;0;False;0;False;0.82;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;34;-1871.904,741.7144;Inherit;False;1;0;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.FmodOpNode;31;-1628.571,748.8813;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;120;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-1809.238,972.3812;Inherit;False;Property;_TimeBoxSpeed1;TimeBoxSpeed;1;0;Create;True;0;0;0;False;0;False;2;2;0;75;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-1417.904,863.7144;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;35;-1251.333,832.167;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;32;-1134.571,815.0479;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;29;-1026.571,821.0477;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;27;-835.3331,1155.5;Inherit;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;0;False;0;False;0,-0.5,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;28;-821.2378,834.381;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-125.3333,497.5001;Inherit;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-593.3331,1094.834;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;25;-1104.666,976.167;Inherit;False;SHAF_TimeBox;2;;2;fe42b26378975cf4dbc110668364facf;0;1;10;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;36;-7.999268,307.5005;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;39;160.6674,-43.16614;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;41;335.334,216.8338;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;544.0001,28.66667;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;AOJ_Sha_Balloon;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;21;0;13;1
WireConnection;21;1;13;3
WireConnection;8;0;6;0
WireConnection;14;0;21;0
WireConnection;14;1;15;0
WireConnection;7;0;1;0
WireConnection;7;1;9;0
WireConnection;7;2;8;0
WireConnection;20;1;14;0
WireConnection;23;0;7;0
WireConnection;12;14;23;0
WireConnection;12;15;20;0
WireConnection;31;0;34;0
WireConnection;30;0;31;0
WireConnection;30;1;33;0
WireConnection;35;0;20;3
WireConnection;35;1;30;0
WireConnection;32;0;35;0
WireConnection;29;0;32;0
WireConnection;28;0;29;0
WireConnection;26;0;28;0
WireConnection;26;1;27;0
WireConnection;39;0;38;0
WireConnection;39;1;12;0
WireConnection;39;2;40;3
WireConnection;41;1;11;0
WireConnection;41;2;40;3
WireConnection;0;0;39;0
WireConnection;0;3;10;0
WireConnection;0;4;41;0
ASEEND*/
//CHKSM=AD43D2FAA8510CD2BDBDA3802F71B13C2FE393F5
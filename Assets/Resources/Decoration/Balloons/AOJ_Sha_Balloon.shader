// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AOJ_Sha_Balloon"
{
	Properties
	{
		_Size_Color("Size_Color", Float) = 500
		_Size_Movement("Size_Movement", Float) = 500
		_Smooth("Smooth", Range( 0 , 1)) = 0.82
		_TimeBoxSpeedMin("TimeBoxSpeedMin", Range( 0 , 75)) = 2
		_BalloonOffset("BalloonOffset", Vector) = (0,-0.5,0,0)
		_TimeBoxSpeedMax("TimeBoxSpeedMax", Range( 0 , 75)) = 2
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
			float3 worldPos;
			float4 vertexToFrag23;
			float4 vertexColor : COLOR;
		};

		uniform float _TimeBoxSpeedMin;
		uniform float _TimeBoxSpeedMax;
		uniform float _Size_Movement;
		uniform float3 _BalloonOffset;
		uniform float _Size_Color;
		uniform float _Smooth;


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
			float mulTime34 = _Time.y * 0.05;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult65 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 break71 = ( ( sin( ( appendResult65 / _Size_Movement ) ) + float2( 1,1 ) ) / float2( 2,2 ) );
			float4 appendResult72 = (float4(break71.x , 0.0 , break71.y , 0.0));
			float lerpResult52 = lerp( _TimeBoxSpeedMin , _TimeBoxSpeedMax , appendResult72.x);
			v.vertex.xyz += ( ( ( sin( ( fmod( mulTime34 , 120.0 ) * lerpResult52 ) ) + 1.0 ) / 2.0 ) * _BalloonOffset );
			v.vertex.w = 1;
			float4 color1 = IsGammaSpace() ? float4(1,0,0,0) : float4(1,0,0,0);
			float4 color9 = IsGammaSpace() ? float4(0.9014575,1,0,0) : float4(0.7902996,1,0,0);
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
			float2 break63 = ( ( sin( ( appendResult21 / _Size_Color ) ) + float2( 1,1 ) ) / float2( 2,2 ) );
			float4 appendResult62 = (float4(break63.x , 0.0 , break63.y , 0.0));
			float3 hsvTorgb8_g1 = HSVToRGB( float3(( hsvTorgb4_g1.x + appendResult62.x ),( hsvTorgb4_g1.y + 0.0 ),( hsvTorgb4_g1.z + 0.0 )) );
			float4 lerpResult39 = lerp( color38 , float4( saturate( hsvTorgb8_g1 ) , 0.0 ) , i.vertexColor.b);
			float4 temp_output_42_0 = ( lerpResult39 * 0.05 );
			o.Albedo = temp_output_42_0.rgb;
			o.Emission = temp_output_42_0.rgb;
			o.Metallic = 0.0;
			float lerpResult41 = lerp( 0.0 , _Smooth , i.vertexColor.b);
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
Node;AmplifyShaderEditor.WorldPosInputsNode;64;-3031.183,1109.723;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;66;-2888.516,1278.389;Inherit;False;Property;_Size_Movement;Size_Movement;1;0;Create;True;0;0;0;False;0;False;500;100;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;65;-2815.851,1123.723;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;67;-2633.184,1212.389;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldPosInputsNode;13;-2237.333,122.1667;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SinOpNode;68;-2420.516,1111.724;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;21;-1963.334,148.8335;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-2094.666,290.8335;Inherit;False;Property;_Size_Color;Size_Color;0;0;Create;True;0;0;0;False;0;False;500;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;-2273.849,1147.723;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;14;-1839.334,224.8335;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;70;-2155.849,1142.39;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SinOpNode;59;-1626.666,124.1676;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;71;-1920.515,1184.39;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.FresnelNode;6;-1492,-165.8333;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;34;-1807.237,693.0477;Inherit;False;1;0;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;60;-1479.999,160.1675;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-2224.665,998.6676;Inherit;False;Property;_TimeBoxSpeedMax;TimeBoxSpeedMax;5;0;Create;True;0;0;0;False;0;False;2;50;0;75;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;72;-1785.182,1182.39;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-2216.571,885.048;Inherit;False;Property;_TimeBoxSpeedMin;TimeBoxSpeedMin;3;0;Create;True;0;0;0;False;0;False;2;13.8;0;75;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;52;-1767.998,932.6674;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FmodOpNode;31;-1588.571,728.8813;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;120;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-1140.667,-578.4999;Inherit;False;Constant;_Color0;Color 0;0;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;9;-1153.333,-393.8333;Inherit;False;Constant;_Color01;Color 01;0;0;Create;True;0;0;0;False;0;False;0.9014575,1,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;8;-1150,-173.1666;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;61;-1195.332,221.5009;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;7;-815.3332,-341.8333;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-1366.571,829.0477;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;63;-959.9988,263.5009;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SinOpNode;32;-1134.571,815.0479;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;23;-606,-335.1663;Inherit;False;False;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;62;-824.6655,261.5008;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.VertexColorNode;40;-420.666,210.1672;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;12;-150.6667,-245.1666;Inherit;False;HueShift;-1;;1;9f07e9ddd8ab81c47b3582f22189b65b;0;4;14;COLOR;0,0,0,0;False;15;FLOAT;0;False;16;FLOAT;0;False;17;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;38;-221.3326,-31.83282;Inherit;False;Constant;_Color1;Color 1;4;0;Create;True;0;0;0;False;0;False;0.3522012,0.3522012,0.3522012,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;49;-966.665,792.6674;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-521.9998,451.5;Inherit;False;Property;_Smooth;Smooth;2;0;Create;True;0;0;0;False;0;False;0.82;0.921;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;28;-790.571,789.7143;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;39;160.6674,-43.16614;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;43;312.6683,98.66745;Inherit;False;Constant;_Float2;Float 2;4;0;Create;True;0;0;0;False;0;False;0.05;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;27;-835.3331,1155.5;Inherit;False;Property;_BalloonOffset;BalloonOffset;4;0;Create;True;0;0;0;False;0;False;0,-0.5,0;0.1,-0.05,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;36;-7.999268,307.5005;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;41;335.334,216.8338;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;388.0018,-115.9992;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.1006289;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;10;624.6667,125.5;Inherit;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-581.9999,1010.167;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;928.6669,57.33333;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;AOJ_Sha_Balloon;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;65;0;64;1
WireConnection;65;1;64;3
WireConnection;67;0;65;0
WireConnection;67;1;66;0
WireConnection;68;0;67;0
WireConnection;21;0;13;1
WireConnection;21;1;13;3
WireConnection;69;0;68;0
WireConnection;14;0;21;0
WireConnection;14;1;15;0
WireConnection;70;0;69;0
WireConnection;59;0;14;0
WireConnection;71;0;70;0
WireConnection;60;0;59;0
WireConnection;72;0;71;0
WireConnection;72;2;71;1
WireConnection;52;0;33;0
WireConnection;52;1;51;0
WireConnection;52;2;72;0
WireConnection;31;0;34;0
WireConnection;8;0;6;0
WireConnection;61;0;60;0
WireConnection;7;0;1;0
WireConnection;7;1;9;0
WireConnection;7;2;8;0
WireConnection;30;0;31;0
WireConnection;30;1;52;0
WireConnection;63;0;61;0
WireConnection;32;0;30;0
WireConnection;23;0;7;0
WireConnection;62;0;63;0
WireConnection;62;2;63;1
WireConnection;12;14;23;0
WireConnection;12;15;62;0
WireConnection;49;0;32;0
WireConnection;28;0;49;0
WireConnection;39;0;38;0
WireConnection;39;1;12;0
WireConnection;39;2;40;3
WireConnection;41;1;11;0
WireConnection;41;2;40;3
WireConnection;42;0;39;0
WireConnection;42;1;43;0
WireConnection;26;0;28;0
WireConnection;26;1;27;0
WireConnection;0;0;42;0
WireConnection;0;2;42;0
WireConnection;0;3;10;0
WireConnection;0;4;41;0
WireConnection;0;11;26;0
ASEEND*/
//CHKSM=8B21F142438B613740B4BDE5BB7D3390D653C9A9
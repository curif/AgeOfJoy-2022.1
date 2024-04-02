// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/Flag"
{
	Properties
	{
		_Texture0("Texture 0", 2D) = "white" {}
		_SmallWaveWPO("SmallWaveWPO", Vector) = (0,0,0,0)
		_TextureSample2("Texture Sample 2", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _Texture0;
		uniform half3 _SmallWaveWPO;
		uniform sampler2D _TextureSample2;
		uniform half4 _TextureSample2_ST;


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
			half2 panner48 = ( 1.0 * _Time.y * float2( 0.3,0 ) + v.texcoord.xy);
			half2 vertexToFrag83 = panner48;
			half2 panner64 = ( 1.0 * _Time.y * float2( 0.25,0 ) + v.texcoord.xy);
			half2 vertexToFrag84 = panner64;
			v.vertex.xyz += ( ( ( tex2Dlod( _Texture0, float4( vertexToFrag83, 0, 0.0) ) * half4( half3(0,0,1) , 0.0 ) ) * ( v.texcoord.xy.x * ( 1.0 - v.texcoord.xy.x ) ) ) + ( ( tex2Dlod( _Texture0, float4( vertexToFrag84, 0, 0.0) ) * half4( _SmallWaveWPO , 0.0 ) ) * v.color.r ) ).rgb;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TextureSample2 = i.uv_texcoord * _TextureSample2_ST.xy + _TextureSample2_ST.zw;
			float3 hsvTorgb4_g2 = RGBToHSV( half4( half3(1,0,0) , 0.0 ).rgb );
			float3 hsvTorgb8_g2 = HSVToRGB( float3(( hsvTorgb4_g2.x + ( i.vertexColor.b * 0.75 ) ),( hsvTorgb4_g2.y + 0.0 ),( hsvTorgb4_g2.z + 0.0 )) );
			o.Albedo = ( ( tex2D( _TextureSample2, uv_TextureSample2 ) * ( 1.0 - i.vertexColor.g ) ) + half4( ( i.vertexColor.r * saturate( hsvTorgb8_g2 ) ) , 0.0 ) ).rgb;
			o.Metallic = 0.0;
			o.Smoothness = 0.0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.TextureCoordinatesNode;54;-152.4125,967.4088;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;63;-34.73849,1415.599;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;48;253.0405,974.3834;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0.3,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;64;367.3811,1408.574;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0.25,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;1464.721,870.4308;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;67;413.9211,367.3877;Inherit;True;Property;_Texture0;Texture 0;0;0;Create;True;0;0;0;False;0;False;None;a11563a4124b96c4db654a9ea7217d2c;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.VertexToFragmentNode;83;663.1827,857.0206;Inherit;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexToFragmentNode;84;732.1827,1385.021;Inherit;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexColorNode;60;1308.613,86.67334;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;19;1709.164,926.5361;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;49;934.3773,512.4991;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;a11563a4124b96c4db654a9ea7217d2c;a11563a4124b96c4db654a9ea7217d2c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;55;1302.084,666.3643;Inherit;False;Constant;_BigWaveWPO;BigWaveWPO;2;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;68;1313.252,1289.574;Inherit;False;Property;_SmallWaveWPO;SmallWaveWPO;1;0;Create;True;0;0;0;False;0;False;0,0,0;0.2,0.01,0.01;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;65;1083.718,1051.69;Inherit;True;Property;_TextureSample1;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;a11563a4124b96c4db654a9ea7217d2c;a11563a4124b96c4db654a9ea7217d2c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;74;1634.253,-311.7592;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;62;1276.121,-115.3338;Inherit;False;Constant;_Vector0;Vector 0;1;0;Create;True;0;0;0;False;0;False;1,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;1553.587,228.9072;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.75;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;1472.38,510.5876;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;1876.758,827.7216;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;1522.252,1091.574;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;71;1759.586,1334.908;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;80;1890.92,-239.0929;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;76;1223.253,-835.0928;Inherit;True;Property;_TextureSample2;Texture Sample 2;2;0;Create;True;0;0;0;False;0;False;-1;None;096a8a0c0979cef41b973167047d18e9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;61;1728.159,72.41699;Inherit;False;HueShift;-1;;2;9f07e9ddd8ab81c47b3582f22189b65b;0;4;14;COLOR;0,0,0,0;False;15;FLOAT;0;False;16;FLOAT;0;False;17;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;1712.878,469.9692;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;1955.586,1167.574;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;1990.919,-22.42581;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;1900.253,-422.426;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;58;2152.529,475.1622;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;72;2020.253,175.5743;Inherit;False;Constant;_meta;meta;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;73;2001.586,269.5744;Inherit;False;Constant;_Smoov;Smoov;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;79;2182.919,-103.0926;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2487.276,-28.51797;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;AgeOfJoy/Flag;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;48;0;54;0
WireConnection;64;0;63;0
WireConnection;83;0;48;0
WireConnection;84;0;64;0
WireConnection;19;0;18;1
WireConnection;49;0;67;0
WireConnection;49;1;83;0
WireConnection;65;0;67;0
WireConnection;65;1;84;0
WireConnection;81;0;60;3
WireConnection;51;0;49;0
WireConnection;51;1;55;0
WireConnection;21;0;18;1
WireConnection;21;1;19;0
WireConnection;69;0;65;0
WireConnection;69;1;68;0
WireConnection;80;0;74;2
WireConnection;61;14;62;0
WireConnection;61;15;81;0
WireConnection;56;0;51;0
WireConnection;56;1;21;0
WireConnection;70;0;69;0
WireConnection;70;1;71;1
WireConnection;75;0;74;1
WireConnection;75;1;61;0
WireConnection;77;0;76;0
WireConnection;77;1;80;0
WireConnection;58;0;56;0
WireConnection;58;1;70;0
WireConnection;79;0;77;0
WireConnection;79;1;75;0
WireConnection;0;0;79;0
WireConnection;0;3;72;0
WireConnection;0;4;73;0
WireConnection;0;11;58;0
ASEEND*/
//CHKSM=A3594F416A24E2A92992534CF96D8C185081A2A8
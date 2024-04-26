// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_FadeSphere"
{
	Properties
	{
		_FadeAmount("FadeAmount", Range( 0 , 1)) = 0
		_ColorShiftMin("ColorShiftMin", Range( 0 , 1)) = 0
		_ColorShiftMax("ColorShiftMax", Range( 0 , 1)) = 0
		_Scale("Scale", Vector) = (0,0,0,0)
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _FadeAmount;
		uniform float3 _Scale;
		uniform sampler2D _TextureSample0;
		uniform float _ColorShiftMin;
		uniform float _ColorShiftMax;


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
			float lerpResult6 = lerp( -1.0 , 1.0 , _FadeAmount);
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( ( saturate( ( v.color.r + lerpResult6 ) ) * ase_vertexNormal ) * _Scale );
			v.vertex.w = 1;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 hsvTorgb4_g1 = RGBToHSV( float4( float3(0,0,1) , 0.0 ).rgb );
			float lerpResult19 = lerp( _ColorShiftMin , _ColorShiftMax , i.vertexColor.r);
			float3 hsvTorgb8_g1 = HSVToRGB( float3(( hsvTorgb4_g1.x + lerpResult19 ),( hsvTorgb4_g1.y + lerpResult19 ),( hsvTorgb4_g1.z + lerpResult19 )) );
			o.Emission = ( ( tex2D( _TextureSample0, i.uv_texcoord ).g * saturate( hsvTorgb8_g1 ) ) * saturate( pow( _FadeAmount , 1.5 ) ) );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.RangedFloatNode;7;-1542,692.1667;Inherit;False;Property;_FadeAmount;FadeAmount;0;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;13;-1168.667,658.1669;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;1;-1734,8.166874;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;6;-588.6667,442.1668;Inherit;False;3;0;FLOAT;-1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-1626,383.8338;Inherit;False;Property;_ColorShiftMax;ColorShiftMax;4;0;Create;True;0;0;0;False;0;False;0;0.283;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-1737.333,280.5004;Inherit;False;Property;_ColorShiftMin;ColorShiftMin;3;0;Create;True;0;0;0;False;0;False;0;0.535;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-636,171.5001;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;21;-1396,-509.1664;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;17;-1597.333,-204.4996;Inherit;False;Constant;_Vector0;Vector 0;2;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;19;-1326,-28.49969;Inherit;False;3;0;FLOAT;-0.5;False;1;FLOAT;0.5;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;39;-1115.333,753.834;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;2;-1140.667,416.1668;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;9;-418.667,166.8337;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;18;-1160,-217.1663;Inherit;False;HueShift;-1;;1;9f07e9ddd8ab81c47b3582f22189b65b;0;4;14;COLOR;0,0,0,0;False;15;FLOAT;0;False;16;FLOAT;0;False;17;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;20;-780.6665,-369.1664;Inherit;True;Property;_TextureSample0;Texture Sample 0;6;0;Create;True;0;0;0;False;0;False;-1;None;290a83dea9f03cf4c8d09cd8db33fdd7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;14;-921.3336,760.167;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;33;-242.0004,-48.49966;Inherit;False;228;186.3333;fade;1;16;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;12;-276.667,555.5003;Inherit;False;Property;_Scale;Scale;5;0;Create;True;0;0;0;False;0;False;0,0,0;-0.1,20,-0.1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SaturateNode;15;-678.0002,746.8337;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-417.9998,-230.1662;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-332.6665,288.167;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;37;-1486.667,121.1671;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-67.33374,308.8337;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;-1,-1,-1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-192.0004,1.500337;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;38;-1492.667,551.1673;Inherit;False;SHAF_TimeBox;1;;2;fe42b26378975cf4dbc110668364facf;0;1;10;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;86,1.333344;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;SHA_FadeSphere;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;13;0;7;0
WireConnection;6;2;13;0
WireConnection;8;0;1;1
WireConnection;8;1;6;0
WireConnection;19;0;35;0
WireConnection;19;1;36;0
WireConnection;19;2;1;1
WireConnection;39;0;7;0
WireConnection;9;0;8;0
WireConnection;18;14;17;0
WireConnection;18;15;19;0
WireConnection;18;16;19;0
WireConnection;18;17;19;0
WireConnection;20;1;21;0
WireConnection;14;0;39;0
WireConnection;15;0;14;0
WireConnection;34;0;20;2
WireConnection;34;1;18;0
WireConnection;3;0;9;0
WireConnection;3;1;2;0
WireConnection;37;0;1;1
WireConnection;10;0;3;0
WireConnection;10;1;12;0
WireConnection;16;0;34;0
WireConnection;16;1;15;0
WireConnection;0;2;16;0
WireConnection;0;11;10;0
ASEEND*/
//CHKSM=E5B71B319652179EE74635145A82ABBF77E8940D
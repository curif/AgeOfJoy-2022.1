// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/SHA_AOJ_FishScreenSaver"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_TextureSample1("Texture Sample 0", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _TextureSample0;
		uniform sampler2D _TextureSample1;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 color14 = IsGammaSpace() ? float4(0,0.3684628,1,0) : float4(0,0.111828,1,0);
			float4 color18 = IsGammaSpace() ? float4(1,0,0.882441,0) : float4(1,0,0.7531122,0);
			float temp_output_32_0 = ( ( saturate( sin( ( i.uv_texcoord.y + _CosTime.w ) ) ) + 0.0 ) * 0.5 );
			float4 lerpResult17 = lerp( color14 , color18 , temp_output_32_0);
			float4 color21 = IsGammaSpace() ? float4(0.2458851,0,0.3144653,0) : float4(0.04924426,0,0.08060668,0);
			float4 color20 = IsGammaSpace() ? float4(0.3553458,1,0.3815483,0) : float4(0.1036941,1,0.1203017,0);
			float lerpResult36 = lerp( 2.0 , 6.0 , temp_output_32_0);
			float2 appendResult34 = (float2(lerpResult36 , lerpResult36));
			float2 uv_TexCoord2 = i.uv_texcoord * appendResult34;
			float cos37 = cos( _CosTime.w );
			float sin37 = sin( _CosTime.w );
			float2 rotator37 = mul( uv_TexCoord2 - float2( 0,0 ) , float2x2( cos37 , -sin37 , sin37 , cos37 )) + float2( 0,0 );
			float2 panner3 = ( 1.0 * _Time.y * float2( 0.5,0 ) + rotator37);
			float4 tex2DNode1 = tex2D( _TextureSample0, panner3 );
			float4 lerpResult19 = lerp( color21 , color20 , tex2DNode1.r);
			float4 lerpResult8 = lerp( lerpResult17 , lerpResult19 , tex2DNode1.b);
			float2 panner6 = ( 1.0 * _Time.y * float2( -0.2,-0.5 ) + i.uv_texcoord);
			float4 color23 = IsGammaSpace() ? float4(0.07232696,0.6923644,1,0) : float4(0.00625177,0.4371918,1,0);
			o.Emission = ( lerpResult8 + ( tex2D( _TextureSample1, panner6 ).g * color23 ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.TextureCoordinatesNode;12;-2014.557,884.7319;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CosTime;27;-1976.557,1114.065;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;25;-1745.224,993.3986;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;29;-1588.557,885.3986;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;28;-1450.557,888.0651;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;31;-1229.224,874.7319;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-1047.89,915.3986;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;36;-2814.557,-39.93488;Inherit;False;3;0;FLOAT;2;False;1;FLOAT;6;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;34;-2567.224,-112.6015;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-2332.666,-69.16664;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CosTime;35;-2973.224,244.7319;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RotatorNode;37;-2099.89,-8.601501;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;3;-1917.333,-32.49997;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.5,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-1518.557,241.7319;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;14;-1348.557,384.065;Inherit;False;Constant;_Color0;Color 0;2;0;Create;True;0;0;0;False;0;False;0,0.3684628,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;18;-1324.557,567.3985;Inherit;False;Constant;_Color1;Color 1;2;0;Create;True;0;0;0;False;0;False;1,0,0.882441,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-1619.333,-71.83331;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;073284018509770429ab37bc525ce38a;073284018509770429ab37bc525ce38a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;20;-1308.557,-505.9348;Inherit;False;Constant;_Color2;Color 2;2;0;Create;True;0;0;0;False;0;False;0.3553458,1,0.3815483,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;21;-1315.891,-297.9348;Inherit;False;Constant;_Color3;Color 3;2;0;Create;True;0;0;0;False;0;False;0.2458851,0,0.3144653,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;6;-1238.557,274.3986;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.2,-0.5;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;19;-957.224,-118.6015;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;4;-929.2233,228.7319;Inherit;True;Property;_TextureSample1;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;073284018509770429ab37bc525ce38a;073284018509770429ab37bc525ce38a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-599.8905,426.7319;Inherit;False;Constant;_Color4;Color 4;2;0;Create;True;0;0;0;False;0;False;0.07232696,0.6923644,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;17;-834.5572,414.732;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;8;-441.2233,-54.60141;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-405.2239,258.7319;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;33;-2529.89,61.3985;Inherit;False;Constant;_Vector0;Vector 0;2;0;Create;True;0;0;0;False;0;False;1,2;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;15;-70.55676,103.3985;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;147.3333,23.33334;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Custom/SHA_AOJ_FishScreenSaver;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;25;0;12;2
WireConnection;25;1;27;4
WireConnection;29;0;25;0
WireConnection;28;0;29;0
WireConnection;31;0;28;0
WireConnection;32;0;31;0
WireConnection;36;2;32;0
WireConnection;34;0;36;0
WireConnection;34;1;36;0
WireConnection;2;0;34;0
WireConnection;37;0;2;0
WireConnection;37;2;35;4
WireConnection;3;0;37;0
WireConnection;1;1;3;0
WireConnection;6;0;5;0
WireConnection;19;0;21;0
WireConnection;19;1;20;0
WireConnection;19;2;1;1
WireConnection;4;1;6;0
WireConnection;17;0;14;0
WireConnection;17;1;18;0
WireConnection;17;2;32;0
WireConnection;8;0;17;0
WireConnection;8;1;19;0
WireConnection;8;2;1;3
WireConnection;22;0;4;2
WireConnection;22;1;23;0
WireConnection;15;0;8;0
WireConnection;15;1;22;0
WireConnection;0;2;15;0
ASEEND*/
//CHKSM=00AD8D4E6920AD89A002A0F5725C2456FBD291DB
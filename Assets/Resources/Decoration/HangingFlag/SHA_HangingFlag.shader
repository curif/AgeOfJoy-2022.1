// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_HangingFlag"
{
	Properties
	{
		_Texture0("Texture 0", 2D) = "white" {}
		_BigWaveWPO("BigWaveWPO", Vector) = (0,0,1,0)
		_PannerSpeed("PannerSpeed", Vector) = (0.3,0.2,0,0)
		_Texture1("Texture 1", 2D) = "white" {}
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
			half ASEIsFrontFacing : VFACE;
		};

		uniform sampler2D _Texture0;
		uniform half2 _PannerSpeed;
		uniform half3 _BigWaveWPO;
		uniform sampler2D _Texture1;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			half2 panner4 = ( 1.0 * _Time.y * _PannerSpeed + v.texcoord.xy);
			v.vertex.xyz += ( ( tex2Dlod( _Texture0, float4( panner4, 0, 0.0) ).b * _BigWaveWPO ) * v.color.r );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			half4 lerpResult22 = lerp( tex2D( _Texture1, ( i.uv_texcoord * float2( -1,1 ) ) ) , tex2D( _Texture1, i.uv_texcoord ) , ( ( (i.ASEIsFrontFacing > 0 ? +1 : -1 ) + 1.0 ) / 2.0 ));
			o.Albedo = lerpResult22.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-2063.647,586.0787;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;20;-1970.023,873.8446;Inherit;False;Property;_PannerSpeed;PannerSpeed;2;0;Create;True;0;0;0;False;0;False;0.3,0.2;0.05,-0.2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TwoSidedSign;21;-1041.357,303.845;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;32;-1820.69,57.84494;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;4;-1699.194,593.0533;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0.3,0.02;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;18;-1735.567,297.7243;Inherit;True;Property;_Texture0;Texture 0;0;0;Create;True;0;0;0;False;0;False;None;a11563a4124b96c4db654a9ea7217d2c;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;23;-1540.024,-180.8218;Inherit;True;Property;_Texture1;Texture 1;3;0;Create;True;0;0;0;False;0;False;3a0dc25f345d6d942aa6dadbde799a36;4ae7cd56e1b1cfc45b7072a92f22a132;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;29;-718.0234,217.8449;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;33;-1875.357,-115.4884;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-1416.024,80.51157;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;-1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;7;-1321.858,417.1691;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;a11563a4124b96c4db654a9ea7217d2c;a11563a4124b96c4db654a9ea7217d2c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;8;-1098.151,660.0343;Inherit;False;Property;_BigWaveWPO;BigWaveWPO;1;0;Create;True;0;0;0;False;0;False;0,0,1;0.05,0,0.05;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;19;-1132.023,-168.8223;Inherit;True;Property;_TextureSample2;Texture Sample 2;1;0;Create;True;0;0;0;False;0;False;-1;3006adc7839db01be9c8c9e7acd22c0d;3a0dc25f345d6d942aa6dadbde799a36;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;30;-530.0234,260.5116;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;25;-1122.024,75.17825;Inherit;True;Property;_TextureSample3;Texture Sample 2;1;0;Create;True;0;0;0;False;0;False;-1;3006adc7839db01be9c8c9e7acd22c0d;3a0dc25f345d6d942aa6dadbde799a36;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-885.855,520.2576;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;14;-783.6489,701.5779;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;22;-458.6902,25.17822;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-542.6489,527.2439;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;526.0001,42;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_HangingFlag;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;1;0
WireConnection;4;2;20;0
WireConnection;29;0;21;0
WireConnection;34;0;32;0
WireConnection;7;0;18;0
WireConnection;7;1;4;0
WireConnection;19;0;23;0
WireConnection;19;1;33;0
WireConnection;30;0;29;0
WireConnection;25;0;23;0
WireConnection;25;1;34;0
WireConnection;11;0;7;3
WireConnection;11;1;8;0
WireConnection;22;0;25;0
WireConnection;22;1;19;0
WireConnection;22;2;30;0
WireConnection;16;0;11;0
WireConnection;16;1;14;1
WireConnection;0;0;22;0
WireConnection;0;11;16;0
ASEEND*/
//CHKSM=3E4A76028035F3A4B171ADFEC2BC8CD2B8CB50CF
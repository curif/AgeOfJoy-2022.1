// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_Pool_Pool"
{
	Properties
	{
		_TextureSample2("Texture Sample 1", 2D) = "bump" {}
		_TextureSample0("Texture Sample 0", 2D) = "bump" {}
		_Texture0("Texture 0", 2D) = "white" {}
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
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _TextureSample2;
		uniform half4 _TextureSample2_ST;
		uniform sampler2D _Texture0;
		uniform half4 _Texture0_ST;
		uniform sampler2D _TextureSample0;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TextureSample2 = i.uv_texcoord * _TextureSample2_ST.xy + _TextureSample2_ST.zw;
			o.Normal = UnpackNormal( tex2D( _TextureSample2, uv_TextureSample2 ) );
			float2 uv_Texture0 = i.uv_texcoord * _Texture0_ST.xy + _Texture0_ST.zw;
			o.Albedo = tex2D( _Texture0, uv_Texture0 ).rgb;
			float2 uv_TexCoord9 = i.uv_texcoord * float2( 2,2 );
			half2 panner14 = ( 1.0 * _Time.y * float2( 0.1,0.05 ) + uv_TexCoord9);
			half3 tex2DNode13 = UnpackNormal( tex2D( _TextureSample0, panner14 ) );
			half2 appendResult17 = (half2(tex2DNode13.r , tex2DNode13.g));
			half3 temp_cast_1 = (( i.vertexColor.r * ( tex2D( _Texture0, ( i.uv_texcoord + ( appendResult17 * float2( 0.05,0.05 ) ) ) ).a * 0.2 ) )).xxx;
			o.Emission = temp_cast_1;
			o.Metallic = 0.0;
			o.Smoothness = 0.75;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-2266,156.6668;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;2,2;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;14;-2029,156.5;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.1,0.05;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;13;-1825,148.5;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;2e7df1fb15fc2af439af22ebf06d4591;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;17;-1506.334,219.1667;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;15;-1612.334,-28.83334;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1348.334,235.1667;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.05,0.05;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;11;-1632.334,-348.1666;Inherit;True;Property;_Texture0;Texture 0;2;0;Create;True;0;0;0;False;0;False;None;158f3f24c891e1e4db2b45a5d32ef8a4;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-1363.001,27.16666;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;10;-1183.667,-38.83322;Inherit;True;Property;_TextureSample3;Texture Sample 1;0;0;Create;True;0;0;0;False;0;False;-1;70b3059f66f7eb04085ace1304cb093e;158f3f24c891e1e4db2b45a5d32ef8a4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;2;-560.3333,-94.83334;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-805.6672,69.16669;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-730.3337,-327.5002;Inherit;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;0;False;0;False;-1;70b3059f66f7eb04085ace1304cb093e;158f3f24c891e1e4db2b45a5d32ef8a4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;6;-268.3337,160.4999;Inherit;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-265.0006,248.5;Inherit;False;Constant;_Float1;Float 0;2;0;Create;True;0;0;0;False;0;False;0.75;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-245.0005,22.49994;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-853.0007,253.1666;Inherit;True;Property;_TextureSample2;Texture Sample 1;0;0;Create;True;0;0;0;False;0;False;-1;8cc910b7d02a4f242ae375d2caffdec9;8cc910b7d02a4f242ae375d2caffdec9;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_Pool_Pool;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;14;0;9;0
WireConnection;13;1;14;0
WireConnection;17;0;13;1
WireConnection;17;1;13;2
WireConnection;18;0;17;0
WireConnection;16;0;15;0
WireConnection;16;1;18;0
WireConnection;10;0;11;0
WireConnection;10;1;16;0
WireConnection;19;0;10;4
WireConnection;4;0;11;0
WireConnection;8;0;2;1
WireConnection;8;1;19;0
WireConnection;0;0;4;0
WireConnection;0;1;5;0
WireConnection;0;2;8;0
WireConnection;0;3;6;0
WireConnection;0;4;7;0
ASEEND*/
//CHKSM=BA345CBC9196FD02CBB1B412CE332CA01FE9C155
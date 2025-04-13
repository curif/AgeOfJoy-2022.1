// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SH_DrapePipe"
{
	Properties
	{
		_D("D", 2D) = "white" {}
		_N("N", 2D) = "bump" {}
		_ORM("ORM", 2D) = "white" {}
		_ClothTint("ClothTint", Color) = (1,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _N;
		uniform half4 _N_ST;
		uniform sampler2D _D;
		uniform half4 _D_ST;
		uniform half4 _ClothTint;
		uniform sampler2D _ORM;
		uniform half4 _ORM_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_N = i.uv_texcoord * _N_ST.xy + _N_ST.zw;
			o.Normal = UnpackNormal( tex2D( _N, uv_N ) );
			float2 uv_D = i.uv_texcoord * _D_ST.xy + _D_ST.zw;
			half4 lerpResult7 = lerp( half4( half3(1,1,1) , 0.0 ) , ( tex2D( _D, uv_D ) * _ClothTint ) , i.vertexColor.g);
			o.Albedo = lerpResult7.rgb;
			float2 uv_ORM = i.uv_texcoord * _ORM_ST.xy + _ORM_ST.zw;
			half4 tex2DNode3 = tex2D( _ORM, uv_ORM );
			o.Metallic = tex2DNode3.b;
			o.Smoothness = ( 1.0 - tex2DNode3.g );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.SamplerNode;1;-676,-509.5;Inherit;True;Property;_D;D;0;0;Create;True;0;0;0;False;0;False;-1;8f801125e95667f42aedf91274998d88;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-330,-324.5;Inherit;False;Property;_ClothTint;ClothTint;3;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;6;-238,-146.5;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-13,-465.5;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;3;-718,205.5;Inherit;True;Property;_ORM;ORM;2;0;Create;True;0;0;0;False;0;False;-1;a112ebebd5c25b34e9caf14b3f5d0d87;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;8;-517,-139.5;Inherit;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;0;False;0;False;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;7;48,-131.5;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;4;-289,398.5;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-590,21.5;Inherit;True;Property;_N;N;1;0;Create;True;0;0;0;False;0;False;-1;ef171ce165cc01e4b99a6c95e1d5a21a;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;333,16;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SH_DrapePipe;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;1;0
WireConnection;5;1;11;0
WireConnection;7;0;8;0
WireConnection;7;1;5;0
WireConnection;7;2;6;2
WireConnection;4;0;3;2
WireConnection;0;0;7;0
WireConnection;0;1;2;0
WireConnection;0;3;3;3
WireConnection;0;4;4;0
ASEEND*/
//CHKSM=48236AC53E544DC688C860855DFF914441F59595
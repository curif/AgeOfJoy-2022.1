// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_AOJ_TV_Panner"
{
	Properties
	{
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.4927534
		_TimeBoxSpeed("TimeBoxSpeed", Range( 0 , 75)) = 2
		_TextColor("TextColor", Color) = (0.9500964,1,0,0)
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
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			half vertexToFrag5_g2;
		};

		uniform sampler2D _Texture0;
		uniform half4 _TextColor;
		uniform half _TimeBoxSpeed;
		uniform half _Smoothness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.vertexToFrag5_g2 = ( ( sin( ( fmod( _Time.y , 120.0 ) * _TimeBoxSpeed ) ) + 1.0 ) / 2.0 );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			half lerpResult8 = lerp( 0.0 , 0.25 , _SinTime.z);
			half2 appendResult9 = (half2(lerpResult8 , 0.0));
			half lerpResult24 = lerp( tex2D( _Texture0, i.uv_texcoord ).a , 0.0 , round( i.vertexToFrag5_g2 ));
			half4 lerpResult18 = lerp( tex2D( _Texture0, ( ( ( i.uv_texcoord + half2( 0.5,1 ) ) * half2( 0.5,1 ) ) + appendResult9 ) ) , _TextColor , lerpResult24);
			o.Emission = lerpResult18.rgb;
			o.Metallic = 0.0;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.Vector2Node;14;-1540,-3.166718;Inherit;False;Constant;_Vector1;Vector 0;2;0;Create;True;0;0;0;False;0;False;0.5,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TexCoordVertexDataNode;3;-1734,-175.1668;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinTimeNode;2;-1501.333,182.8334;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;13;-1424.666,-181.8334;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;12;-1204,-51.8334;Inherit;False;Constant;_Vector0;Vector 0;2;0;Create;True;0;0;0;False;0;False;0.5,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;8;-1278.667,177.5;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.25;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-1074,-198.5001;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;9;-1023.333,132.8333;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;21;-1082,318.8332;Inherit;True;Property;_Texture0;Texture 0;4;0;Create;True;0;0;0;False;0;False;eac733f4668a73e469f7df90927734d5;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexCoordVertexDataNode;23;-1108.621,552.2957;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;25;-683.4452,605.7415;Inherit;False;SHAF_TimeBox;1;;2;fe42b26378975cf4dbc110668364facf;0;1;10;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;5;-820.6669,2.833313;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;22;-695.9995,221.4999;Inherit;True;Property;_TextureSample1;Texture Sample 1;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RoundOpNode;26;-397.4452,645.7415;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-616.6666,-17.83337;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;eac733f4668a73e469f7df90927734d5;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;20;-256.6659,-237.8334;Inherit;False;Property;_TextColor;TextColor;3;0;Create;True;0;0;0;False;0;False;0.9500964,1,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;24;-341.1118,394.4081;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;18;-56.66589,51.49988;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-229.3325,266.8332;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;60.66718,168.1667;Inherit;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-49.99969,246.1666;Inherit;False;Property;_Smoothness;Smoothness;0;0;Create;True;0;0;0;False;0;False;0.4927534;0.4927534;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;253.3334,-40;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_AOJ_TV_Panner;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;13;0;3;0
WireConnection;13;1;14;0
WireConnection;8;2;2;3
WireConnection;7;0;13;0
WireConnection;7;1;12;0
WireConnection;9;0;8;0
WireConnection;5;0;7;0
WireConnection;5;1;9;0
WireConnection;22;0;21;0
WireConnection;22;1;23;0
WireConnection;26;0;25;0
WireConnection;1;0;21;0
WireConnection;1;1;5;0
WireConnection;24;0;22;4
WireConnection;24;2;26;0
WireConnection;18;0;1;0
WireConnection;18;1;20;0
WireConnection;18;2;24;0
WireConnection;0;2;18;0
WireConnection;0;3;10;0
WireConnection;0;4;11;0
ASEEND*/
//CHKSM=C57907E9FF4DAF3FDD14B9371C83527BD7CE1806
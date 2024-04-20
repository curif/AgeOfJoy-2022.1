// Upgrade NOTE: upgraded instancing buffer 'AgeOfJoySHA_Eye' to new syntax.

// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/SHA_Eye"
{
	Properties
	{
		_Eye_H("Eye_H", Range( 0 , 1)) = 0
		_Eye_H_MaxBounds("Eye_H_MaxBounds", Range( 0 , 1)) = 0.12
		_Eye_V_MaxBounds("Eye_V_MaxBounds", Range( 0 , 1)) = 0.08
		_Eye_Focus_MaxBounds("Eye_Focus_MaxBounds", Range( 0 , 1)) = 0.027
		_Eye_V("Eye_V", Range( 0 , 1)) = 0
		_Tex_EyeMaster("Tex_EyeMaster", 2D) = "white" {}
		_Eye_Focus("Eye_Focus", Range( 0 , 1)) = 0
		_Eye_Tint("Eye_Tint", Color) = (0,1,0.9583442,0)
		_EyeWhite_Tint("EyeWhite_Tint", Color) = (1,1,1,0)
		_Tex_Eyemaster_UV("Tex_Eyemaster_UV", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Tex_EyeMaster;
		uniform sampler2D _Tex_Eyemaster_UV;
		uniform half4 _EyeWhite_Tint;
		uniform half4 _Eye_Tint;

		UNITY_INSTANCING_BUFFER_START(AgeOfJoySHA_Eye)
			UNITY_DEFINE_INSTANCED_PROP(half4, _Tex_Eyemaster_UV_ST)
#define _Tex_Eyemaster_UV_ST_arr AgeOfJoySHA_Eye
			UNITY_DEFINE_INSTANCED_PROP(half, _Eye_H_MaxBounds)
#define _Eye_H_MaxBounds_arr AgeOfJoySHA_Eye
			UNITY_DEFINE_INSTANCED_PROP(half, _Eye_H)
#define _Eye_H_arr AgeOfJoySHA_Eye
			UNITY_DEFINE_INSTANCED_PROP(half, _Eye_V_MaxBounds)
#define _Eye_V_MaxBounds_arr AgeOfJoySHA_Eye
			UNITY_DEFINE_INSTANCED_PROP(half, _Eye_V)
#define _Eye_V_arr AgeOfJoySHA_Eye
			UNITY_DEFINE_INSTANCED_PROP(half, _Eye_Focus_MaxBounds)
#define _Eye_Focus_MaxBounds_arr AgeOfJoySHA_Eye
			UNITY_DEFINE_INSTANCED_PROP(half, _Eye_Focus)
#define _Eye_Focus_arr AgeOfJoySHA_Eye
		UNITY_INSTANCING_BUFFER_END(AgeOfJoySHA_Eye)

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			half _Eye_H_MaxBounds_Instance = UNITY_ACCESS_INSTANCED_PROP(_Eye_H_MaxBounds_arr, _Eye_H_MaxBounds);
			half _Eye_H_Instance = UNITY_ACCESS_INSTANCED_PROP(_Eye_H_arr, _Eye_H);
			half lerpResult41 = lerp( ( _Eye_H_MaxBounds_Instance * -1.0 ) , _Eye_H_MaxBounds_Instance , _Eye_H_Instance);
			half _Eye_V_MaxBounds_Instance = UNITY_ACCESS_INSTANCED_PROP(_Eye_V_MaxBounds_arr, _Eye_V_MaxBounds);
			half _Eye_V_Instance = UNITY_ACCESS_INSTANCED_PROP(_Eye_V_arr, _Eye_V);
			half lerpResult43 = lerp( ( _Eye_V_MaxBounds_Instance * -1.0 ) , _Eye_V_MaxBounds_Instance , _Eye_V_Instance);
			half2 appendResult31 = (half2(lerpResult41 , lerpResult43));
			half _Eye_Focus_MaxBounds_Instance = UNITY_ACCESS_INSTANCED_PROP(_Eye_Focus_MaxBounds_arr, _Eye_Focus_MaxBounds);
			half _Eye_Focus_Instance = UNITY_ACCESS_INSTANCED_PROP(_Eye_Focus_arr, _Eye_Focus);
			half lerpResult48 = lerp( 0.0 , _Eye_Focus_MaxBounds_Instance , _Eye_Focus_Instance);
			half2 appendResult46 = (half2(lerpResult48 , 0.0));
			half2 appendResult33 = (half2(( lerpResult41 * -1.0 ) , lerpResult43));
			half4 _Tex_Eyemaster_UV_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_Tex_Eyemaster_UV_ST_arr, _Tex_Eyemaster_UV_ST);
			float2 uv_Tex_Eyemaster_UV = i.uv_texcoord * _Tex_Eyemaster_UV_ST_Instance.xy + _Tex_Eyemaster_UV_ST_Instance.zw;
			half2 lerpResult28 = lerp( ( i.uv_texcoord + ( appendResult31 + appendResult46 ) ) , ( i.uv_texcoord + ( appendResult46 + appendResult33 ) ) , tex2D( _Tex_Eyemaster_UV, uv_Tex_Eyemaster_UV ).r);
			half4 tex2DNode1 = tex2D( _Tex_EyeMaster, lerpResult28 );
			half4 lerpResult56 = lerp( ( tex2DNode1.g * _EyeWhite_Tint ) , ( tex2DNode1.g * _Eye_Tint ) , tex2DNode1.b);
			o.Albedo = lerpResult56.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.RangedFloatNode;63;-2341.476,-798.6155;Inherit;False;InstancedProperty;_Eye_H_MaxBounds;Eye_H_MaxBounds;1;0;Create;True;0;0;0;False;0;False;0.12;0.102;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-2674.477,-611.959;Inherit;False;InstancedProperty;_Eye_H;Eye_H;0;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;-1960.142,-744.8821;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-2342.342,-286.4155;Inherit;False;InstancedProperty;_Eye_V_MaxBounds;Eye_V_MaxBounds;2;0;Create;True;0;0;0;False;0;False;0.08;0.049;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;41;-1863.342,-606.3592;Inherit;False;3;0;FLOAT;-0.12;False;1;FLOAT;0.12;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-2582.213,-143.6256;Inherit;False;InstancedProperty;_Eye_V;Eye_V;4;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-1989.609,-315.8821;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-2182.009,220.3076;Inherit;False;InstancedProperty;_Eye_Focus;Eye_Focus;6;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;67;-2318.074,61.9846;Inherit;False;InstancedProperty;_Eye_Focus_MaxBounds;Eye_Focus_MaxBounds;3;0;Create;True;0;0;0;False;0;False;0.027;0.049;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-1679.241,-499.1505;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;43;-1830.009,-216.3591;Inherit;False;3;0;FLOAT;-0.085;False;1;FLOAT;0.08;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;48;-1780.009,164.6413;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.027;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;46;-1632.008,19.64093;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;31;-1272.308,-600.4835;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;33;-1353.908,-231.1504;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;25;-1157.303,-802.717;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;45;-968.675,-565.0258;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;5;-1082.667,-352.8333;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;47;-984.0084,-158.3591;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;-783.211,-617.7006;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;-787.9083,-136.4836;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;62;-1126.408,64.5832;Inherit;True;Property;_Tex_Eyemaster_UV;Tex_Eyemaster_UV;9;0;Create;True;0;0;0;False;0;False;-1;None;667625a6dbed8767d838b487350a110d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;28;-701.3087,-418.9507;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;59;-386.7076,-371.8189;Inherit;False;Property;_Eye_Tint;Eye_Tint;7;0;Create;True;0;0;0;False;0;False;0,1,0.9583442,0;0,1,0.9583442,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;60;-314.041,-93.81891;Inherit;False;Property;_EyeWhite_Tint;EyeWhite_Tint;8;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,0.9292453,0.9292453,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-561.3334,-616.8335;Inherit;True;Property;_Tex_EyeMaster;Tex_EyeMaster;5;0;Create;True;0;0;0;False;0;False;-1;None;667625a6dbed8767d838b487350a110d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;104.3956,-692.0569;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;12.62561,-424.4856;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-184.6669,407.8333;Inherit;False;Constant;_Float1;Float 1;1;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-172.6669,488.5;Inherit;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;56;402.3958,-602.0569;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;394,-2;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;AgeOfJoy/SHA_Eye;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;64;0;63;0
WireConnection;41;0;64;0
WireConnection;41;1;63;0
WireConnection;41;2;40;0
WireConnection;66;0;65;0
WireConnection;36;0;41;0
WireConnection;43;0;66;0
WireConnection;43;1;65;0
WireConnection;43;2;42;0
WireConnection;48;1;67;0
WireConnection;48;2;44;0
WireConnection;46;0;48;0
WireConnection;31;0;41;0
WireConnection;31;1;43;0
WireConnection;33;0;36;0
WireConnection;33;1;43;0
WireConnection;45;0;31;0
WireConnection;45;1;46;0
WireConnection;47;0;46;0
WireConnection;47;1;33;0
WireConnection;27;0;25;0
WireConnection;27;1;45;0
WireConnection;22;0;5;0
WireConnection;22;1;47;0
WireConnection;28;0;27;0
WireConnection;28;1;22;0
WireConnection;28;2;62;1
WireConnection;1;1;28;0
WireConnection;54;0;1;2
WireConnection;54;1;59;0
WireConnection;61;0;1;2
WireConnection;61;1;60;0
WireConnection;56;0;61;0
WireConnection;56;1;54;0
WireConnection;56;2;1;3
WireConnection;0;0;56;0
WireConnection;0;3;4;0
WireConnection;0;4;3;0
ASEEND*/
//CHKSM=3647F2CB633036D66D643D4FCD5C86756D79BC35
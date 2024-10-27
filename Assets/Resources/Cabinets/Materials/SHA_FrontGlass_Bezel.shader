// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_FrontGlass_Bezel"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Dirt("Dirt", 2D) = "white" {}
		_Metallic("Metallic", Float) = 1
		_GlossinessLow("Glossiness Low", Range( 0 , 1)) = 1
		_GlossinessHigh("Glossiness High", Range( 0 , 1)) = 1
		_GlassTranslucency("GlassTranslucency", Range( 0 , 1)) = 1
		_Dirtiness("Dirtiness", Range( 0 , 1)) = 0.38
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Dirt;
		uniform half4 _Dirt_ST;
		uniform half _Dirtiness;
		uniform sampler2D _MainTex;
		uniform half4 _MainTex_ST;
		uniform half _Metallic;
		uniform half _GlossinessHigh;
		uniform half _GlossinessLow;
		uniform half _GlassTranslucency;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Dirt = i.uv_texcoord * _Dirt_ST.xy + _Dirt_ST.zw;
			half4 tex2DNode1 = tex2D( _Dirt, uv_Dirt );
			half temp_output_13_0 = ( tex2DNode1.g * _Dirtiness );
			half4 temp_cast_0 = (temp_output_13_0).xxxx;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			half4 tex2DNode9 = tex2D( _MainTex, uv_MainTex );
			half4 lerpResult7 = lerp( temp_cast_0 , ( tex2DNode9 + temp_output_13_0 ) , tex2DNode9.a);
			o.Albedo = lerpResult7.rgb;
			o.Metallic = _Metallic;
			half lerpResult19 = lerp( _GlossinessHigh , _GlossinessLow , tex2DNode1.g);
			o.Smoothness = lerpResult19;
			half lerpResult15 = lerp( _GlassTranslucency , 1.0 , tex2DNode9.a);
			o.Alpha = lerpResult15;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows exclude_path:deferred 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.SamplerNode;1;-1256,-731.7593;Inherit;True;Property;_Dirt;Dirt;1;0;Create;True;0;0;0;False;0;False;-1;df7312da5fda7e447bcda7902535ea57;df7312da5fda7e447bcda7902535ea57;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;17;-1178,-517.9999;Inherit;False;Property;_Dirtiness;Dirtiness;6;0;Create;True;0;0;0;False;0;False;0.38;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-1076.667,-942.4258;Inherit;True;Property;_MainTex;MainTex;0;0;Create;False;0;0;0;False;0;False;-1;81c9525308aab3141a01203c34a4fbfa;81c9525308aab3141a01203c34a4fbfa;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-865.3333,-681.1668;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1490196;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;2;-640.6667,-942.4259;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-651.9999,-491.1669;Inherit;False;Property;_GlassTranslucency;GlassTranslucency;5;0;Create;True;0;0;0;False;0;False;1;0.546;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-755.3334,-206.0003;Inherit;False;Property;_GlossinessLow;Glossiness Low;3;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-752.6664,-283.1671;Inherit;False;Property;_GlossinessHigh;Glossiness High;4;0;Create;True;0;0;0;False;0;False;1;0.9565224;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-145.9999,-404.5002;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;81.33337,-1141.167;Inherit;False;Property;_Metallic;Metallic;2;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;7;-160.6666,-932.4261;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;15;-221.9999,-611.8336;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;19;-345.9996,-219.1669;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;12;-712.6666,-1171.833;Inherit;False;Constant;_Vector0;Vector 0;4;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;250.6666,-514.6667;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_FrontGlass_Bezel;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;13;0;1;2
WireConnection;13;1;17;0
WireConnection;2;0;9;0
WireConnection;2;1;13;0
WireConnection;7;0;13;0
WireConnection;7;1;2;0
WireConnection;7;2;9;4
WireConnection;15;0;16;0
WireConnection;15;2;9;4
WireConnection;19;0;20;0
WireConnection;19;1;4;0
WireConnection;19;2;1;2
WireConnection;0;0;7;0
WireConnection;0;3;11;0
WireConnection;0;4;19;0
WireConnection;0;9;15;0
ASEEND*/
//CHKSM=44A0B44680EA48DB20D981BC49B41B7A04EF89A0
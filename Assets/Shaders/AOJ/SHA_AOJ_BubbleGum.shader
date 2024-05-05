// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/SHA_AOJ_BubbleGum"
{
	Properties
	{
		_SmoothnessStart("SmoothnessStart", Range( 0 , 1)) = 1
		_SmoothnessEnd("SmoothnessEnd", Range( 0 , 1)) = 1
		_TransparencyStart("TransparencyStart", Range( 0 , 1)) = 0.9
		_TransparencyEnd("TransparencyEnd", Range( 0 , 1)) = 0.2753628
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float2 uv_texcoord;
		};

		uniform float _SmoothnessStart;
		uniform float _SmoothnessEnd;
		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform float _TransparencyStart;
		uniform float _TransparencyEnd;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float mulTime30 = _Time.y * 0.1;
			float temp_output_34_0 = frac( fmod( mulTime30 , 120.0 ) );
			float lerpResult6 = lerp( -1.0 , 0.5 , temp_output_34_0);
			v.vertex.xyz += ( ase_vertex3Pos * lerpResult6 );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color1 = IsGammaSpace() ? float4(1,0.5125785,0.915896,0) : float4(1,0.2258689,0.8192353,0);
			float4 color14 = IsGammaSpace() ? float4(1,0.7138364,0.9002905,0) : float4(1,0.4679458,0.7879874,0);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV12 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode12 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV12, 1.0 ) );
			float4 lerpResult13 = lerp( color1 , color14 , fresnelNode12);
			o.Albedo = lerpResult13.rgb;
			float mulTime30 = _Time.y * 0.1;
			float temp_output_34_0 = frac( fmod( mulTime30 , 120.0 ) );
			float lerpResult10 = lerp( _SmoothnessStart , _SmoothnessEnd , temp_output_34_0);
			o.Smoothness = lerpResult10;
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float saferPower25 = abs( temp_output_34_0 );
			float lerpResult23 = lerp( 0.6 , -0.33 , saturate( pow( saferPower25 , 100.0 ) ));
			float lerpResult8 = lerp( _TransparencyStart , _TransparencyEnd , temp_output_34_0);
			o.Alpha = ( round( ( tex2D( _TextureSample0, uv_TextureSample0 ).r + lerpResult23 ) ) * lerpResult8 );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

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
				float3 worldNormal : TEXCOORD3;
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
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
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
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
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
Node;AmplifyShaderEditor.SimpleTimeNode;30;-1348.333,517.8752;Inherit;False;1;0;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FmodOpNode;31;-1156.333,353.0422;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;120;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;34;-817.9999,437.8336;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-644.6667,645.8334;Inherit;False;Constant;_GumPopSpeed;GumPopSpeed;5;0;Create;True;0;0;0;False;0;False;100;0;0;500;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;25;-371.3329,569.1669;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;80;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;27;-48.66638,595.1669;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;23;12.66699,256.5002;Inherit;False;3;0;FLOAT;0.6;False;1;FLOAT;-0.33;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;16;-168.6663,-47.49982;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;a11563a4124b96c4db654a9ea7217d2c;a11563a4124b96c4db654a9ea7217d2c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;15;-310.6663,-282.8333;Inherit;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;226.6671,91.16692;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.33;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-833.3326,-71.50003;Inherit;False;Property;_TransparencyStart;TransparencyStart;2;0;Create;True;0;0;0;False;0;False;0.9;0.897;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-869.9995,57.16672;Inherit;False;Property;_TransparencyEnd;TransparencyEnd;3;0;Create;True;0;0;0;False;0;False;0.2753628;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-702.0002,-433.5001;Inherit;False;Property;_SmoothnessStart;SmoothnessStart;0;0;Create;True;0;0;0;False;0;False;1;0.516;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-761.3329,-270.1666;Inherit;False;Property;_SmoothnessEnd;SmoothnessEnd;1;0;Create;True;0;0;0;False;0;False;1;0.948;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;12;-127.3329,-437.5;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;14;-101.9996,-598.1666;Inherit;False;Constant;_Color1;Color 1;5;0;Create;True;0;0;0;False;0;False;1,0.7138364,0.9002905,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;1;-25.33331,-769.4999;Inherit;False;Constant;_Color0;Color 0;0;0;Create;True;0;0;0;False;0;False;1,0.5125785,0.915896,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;8;-336.666,131.1666;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;17;404.0001,99.16682;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;3;-889.3329,218.5;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;6;-533.9996,427.8333;Inherit;False;3;0;FLOAT;-1;False;1;FLOAT;0.5;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;10;-299.3328,-106.1666;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;13;248.6671,-368.8333;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-216.6663,445.1667;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;582.6667,139.8335;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;704.6666,-82;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Custom/SHA_AOJ_BubbleGum;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;31;0;30;0
WireConnection;34;0;31;0
WireConnection;25;0;34;0
WireConnection;25;1;36;0
WireConnection;27;0;25;0
WireConnection;23;2;27;0
WireConnection;22;0;16;1
WireConnection;22;1;23;0
WireConnection;12;3;15;0
WireConnection;8;0;7;0
WireConnection;8;1;9;0
WireConnection;8;2;34;0
WireConnection;17;0;22;0
WireConnection;6;2;34;0
WireConnection;10;0;2;0
WireConnection;10;1;11;0
WireConnection;10;2;34;0
WireConnection;13;0;1;0
WireConnection;13;1;14;0
WireConnection;13;2;12;0
WireConnection;5;0;3;0
WireConnection;5;1;6;0
WireConnection;35;0;17;0
WireConnection;35;1;8;0
WireConnection;0;0;13;0
WireConnection;0;4;10;0
WireConnection;0;9;35;0
WireConnection;0;11;5;0
ASEEND*/
//CHKSM=6BB23888DFE2F592CF90E2517BBEA34B9363F32E
// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/LitVtxGlow_Phone"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Diffuse("Diffuse", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_PulseSpeed("PulseSpeed", Float) = 8
		_GlowMin("GlowMin", Range( 0 , 5)) = 0.7
		_GlowMax("GlowMax", Range( 0 , 1)) = 1
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.5
		_EmissiveTint("EmissiveTint", Color) = (1,1,1,0)
		_EmissiveTintFresnel("EmissiveTintFresnel", Color) = (1,1,1,0)
		[Toggle(_USEEMISSIVETINT_ON)] _useEmissiveTint("useEmissiveTint", Float) = 0
		_FresnelBias("FresnelBias", Float) = 0
		_AlbedoTint("AlbedoTint", Color) = (0,0,0,0)
		_Opacity("Opacity", Range( 0 , 1)) = 1
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USEEMISSIVETINT_ON
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float3 worldPos;
			half3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _Normal;
		uniform half4 _Normal_ST;
		uniform sampler2D _Diffuse;
		uniform half4 _Diffuse_ST;
		uniform half4 _AlbedoTint;
		uniform half _GlowMin;
		uniform half _GlowMax;
		uniform half _PulseSpeed;
		uniform half4 _EmissiveTint;
		uniform half4 _EmissiveTintFresnel;
		uniform half _FresnelBias;
		uniform half _Metallic;
		uniform half _Smoothness;
		uniform half _Opacity;
		uniform sampler2D _TextureSample0;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Diffuse = i.uv_texcoord * _Diffuse_ST.xy + _Diffuse_ST.zw;
			half4 tex2DNode6 = tex2D( _Diffuse, uv_Diffuse );
			o.Albedo = ( tex2DNode6 * _AlbedoTint ).rgb;
			half lerpResult36 = lerp( _GlowMin , _GlowMax , ( ( sin( ( _Time.y * _PulseSpeed ) ) + 2.0 ) - 1.0 ));
			half4 temp_output_20_0 = ( ( tex2DNode6 * i.vertexColor.r ) * lerpResult36 );
			float3 ase_worldPos = i.worldPos;
			half3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			half3 ase_worldNormal = WorldNormalVector( i, half3( 0, 0, 1 ) );
			half fresnelNdotV47 = dot( ase_worldNormal, ase_worldViewDir );
			half fresnelNode47 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV47, _FresnelBias ) );
			half clampResult50 = clamp( fresnelNode47 , 0.0 , 1.0 );
			half4 lerpResult45 = lerp( _EmissiveTint , _EmissiveTintFresnel , clampResult50);
			#ifdef _USEEMISSIVETINT_ON
				half4 staticSwitch44 = ( temp_output_20_0 * lerpResult45 );
			#else
				half4 staticSwitch44 = temp_output_20_0;
			#endif
			o.Emission = staticSwitch44.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
			half lerpResult56 = lerp( -1.0 , 1.0 , _Opacity);
			float2 uv_TexCoord61 = i.uv_texcoord * float2( 4,4 );
			half2 panner62 = ( 1.0 * _Time.y * float2( 0,0.2 ) + uv_TexCoord61);
			clip( saturate( ( lerpResult56 + tex2D( _TextureSample0, panner62 ).g ) ) - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				half4 color : COLOR0;
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
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
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
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
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
Node;AmplifyShaderEditor.SimpleTimeNode;23;-206.3328,452.3329;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-139.6663,548.3329;Inherit;False;Property;_PulseSpeed;PulseSpeed;3;0;Create;True;0;0;0;False;0;False;8;8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;15.66699,322.333;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;35;232.3339,325.6662;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;260.3338,504.9995;Inherit;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;31;371.0006,379.6663;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;1088.919,835.5659;Inherit;False;Property;_FresnelBias;FresnelBias;11;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;311.6673,610.3329;Inherit;False;Constant;_Float2;Float 2;2;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;16;-314.9996,235.6663;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;33;509.0005,460.3329;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;6;-405,-275.6669;Inherit;True;Property;_Diffuse;Diffuse;1;0;Create;True;0;0;0;False;0;False;-1;07dbb2281ffb1c84e8a3e66a0f22fbc9;8fa709cbe7bcabd46b65d7266a5013f4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;38;481.6672,719.6663;Inherit;False;Property;_GlowMax;GlowMax;5;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;487.6671,626.3329;Inherit;False;Property;_GlowMin;GlowMin;4;0;Create;True;0;0;0;False;0;False;0.7;0.7;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;47;1094.397,558.4325;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;61;1661.432,429.968;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;4,4;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-76.99963,104.9997;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;36;731.0006,478.3329;Inherit;False;3;0;FLOAT;0.7;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;42;902.1187,177.385;Inherit;False;Property;_EmissiveTint;EmissiveTint;8;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;46;933.8756,368.2907;Inherit;False;Property;_EmissiveTintFresnel;EmissiveTintFresnel;9;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;50;1232.253,465.5659;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;53;1865.432,237.968;Inherit;False;Property;_Opacity;Opacity;13;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;62;1902.432,398.968;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0.2;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;569.6671,-17.00044;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;45;1252.052,325.2939;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;56;2237.432,110.968;Inherit;False;3;0;FLOAT;-1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;55;2135.432,417.9681;Inherit;True;Property;_TextureSample0;Texture Sample 0;14;0;Create;True;0;0;0;False;0;False;-1;a11563a4124b96c4db654a9ea7217d2c;a11563a4124b96c4db654a9ea7217d2c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;1292.018,101.4697;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;52;1040.207,-297.1347;Inherit;False;Property;_AlbedoTint;AlbedoTint;12;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;57;2628.432,213.968;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-424.9996,-45.66711;Inherit;True;Property;_Normal;Normal;2;0;Create;True;0;0;0;False;0;False;-1;6c5f45a17f5a70e4ba7f2546220da66f;490c870894e3c6846a6d738cfaa121f5;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;44;1540.121,-135.3106;Inherit;False;Property;_useEmissiveTint;useEmissiveTint;10;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;1315.207,-411.1347;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;39;1526.334,40.66599;Inherit;False;Property;_Metallic;Metallic;6;0;Create;True;0;0;0;False;0;False;0;0.614;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;1593.334,108.3327;Inherit;False;Property;_Smoothness;Smoothness;7;0;Create;True;0;0;0;False;0;False;0.5;0.471;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;59;2763.432,260.968;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;54;2544.667,-454.3334;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;AgeOfJoy/LitVtxGlow_Phone;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;26;0;23;0
WireConnection;26;1;27;0
WireConnection;35;0;26;0
WireConnection;31;0;35;0
WireConnection;31;1;32;0
WireConnection;33;0;31;0
WireConnection;33;1;34;0
WireConnection;47;3;49;0
WireConnection;15;0;6;0
WireConnection;15;1;16;1
WireConnection;36;0;37;0
WireConnection;36;1;38;0
WireConnection;36;2;33;0
WireConnection;50;0;47;0
WireConnection;62;0;61;0
WireConnection;20;0;15;0
WireConnection;20;1;36;0
WireConnection;45;0;42;0
WireConnection;45;1;46;0
WireConnection;45;2;50;0
WireConnection;56;2;53;0
WireConnection;55;1;62;0
WireConnection;41;0;20;0
WireConnection;41;1;45;0
WireConnection;57;0;56;0
WireConnection;57;1;55;2
WireConnection;44;1;20;0
WireConnection;44;0;41;0
WireConnection;51;0;6;0
WireConnection;51;1;52;0
WireConnection;59;0;57;0
WireConnection;54;0;51;0
WireConnection;54;1;7;0
WireConnection;54;2;44;0
WireConnection;54;3;39;0
WireConnection;54;4;40;0
WireConnection;54;10;59;0
ASEEND*/
//CHKSM=576CD4C2B30CE7F3136FE7CF0F4108C4BD71E780
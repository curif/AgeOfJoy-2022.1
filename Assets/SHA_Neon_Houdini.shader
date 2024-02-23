// Upgrade NOTE: upgraded instancing buffer 'AgeOfJoyNeon_Houdini' to new syntax.

// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/Neon_Houdini"
{
	Properties
	{
		_FresnelScale("FresnelScale", Range( 0 , 5)) = 2.32
		_ColorA("Color A", Color) = (0.0660376,0.636061,1,0)
		_ColorCore("Color Core", Color) = (0.7735849,0.6738459,0.6738459,0)
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
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
			float4 vertexColor : COLOR;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform float _FresnelScale;

		UNITY_INSTANCING_BUFFER_START(AgeOfJoyNeon_Houdini)
			UNITY_DEFINE_INSTANCED_PROP(float4, _ColorA)
#define _ColorA_arr AgeOfJoyNeon_Houdini
			UNITY_DEFINE_INSTANCED_PROP(float4, _ColorCore)
#define _ColorCore_arr AgeOfJoyNeon_Houdini
		UNITY_INSTANCING_BUFFER_END(AgeOfJoyNeon_Houdini)

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color28 = IsGammaSpace() ? float4(0.1736085,0.8282693,0.9685534,0) : float4(0.02546923,0.6528565,0.9299493,0);
			float4 lerpResult14 = lerp( float4( 0,0,0,0 ) , color28 , i.vertexColor.b);
			o.Albedo = lerpResult14.rgb;
			float4 _ColorA_Instance = UNITY_ACCESS_INSTANCED_PROP(_ColorA_arr, _ColorA);
			float4 _ColorCore_Instance = UNITY_ACCESS_INSTANCED_PROP(_ColorCore_arr, _ColorCore);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			ase_vertexNormal = normalize( ase_vertexNormal );
			float fresnelNdotV3 = dot( ase_vertexNormal, ase_worldViewDir );
			float fresnelNode3 = ( 0.0 + _FresnelScale * pow( 1.0 - fresnelNdotV3, 5.0 ) );
			float clampResult17 = clamp( fresnelNode3 , 0.0 , 1.0 );
			float4 lerpResult7 = lerp( _ColorA_Instance , _ColorCore_Instance , clampResult17);
			float mulTime21 = _Time.y * 5.0;
			float lerpResult27 = lerp( 1.0 , 0.8 , ( ( sin( mulTime21 ) * 2.0 ) - 1.0 ));
			float4 lerpResult16 = lerp( float4( 0,0,0,0 ) , ( lerpResult7 * lerpResult27 ) , i.vertexColor.r);
			o.Emission = lerpResult16.rgb;
			float lerpResult9 = lerp( 0.0 , 1.0 , i.vertexColor.b);
			o.Metallic = lerpResult9;
			float lerpResult13 = lerp( 0.0 , 0.8 , i.vertexColor.b);
			o.Smoothness = lerpResult13;
			o.Alpha = 1;
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
				float4 tSpace0 : TEXCOORD1;
				float4 tSpace1 : TEXCOORD2;
				float4 tSpace2 : TEXCOORD3;
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
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
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
Node;AmplifyShaderEditor.SimpleTimeNode;21;-1327.667,-23.5004;Inherit;False;1;0;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;2;-1007.25,332.9998;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1;-813.9167,493.3333;Inherit;False;Property;_FresnelScale;FresnelScale;0;0;Create;True;0;0;0;False;0;False;2.32;2.67;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;24;-1105.667,1.16626;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;3;-516.5836,168;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-952.3332,-12.16708;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;4;-535.9166,-191.6669;Inherit;False;InstancedProperty;_ColorCore;Color Core;2;0;Create;True;0;0;0;False;0;False;0.7735849,0.6738459,0.6738459,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;8;-518.5832,-388.6668;Inherit;False;InstancedProperty;_ColorA;Color A;1;0;Create;True;0;0;0;False;0;False;0.0660376,0.636061,1,0;0,0.6092436,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;17;-269.6666,207.4998;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;26;-800.3332,20.4996;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;7;-255.2501,-304.6668;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;27;-504.9999,23.83295;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0.8;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;10;-273,529.5;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-55.66663,-149.5005;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;28;127.0002,-288.8336;Inherit;False;Constant;_Color0;Color 0;3;0;Create;True;0;0;0;False;0;False;0.1736085,0.8282693,0.9685534,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;9;53.66693,553.5;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;16;-30.33325,112.1666;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;14;348.3334,-101.1667;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;13;230.3335,231.5;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.8;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;637.3334,-12.66666;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;AgeOfJoy/Neon_Houdini;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;24;0;21;0
WireConnection;3;0;2;0
WireConnection;3;2;1;0
WireConnection;25;0;24;0
WireConnection;17;0;3;0
WireConnection;26;0;25;0
WireConnection;7;0;8;0
WireConnection;7;1;4;0
WireConnection;7;2;17;0
WireConnection;27;2;26;0
WireConnection;18;0;7;0
WireConnection;18;1;27;0
WireConnection;9;2;10;3
WireConnection;16;1;18;0
WireConnection;16;2;10;1
WireConnection;14;1;28;0
WireConnection;14;2;10;3
WireConnection;13;2;10;3
WireConnection;0;0;14;0
WireConnection;0;2;16;0
WireConnection;0;3;9;0
WireConnection;0;4;13;0
ASEEND*/
//CHKSM=295CD6C896CAB7BFD14B9AB6E891904839B243D0
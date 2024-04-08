// Upgrade NOTE: upgraded instancing buffer 'AgeOfJoyNeon_Houdini_Simple' to new syntax.

// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/Neon_Houdini_Simple"
{
	Properties
	{
		_FresnelScale("FresnelScale", Range( 0 , 5)) = 2.32
		[HDR]_ColorA("Color A", Color) = (0.0660376,0.636061,1,0)
		[HDR]_ColorB("Color B", Color) = (0.0660376,0.636061,1,0)
		[HDR]_ColorC("Color C", Color) = (0.0660376,0.636061,1,0)
		_ColorCore("Color Core", Color) = (0.7735849,0.6738459,0.6738459,0)
		_FlickerSpeed("FlickerSpeed", Range( 1 , 50)) = 1
		_FlickerMin("FlickerMin", Range( 0 , 2)) = 0
		_FlickerMax("FlickerMax", Range( 0 , 10)) = 1
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
			half4 vertexToFrag58;
			float3 worldPos;
			half3 worldNormal;
			INTERNAL_DATA
			half vertexToFrag49;
		};

		uniform half _FresnelScale;
		uniform half _FlickerMin;
		uniform half _FlickerMax;
		uniform half _FlickerSpeed;

		UNITY_INSTANCING_BUFFER_START(AgeOfJoyNeon_Houdini_Simple)
			UNITY_DEFINE_INSTANCED_PROP(half4, _ColorA)
#define _ColorA_arr AgeOfJoyNeon_Houdini_Simple
			UNITY_DEFINE_INSTANCED_PROP(half4, _ColorB)
#define _ColorB_arr AgeOfJoyNeon_Houdini_Simple
			UNITY_DEFINE_INSTANCED_PROP(half4, _ColorC)
#define _ColorC_arr AgeOfJoyNeon_Houdini_Simple
			UNITY_DEFINE_INSTANCED_PROP(half4, _ColorCore)
#define _ColorCore_arr AgeOfJoyNeon_Houdini_Simple
		UNITY_INSTANCING_BUFFER_END(AgeOfJoyNeon_Houdini_Simple)

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			half4 _ColorA_Instance = UNITY_ACCESS_INSTANCED_PROP(_ColorA_arr, _ColorA);
			half4 _ColorB_Instance = UNITY_ACCESS_INSTANCED_PROP(_ColorB_arr, _ColorB);
			half4 _ColorC_Instance = UNITY_ACCESS_INSTANCED_PROP(_ColorC_arr, _ColorC);
			o.vertexToFrag58 = ( ( _ColorA_Instance * v.color.r ) + ( v.color.g * _ColorB_Instance ) + ( v.color.b * _ColorC_Instance ) );
			half mulTime36 = _Time.y * _FlickerSpeed;
			half lerpResult46 = lerp( _FlickerMin , _FlickerMax , ( ( sin( mulTime36 ) + 1.0 ) * 0.5 ));
			o.vertexToFrag49 = lerpResult46;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			half4 _ColorCore_Instance = UNITY_ACCESS_INSTANCED_PROP(_ColorCore_arr, _ColorCore);
			float3 ase_worldPos = i.worldPos;
			half3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			half3 ase_worldNormal = i.worldNormal;
			half3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			ase_vertexNormal = normalize( ase_vertexNormal );
			half fresnelNdotV3 = dot( ase_vertexNormal, ase_worldViewDir );
			half fresnelNode3 = ( 0.0 + _FresnelScale * pow( 1.0 - fresnelNdotV3, 5.0 ) );
			half clampResult17 = clamp( fresnelNode3 , 0.0 , 1.0 );
			half4 lerpResult7 = lerp( i.vertexToFrag58 , _ColorCore_Instance , clampResult17);
			o.Emission = ( lerpResult7 * i.vertexToFrag49 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows vertex:vertexDataFunc 

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
				float4 customPack1 : TEXCOORD1;
				float1 customPack2 : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
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
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xyzw = customInputData.vertexToFrag58;
				o.customPack2.x = customInputData.vertexToFrag49;
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
				surfIN.vertexToFrag58 = IN.customPack1.xyzw;
				surfIN.vertexToFrag49 = IN.customPack2.x;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
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
Node;AmplifyShaderEditor.RangedFloatNode;35;-2220.65,199.3895;Inherit;False;Property;_FlickerSpeed;FlickerSpeed;5;0;Create;True;0;0;0;False;0;False;1;0;1;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;36;-1962.241,384.0715;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;53;-1085.685,-437.6787;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;8;-1137.446,-1239.991;Inherit;False;InstancedProperty;_ColorA;Color A;1;1;[HDR];Create;True;0;0;0;False;0;False;0.0660376,0.636061,1,0;0,1.218487,2,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;50;-1156.959,-1005.871;Inherit;False;InstancedProperty;_ColorB;Color B;2;1;[HDR];Create;True;0;0;0;False;0;False;0.0660376,0.636061,1,0;2,1.645188,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;51;-1170.18,-756.9575;Inherit;False;InstancedProperty;_ColorC;Color C;3;1;[HDR];Create;True;0;0;0;False;0;False;0.0660376,0.636061,1,0;2,0,0.8089676,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;39;-1590.136,443.534;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;2;-1077.75,431.9103;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1;-1001.215,725.8779;Inherit;False;Property;_FresnelScale;FresnelScale;0;0;Create;True;0;0;0;False;0;False;2.32;2.67;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-826.0731,-1132.985;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-818.0731,-993.6514;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;-796.0731,-832.9848;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-1400.507,187.2398;Inherit;False;Property;_FlickerMax;FlickerMax;7;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-1386.507,81.23983;Inherit;False;Property;_FlickerMin;FlickerMin;6;0;Create;True;0;0;0;False;0;False;0;0.7;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;45;-1408.736,325.1689;Inherit;False;ConstantBiasScale;-1;;3;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;3;-553.412,497.3509;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;57;-370.0731,-927.6515;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;46;-1035.507,161.2399;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;4;-603.2599,-193.7713;Inherit;False;InstancedProperty;_ColorCore;Color Core;4;0;Create;True;0;0;0;False;0;False;0.7735849,0.6738459,0.6738459,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;17;-265.4578,478.9774;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;58;-256.7397,-677.6514;Inherit;False;False;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;7;-255.2501,-304.6668;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexToFragmentNode;49;-795.6022,66.04778;Inherit;False;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-55.66663,-149.5005;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;969.738,-85.91838;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;AgeOfJoy/Neon_Houdini_Simple;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;36;0;35;0
WireConnection;39;0;36;0
WireConnection;54;0;8;0
WireConnection;54;1;53;1
WireConnection;55;0;53;2
WireConnection;55;1;50;0
WireConnection;56;0;53;3
WireConnection;56;1;51;0
WireConnection;45;3;39;0
WireConnection;3;0;2;0
WireConnection;3;2;1;0
WireConnection;57;0;54;0
WireConnection;57;1;55;0
WireConnection;57;2;56;0
WireConnection;46;0;47;0
WireConnection;46;1;48;0
WireConnection;46;2;45;0
WireConnection;17;0;3;0
WireConnection;58;0;57;0
WireConnection;7;0;58;0
WireConnection;7;1;4;0
WireConnection;7;2;17;0
WireConnection;49;0;46;0
WireConnection;18;0;7;0
WireConnection;18;1;49;0
WireConnection;0;2;18;0
ASEEND*/
//CHKSM=3B6A129292694D59A437F7DA0C59622FD25A14E0
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
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_NonMetalSmoothness("NonMetal Smoothness", Range( 0 , 1)) = 0
		_MetalColor("Metal Color", Color) = (0.1736085,0.8282693,0.9685534,0)
		_NonMetalColor("NonMetal Color", Color) = (0.1736085,0.8282693,0.9685534,0)
		_FlickerSpeed("FlickerSpeed", Range( 1 , 50)) = 0
		_FlickerMin("FlickerMin", Range( 0 , 0.7)) = 0
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
			float4 vertexColor : COLOR;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform float4 _NonMetalColor;
		uniform float4 _MetalColor;
		uniform float _FresnelScale;
		uniform float _FlickerMin;
		uniform float _FlickerMax;
		uniform float _FlickerSpeed;
		uniform float _Metallic;
		uniform float _NonMetalSmoothness;

		UNITY_INSTANCING_BUFFER_START(AgeOfJoyNeon_Houdini)
			UNITY_DEFINE_INSTANCED_PROP(float4, _ColorA)
#define _ColorA_arr AgeOfJoyNeon_Houdini
			UNITY_DEFINE_INSTANCED_PROP(float4, _ColorCore)
#define _ColorCore_arr AgeOfJoyNeon_Houdini
		UNITY_INSTANCING_BUFFER_END(AgeOfJoyNeon_Houdini)

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 lerpResult14 = lerp( _NonMetalColor , _MetalColor , i.vertexColor.b);
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
			float mulTime36 = _Time.y * _FlickerSpeed;
			float lerpResult46 = lerp( _FlickerMin , _FlickerMax , ( ( sin( mulTime36 ) + 1.0 ) * 0.5 ));
			float4 lerpResult16 = lerp( float4( 0,0,0,0 ) , ( lerpResult7 * lerpResult46 ) , i.vertexColor.r);
			o.Emission = lerpResult16.rgb;
			float lerpResult9 = lerp( _Metallic , 1.0 , i.vertexColor.b);
			o.Metallic = lerpResult9;
			float lerpResult13 = lerp( _NonMetalSmoothness , 0.8 , i.vertexColor.b);
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
Node;AmplifyShaderEditor.RangedFloatNode;35;-2173.65,203.3895;Inherit;False;Property;_FlickerSpeed;FlickerSpeed;7;0;Create;True;0;0;0;False;0;False;0;0;1;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;2;-1007.25,332.9998;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1;-813.9167,493.3333;Inherit;False;Property;_FresnelScale;FresnelScale;0;0;Create;True;0;0;0;False;0;False;2.32;2.67;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;36;-1754.95,180.9895;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;3;-516.5836,168;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;39;-1525.95,210.9894;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;4;-535.9166,-191.6669;Inherit;False;InstancedProperty;_ColorCore;Color Core;2;0;Create;True;0;0;0;False;0;False;0.7735849,0.6738459,0.6738459,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;8;-518.5832,-388.6668;Inherit;False;InstancedProperty;_ColorA;Color A;1;0;Create;True;0;0;0;False;0;False;0.0660376,0.636061,1,0;1,0.6396225,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;17;-269.6666,207.4998;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;45;-1408.736,325.1689;Inherit;False;ConstantBiasScale;-1;;3;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-1400.507,187.2398;Inherit;False;Property;_FlickerMax;FlickerMax;9;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-1386.507,81.23983;Inherit;False;Property;_FlickerMin;FlickerMin;8;0;Create;True;0;0;0;False;0;False;0;0.7;0;0.7;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;7;-255.2501,-304.6668;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;46;-1035.507,161.2399;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;10;-273,529.5;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;28;185.667,-294.8336;Inherit;False;Property;_MetalColor;Metal Color;5;0;Create;True;0;0;0;False;0;False;0.1736085,0.8282693,0.9685534,0;0,0.8226602,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;30;311.0002,-524.5004;Inherit;False;Property;_NonMetalColor;NonMetal Color;6;0;Create;True;0;0;0;False;0;False;0.1736085,0.8282693,0.9685534,0;0.05660377,0.05660377,0.05660377,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;29;-24.33321,736.8329;Inherit;False;Property;_Metallic;Metallic;3;0;Create;True;0;0;0;False;0;False;0;0.633;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;107.0002,445.4997;Inherit;False;Property;_NonMetalSmoothness;NonMetal Smoothness;4;0;Create;True;0;0;0;False;0;False;0;0.758;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-55.66663,-149.5005;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleTimeNode;21;-1327.667,-23.5004;Inherit;False;1;0;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;24;-1105.667,1.16626;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-952.3332,-12.16708;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;26;-800.3332,20.4996;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;16;-30.33325,112.1666;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;9;371.0003,547.5;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;13;394.3334,257.5;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.8;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;14;637.0928,-458.1978;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;27;-504.9999,23.83295;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0.8;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;969.738,-85.91838;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;AgeOfJoy/Neon_Houdini;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;36;0;35;0
WireConnection;3;0;2;0
WireConnection;3;2;1;0
WireConnection;39;0;36;0
WireConnection;17;0;3;0
WireConnection;45;3;39;0
WireConnection;7;0;8;0
WireConnection;7;1;4;0
WireConnection;7;2;17;0
WireConnection;46;0;47;0
WireConnection;46;1;48;0
WireConnection;46;2;45;0
WireConnection;18;0;7;0
WireConnection;18;1;46;0
WireConnection;24;0;21;0
WireConnection;25;0;24;0
WireConnection;26;0;25;0
WireConnection;16;1;18;0
WireConnection;16;2;10;1
WireConnection;9;0;29;0
WireConnection;9;2;10;3
WireConnection;13;0;31;0
WireConnection;13;2;10;3
WireConnection;14;0;30;0
WireConnection;14;1;28;0
WireConnection;14;2;10;3
WireConnection;27;2;26;0
WireConnection;0;0;14;0
WireConnection;0;2;16;0
WireConnection;0;3;9;0
WireConnection;0;4;13;0
ASEEND*/
//CHKSM=C5CAD55E78C955FA25B8DDF82709067A8A07FD94
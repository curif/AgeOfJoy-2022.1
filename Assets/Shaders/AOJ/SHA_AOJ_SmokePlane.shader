// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/SmokePlane"
{
	Properties
	{
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_Texture0("Texture 0", 2D) = "white" {}
		_Color1("Color 1", Color) = (0,0.4753864,1,0)
		_Color0("Color 0", Color) = (0,0.4753864,1,0)
		_FlickerSpeed("FlickerSpeed", Range( 1 , 50)) = 0
		_FlickerMin("FlickerMin", Range( 0 , 1)) = 0
		_FlickerMax("FlickerMax", Range( 0 , 10)) = 0
		_SmokeTilings("SmokeTilings", Vector) = (1.2,1.2,1,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 vertexToFrag36;
			float2 vertexToFrag37;
			float2 uv_texcoord;
		};

		uniform float4 _Color0;
		uniform float4 _Color1;
		uniform sampler2D _Texture0;
		uniform float4 _SmokeTilings;
		uniform sampler2D _TextureSample1;
		uniform float4 _TextureSample1_ST;
		uniform float _FlickerMin;
		uniform float _FlickerMax;
		uniform float _FlickerSpeed;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 appendResult31 = (float2(_SmokeTilings.x , _SmokeTilings.y));
			o.vertexToFrag36 = ( v.texcoord.xy * appendResult31 );
			float2 uv_TexCoord4 = v.texcoord.xy * float2( 1.2,1.2 );
			float2 appendResult33 = (float2(_SmokeTilings.z , _SmokeTilings.w));
			o.vertexToFrag37 = ( uv_TexCoord4 * appendResult33 );
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 panner9 = ( 1.0 * _Time.y * float2( 0.05,0.1 ) + i.vertexToFrag36);
			float4 tex2DNode14 = tex2D( _Texture0, panner9 );
			float4 lerpResult35 = lerp( _Color0 , _Color1 , tex2DNode14.b);
			o.Emission = lerpResult35.rgb;
			float2 panner2 = ( 1.0 * _Time.y * float2( -0.05,-0.1 ) + i.vertexToFrag37);
			float2 uv_TextureSample1 = i.uv_texcoord * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
			float mulTime20 = _Time.y * _FlickerSpeed;
			float lerpResult23 = lerp( _FlickerMin , _FlickerMax , ( ( sin( fmod( mulTime20 , 120.0 ) ) + 1.0 ) * 0.5 ));
			o.Alpha = ( ( tex2DNode14.g * tex2D( _Texture0, panner2 ).g * tex2D( _TextureSample1, uv_TextureSample1 ).g ) * lerpResult23 );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

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
				float4 customPack1 : TEXCOORD1;
				float2 customPack2 : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
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
				o.customPack1.xy = customInputData.vertexToFrag36;
				o.customPack1.zw = customInputData.vertexToFrag37;
				o.customPack2.xy = customInputData.uv_texcoord;
				o.customPack2.xy = v.texcoord;
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
				surfIN.vertexToFrag36 = IN.customPack1.xy;
				surfIN.vertexToFrag37 = IN.customPack1.zw;
				surfIN.uv_texcoord = IN.customPack2.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
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
Node;AmplifyShaderEditor.Vector4Node;30;-1866.292,-10.4942;Inherit;False;Property;_SmokeTilings;SmokeTilings;7;0;Create;True;0;0;0;False;0;False;1.2,1.2,1,1;1.5,3,1,2.5;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;31;-1633.292,-28.4942;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-1740.827,-167.1689;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;33;-1623.292,93.5058;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-1764,203.1666;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1.2,1.2;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;19;-1727.397,1009.692;Inherit;False;Property;_FlickerSpeed;FlickerSpeed;4;0;Create;True;0;0;0;False;0;False;0;5;1;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-1491.292,-130.4942;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-1368.292,175.5058;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;20;-1375.365,1009.292;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;36;-1308.292,-229.994;Inherit;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexToFragmentNode;37;-1176.292,217.006;Inherit;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FmodOpNode;46;-1148.759,1016.621;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;120;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;9;-1130.217,-101.5586;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.05,0.1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;12;-717.5076,-518.2803;Inherit;True;Property;_Texture0;Texture 0;1;0;Create;True;0;0;0;False;0;False;None;a11563a4124b96c4db654a9ea7217d2c;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.PannerNode;2;-878,222.1667;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.05,-0.1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SinOpNode;21;-950.3647,1007.958;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-456.255,880.8756;Inherit;False;Property;_FlickerMax;FlickerMax;6;0;Create;True;0;0;0;False;0;False;0;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-442.255,775.8756;Inherit;False;Property;_FlickerMin;FlickerMin;5;0;Create;True;0;0;0;False;0;False;0;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;14;-349.7671,-213.0958;Inherit;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;0;False;0;False;-1;None;a11563a4124b96c4db654a9ea7217d2c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;15;-337.6645,-12.32056;Inherit;True;Property;_TextureSample2;Texture Sample 0;2;0;Create;True;0;0;0;False;0;False;-1;None;a11563a4124b96c4db654a9ea7217d2c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;6;-436.8085,274.4599;Inherit;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;0;False;0;False;-1;None;29457f7fa74e71c46a70bf45345e8884;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;25;-865.1508,775.4717;Inherit;False;ConstantBiasScale;-1;;10;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;23;-91.255,854.8757;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-27.33086,-54.55734;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;34;-87.29199,-558.3275;Inherit;False;Property;_Color1;Color 1;2;0;Create;True;0;0;0;False;0;False;0,0.4753864,1,0;0,0.7420225,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;5;-103.4653,-766.0062;Inherit;False;Property;_Color0;Color 0;3;0;Create;True;0;0;0;False;0;False;0,0.4753864,1,0;0,0.112698,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;285.708,3.672424;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;35;242.708,-551.3275;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;523.045,-195.0991;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;AgeOfJoy/SmokePlane;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;31;0;30;1
WireConnection;31;1;30;2
WireConnection;33;0;30;3
WireConnection;33;1;30;4
WireConnection;28;0;8;0
WireConnection;28;1;31;0
WireConnection;29;0;4;0
WireConnection;29;1;33;0
WireConnection;20;0;19;0
WireConnection;36;0;28;0
WireConnection;37;0;29;0
WireConnection;46;0;20;0
WireConnection;9;0;36;0
WireConnection;2;0;37;0
WireConnection;21;0;46;0
WireConnection;14;0;12;0
WireConnection;14;1;9;0
WireConnection;15;0;12;0
WireConnection;15;1;2;0
WireConnection;25;3;21;0
WireConnection;23;0;24;0
WireConnection;23;1;22;0
WireConnection;23;2;25;0
WireConnection;18;0;14;2
WireConnection;18;1;15;2
WireConnection;18;2;6;2
WireConnection;27;0;18;0
WireConnection;27;1;23;0
WireConnection;35;0;5;0
WireConnection;35;1;34;0
WireConnection;35;2;14;3
WireConnection;0;2;35;0
WireConnection;0;9;27;0
ASEEND*/
//CHKSM=1C8874585A1CEC020BE80D9C4914918153F218D5
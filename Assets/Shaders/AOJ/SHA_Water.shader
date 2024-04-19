// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SHA_Water"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "bump" {}
		_Color0("Color 0", Color) = (1,0,0,0)
		_Opacity("Opacity", Range( 0 , 1)) = 0.15
		_Smooth("Smooth", Range( 0 , 1)) = 1
		_Color1("Color 1", Color) = (0,1,0.9968204,0)
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
			float2 uv_texcoord;
		};

		uniform sampler2D _TextureSample0;
		uniform float4 _Color0;
		uniform float4 _Color1;
		uniform float _Smooth;
		uniform float _Opacity;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 temp_output_34_0_g5 = ( v.texcoord.xy - float2( 0.3,0.5 ) );
			float2 break39_g5 = temp_output_34_0_g5;
			float2 appendResult50_g5 = (float2(( 0.5 * ( length( temp_output_34_0_g5 ) * 2.0 ) ) , ( ( atan2( break39_g5.x , break39_g5.y ) * ( 1.0 / 6.28318548202515 ) ) * 0.5 )));
			float2 break53_g5 = appendResult50_g5;
			float mulTime233 = _Time.y * 0.25;
			float temp_output_333_0 = ( ( sin( ( ( saturate( ( 1.0 - break53_g5.x ) ) + mulTime233 ) * 25.0 ) ) + 1.0 ) / 2.0 );
			v.vertex.xyz += ( temp_output_333_0 * float3(0,0.0003,0) );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord9 = i.uv_texcoord * float2( 2,1 );
			float2 panner8 = ( 1.0 * _Time.y * float2( -0.05,0.01 ) + uv_TexCoord9);
			float3 tex2DNode1 = UnpackNormal( tex2D( _TextureSample0, panner8 ) );
			o.Normal = tex2DNode1;
			float4 lerpResult480 = lerp( _Color0 , _Color1 , tex2DNode1.r);
			o.Albedo = lerpResult480.rgb;
			o.Smoothness = _Smooth;
			float temp_output_5_0 = _Opacity;
			o.Alpha = temp_output_5_0;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

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
Node;AmplifyShaderEditor.FunctionNode;322;-2042.832,860.2789;Inherit;False;Polar Coordinates;-1;;5;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.3,0.5;False;3;FLOAT;0.5;False;4;FLOAT;0.5;False;3;FLOAT2;0;FLOAT;55;FLOAT;56
Node;AmplifyShaderEditor.OneMinusNode;236;-1769.499,847.6122;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;191;-2234.832,716.2791;Inherit;False;1652.457;450.1009;Ripples;4;232;323;298;233;;0,0,0,1;0;0
Node;AmplifyShaderEditor.SaturateNode;300;-1336.166,771.6122;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;233;-1247.498,936.2787;Inherit;False;1;0;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;323;-1114.604,798.4587;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;298;-964.1652,782.2789;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;25;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1011,52.83329;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;2,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;232;-778.8314,764.2789;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;8;-709.0001,141.5;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.05,0.01;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;332;-585.2706,747.792;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-481.0001,172.8333;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;2e7df1fb15fc2af439af22ebf06d4591;2e7df1fb15fc2af439af22ebf06d4591;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;333;-476.6039,743.1253;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;4;-419.6667,-525.8333;Inherit;False;Property;_Color0;Color 0;1;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;479;-439.2188,-319.855;Inherit;False;Property;_Color1;Color 1;4;0;Create;True;0;0;0;False;0;False;0,1,0.9968204,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;330;-274.6039,839.1252;Inherit;False;Constant;_Vector0;Vector 0;4;0;Create;True;0;0;0;False;0;False;0,0.0003,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;488;-142.8467,-118.963;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;489;31.15332,-149.6296;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-121.6668,252.1667;Inherit;False;Property;_Opacity;Opacity;2;0;Create;True;0;0;0;False;0;False;0.15;0.162;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-422.3334,535.5001;Inherit;False;Property;_Smooth;Smooth;3;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;329;-131.9373,653.7921;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;480;-69.88538,-469.1884;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-73.00006,501.5001;Inherit;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;486;157.1538,308.7036;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;487;238.4871,-29.29639;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;604.6667,-247.3334;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SHA_Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;236;0;322;55
WireConnection;300;0;236;0
WireConnection;323;0;300;0
WireConnection;323;1;233;0
WireConnection;298;0;323;0
WireConnection;232;0;298;0
WireConnection;8;0;9;0
WireConnection;332;0;232;0
WireConnection;1;1;8;0
WireConnection;333;0;332;0
WireConnection;488;0;1;1
WireConnection;489;0;488;0
WireConnection;329;0;333;0
WireConnection;329;1;330;0
WireConnection;480;0;4;0
WireConnection;480;1;479;0
WireConnection;480;2;1;1
WireConnection;486;0;5;0
WireConnection;486;1;333;0
WireConnection;0;0;480;0
WireConnection;0;1;1;0
WireConnection;0;4;6;0
WireConnection;0;9;5;0
WireConnection;0;11;329;0
ASEEND*/
//CHKSM=1531055491551CE3AFBBECF4062E9904BF755376
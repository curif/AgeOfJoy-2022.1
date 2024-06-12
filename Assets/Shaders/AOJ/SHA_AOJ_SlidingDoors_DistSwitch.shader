// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/SlidingDoors_DistSwitch"
{
	Properties
	{
		_RoomCubemap("RoomCubemap", CUBE) = "black" {}
		_SmaskRaidus("SmaskRaidus", Range( 0 , 20)) = 20
		_Bias("Bias", Range( -5 , 5)) = -1
		_ChangeDistance("ChangeDistance", Range( 0 , 20)) = 9.7
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[Toggle]_useTextureMask("useTextureMask", Float) = 0
		[Toggle]_bypassSphereMask("bypassSphereMask", Float) = 0
		_RoomCubemap_Back("RoomCubemap_Back", CUBE) = "black" {}
		_NormalsDistance("NormalsDistance", 2D) = "bump" {}
		_DecalSmooth("DecalSmooth", Range( 0 , 1)) = 0
		_Texture_Diffuse("Texture_Diffuse", 2D) = "white" {}
		_Texture_Diffuse2("Texture_Diffuse2", 2D) = "white" {}
		_Texture_ORM("Texture_ORM", 2D) = "white" {}
		_NormalsTiling("NormalsTiling", Vector) = (2,2,0,0)
		_GlassDistort("GlassDistort", Range( 0 , 1)) = 0.5
		_RotationAngleFront("RotationAngleFront", Range( -6.28 , 6.28)) = 0.5
		_RotationAngleBack("RotationAngleBack", Range( -6.23 , 6.28)) = 0.5
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "DisableBatching" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
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
			float3 worldPos;
			float4 vertexColor : COLOR;
			half3 viewDir;
			INTERNAL_DATA
			float2 uv2_texcoord2;
		};

		uniform sampler2D _Texture_Diffuse;
		uniform half4 _Texture_Diffuse_ST;
		uniform sampler2D _Texture_Diffuse2;
		uniform half4 _Texture_Diffuse2_ST;
		uniform half _bypassSphereMask;
		uniform half _useTextureMask;
		uniform half _SmaskRaidus;
		uniform half _Bias;
		uniform half _ChangeDistance;
		uniform sampler2D _TextureSample0;
		uniform samplerCUBE _RoomCubemap;
		uniform sampler2D _NormalsDistance;
		uniform half2 _NormalsTiling;
		uniform half _GlassDistort;
		uniform half _RotationAngleFront;
		uniform half _RotationAngleBack;
		uniform samplerCUBE _RoomCubemap_Back;
		uniform sampler2D _Texture_ORM;
		uniform half4 _Texture_ORM_ST;
		uniform half _DecalSmooth;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Normal = float3(0,0,1);
			float2 uv_Texture_Diffuse = i.uv_texcoord * _Texture_Diffuse_ST.xy + _Texture_Diffuse_ST.zw;
			half4 tex2DNode49 = tex2D( _Texture_Diffuse, uv_Texture_Diffuse );
			float2 uv_Texture_Diffuse2 = i.uv_texcoord * _Texture_Diffuse2_ST.xy + _Texture_Diffuse2_ST.zw;
			half4 tex2DNode148 = tex2D( _Texture_Diffuse2, uv_Texture_Diffuse2 );
			float3 ase_worldPos = i.worldPos;
			half3 _Vector0 = half3(7.063,1.434,-9.331);
			half saferPower7_g1 = abs( ( ( distance( _Vector0 , _WorldSpaceCameraPos ) + _Bias ) / _ChangeDistance ) );
			half temp_output_8_0_g1 = saturate( pow( saferPower7_g1 , 3.0 ) );
			half lerpResult12_g1 = lerp( _SmaskRaidus , 0.0 , temp_output_8_0_g1);
			half3 temp_output_5_0_g4 = ( ( ase_worldPos - _Vector0 ) / lerpResult12_g1 );
			half dotResult8_g4 = dot( temp_output_5_0_g4 , temp_output_5_0_g4 );
			half temp_output_14_0_g1 = pow( saturate( dotResult8_g4 ) , 5.0 );
			half temp_output_150_0 = (( _bypassSphereMask )?( temp_output_8_0_g1 ):( (( _useTextureMask )?( temp_output_14_0_g1 ):( saturate( ( ( temp_output_14_0_g1 * 2.0 ) - tex2D( _TextureSample0, ( i.uv_texcoord * half2( 4,4 ) ) ).g ) ) )) ));
			half4 lerpResult151 = lerp( tex2DNode49 , tex2DNode148 , temp_output_150_0);
			half4 lerpResult9 = lerp( lerpResult151 , float4( 0,0,0,0 ) , i.vertexColor.b);
			half lerpResult149 = lerp( tex2DNode49.a , tex2DNode148.a , temp_output_150_0);
			half4 lerpResult57 = lerp( lerpResult9 , lerpResult151 , lerpResult149);
			o.Albedo = lerpResult57.rgb;
			float2 uv_TexCoord38 = i.uv_texcoord * float2( 4,4 );
			half3 tex2DNode36 = UnpackNormal( tex2D( _NormalsDistance, ( uv_TexCoord38 * _NormalsTiling ) ) );
			half lerpResult146 = lerp( _RotationAngleFront , _RotationAngleBack , i.vertexColor.g);
			half temp_output_132_0 = cos( lerpResult146 );
			half temp_output_131_0 = sin( lerpResult146 );
			half3 appendResult138 = (half3(( ( i.viewDir.x * temp_output_132_0 ) + ( i.viewDir.z * temp_output_131_0 ) ) , i.viewDir.y , ( -( i.viewDir.x * temp_output_131_0 ) + ( i.viewDir.z * temp_output_132_0 ) )));
			half3 temp_output_102_0 = ( appendResult138 * float3( -1,1,1 ) );
			half3 temp_output_104_0 = ( float3( 1,1,1 ) / temp_output_102_0 );
			half3 appendResult103 = (half3(( ( frac( i.uv2_texcoord2 ) * float2( 2,-2 ) ) - float2( 1,-1 ) ) , -1.0));
			half3 break108 = ( abs( temp_output_104_0 ) - ( temp_output_104_0 * appendResult103 ) );
			half3 temp_output_34_0 = ( ( tex2DNode36 * _GlassDistort ) + ( ( ( min( min( break108.x , break108.y ) , break108.z ) * temp_output_102_0 ) + appendResult103 ) * half3(-1,1,1) ) );
			half4 lerpResult144 = lerp( texCUBE( _RoomCubemap, temp_output_34_0 ) , texCUBE( _RoomCubemap_Back, temp_output_34_0 ) , i.vertexColor.g);
			half4 lerpResult48 = lerp( float4( 0,0,0,0 ) , lerpResult144 , i.vertexColor.b);
			o.Emission = ( lerpResult48 * ( 1.0 - lerpResult149 ) ).rgb;
			float2 uv_Texture_ORM = i.uv_texcoord * _Texture_ORM_ST.xy + _Texture_ORM_ST.zw;
			half4 tex2DNode76 = tex2D( _Texture_ORM, uv_Texture_ORM );
			half lerpResult71 = lerp( tex2DNode76.b , 0.0 , lerpResult149);
			o.Metallic = lerpResult71;
			half lerpResult70 = lerp( ( 1.0 - tex2DNode76.g ) , _DecalSmooth , lerpResult149);
			o.Smoothness = lerpResult70;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows noinstancing 

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
				o.customPack1.zw = customInputData.uv2_texcoord2;
				o.customPack1.zw = v.texcoord1;
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
				surfIN.uv2_texcoord2 = IN.customPack1.zw;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = IN.tSpace0.xyz * worldViewDir.x + IN.tSpace1.xyz * worldViewDir.y + IN.tSpace2.xyz * worldViewDir.z;
				surfIN.worldPos = worldPos;
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
Node;AmplifyShaderEditor.RangedFloatNode;130;-6347.094,-1302.958;Inherit;False;Property;_RotationAngleFront;RotationAngleFront;16;0;Create;True;0;0;0;False;0;False;0.5;0;-6.28;6.28;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;145;-6313.779,-1188.2;Inherit;False;Property;_RotationAngleBack;RotationAngleBack;17;0;Create;True;0;0;0;False;0;False;0.5;1;-6.23;6.28;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;147;-6217.651,-1064.177;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;146;-5988.654,-1295.469;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;133;-5813.009,-1557.946;Inherit;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SinOpNode;131;-5659.301,-1239.461;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;116;-5420.548,-1155.19;Inherit;False;2906.961;1447.64;Interior Mapping;18;96;102;97;99;101;117;119;112;111;110;109;108;107;106;105;104;103;141;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CosOpNode;132;-5679.301,-1397.46;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;139;-5319.765,-1378.364;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;-5380.431,-1697.697;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;140;-5177.097,-1376.364;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;141;-5204.431,-1209.031;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;135;-5385.764,-1576.364;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;96;-5076.264,-723.8438;Inherit;False;1;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FractNode;97;-4761.357,-749.7042;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;136;-5103.764,-1659.03;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;142;-4970.431,-1289.697;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-4620.92,-749.8164;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;2,-2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;138;-4843.098,-1488.364;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;101;-4451.279,-749.7113;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;1,-1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;102;-4499.814,-945.6333;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;-1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;103;-4268.277,-748.7113;Inherit;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;-1;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;104;-4256.841,-1019.216;Inherit;False;2;0;FLOAT3;1,1,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;105;-4116.211,-860.3225;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.AbsOpNode;106;-4098.046,-1019.715;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;107;-3941.637,-1019.216;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;108;-3794.773,-1022.465;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMinOpNode;109;-3652.773,-1023.465;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;110;-3500.773,-996.4646;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;38;-3842.223,-1932.954;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;4,4;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;88;-3815.461,-1743.226;Inherit;False;Property;_NormalsTiling;NormalsTiling;14;0;Create;True;0;0;0;False;0;False;2,2;2,2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;111;-3337.772,-973.5415;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;-3368.96,-1879.291;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;112;-3166.973,-778.6353;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;91;-2735.463,-1654.99;Inherit;False;Property;_GlassDistort;GlassDistort;15;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;36;-3037.096,-1905.571;Inherit;True;Property;_NormalsDistance;NormalsDistance;9;0;Create;True;0;0;0;False;0;False;-1;None;2e7df1fb15fc2af439af22ebf06d4591;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;119;-3056.176,-678.854;Inherit;False;Constant;_Vector7;Vector 7;9;0;Create;True;0;0;0;False;0;False;-1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;117;-2763.509,-798.854;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-2656.195,-1462.837;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.3;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-2007.564,-977.6804;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;1;-1294.228,-72.48106;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;143;-1680.867,-927.1766;Inherit;True;Property;_RoomCubemap_Back;RoomCubemap_Back;8;0;Create;True;0;0;0;False;0;False;-1;1064b3af7c0210041ac63a494868bdef;1dfe4e7bf47a0cc49918e27ed9d6af91;True;0;False;black;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;35;-1681.069,-1119.406;Inherit;True;Property;_RoomCubemap;RoomCubemap;0;0;Create;True;0;0;0;False;0;False;-1;1064b3af7c0210041ac63a494868bdef;47459a021f09a7c4296bc9c1184047b4;True;0;False;black;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;49;-2450.906,-450.4217;Inherit;True;Property;_Texture_Diffuse;Texture_Diffuse;11;0;Create;True;0;0;0;False;0;False;-1;None;dde2996015b322c4899060b7213f12b1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;148;-2221.493,-175.1945;Inherit;True;Property;_Texture_Diffuse2;Texture_Diffuse2;12;0;Create;True;0;0;0;False;0;False;-1;None;dde2996015b322c4899060b7213f12b1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;150;-2011.493,59.47217;Inherit;False;SHAF_DistanceSwitch;1;;1;093fd74bb1b269c45832e583a601c99c;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;144;-1195.801,-994.6722;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;149;-1753.493,-179.8612;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;151;-1728.16,-457.1945;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;58;-852.0833,-749.2839;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;85;-1385.898,-246.8375;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;82;-1392.565,-302.1707;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;9;-325.6674,-982.8701;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;76;-669.1334,-1475.37;Inherit;True;Property;_Texture_ORM;Texture_ORM;13;0;Create;True;0;0;0;False;0;False;-1;None;626d7d4c9b59e8b489ad563f10c216cd;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;61;699.1879,-176.0157;Inherit;False;228;210;Normal lerp - Alpha = Frame;1;59;;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;52;219.4812,-297.6888;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;77;-58.31451,-1151.946;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;81;-1407.232,-314.8375;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;84;-1394.565,-278.1708;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;46;260.5606,-1496.473;Inherit;False;Constant;_MetallicDecal;MetallicDecal;6;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;57;91.11432,-818.1927;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;47;339.5966,-1017.217;Inherit;False;Property;_DecalSmooth;DecalSmooth;10;0;Create;True;0;0;0;False;0;False;0;0.885;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;48;88.61619,-656.7098;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;74;-2467.669,-1711.418;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;66;-436.145,-4.015808;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;86;-1388.565,-222.8375;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;60;505.8547,56.65102;Inherit;False;Constant;_Vector0;Vector 0;8;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;59;749.1879,-126.0157;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;494.8147,-405.022;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;67;676.5658,-728.6603;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;70;804.509,-1055.398;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;71;776.8518,-1371.385;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;65;308.5216,-64.01581;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;36.52148,-77.34918;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1593.874,-333.5654;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;AgeOfJoy/SlidingDoors_DistSwitch;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;146;0;130;0
WireConnection;146;1;145;0
WireConnection;146;2;147;2
WireConnection;131;0;146;0
WireConnection;132;0;146;0
WireConnection;139;0;133;1
WireConnection;139;1;131;0
WireConnection;134;0;133;1
WireConnection;134;1;132;0
WireConnection;140;0;139;0
WireConnection;141;0;133;3
WireConnection;141;1;132;0
WireConnection;135;0;133;3
WireConnection;135;1;131;0
WireConnection;97;0;96;0
WireConnection;136;0;134;0
WireConnection;136;1;135;0
WireConnection;142;0;140;0
WireConnection;142;1;141;0
WireConnection;99;0;97;0
WireConnection;138;0;136;0
WireConnection;138;1;133;2
WireConnection;138;2;142;0
WireConnection;101;0;99;0
WireConnection;102;0;138;0
WireConnection;103;0;101;0
WireConnection;104;1;102;0
WireConnection;105;0;104;0
WireConnection;105;1;103;0
WireConnection;106;0;104;0
WireConnection;107;0;106;0
WireConnection;107;1;105;0
WireConnection;108;0;107;0
WireConnection;109;0;108;0
WireConnection;109;1;108;1
WireConnection;110;0;109;0
WireConnection;110;1;108;2
WireConnection;111;0;110;0
WireConnection;111;1;102;0
WireConnection;87;0;38;0
WireConnection;87;1;88;0
WireConnection;112;0;111;0
WireConnection;112;1;103;0
WireConnection;36;1;87;0
WireConnection;117;0;112;0
WireConnection;117;1;119;0
WireConnection;45;0;36;0
WireConnection;45;1;91;0
WireConnection;34;0;45;0
WireConnection;34;1;117;0
WireConnection;143;1;34;0
WireConnection;35;1;34;0
WireConnection;144;0;35;0
WireConnection;144;1;143;0
WireConnection;144;2;1;2
WireConnection;149;0;49;4
WireConnection;149;1;148;4
WireConnection;149;2;150;0
WireConnection;151;0;49;0
WireConnection;151;1;148;0
WireConnection;151;2;150;0
WireConnection;58;0;144;0
WireConnection;85;0;149;0
WireConnection;82;0;149;0
WireConnection;9;0;151;0
WireConnection;9;2;1;3
WireConnection;52;0;85;0
WireConnection;77;0;76;2
WireConnection;81;0;149;0
WireConnection;84;0;149;0
WireConnection;57;0;9;0
WireConnection;57;1;151;0
WireConnection;57;2;82;0
WireConnection;48;1;58;0
WireConnection;48;2;1;3
WireConnection;74;0;36;0
WireConnection;66;0;1;3
WireConnection;86;0;149;0
WireConnection;59;0;74;0
WireConnection;59;1;60;0
WireConnection;59;2;65;0
WireConnection;51;0;48;0
WireConnection;51;1;52;0
WireConnection;67;0;57;0
WireConnection;70;0;77;0
WireConnection;70;1;47;0
WireConnection;70;2;84;0
WireConnection;71;0;76;3
WireConnection;71;1;46;0
WireConnection;71;2;81;0
WireConnection;65;0;63;0
WireConnection;63;0;86;0
WireConnection;63;1;66;0
WireConnection;0;0;67;0
WireConnection;0;2;51;0
WireConnection;0;3;71;0
WireConnection;0;4;70;0
ASEEND*/
//CHKSM=98C1A20932E07EC14AB120AC94D7D1966AA59619
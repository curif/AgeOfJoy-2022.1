// Upgrade NOTE: upgraded instancing buffer 'AgeOfJoyCRT_01_Additive' to new syntax.

// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/CRT_01_Additive"
{
	Properties
	{
		_Damage_Vignette_Hardness("Damage_Vignette_Hardness", Range( 0 , 1)) = 1
		_Damage_Desaturation("Damage_Desaturation", Range( -0.5 , 1)) = 0
		_Damage_VIgnette_Radius("Damage_VIgnette_Radius", Range( 0 , 1)) = 0
		_MainTex("MainTex", 2D) = "black" {}
		_Scanline_GameScreenBrightness("Scanline_GameScreenBrightness", Range( 0 , 10)) = 2.5
		_Scanline_Amount("Scanline_Amount", Range( 0 , 1)) = 1
		_Distance_Scanline("Distance_Scanline", Range( 0 , 5)) = 1
		_Distance_DotMask("Distance_DotMask", Range( 0 , 1)) = 0.15
		_Distance_Scanline_Power("Distance_Scanline_Power", Float) = 5
		_Distance_DotMask_Power("Distance_DotMask_Power", Float) = 5
		_Dotmask_GameScreenBrightness("Dotmask_GameScreenBrightness", Range( 0 , 10)) = 1
		_Dotmask_ScanlineRemoval("Dotmask_ScanlineRemoval", Range( 0 , 1)) = 0
		_CRTBrightnessFlickerMax("CRT Brightness Flicker Max", Range( -0.5 , 0.5)) = 0.1
		_CRTBrightnessFlickerMin("CRT Brightness Flicker Min", Range( -0.5 , 0.5)) = 0.1
		_CRTBrightnessFlickerTime("CRT Brightness Flicker Time", Range( 0 , 50)) = 0
		_Damage_RGB_Offset("Damage_RGB_Offset", Vector) = (1,1,1,0)
		_MaskVTXRedOnly("MaskVTXRedOnly", Range( 0 , 1)) = 0
		_CRTParameters("CRTParameters", Vector) = (256,256,1,0.59158)
		_CRTTiling("CRTTiling", Vector) = (1,1,0,0)
		_Dotmask_Scanlines("Dotmask_Scanlines", 2D) = "white" {}
		[Toggle(_ISPROJECTIONSCREEN_ON)] _isProjectionScreen("isProjectionScreen", Float) = 0
		_MipBias("MipBias", Range( -1 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		Blend One One , One One
		BlendOp Add
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature_local _ISPROJECTIONSCREEN_ON
		#pragma surface surf Unlit keepalpha noshadow exclude_path:deferred vertex:vertexDataFunc 
		struct Input
		{
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
			half vertexToFrag250;
			float3 worldPos;
			half2 vertexToFrag251;
			half3 viewDir;
			INTERNAL_DATA
			half3 worldNormal;
		};

		uniform half _MaskVTXRedOnly;
		uniform sampler2D _MainTex;
		uniform half2 _CRTTiling;
		uniform half3 _Damage_RGB_Offset;
		uniform half _Damage_VIgnette_Radius;
		uniform half _Damage_Vignette_Hardness;
		uniform half _Damage_Desaturation;
		uniform half _CRTBrightnessFlickerMin;
		uniform half _CRTBrightnessFlickerMax;
		uniform half _CRTBrightnessFlickerTime;
		uniform half _Dotmask_GameScreenBrightness;
		uniform half _Distance_DotMask;
		uniform half _Distance_DotMask_Power;
		uniform half _Scanline_GameScreenBrightness;
		uniform sampler2D _Dotmask_Scanlines;
		uniform half4 _CRTParameters;
		uniform half _Scanline_Amount;
		uniform half _Dotmask_ScanlineRemoval;
		uniform half _Distance_Scanline;
		uniform half _Distance_Scanline_Power;

		UNITY_INSTANCING_BUFFER_START(AgeOfJoyCRT_01_Additive)
			UNITY_DEFINE_INSTANCED_PROP(half, _MipBias)
#define _MipBias_arr AgeOfJoyCRT_01_Additive
		UNITY_INSTANCING_BUFFER_END(AgeOfJoyCRT_01_Additive)

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			half mulTime203 = _Time.y * _CRTBrightnessFlickerTime;
			half lerpResult196 = lerp( _CRTBrightnessFlickerMin , _CRTBrightnessFlickerMax , ( ( sin( mulTime203 ) + 1.0 ) * 0.5 ));
			o.vertexToFrag250 = lerpResult196;
			half2 appendResult128 = (half2(1.0 , ( _CRTParameters.y / 32.0 )));
			float2 uv_TexCoord2 = v.texcoord.xy * appendResult128;
			o.vertexToFrag251 = uv_TexCoord2;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			half lerpResult233 = lerp( 1.0 , i.vertexColor.r , _MaskVTXRedOnly);
			half _MipBias_Instance = UNITY_ACCESS_INSTANCED_PROP(_MipBias_arr, _MipBias);
			half3 desaturateInitialColor185 = ( ( (pow( tex2Dbias( _MainTex, float4( ( i.uv_texcoord * _CRTTiling ), 0, _MipBias_Instance) ) , 2.0 )).rgb * _Damage_RGB_Offset ) * ( 1.0 - ( saturate( ( distance( i.uv_texcoord , half2( 0.5,0.5 ) ) - _Damage_VIgnette_Radius ) ) / ( 1.0 - _Damage_Vignette_Hardness ) ) ) );
			half desaturateDot185 = dot( desaturateInitialColor185, float3( 0.299, 0.587, 0.114 ));
			half3 desaturateVar185 = lerp( desaturateInitialColor185, desaturateDot185.xxx, _Damage_Desaturation );
			half3 temp_output_126_0 = ( desaturateVar185 + i.vertexToFrag250 );
			float3 ase_worldPos = i.worldPos;
			half temp_output_263_0 = saturate( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / _Distance_DotMask ) , _Distance_DotMask_Power ) );
			half lerpResult93 = lerp( _Dotmask_GameScreenBrightness , 1.0 , temp_output_263_0);
			half lerpResult101 = lerp( _Dotmask_ScanlineRemoval , 0.0 , temp_output_263_0);
			half lerpResult18 = lerp( 1.0 , tex2D( _Dotmask_Scanlines, i.vertexToFrag251 ).a , ( _Scanline_Amount - lerpResult101 ));
			half3 temp_output_17_0 = ( ( ( temp_output_126_0 * lerpResult93 ) * _Scanline_GameScreenBrightness ) * saturate( lerpResult18 ) );
			half2 appendResult236 = (half2(_CRTParameters.x , _CRTParameters.y));
			float2 uv_TexCoord61 = i.uv_texcoord * appendResult236;
			half3 lerpResult28 = lerp( temp_output_17_0 , temp_output_126_0 , saturate( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / _Distance_Scanline ) , _Distance_Scanline_Power ) ));
			half4 lerpResult58 = lerp( ( half4( temp_output_17_0 , 0.0 ) * tex2D( _Dotmask_Scanlines, uv_TexCoord61 ) ) , half4( lerpResult28 , 0.0 ) , temp_output_263_0);
			half4 temp_output_218_0 = ( lerpResult233 * lerpResult58 );
			half4 color296 = IsGammaSpace() ? half4(0.0754717,0.0754717,0.0754717,0) : half4(0.006628775,0.006628775,0.006628775,0);
			half3 ase_worldNormal = WorldNormalVector( i, half3( 0, 0, 1 ) );
			half fresnelNdotV286 = dot( normalize( ase_worldNormal ), i.viewDir );
			half fresnelNode286 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV286, 1.0 ) );
			half4 lerpResult283 = lerp( temp_output_218_0 , color296 , saturate( fresnelNode286 ));
			#ifdef _ISPROJECTIONSCREEN_ON
				half4 staticSwitch282 = lerpResult283;
			#else
				half4 staticSwitch282 = temp_output_218_0;
			#endif
			o.Emission = staticSwitch282.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CommentaryNode;136;-5651.142,-81.46181;Inherit;False;1558;351;SphereMask;10;157;158;179;180;156;155;154;153;152;151;;0,0,0,1;0;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;180;-5635.142,-17.46176;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;179;-5602.142,115.5385;Inherit;False;Constant;_SphereCenter;Sphere Center;0;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;192;-6412.66,-867.2523;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;249;-6311.903,-691.4919;Inherit;False;Property;_CRTTiling;CRTTiling;21;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DistanceOpNode;151;-5379.142,-17.46176;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;158;-5379.142,94.53832;Inherit;False;Property;_Damage_VIgnette_Radius;Damage_VIgnette_Radius;3;0;Create;True;0;0;0;False;0;False;0;0.435;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;46;-2599.804,-1052.528;Inherit;False;1297.335;546.6675;Transition DotMask;8;55;54;53;52;51;50;48;263;;0,0.8417869,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;90;-5678.854,-704.6508;Inherit;False;363.3334;277;GameScreen;1;11;;1,0,0.9828625,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;248;-6091.572,-827.1584;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;280;-6019.468,-402.4124;Inherit;False;InstancedProperty;_MipBias;MipBias;23;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;152;-5187.141,-17.46176;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;157;-5409.227,199.1412;Inherit;False;Property;_Damage_Vignette_Hardness;Damage_Vignette_Hardness;1;0;Create;True;0;0;0;False;0;False;1;0.596;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;200;-3784.776,126.5267;Inherit;False;Property;_CRTBrightnessFlickerTime;CRT Brightness Flicker Time;15;0;Create;True;0;0;0;False;0;False;0;35;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;51;-2461.805,-1002.528;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;52;-2549.805,-825.1956;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;11;-5632.861,-639.4778;Inherit;True;Property;_MainTex;MainTex;4;0;Create;True;0;0;0;False;0;False;-1;8f939eab460ea6041bc9d72da92c13a6;e57652f0ab2605e4cb8bae96179146ab;True;0;False;black;Auto;False;Object;-1;MipBias;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;89;-2802.326,414.804;Inherit;False;2502;953.5816;Scanlines;11;98;100;99;16;2;18;102;130;251;255;265;;0.1536936,1,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;103;-4110.326,569.8041;Inherit;False;260;210;Vertical Screen res / 8;1;235;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;189;-4301.94,-519.5765;Inherit;False;603.7002;397.2666;Damage RGB;2;187;188;;0,0,0,1;0;0
Node;AmplifyShaderEditor.SaturateNode;153;-5027.142,-17.46176;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;154;-5027.142,94.53832;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;203;-3503.516,138.4411;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;53;-2215.807,-941.8624;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-2203.773,-763.5289;Inherit;False;Property;_Distance_DotMask;Distance_DotMask;8;0;Create;True;0;0;0;False;0;False;0.15;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;279;-4882.135,-639.0789;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector4Node;235;-4069.761,610.088;Inherit;False;Property;_CRTParameters;CRTParameters;18;0;Create;True;0;0;0;False;0;False;256,256,1,0.59158;320,256,1,0.59158;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;130;-2774.302,670.5531;Inherit;False;471;230.3333;Scanlines technically appearing every other line;2;128;127;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;155;-4835.143,-17.46176;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;187;-4251.94,-305.9766;Inherit;False;Property;_Damage_RGB_Offset;Damage_RGB_Offset;16;0;Create;True;0;0;0;False;0;False;1,1,1;1,1,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SinOpNode;204;-3274.515,168.4411;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;54;-1859.139,-891.8629;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;264;-4419.631,-624.3477;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-2008.139,-644.5283;Inherit;False;Property;_Distance_DotMask_Power;Distance_DotMask_Power;10;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;127;-2742.261,745.0427;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;32;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;190;-3496.94,-495.9763;Inherit;False;558.502;338.8665;Damage Desaturation;2;186;185;;0,0,0,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;156;-4691.142,-17.46176;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;188;-3876.24,-469.5765;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;201;-3131.301,143.6211;Inherit;False;ConstantBiasScale;-1;;3;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;197;-3337.776,-35.47342;Inherit;False;Property;_CRTBrightnessFlickerMax;CRT Brightness Flicker Max;13;0;Create;True;0;0;0;False;0;False;0.1;0.004;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;125;-3345.055,-137.4754;Inherit;False;Property;_CRTBrightnessFlickerMin;CRT Brightness Flicker Min;14;0;Create;True;0;0;0;False;0;False;0.1;0.005;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;48;-1650.472,-858.5297;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;128;-2505.998,705.187;Inherit;False;FLOAT2;4;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;184;-3629.251,-482.074;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;186;-3446.94,-269.7765;Inherit;False;Property;_Damage_Desaturation;Damage_Desaturation;2;0;Create;True;0;0;0;False;0;False;0;0;-0.5;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;196;-2807.776,-151.4734;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;263;-1476.922,-855.4728;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-2197.697,655.5521;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;94;-1190.249,-630.0359;Inherit;False;620;541;Boost Texture when Dotmask Present;5;91;95;93;92;96;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DesaturateOpNode;185;-3148.438,-445.9763;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexToFragmentNode;250;-2857.292,-354.037;Inherit;False;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;102;-1111.793,942.348;Inherit;False;228;210;A=dotmask, B=nodotmask;1;101;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;30;-2559.737,-273.6253;Inherit;False;1297.335;546.6675;Transition Scanline;8;25;26;20;27;21;22;23;259;;0,0.8326645,1,1;0;0
Node;AmplifyShaderEditor.WireNode;262;-1220.922,-710.4729;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-1448.597,993.3577;Inherit;False;Property;_Dotmask_ScanlineRemoval;Dotmask_ScanlineRemoval;12;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;251;-1960.461,661.2367;Inherit;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-1173.237,-473.562;Inherit;False;Property;_Dotmask_GameScreenBrightness;Dotmask_GameScreenBrightness;11;0;Create;True;0;0;0;False;0;False;1;1.21;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;126;-2738.403,-519.7662;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;101;-1065.793,998.348;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;25;-2421.738,-223.6252;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;26;-2509.738,-46.29104;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WireNode;261;-1263.922,-769.4728;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1254.379,769.667;Inherit;False;Property;_Scanline_Amount;Scanline_Amount;6;0;Create;True;0;0;0;False;0;False;1;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;278;-2926.216,-1699.063;Inherit;True;Property;_Dotmask_Scanlines;Dotmask_Scanlines;22;0;Create;True;0;0;0;False;0;False;5691e46c711683e4a80cc7e1322070d0;5691e46c711683e4a80cc7e1322070d0;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CommentaryNode;97;-904.7473,91.75391;Inherit;False;614.1814;256.0048;Boost Texture when Scanlines present;2;31;34;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;92;-911.4401,-244.3924;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;93;-874.7485,-602.4357;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;99;-868.5969,799.6912;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;27;-2175.737,-162.9586;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;265;-873.6919,498.5084;Inherit;False;Constant;_Float0;Float 0;27;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;255;-1713.815,636.4662;Inherit;True;Property;_PackedScanlines;PackedScanlines;26;0;Create;True;0;0;0;False;0;False;-1;b9bcd261404598f49ada1f9d5ec82d41;b9bcd261404598f49ada1f9d5ec82d41;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;20;-2131.704,0.7089081;Inherit;False;Property;_Distance_Scanline;Distance_Scanline;7;0;Create;True;0;0;0;False;0;False;1;0.5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-753.2373,-184.8619;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-854.7473,235.092;Inherit;False;Property;_Scanline_GameScreenBrightness;Scanline_GameScreenBrightness;5;0;Create;True;0;0;0;False;0;False;2.5;1.5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;21;-1819.07,-112.9584;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;236;-3622.761,583.0881;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;18;-705.0249,608.9037;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-2028.07,111.3759;Inherit;False;Property;_Distance_Scanline_Power;Distance_Scanline_Power;9;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-468.5659,141.7539;Inherit;False;2;2;0;FLOAT3;1,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;98;-481.5969,618.691;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;87;-1646.941,-1524.502;Inherit;False;1235.334;406.3926;DotMask;2;61;62;;1,0,0,1;0;0
Node;AmplifyShaderEditor.PowerNode;23;-1571.401,-28.32484;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;131;-2692.291,-1035.823;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-212.6577,136.7485;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;65;84.48536,76.33668;Inherit;False;273;157;Frame and Scanlines;1;64;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;61;-1013.941,-1388.502;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;259;-1409.786,-14.58065;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;88;238.744,-999.3019;Inherit;False;228;186.3333;Multiply Dotmask by Frame;1;76;;1,0.7610062,0.7610062,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;29;250.8946,-222.7503;Inherit;False;415;219;A = Frame and Scanlines, B = Raw Frame;1;28;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;33;-1124.618,-64.38861;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;64;115.8195,140.9099;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;91;-842.5527,-362.1745;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;62;-724.9406,-1424.502;Inherit;True;Property;_TextureSampleDotMask;Texture Sample DotMask;3;0;Create;True;0;0;0;False;0;False;-1;52fa22fcfb11d0e4280b2efa2ffb323b;52fa22fcfb11d0e4280b2efa2ffb323b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;28;300.8942,-172.7503;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;288.744,-949.3019;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;217;1072.291,-1036.747;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;234;1146.291,-1159.914;Inherit;False;Constant;_Float1;Float 1;34;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;231;1004.691,-810.7473;Inherit;False;Property;_MaskVTXRedOnly;MaskVTXRedOnly;17;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;260;-1238.922,-838.4728;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;290;1286.811,-276.6597;Inherit;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;289;1268.144,-506.6597;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;287;1344.344,-117.1264;Inherit;False;Constant;_ProjScreenFresnelView;ProjScreenFresnelView;25;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;58;679.7466,-651.7343;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;233;1431.291,-720.9138;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;286;1596.81,-408.6597;Inherit;False;Standard;WorldNormal;ViewDir;True;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;295;1834.811,-383.993;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;218;1691.958,-687.414;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;296;1641.478,-205.3264;Inherit;False;Constant;_ProjScreenAngleColor_Emissive;ProjScreenAngleColor_Emissive;23;0;Create;True;0;0;0;False;0;False;0.0754717,0.0754717,0.0754717,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;283;1980.143,-476.6598;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;1,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector4Node;241;1396.379,266.7527;Inherit;False;Property;_Dirt_RGBAmount_APower;Dirt_RGBAmount_APower;20;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0.5;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;237;1354.225,7.133547;Inherit;True;Property;_CRTFX;CRTFX;19;0;Create;True;0;0;0;False;0;False;-1;3b4bc173790b8f34e918d52865e6eae0;3b4bc173790b8f34e918d52865e6eae0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;244;1682.639,216.5506;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;242;1679.435,37.10436;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;238;1851.408,100.1243;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;247;2024.442,69.14873;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;245;2161.16,59.5355;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;240;2345.182,-51.11256;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;281;1913.477,-1149.993;Inherit;False;Constant;_AlbedoColor;AlbedoColor;23;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;284;1860.81,-974.6603;Inherit;False;Constant;_ProjScreenAngleColor_Diff;ProjScreenAngleColor_Diff;23;0;Create;True;0;0;0;False;0;False;0.06289297,0.06289297,0.06289297,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;117;2226.787,-344.265;Inherit;False;Constant;_Metallic;Metallic;17;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;298;2240.501,-173.358;Inherit;False;Constant;_ProjScreenMetallic;ProjScreenMetallic;26;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;299;2620.811,-158.6594;Inherit;False;Property;_isProjectionScreen;isProjectionScreen;24;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;285;2465.478,-1096.66;Inherit;False;Property;_isProjectionScreen;isProjectionScreen;24;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;282;2468.144,-691.9933;Inherit;False;Property;_isProjectionScreen;isProjectionScreen;23;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;297;2537.834,-341.3582;Inherit;False;Property;_isProjectionScreen;isProjectionScreen;24;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;300;2532.144,102.0073;Inherit;False;Constant;_ProjScreenSmooth;ProjScreenSmooth;27;0;Create;True;0;0;0;False;0;False;0.25;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;256;2950.003,-628.8728;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;AgeOfJoy/CRT_01_Additive;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;False;0;False;Custom;;Transparent;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;False;4;1;False;;1;False;;4;1;False;;1;False;;1;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.CommentaryNode;45;-215.0649,-1561.633;Inherit;False;514;215;Rolling - can be moving or stationary - take scanline and scroll it over the scanline bit;0;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;84;-194.3853,-1840.56;Inherit;False;514;215;Color Blobs;0;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;85;-216.3853,-2094.561;Inherit;False;514;215;Color Mismatch;0;;1,1,1,1;0;0
WireConnection;151;0;180;0
WireConnection;151;1;179;0
WireConnection;248;0;192;0
WireConnection;248;1;249;0
WireConnection;152;0;151;0
WireConnection;152;1;158;0
WireConnection;11;1;248;0
WireConnection;11;2;280;0
WireConnection;153;0;152;0
WireConnection;154;0;157;0
WireConnection;203;0;200;0
WireConnection;53;0;51;0
WireConnection;53;1;52;0
WireConnection;279;0;11;0
WireConnection;155;0;153;0
WireConnection;155;1;154;0
WireConnection;204;0;203;0
WireConnection;54;0;53;0
WireConnection;54;1;55;0
WireConnection;264;0;279;0
WireConnection;127;0;235;2
WireConnection;156;0;155;0
WireConnection;188;0;264;0
WireConnection;188;1;187;0
WireConnection;201;3;204;0
WireConnection;48;0;54;0
WireConnection;48;1;50;0
WireConnection;128;1;127;0
WireConnection;184;0;188;0
WireConnection;184;1;156;0
WireConnection;196;0;125;0
WireConnection;196;1;197;0
WireConnection;196;2;201;0
WireConnection;263;0;48;0
WireConnection;2;0;128;0
WireConnection;185;0;184;0
WireConnection;185;1;186;0
WireConnection;250;0;196;0
WireConnection;262;0;263;0
WireConnection;251;0;2;0
WireConnection;126;0;185;0
WireConnection;126;1;250;0
WireConnection;101;0;100;0
WireConnection;101;2;262;0
WireConnection;261;0;263;0
WireConnection;92;0;126;0
WireConnection;93;0;96;0
WireConnection;93;2;261;0
WireConnection;99;0;16;0
WireConnection;99;1;101;0
WireConnection;27;0;25;0
WireConnection;27;1;26;0
WireConnection;255;0;278;0
WireConnection;255;1;251;0
WireConnection;95;0;92;0
WireConnection;95;1;93;0
WireConnection;21;0;27;0
WireConnection;21;1;20;0
WireConnection;236;0;235;1
WireConnection;236;1;235;2
WireConnection;18;0;265;0
WireConnection;18;1;255;4
WireConnection;18;2;99;0
WireConnection;34;0;95;0
WireConnection;34;1;31;0
WireConnection;98;0;18;0
WireConnection;23;0;21;0
WireConnection;23;1;22;0
WireConnection;131;0;236;0
WireConnection;17;0;34;0
WireConnection;17;1;98;0
WireConnection;61;0;131;0
WireConnection;259;0;23;0
WireConnection;33;0;259;0
WireConnection;64;0;17;0
WireConnection;91;0;126;0
WireConnection;62;0;278;0
WireConnection;62;1;61;0
WireConnection;28;0;64;0
WireConnection;28;1;91;0
WireConnection;28;2;33;0
WireConnection;76;0;17;0
WireConnection;76;1;62;0
WireConnection;260;0;263;0
WireConnection;58;0;76;0
WireConnection;58;1;28;0
WireConnection;58;2;260;0
WireConnection;233;0;234;0
WireConnection;233;1;217;1
WireConnection;233;2;231;0
WireConnection;286;0;289;0
WireConnection;286;4;290;0
WireConnection;286;3;287;0
WireConnection;295;0;286;0
WireConnection;218;0;233;0
WireConnection;218;1;58;0
WireConnection;283;0;218;0
WireConnection;283;1;296;0
WireConnection;283;2;295;0
WireConnection;244;0;241;1
WireConnection;244;1;241;2
WireConnection;244;2;241;3
WireConnection;242;0;237;1
WireConnection;242;1;237;2
WireConnection;242;2;237;3
WireConnection;238;0;242;0
WireConnection;238;1;244;0
WireConnection;247;0;238;0
WireConnection;245;0;247;0
WireConnection;245;1;247;1
WireConnection;245;2;247;2
WireConnection;240;1;245;0
WireConnection;240;2;241;4
WireConnection;299;1;240;0
WireConnection;299;0;300;0
WireConnection;285;1;281;0
WireConnection;285;0;284;0
WireConnection;282;1;218;0
WireConnection;282;0;283;0
WireConnection;297;1;117;0
WireConnection;297;0;298;0
WireConnection;256;2;282;0
ASEEND*/
//CHKSM=64C07253376182FDAE64419EE60F3713453C0DCF
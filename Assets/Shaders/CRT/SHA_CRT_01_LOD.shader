// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/CRT_01_LOD"
{
	Properties
	{
		_Damage_Vignette_Hardness("Damage_Vignette_Hardness", Range( 0 , 1)) = 1
		_Damage_Desaturation("Damage_Desaturation", Range( -0.5 , 1)) = 0
		_Damage_VIgnette_Radius("Damage_VIgnette_Radius", Range( 0 , 1)) = 0
		_TextureSampleScanline_Near("Texture Sample Scanline_Near", 2D) = "white" {}
		_TextureSampleScanline_Far("Texture Sample Scanline_Far", 2D) = "white" {}
		_TextureSampleDotMask("Texture Sample DotMask", 2D) = "white" {}
		_MainTex("MainTex", 2D) = "black" {}
		_Scanline_GameScreenBrightness("Scanline_GameScreenBrightness", Range( 0 , 10)) = 2.5
		_Scanline_Amount("Scanline_Amount", Range( 0 , 1)) = 1
		_Distance_Scanline_Far("Distance_Scanline_Far", Range( 0 , 5)) = 1
		_Distance_Scanline_Near("Distance_Scanline_Near", Range( 0 , 1)) = 0.05
		_Distance_DotMask("Distance_DotMask", Range( 0 , 1)) = 0.15
		_Scanline_Color("Scanline_Color", Color) = (1,1,1,0)
		_Distance_Scanline_Far_Power("Distance_Scanline_Far_Power", Float) = 5
		_Distance_DotMask_Power("Distance_DotMask_Power", Float) = 5
		_Distance_Scanline_Near_Power("Distance_Scanline_Near_Power", Float) = 5
		_DotMask_Brighten("DotMask_Brighten", Range( 0 , 1)) = 0.273
		_DotMask_Saturate("DotMask_Saturate", Range( -1 , 1)) = 0
		_Dotmask_GameScreenBrightness("Dotmask_GameScreenBrightness", Range( 0 , 10)) = 1
		_Dotmask_ScanlineRemoval("Dotmask_ScanlineRemoval", Range( 0 , 1)) = 0
		_MipBias("Mip Bias", Range( -1 , 1)) = 0.5
		_CRTBrightnessFlickerMax("CRT Brightness Flicker Max", Range( -0.5 , 0.5)) = 0.1
		_CRTBrightnessFlickerMin("CRT Brightness Flicker Min", Range( -0.5 , 0.5)) = 0.1
		_CRTBrightnessFlickerTime("CRT Brightness Flicker Time", Range( 0 , 50)) = 0
		_Damage_RGB_Offset("Damage_RGB_Offset", Vector) = (1,1,1,0)
		_MaskVTXRedOnly("MaskVTXRedOnly", Range( 0 , 1)) = 0
		_CRTParameters("CRTParameters", Vector) = (256,256,1,0.59158)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float _MaskVTXRedOnly;
		uniform sampler2D _MainTex;
		uniform float3 _Damage_RGB_Offset;
		uniform float _Damage_VIgnette_Radius;
		uniform float _Damage_Vignette_Hardness;
		uniform float _Damage_Desaturation;
		uniform float _CRTBrightnessFlickerMin;
		uniform float _CRTBrightnessFlickerMax;
		uniform float _CRTBrightnessFlickerTime;
		uniform float _Dotmask_GameScreenBrightness;
		uniform float _Distance_DotMask;
		uniform float _Distance_DotMask_Power;
		uniform float _Scanline_GameScreenBrightness;
		uniform float4 _Scanline_Color;
		uniform sampler2D _TextureSampleScanline_Near;
		uniform float4 _CRTParameters;
		uniform sampler2D _TextureSampleScanline_Far;
		uniform float _MipBias;
		uniform float _Distance_Scanline_Near;
		uniform float _Distance_Scanline_Near_Power;
		uniform float _Scanline_Amount;
		uniform float _Dotmask_ScanlineRemoval;
		uniform float _DotMask_Saturate;
		uniform float _DotMask_Brighten;
		uniform sampler2D _TextureSampleDotMask;
		uniform float _Distance_Scanline_Far;
		uniform float _Distance_Scanline_Far_Power;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float lerpResult233 = lerp( 1.0 , i.vertexColor.r , _MaskVTXRedOnly);
			float3 desaturateInitialColor185 = ( ( tex2D( _MainTex, i.uv_texcoord ) * float4( _Damage_RGB_Offset , 0.0 ) ) * ( 1.0 - ( saturate( ( distance( i.uv_texcoord , float2( 0.5,0.5 ) ) - _Damage_VIgnette_Radius ) ) / ( 1.0 - _Damage_Vignette_Hardness ) ) ) ).rgb;
			float desaturateDot185 = dot( desaturateInitialColor185, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar185 = lerp( desaturateInitialColor185, desaturateDot185.xxx, _Damage_Desaturation );
			float mulTime203 = _Time.y * _CRTBrightnessFlickerTime;
			float lerpResult196 = lerp( _CRTBrightnessFlickerMin , _CRTBrightnessFlickerMax , ( ( sin( mulTime203 ) + 1.0 ) * 0.5 ));
			float3 temp_output_126_0 = ( desaturateVar185 + lerpResult196 );
			float3 ase_worldPos = i.worldPos;
			float clampResult49 = clamp( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / _Distance_DotMask ) , _Distance_DotMask_Power ) , 0.0 , 1.0 );
			float lerpResult93 = lerp( _Dotmask_GameScreenBrightness , 1.0 , clampResult49);
			float2 appendResult128 = (float2(1.0 , ( _CRTParameters.y / 16.0 )));
			float2 uv_TexCoord2 = i.uv_texcoord * appendResult128;
			float clampResult214 = clamp( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / _Distance_Scanline_Near ) , _Distance_Scanline_Near_Power ) , 0.0 , 1.0 );
			float4 lerpResult215 = lerp( tex2Dbias( _TextureSampleScanline_Near, float4( uv_TexCoord2, 0, 1.0) ) , tex2Dbias( _TextureSampleScanline_Far, float4( uv_TexCoord2, 0, _MipBias) ) , clampResult214);
			float lerpResult101 = lerp( _Dotmask_ScanlineRemoval , 0.0 , clampResult49);
			float4 lerpResult18 = lerp( _Scanline_Color , lerpResult215 , ( _Scanline_Amount - lerpResult101 ));
			float4 temp_output_17_0 = ( float4( ( ( temp_output_126_0 * lerpResult93 ) * _Scanline_GameScreenBrightness ) , 0.0 ) * saturate( lerpResult18 ) );
			float3 desaturateInitialColor82 = temp_output_17_0.rgb;
			float desaturateDot82 = dot( desaturateInitialColor82, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar82 = lerp( desaturateInitialColor82, desaturateDot82.xxx, _DotMask_Saturate );
			float2 appendResult236 = (float2(_CRTParameters.x , _CRTParameters.y));
			float2 uv_TexCoord61 = i.uv_texcoord * appendResult236;
			float clampResult24 = clamp( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / _Distance_Scanline_Far ) , _Distance_Scanline_Far_Power ) , 0.0 , 1.0 );
			float4 lerpResult28 = lerp( temp_output_17_0 , float4( temp_output_126_0 , 0.0 ) , clampResult24);
			float4 lerpResult58 = lerp( ( float4( ( desaturateVar82 + _DotMask_Brighten ) , 0.0 ) * tex2D( _TextureSampleDotMask, uv_TexCoord61 ) ) , lerpResult28 , clampResult49);
			o.Emission = ( lerpResult233 * lerpResult58 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CommentaryNode;136;-5651.142,-81.46181;Inherit;False;1558;351;SphereMask;10;157;158;179;180;156;155;154;153;152;151;;0,0,0,1;0;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;180;-5635.142,-17.46176;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;179;-5602.142,115.5385;Inherit;False;Constant;_SphereCenter;Sphere Center;0;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DistanceOpNode;151;-5379.142,-17.46176;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;158;-5379.142,94.53832;Inherit;False;Property;_Damage_VIgnette_Radius;Damage_VIgnette_Radius;2;0;Create;True;0;0;0;False;0;False;0;0.435;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;89;-3442.326,416.804;Inherit;False;2502;953.5816;Scanlines;15;236;205;104;215;98;100;99;16;1;2;18;19;102;103;130;;0.1536936,1,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;46;-2599.804,-1052.528;Inherit;False;1297.335;546.6675;Transition DotMask;8;55;54;53;52;51;50;49;48;;0,0.8417869,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;152;-5187.141,-17.46176;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;157;-5409.227,199.1412;Inherit;False;Property;_Damage_Vignette_Hardness;Damage_Vignette_Hardness;0;0;Create;True;0;0;0;False;0;False;1;0.596;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;189;-4301.94,-519.5765;Inherit;False;603.7002;397.2666;Damage RGB;2;187;188;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;103;-3386.326,883.8041;Inherit;False;260;210;Vertical Screen res / 8;1;235;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;90;-4816.853,-692.6508;Inherit;False;363.3334;277;GameScreen;1;11;;1,0,0.9828625,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;206;-3238.498,1647.25;Inherit;False;1297.335;546.6675;Transition Scanlines Near;8;214;213;212;211;210;209;208;207;;0,0.8417869,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;153;-5027.142,-17.46176;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;154;-5027.142,94.53832;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;192;-5057.66,-621.2523;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;51;-2461.805,-1002.528;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;52;-2549.805,-825.1956;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;200;-3784.776,126.5267;Inherit;False;Property;_CRTBrightnessFlickerTime;CRT Brightness Flicker Time;23;0;Create;True;0;0;0;False;0;False;0;35;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;130;-3056.968,663.22;Inherit;False;471;230.3333;Scanlines technically appearing every other line;2;128;127;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;155;-4835.143,-17.46176;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;187;-4251.94,-305.9766;Inherit;False;Property;_Damage_RGB_Offset;Damage_RGB_Offset;24;0;Create;True;0;0;0;False;0;False;1,1,1;1,1,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;203;-3503.516,138.4411;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;53;-2215.807,-941.8624;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;207;-3100.499,1697.25;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;208;-3188.499,1874.582;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;55;-2203.773,-763.5289;Inherit;False;Property;_Distance_DotMask;Distance_DotMask;11;0;Create;True;0;0;0;False;0;False;0.15;0.15;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;235;-3345.761,924.0881;Inherit;False;Property;_CRTParameters;CRTParameters;26;0;Create;True;0;0;0;False;0;False;256,256,1,0.59158;256,256,1,0.59158;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;11;-4769.454,-642.6509;Inherit;True;Property;_MainTex;MainTex;6;0;Create;True;0;0;0;False;0;False;-1;8f939eab460ea6041bc9d72da92c13a6;8f939eab460ea6041bc9d72da92c13a6;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;190;-3496.94,-495.9763;Inherit;False;558.502;338.8665;Damage Desaturation;2;186;185;;0,0,0,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;156;-4691.142,-17.46176;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;127;-2958.795,701.1768;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;16;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;188;-3876.24,-469.5765;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SinOpNode;204;-3274.515,168.4411;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;54;-1859.139,-891.8629;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-2008.139,-644.5283;Inherit;False;Property;_Distance_DotMask_Power;Distance_DotMask_Power;14;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;209;-2854.501,1757.916;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;210;-2842.467,1936.249;Inherit;False;Property;_Distance_Scanline_Near;Distance_Scanline_Near;10;0;Create;True;0;0;0;False;0;False;0.05;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;128;-2788.664,697.8539;Inherit;False;FLOAT2;4;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;184;-3629.251,-482.074;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;186;-3446.94,-269.7765;Inherit;False;Property;_Damage_Desaturation;Damage_Desaturation;1;0;Create;True;0;0;0;False;0;False;0;0;-0.5;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;201;-3131.301,143.6211;Inherit;False;ConstantBiasScale;-1;;3;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;197;-3337.776,-35.47342;Inherit;False;Property;_CRTBrightnessFlickerMax;CRT Brightness Flicker Max;21;0;Create;True;0;0;0;False;0;False;0.1;0.02;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;125;-3345.055,-137.4754;Inherit;False;Property;_CRTBrightnessFlickerMin;CRT Brightness Flicker Min;22;0;Create;True;0;0;0;False;0;False;0.1;0.015;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;48;-1650.472,-858.5297;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;211;-2497.833,1807.915;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;212;-2646.833,2055.25;Inherit;False;Property;_Distance_Scanline_Near_Power;Distance_Scanline_Near_Power;15;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;94;-1190.249,-630.0359;Inherit;False;620;541;Boost Texture when Dotmask Present;5;91;95;93;92;96;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;102;-1635.793,1072.348;Inherit;False;228;210;A=dotmask, B=nodotmask;1;101;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-2491.094,666.093;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DesaturateOpNode;185;-3148.438,-445.9763;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;196;-2807.776,-151.4734;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;49;-1480.472,-858.5297;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;213;-2289.167,1841.248;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-1943.597,1132.691;Inherit;False;Property;_Dotmask_ScanlineRemoval;Dotmask_ScanlineRemoval;19;0;Create;True;0;0;0;False;0;False;0;0.124;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;104;-2690.115,916.6333;Inherit;False;Property;_MipBias;Mip Bias;20;0;Create;True;0;0;0;False;0;False;0.5;0.5;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;30;-2639.204,-353.0924;Inherit;False;1297.335;546.6675;Transition Scanline;8;25;26;20;27;21;22;24;23;;0,0.8326645,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-1173.237,-473.562;Inherit;False;Property;_Dotmask_GameScreenBrightness;Dotmask_GameScreenBrightness;18;0;Create;True;0;0;0;False;0;False;1;2.24;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;126;-2738.403,-519.7662;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;1;-2236.325,645.8042;Inherit;True;Property;_TextureSampleScanline_Far;Texture Sample Scanline_Far;4;0;Create;True;0;0;0;False;0;False;-1;2fd07d7dd5fe8b046a2eaaf43291b55e;2fd07d7dd5fe8b046a2eaaf43291b55e;True;0;False;white;Auto;False;Object;-1;MipBias;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;214;-2119.167,1841.248;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1827.197,804.719;Inherit;False;Property;_Scanline_Amount;Scanline_Amount;8;0;Create;True;0;0;0;False;0;False;1;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;101;-1587.793,1123.348;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;205;-2235.679,951.5669;Inherit;True;Property;_TextureSampleScanline_Near;Texture Sample Scanline_Near;3;0;Create;True;0;0;0;False;0;False;-1;e03c6b81d5a1d1b40879ce22e711aed5;e03c6b81d5a1d1b40879ce22e711aed5;True;0;False;white;Auto;False;Object;-1;MipBias;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;97;-904.7473,91.75391;Inherit;False;614.1814;256.0048;Boost Texture when Scanlines present;2;31;34;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;25;-2501.205,-303.0924;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;26;-2589.205,-125.7583;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;19;-1616.324,466.804;Inherit;False;Property;_Scanline_Color;Scanline_Color;12;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;92;-911.4401,-244.3924;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;93;-874.7485,-602.4357;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;99;-1508.597,801.6913;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;215;-1875.659,567.4342;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DistanceOpNode;27;-2255.204,-242.4257;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-753.2373,-184.8619;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-854.7473,235.092;Inherit;False;Property;_Scanline_GameScreenBrightness;Scanline_GameScreenBrightness;7;0;Create;True;0;0;0;False;0;False;2.5;1.5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;18;-1359.324,614.8042;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-2125.171,-56.0917;Inherit;False;Property;_Distance_Scanline_Far;Distance_Scanline_Far;9;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;86;-750.9811,-1019.187;Inherit;False;673.8616;357.9644;Dotmask Desaturation;4;79;80;82;83;;1,0.7044024,0.7044024,1;0;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;21;-1898.537,-192.4256;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-468.5659,141.7539;Inherit;False;2;2;0;FLOAT3;1,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;98;-1121.597,620.6912;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-2107.537,31.90859;Inherit;False;Property;_Distance_Scanline_Far_Power;Distance_Scanline_Far_Power;13;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;236;-3083.761,944.0881;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;87;-1646.941,-1524.502;Inherit;False;1235.334;406.3926;DotMask;2;61;62;;1,0,0,1;0;0
Node;AmplifyShaderEditor.PowerNode;23;-1650.868,-107.7921;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-212.6577,136.7485;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-699.1408,-782.6078;Inherit;False;Property;_DotMask_Saturate;DotMask_Saturate;17;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;131;-2692.291,-1035.823;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;65;84.48536,76.33668;Inherit;False;273;157;Frame and Scanlines;1;64;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;24;-1480.869,-107.7921;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-700.9811,-927.5185;Inherit;False;Property;_DotMask_Brighten;DotMask_Brighten;16;0;Create;True;0;0;0;False;0;False;0.273;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;82;-287.12,-797.5563;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;61;-1013.941,-1388.502;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;88;238.744,-999.3019;Inherit;False;228;186.3333;Multiply Dotmask by Frame;1;76;;1,0.7610062,0.7610062,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;29;250.8946,-222.7503;Inherit;False;415;219;A = Frame and Scanlines, B = Raw Frame;1;28;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;33;-1124.618,-64.38861;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;64;115.8195,140.9099;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;91;-842.5527,-362.1745;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;79;-274.2234,-969.1874;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;62;-724.9406,-1424.502;Inherit;True;Property;_TextureSampleDotMask;Texture Sample DotMask;5;0;Create;True;0;0;0;False;0;False;-1;52fa22fcfb11d0e4280b2efa2ffb323b;52fa22fcfb11d0e4280b2efa2ffb323b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;28;300.8942,-172.7503;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;288.744,-949.3019;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;231;926.291,-706.7473;Inherit;False;Property;_MaskVTXRedOnly;MaskVTXRedOnly;25;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;217;1072.291,-1036.747;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;234;1146.291,-1159.914;Inherit;False;Constant;_Float1;Float 1;34;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;58;679.7466,-651.7343;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;233;1431.291,-720.9138;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;218;1677.291,-592.7473;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;248;1863.069,-596.2061;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;AgeOfJoy/CRT_01_LOD;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;151;0;180;0
WireConnection;151;1;179;0
WireConnection;152;0;151;0
WireConnection;152;1;158;0
WireConnection;153;0;152;0
WireConnection;154;0;157;0
WireConnection;155;0;153;0
WireConnection;155;1;154;0
WireConnection;203;0;200;0
WireConnection;53;0;51;0
WireConnection;53;1;52;0
WireConnection;11;1;192;0
WireConnection;156;0;155;0
WireConnection;127;0;235;2
WireConnection;188;0;11;0
WireConnection;188;1;187;0
WireConnection;204;0;203;0
WireConnection;54;0;53;0
WireConnection;54;1;55;0
WireConnection;209;0;207;0
WireConnection;209;1;208;0
WireConnection;128;1;127;0
WireConnection;184;0;188;0
WireConnection;184;1;156;0
WireConnection;201;3;204;0
WireConnection;48;0;54;0
WireConnection;48;1;50;0
WireConnection;211;0;209;0
WireConnection;211;1;210;0
WireConnection;2;0;128;0
WireConnection;185;0;184;0
WireConnection;185;1;186;0
WireConnection;196;0;125;0
WireConnection;196;1;197;0
WireConnection;196;2;201;0
WireConnection;49;0;48;0
WireConnection;213;0;211;0
WireConnection;213;1;212;0
WireConnection;126;0;185;0
WireConnection;126;1;196;0
WireConnection;1;1;2;0
WireConnection;1;2;104;0
WireConnection;214;0;213;0
WireConnection;101;0;100;0
WireConnection;101;2;49;0
WireConnection;205;1;2;0
WireConnection;92;0;126;0
WireConnection;93;0;96;0
WireConnection;93;2;49;0
WireConnection;99;0;16;0
WireConnection;99;1;101;0
WireConnection;215;0;205;0
WireConnection;215;1;1;0
WireConnection;215;2;214;0
WireConnection;27;0;25;0
WireConnection;27;1;26;0
WireConnection;95;0;92;0
WireConnection;95;1;93;0
WireConnection;18;0;19;0
WireConnection;18;1;215;0
WireConnection;18;2;99;0
WireConnection;21;0;27;0
WireConnection;21;1;20;0
WireConnection;34;0;95;0
WireConnection;34;1;31;0
WireConnection;98;0;18;0
WireConnection;236;0;235;1
WireConnection;236;1;235;2
WireConnection;23;0;21;0
WireConnection;23;1;22;0
WireConnection;17;0;34;0
WireConnection;17;1;98;0
WireConnection;131;0;236;0
WireConnection;24;0;23;0
WireConnection;82;0;17;0
WireConnection;82;1;83;0
WireConnection;61;0;131;0
WireConnection;33;0;24;0
WireConnection;64;0;17;0
WireConnection;91;0;126;0
WireConnection;79;0;82;0
WireConnection;79;1;80;0
WireConnection;62;1;61;0
WireConnection;28;0;64;0
WireConnection;28;1;91;0
WireConnection;28;2;33;0
WireConnection;76;0;79;0
WireConnection;76;1;62;0
WireConnection;58;0;76;0
WireConnection;58;1;28;0
WireConnection;58;2;49;0
WireConnection;233;0;234;0
WireConnection;233;1;217;1
WireConnection;233;2;231;0
WireConnection;218;0;233;0
WireConnection;218;1;58;0
WireConnection;248;2;218;0
ASEEND*/
//CHKSM=0AB6BEE4E3BE2E2611D9340A989FB89C1C303FE5
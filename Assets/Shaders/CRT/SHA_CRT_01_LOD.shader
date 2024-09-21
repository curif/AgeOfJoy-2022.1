// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/CRT_01_LOD"
{
	Properties
	{
		_Damage_Vignette_Hardness("Damage_Vignette_Hardness", Range( 0 , 1)) = 1
		_Damage_Desaturation("Damage_Desaturation", Range( -0.5 , 1)) = 0
		_Damage_VIgnette_Radius("Damage_VIgnette_Radius", Range( 0 , 1)) = 0
		_MainTex("MainTex", 2D) = "black" {}
		_Scanline_GameScreenBrightness("Scanline_GameScreenBrightness", Range( 0 , 10)) = 2.5
		_Scanline_Amount("Scanline_Amount", Range( 0 , 1)) = 0.5
		_Distance_Scanline("Distance_Scanline", Range( 0 , 5)) = 1
		_Distance_Scanline_Power("Distance_Scanline_Power", Float) = 5
		_CRTBrightnessFlickerMax("CRT Brightness Flicker Max", Range( -0.5 , 0.5)) = 0.1
		_CRTBrightnessFlickerMin("CRT Brightness Flicker Min", Range( -0.5 , 0.5)) = 0.1
		_CRTBrightnessFlickerTime("CRT Brightness Flicker Time", Range( 0 , 50)) = 0
		_Damage_RGB_Offset("Damage_RGB_Offset", Vector) = (1,1,1,0)
		_MaskVTXRedOnly("MaskVTXRedOnly", Range( 0 , 1)) = 0
		_CRTParameters("CRTParameters", Vector) = (256,256,1,0.59158)
		_CRTTiling("CRTTiling", Vector) = (1,1,0,0)
		_Dotmask_Scanlines("Dotmask_Scanlines", 2D) = "white" {}
		[Toggle(_ISPROJECTIONSCREEN_ON)] _isProjectionScreen("isProjectionScreen", Float) = 0
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
		#pragma shader_feature_local _ISPROJECTIONSCREEN_ON
		#pragma surface surf Unlit keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
			half vertexToFrag250;
			half2 vertexToFrag251;
			float3 worldPos;
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
		uniform half _Scanline_GameScreenBrightness;
		uniform sampler2D _Dotmask_Scanlines;
		uniform half4 _CRTParameters;
		uniform half _Scanline_Amount;
		uniform half _Distance_Scanline;
		uniform half _Distance_Scanline_Power;

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
			float2 uv_TexCoord192 = i.uv_texcoord * _CRTTiling + saturate( ( _CRTTiling * float2( -1,-1 ) ) );
			half3 desaturateInitialColor185 = ( ( (tex2D( _MainTex, uv_TexCoord192 )).rgb * _Damage_RGB_Offset ) * ( 1.0 - ( saturate( ( distance( i.uv_texcoord , half2( 0.5,0.5 ) ) - _Damage_VIgnette_Radius ) ) / ( 1.0 - _Damage_Vignette_Hardness ) ) ) );
			half desaturateDot185 = dot( desaturateInitialColor185, float3( 0.299, 0.587, 0.114 ));
			half3 desaturateVar185 = lerp( desaturateInitialColor185, desaturateDot185.xxx, _Damage_Desaturation );
			half3 temp_output_126_0 = ( desaturateVar185 + i.vertexToFrag250 );
			half lerpResult18 = lerp( 1.0 , tex2D( _Dotmask_Scanlines, i.vertexToFrag251 ).a , _Scanline_Amount);
			float3 ase_worldPos = i.worldPos;
			half3 lerpResult28 = lerp( ( ( temp_output_126_0 * _Scanline_GameScreenBrightness ) * saturate( lerpResult18 ) ) , temp_output_126_0 , saturate( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / _Distance_Scanline ) , _Distance_Scanline_Power ) ));
			half4 color286 = IsGammaSpace() ? half4(0.0754717,0.0754717,0.0754717,0) : half4(0.006628775,0.006628775,0.006628775,0);
			half3 ase_worldNormal = WorldNormalVector( i, half3( 0, 0, 1 ) );
			half fresnelNdotV284 = dot( normalize( ase_worldNormal ), i.viewDir );
			half fresnelNode284 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV284, 1.0 ) );
			half4 lerpResult287 = lerp( half4( lerpResult28 , 0.0 ) , color286 , saturate( fresnelNode284 ));
			#ifdef _ISPROJECTIONSCREEN_ON
				half4 staticSwitch288 = lerpResult287;
			#else
				half4 staticSwitch288 = half4( lerpResult28 , 0.0 );
			#endif
			o.Emission = ( lerpResult233 * staticSwitch288 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CommentaryNode;136;-5651.142,-81.46181;Inherit;False;1558;351;SphereMask;10;157;158;179;180;156;155;154;153;152;151;;0,0,0,1;0;0
Node;AmplifyShaderEditor.Vector2Node;249;-6057.235,-788.8251;Inherit;False;Property;_CRTTiling;CRTTiling;14;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TexCoordVertexDataNode;180;-5635.142,-17.46176;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;179;-5602.142,115.5385;Inherit;False;Constant;_SphereCenter;Sphere Center;0;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;303;-5755.851,-521.895;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;-1,-1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DistanceOpNode;151;-5379.142,-17.46176;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;158;-5379.142,94.53832;Inherit;False;Property;_Damage_VIgnette_Radius;Damage_VIgnette_Radius;2;0;Create;True;0;0;0;False;0;False;0;0.189;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;301;-5511.187,-520.895;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;90;-4816.853,-692.6508;Inherit;False;363.3334;277;GameScreen;1;11;;1,0,0.9828625,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;152;-5187.141,-17.46176;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;157;-5409.227,199.1412;Inherit;False;Property;_Damage_Vignette_Hardness;Damage_Vignette_Hardness;0;0;Create;True;0;0;0;False;0;False;1;0.236;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;89;-2802.326,414.804;Inherit;False;2502;953.5816;Scanlines;9;98;16;2;18;130;251;255;265;278;;0.1536936,1,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;103;-3163.216,622.1735;Inherit;False;260;210;Vertical Screen res / 8;1;235;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;200;-3784.776,126.5267;Inherit;False;Property;_CRTBrightnessFlickerTime;CRT Brightness Flicker Time;10;0;Create;True;0;0;0;False;0;False;0;0;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;192;-5102.661,-826.5857;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;189;-4301.94,-519.5765;Inherit;False;603.7002;397.2666;Damage RGB;2;187;188;;0,0,0,1;0;0
Node;AmplifyShaderEditor.SaturateNode;153;-5027.142,-17.46176;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;154;-5027.142,94.53832;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;130;-2774.302,670.5531;Inherit;False;471;230.3333;Scanlines technically appearing every other line;2;128;127;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;203;-3503.516,138.4411;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;235;-3122.651,662.4575;Inherit;False;Property;_CRTParameters;CRTParameters;13;0;Create;True;0;0;0;False;0;False;256,256,1,0.59158;320,240,1,0.59158;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;11;-4770.861,-627.4778;Inherit;True;Property;_MainTex;MainTex;3;0;Create;True;0;0;0;False;0;False;-1;8f939eab460ea6041bc9d72da92c13a6;8f939eab460ea6041bc9d72da92c13a6;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;187;-4251.94,-305.9766;Inherit;False;Property;_Damage_RGB_Offset;Damage_RGB_Offset;11;0;Create;True;0;0;0;False;0;False;1,1,1;1,1,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;264;-4376.894,-615.1232;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;155;-4835.143,-17.46176;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;204;-3274.515,168.4411;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;127;-2742.261,745.0427;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;32;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;190;-3496.94,-495.9763;Inherit;False;558.502;338.8665;Damage Desaturation;2;186;185;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;30;-2559.737,-273.6253;Inherit;False;1297.335;546.6675;Transition Scanline;8;25;26;20;27;21;22;23;259;;0,0.8326645,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;156;-4691.142,-17.46176;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;188;-3876.24,-469.5765;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;201;-3131.301,143.6211;Inherit;False;ConstantBiasScale;-1;;3;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;197;-3337.776,-35.47342;Inherit;False;Property;_CRTBrightnessFlickerMax;CRT Brightness Flicker Max;8;0;Create;True;0;0;0;False;0;False;0.1;0.1;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;125;-3345.055,-137.4754;Inherit;False;Property;_CRTBrightnessFlickerMin;CRT Brightness Flicker Min;9;0;Create;True;0;0;0;False;0;False;0.1;0.1;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;128;-2505.998,705.187;Inherit;False;FLOAT2;4;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;184;-3629.251,-482.074;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;186;-3446.94,-269.7765;Inherit;False;Property;_Damage_Desaturation;Damage_Desaturation;1;0;Create;True;0;0;0;False;0;False;0;0;-0.5;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;196;-2807.776,-151.4734;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-2197.697,655.5521;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;25;-2421.738,-223.6252;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;26;-2509.738,-46.29104;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DesaturateOpNode;185;-3148.438,-445.9763;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexToFragmentNode;250;-2857.292,-354.037;Inherit;False;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;251;-1960.461,661.2367;Inherit;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;278;-2053.716,408.564;Inherit;True;Property;_Dotmask_Scanlines;Dotmask_Scanlines;15;0;Create;True;0;0;0;False;0;False;5691e46c711683e4a80cc7e1322070d0;5691e46c711683e4a80cc7e1322070d0;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.DistanceOpNode;27;-2175.737,-162.9586;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-2131.704,0.7089081;Inherit;False;Property;_Distance_Scanline;Distance_Scanline;6;0;Create;True;0;0;0;False;0;False;1;0.5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;97;-904.7473,91.75391;Inherit;False;614.1814;256.0048;Boost Texture when Scanlines present;2;31;34;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;126;-2738.403,-519.7662;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;265;-873.6919,498.5084;Inherit;False;Constant;_Float0;Float 0;27;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;255;-1713.815,636.4662;Inherit;True;Property;_PackedScanlines;PackedScanlines;26;0;Create;True;0;0;0;False;0;False;-1;b9bcd261404598f49ada1f9d5ec82d41;b9bcd261404598f49ada1f9d5ec82d41;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;16;-1129.58,811.2672;Inherit;False;Property;_Scanline_Amount;Scanline_Amount;5;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;21;-1819.07,-112.9584;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-2028.07,111.3759;Inherit;False;Property;_Distance_Scanline_Power;Distance_Scanline_Power;7;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-854.7473,235.092;Inherit;False;Property;_Scanline_GameScreenBrightness;Scanline_GameScreenBrightness;4;0;Create;True;0;0;0;False;0;False;2.5;2.5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;92;-942.7735,-207.7257;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;18;-705.0249,608.9037;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;23;-1571.401,-28.32484;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;91;-842.5527,-362.1745;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;259;-1409.786,-14.58065;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;98;-481.5969,618.691;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-468.5659,141.7539;Inherit;False;2;2;0;FLOAT3;1,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;281;981.6045,112.5526;Inherit;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;283;1039.137,272.0859;Inherit;False;Constant;_ProjScreenFresnelView;ProjScreenFresnelView;25;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;282;990.2708,-109.4474;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;29;250.8946,-222.7503;Inherit;False;415;219;A = Frame and Scanlines, B = Raw Frame;1;28;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;280;26.04624,-180.8238;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;33;-1124.618,-64.38861;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;279;-184.4283,73.44623;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FresnelNode;284;1291.604,-19.44739;Inherit;False;Standard;WorldNormal;ViewDir;True;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;28;300.8942,-172.7503;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;285;1529.604,5.219299;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;286;1318.938,207.2192;Inherit;False;Constant;_ProjScreenAngleColor_Emissive;ProjScreenAngleColor_Emissive;23;0;Create;True;0;0;0;False;0;False;0.0754717,0.0754717,0.0754717,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;217;1072.291,-1036.747;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;231;1004.691,-810.7473;Inherit;False;Property;_MaskVTXRedOnly;MaskVTXRedOnly;12;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;234;1105.042,-1154.964;Inherit;False;Constant;_Float1;Float 1;34;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;287;1674.936,-87.44748;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;1,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;233;1431.291,-720.9138;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;288;1297.442,-434.752;Inherit;False;Property;_isProjectionScreen;isProjectionScreen;16;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;218;1722.624,-560.0807;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;256;2066.64,-685.2053;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;AgeOfJoy/CRT_01_LOD;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;False;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;303;0;249;0
WireConnection;151;0;180;0
WireConnection;151;1;179;0
WireConnection;301;0;303;0
WireConnection;152;0;151;0
WireConnection;152;1;158;0
WireConnection;192;0;249;0
WireConnection;192;1;301;0
WireConnection;153;0;152;0
WireConnection;154;0;157;0
WireConnection;203;0;200;0
WireConnection;11;1;192;0
WireConnection;264;0;11;0
WireConnection;155;0;153;0
WireConnection;155;1;154;0
WireConnection;204;0;203;0
WireConnection;127;0;235;2
WireConnection;156;0;155;0
WireConnection;188;0;264;0
WireConnection;188;1;187;0
WireConnection;201;3;204;0
WireConnection;128;1;127;0
WireConnection;184;0;188;0
WireConnection;184;1;156;0
WireConnection;196;0;125;0
WireConnection;196;1;197;0
WireConnection;196;2;201;0
WireConnection;2;0;128;0
WireConnection;185;0;184;0
WireConnection;185;1;186;0
WireConnection;250;0;196;0
WireConnection;251;0;2;0
WireConnection;27;0;25;0
WireConnection;27;1;26;0
WireConnection;126;0;185;0
WireConnection;126;1;250;0
WireConnection;255;0;278;0
WireConnection;255;1;251;0
WireConnection;21;0;27;0
WireConnection;21;1;20;0
WireConnection;92;0;126;0
WireConnection;18;0;265;0
WireConnection;18;1;255;4
WireConnection;18;2;16;0
WireConnection;23;0;21;0
WireConnection;23;1;22;0
WireConnection;91;0;126;0
WireConnection;259;0;23;0
WireConnection;98;0;18;0
WireConnection;34;0;92;0
WireConnection;34;1;31;0
WireConnection;280;0;91;0
WireConnection;33;0;259;0
WireConnection;279;0;34;0
WireConnection;279;1;98;0
WireConnection;284;0;282;0
WireConnection;284;4;281;0
WireConnection;284;3;283;0
WireConnection;28;0;279;0
WireConnection;28;1;280;0
WireConnection;28;2;33;0
WireConnection;285;0;284;0
WireConnection;287;0;28;0
WireConnection;287;1;286;0
WireConnection;287;2;285;0
WireConnection;233;0;234;0
WireConnection;233;1;217;1
WireConnection;233;2;231;0
WireConnection;288;1;28;0
WireConnection;288;0;287;0
WireConnection;218;0;233;0
WireConnection;218;1;288;0
WireConnection;256;2;218;0
ASEEND*/
//CHKSM=4FD42FC4A01A6A3DDA3C1273FF2F88F8A4D3A1D8
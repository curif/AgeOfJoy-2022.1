// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/CRT_01"
{
	Properties
	{
		_TextureSampleScanline("Texture Sample Scanline", 2D) = "white" {}
		_TextureSampleDotMask("Texture Sample DotMask", 2D) = "white" {}
		_Scanline_Tiling("Scanline_Tiling", Vector) = (1,256,0,0)
		_Dotmask_Tiling("Dotmask_Tiling", Vector) = (512,256,0,0)
		_GameScreen("GameScreen", 2D) = "white" {}
		_Scanline_GameScreenBrightness("Scanline_GameScreenBrightness", Range( 0 , 10)) = 2.5
		_Scanline_Amount("Scanline_Amount", Range( 0 , 1)) = 1
		_Distance_Scanline("Distance_Scanline", Range( 0 , 5)) = 0.5
		_Distance_DotMask("Distance_DotMask", Range( 0 , 1)) = 0.05
		_Scanline_Color("Scanline_Color", Color) = (1,1,1,0)
		_Distance_Scanline_Power("Distance_Scanline_Power", Float) = 5
		_Distance_DotMask_Power("Distance_DotMask_Power", Float) = 5
		_DotMask_OffsetAdd("DotMask_OffsetAdd", Range( 0 , 1)) = 0
		_DotMask_Brighten("DotMask_Brighten", Range( 0 , 1)) = 0.273
		_DotMask_Saturate("DotMask_Saturate", Range( -1 , 1)) = 0
		_Dotmask_GameScreenBrightness("Dotmask_GameScreenBrightness", Range( 0 , 10)) = 1
		_Dotmask_ScanlineRemoval("Dotmask_ScanlineRemoval", Range( 0 , 1)) = 0
		_MipBias("Mip Bias", Range( -1 , 1)) = 0
		_TextureSample0("Texture Sample 0", CUBE) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"

			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_FRAG_WORLD_POSITION


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform samplerCUBE _TextureSample0;
			uniform sampler2D _GameScreen;
			uniform float4 _GameScreen_ST;
			uniform float _Dotmask_GameScreenBrightness;
			uniform float _Distance_DotMask;
			uniform float _Distance_DotMask_Power;
			uniform float _Scanline_GameScreenBrightness;
			uniform float4 _Scanline_Color;
			uniform sampler2D _TextureSampleScanline;
			uniform float2 _Scanline_Tiling;
			uniform float _MipBias;
			uniform float _Scanline_Amount;
			uniform float _Dotmask_ScanlineRemoval;
			uniform float _DotMask_Saturate;
			uniform float _DotMask_Brighten;
			uniform sampler2D _TextureSampleDotMask;
			uniform float2 _Dotmask_Tiling;
			uniform float _DotMask_OffsetAdd;
			uniform float _Distance_Scanline;
			uniform float _Distance_Scanline_Power;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord1.xyz = ase_worldNormal;
				float3 ase_worldTangent = UnityObjectToWorldDir(v.ase_tangent);
				o.ase_texcoord2.xyz = ase_worldTangent;
				float ase_vertexTangentSign = v.ase_tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord3.xyz = ase_worldBitangent;
				
				o.ase_texcoord4.xyz = v.ase_texcoord.xyz;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
				o.ase_texcoord2.w = 0;
				o.ase_texcoord3.w = 0;
				o.ase_texcoord4.w = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float3 ase_worldNormal = i.ase_texcoord1.xyz;
				float3 ase_worldTangent = i.ase_texcoord2.xyz;
				float3 ase_worldBitangent = i.ase_texcoord3.xyz;
				float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
				float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
				float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(WorldPosition);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 worldRefl105 = reflect( -ase_worldViewDir, float3( dot( tanToWorld0, ase_worldNormal ), dot( tanToWorld1, ase_worldNormal ), dot( tanToWorld2, ase_worldNormal ) ) );
				float2 uv_GameScreen = i.ase_texcoord4.xyz.xy * _GameScreen_ST.xy + _GameScreen_ST.zw;
				float4 tex2DNode11 = tex2D( _GameScreen, uv_GameScreen );
				float clampResult49 = clamp( pow( ( distance( WorldPosition , _WorldSpaceCameraPos ) / _Distance_DotMask ) , _Distance_DotMask_Power ) , 0.0 , 1.0 );
				float lerpResult93 = lerp( _Dotmask_GameScreenBrightness , 1.0 , clampResult49);
				float2 texCoord2 = i.ase_texcoord4.xyz.xy * _Scanline_Tiling + float2( 0,0 );
				float lerpResult101 = lerp( _Dotmask_ScanlineRemoval , 0.0 , clampResult49);
				float4 lerpResult18 = lerp( _Scanline_Color , tex2Dbias( _TextureSampleScanline, float4( texCoord2, 0, _MipBias) ) , ( _Scanline_Amount - lerpResult101 ));
				float4 temp_output_17_0 = ( ( ( tex2DNode11 * lerpResult93 ) * _Scanline_GameScreenBrightness ) * saturate( lerpResult18 ) );
				float3 desaturateInitialColor82 = temp_output_17_0.rgb;
				float desaturateDot82 = dot( desaturateInitialColor82, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar82 = lerp( desaturateInitialColor82, desaturateDot82.xxx, _DotMask_Saturate );
				float2 appendResult71 = (float2(_Dotmask_Tiling.x , ( _Dotmask_Tiling.y + _DotMask_OffsetAdd )));
				float2 texCoord61 = i.ase_texcoord4.xyz.xy * appendResult71 + float2( 0,0 );
				float clampResult24 = clamp( pow( ( distance( WorldPosition , _WorldSpaceCameraPos ) / _Distance_Scanline ) , _Distance_Scanline_Power ) , 0.0 , 1.0 );
				float4 lerpResult28 = lerp( temp_output_17_0 , tex2DNode11 , clampResult24);
				float4 lerpResult58 = lerp( ( float4( ( desaturateVar82 + _DotMask_Brighten ) , 0.0 ) * tex2D( _TextureSampleDotMask, texCoord61 ) ) , lerpResult28 , clampResult49);
				
				
				finalColor = ( texCUBE( _TextureSample0, worldRefl105 ) + lerpResult58 );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CommentaryNode;46;-2601.134,-1052.528;Inherit;False;1297.335;546.6675;Transition Scanline;8;55;54;53;52;51;50;49;48;;0,0.8417869,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;51;-2463.135,-1002.528;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;52;-2551.135,-825.1956;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DistanceOpNode;53;-2217.137,-941.8624;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-2205.103,-763.5289;Inherit;False;Property;_Distance_DotMask;Distance_DotMask;8;0;Create;True;0;0;0;False;0;False;0.05;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;89;-2808.326,416.804;Inherit;False;1689;555.5815;Scanlines;8;2;19;1;16;18;99;103;104;;0.1536936,1,0,1;0;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;54;-1860.469,-891.8629;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-2009.469,-644.5283;Inherit;False;Property;_Distance_DotMask_Power;Distance_DotMask_Power;11;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;103;-2808.326,637.8041;Inherit;False;260;210;Vertical Screen res / 8;1;10;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PowerNode;48;-1651.802,-858.5297;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;90;-3175.553,-485.6514;Inherit;False;363.3334;277;GameScreen;1;11;;1,0,0.9828625,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;94;-1190.249,-630.0359;Inherit;False;620;541;Boost Texture when Dotmask Present;5;91;95;93;92;96;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;102;-1625.793,1099.348;Inherit;False;228;210;A=dotmask, B=nodotmask;1;101;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;49;-1481.802,-858.5297;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-1903.597,1280.691;Inherit;False;Property;_Dotmask_ScanlineRemoval;Dotmask_ScanlineRemoval;19;0;Create;True;0;0;0;False;0;False;0;0.124;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;10;-2758.326,687.8041;Inherit;False;Property;_Scanline_Tiling;Scanline_Tiling;2;0;Create;True;0;0;0;False;0;False;1,256;1,16;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;30;-2639.204,-353.0924;Inherit;False;1297.335;546.6675;Transition Scanline;8;25;26;20;27;21;22;24;23;;0,0.8326645,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;11;-3125.553,-435.6514;Inherit;True;Property;_GameScreen;GameScreen;4;0;Create;True;0;0;0;False;0;False;-1;None;1e4addacf63225541ad984df94a11789;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;96;-1173.237,-473.562;Inherit;False;Property;_Dotmask_GameScreenBrightness;Dotmask_GameScreenBrightness;18;0;Create;True;0;0;0;False;0;False;1;3;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1762.197,867.719;Inherit;False;Property;_Scanline_Amount;Scanline_Amount;6;0;Create;True;0;0;0;False;0;False;1;0.12;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;101;-1577.793,1150.348;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-2523.326,680.8041;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;104;-2493.115,900.6333;Inherit;False;Property;_MipBias;Mip Bias;20;0;Create;True;0;0;0;False;0;False;0;0.5;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;97;-904.7473,91.75391;Inherit;False;614.1814;256.0048;Boost Texture when Scanlines present;2;31;34;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;87;-1646.941,-1524.502;Inherit;False;1235.334;406.3926;DotMask;6;75;60;73;71;61;62;;1,0,0,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;25;-2501.205,-303.0924;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;26;-2589.205,-125.7583;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;19;-1616.324,466.804;Inherit;False;Property;_Scanline_Color;Scanline_Color;9;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;92;-911.4401,-244.3924;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;93;-874.7485,-602.4357;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;99;-1443.597,770.6913;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-2236.325,644.8042;Inherit;True;Property;_TextureSampleScanline;Texture Sample Scanline;0;0;Create;True;0;0;0;False;0;False;-1;2fd07d7dd5fe8b046a2eaaf43291b55e;2fd07d7dd5fe8b046a2eaaf43291b55e;True;0;False;white;Auto;False;Object;-1;MipBias;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DistanceOpNode;27;-2255.204,-242.4257;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-2125.171,-56.0917;Inherit;False;Property;_Distance_Scanline;Distance_Scanline;7;0;Create;True;0;0;0;False;0;False;0.5;2;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-753.2373,-184.8619;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-854.7473,235.092;Inherit;False;Property;_Scanline_GameScreenBrightness;Scanline_GameScreenBrightness;5;0;Create;True;0;0;0;False;0;False;2.5;1.5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;18;-1359.324,614.8042;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-1554.702,-1230.776;Inherit;False;Property;_DotMask_OffsetAdd;DotMask_OffsetAdd;15;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;60;-1596.941,-1474.502;Inherit;False;Property;_Dotmask_Tiling;Dotmask_Tiling;3;0;Create;True;0;0;0;False;0;False;512,256;256,256;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;86;-750.9811,-1019.187;Inherit;False;673.8616;357.9644;Dotmask Desaturation;4;79;80;82;83;;1,0.7044024,0.7044024,1;0;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;21;-1898.537,-192.4256;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-2107.537,31.90859;Inherit;False;Property;_Distance_Scanline_Power;Distance_Scanline_Power;10;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-468.5659,141.7539;Inherit;False;2;2;0;COLOR;1,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;98;-653.5974,519.6912;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-1337.702,-1370.776;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;23;-1650.868,-107.7921;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-212.6577,136.7485;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-699.1408,-782.6078;Inherit;False;Property;_DotMask_Saturate;DotMask_Saturate;17;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;71;-1201.702,-1424.776;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;65;84.48536,76.33668;Inherit;False;273;157;Frame and Scanlines;1;64;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;24;-1480.869,-107.7921;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-700.9811,-927.5185;Inherit;False;Property;_DotMask_Brighten;DotMask_Brighten;16;0;Create;True;0;0;0;False;0;False;0.273;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;82;-287.12,-797.5563;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;61;-1013.941,-1388.502;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;88;238.744,-999.3019;Inherit;False;228;186.3333;Multiply Dotmask by Frame;1;76;;1,0.7610062,0.7610062,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;29;250.8946,-222.7503;Inherit;False;415;219;A = Frame and Scanlines, B = Raw Frame;1;28;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;33;-1124.618,-64.38861;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;64;115.8195,140.9099;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;91;-842.5527,-362.1745;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;79;-274.2234,-969.1874;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;62;-724.9406,-1424.502;Inherit;True;Property;_TextureSampleDotMask;Texture Sample DotMask;1;0;Create;True;0;0;0;False;0;False;-1;52fa22fcfb11d0e4280b2efa2ffb323b;52fa22fcfb11d0e4280b2efa2ffb323b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;106;286.1982,-1218.795;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;28;300.8942,-172.7503;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;288.744,-949.3019;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldReflectionVector;105;525.1982,-1182.795;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;56;-1070.401,1209.145;Inherit;False;766.0004;356.6671;Fresn;5;41;43;44;40;42;;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;58;679.7466,-651.7343;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;108;784.1982,-1170.795;Inherit;True;Property;_TextureSample0;Texture Sample 0;21;0;Create;True;0;0;0;False;0;False;-1;1064b3af7c0210041ac63a494868bdef;1064b3af7c0210041ac63a494868bdef;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLER2D;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;41;-1020.401,1288.145;Inherit;False;Property;_FresnelBias;FresnelBias;12;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-1013.401,1353.146;Inherit;False;Property;_FresnelScale;FresnelScale;13;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-1000.401,1453.146;Inherit;False;Property;_FresnelPower;FresnelPower;14;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;40;-709.4015,1259.145;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;42;-450.4011,1270.245;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;109;914.1982,-619.7952;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;110;1109.069,-724.2061;Float;False;True;-1;2;ASEMaterialInspector;100;5;AgeOfJoy/CRT_01;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.CommentaryNode;45;-215.0649,-1561.633;Inherit;False;514;215;Flashing - take scanline and scroll it over the scanline bit;0;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;35;-109.6035,1237.028;Inherit;False;641;279.0001;Add vignette;0;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;84;-194.3853,-1840.56;Inherit;False;514;215;Color Blobs;0;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;85;-216.3853,-2094.561;Inherit;False;514;215;Color Mismatch;0;;1,1,1,1;0;0
WireConnection;53;0;51;0
WireConnection;53;1;52;0
WireConnection;54;0;53;0
WireConnection;54;1;55;0
WireConnection;48;0;54;0
WireConnection;48;1;50;0
WireConnection;49;0;48;0
WireConnection;101;0;100;0
WireConnection;101;2;49;0
WireConnection;2;0;10;0
WireConnection;92;0;11;0
WireConnection;93;0;96;0
WireConnection;93;2;49;0
WireConnection;99;0;16;0
WireConnection;99;1;101;0
WireConnection;1;1;2;0
WireConnection;1;2;104;0
WireConnection;27;0;25;0
WireConnection;27;1;26;0
WireConnection;95;0;92;0
WireConnection;95;1;93;0
WireConnection;18;0;19;0
WireConnection;18;1;1;0
WireConnection;18;2;99;0
WireConnection;21;0;27;0
WireConnection;21;1;20;0
WireConnection;34;0;95;0
WireConnection;34;1;31;0
WireConnection;98;0;18;0
WireConnection;73;0;60;2
WireConnection;73;1;75;0
WireConnection;23;0;21;0
WireConnection;23;1;22;0
WireConnection;17;0;34;0
WireConnection;17;1;98;0
WireConnection;71;0;60;1
WireConnection;71;1;73;0
WireConnection;24;0;23;0
WireConnection;82;0;17;0
WireConnection;82;1;83;0
WireConnection;61;0;71;0
WireConnection;33;0;24;0
WireConnection;64;0;17;0
WireConnection;91;0;11;0
WireConnection;79;0;82;0
WireConnection;79;1;80;0
WireConnection;62;1;61;0
WireConnection;28;0;64;0
WireConnection;28;1;91;0
WireConnection;28;2;33;0
WireConnection;76;0;79;0
WireConnection;76;1;62;0
WireConnection;105;0;106;0
WireConnection;58;0;76;0
WireConnection;58;1;28;0
WireConnection;58;2;49;0
WireConnection;108;1;105;0
WireConnection;40;1;41;0
WireConnection;40;2;43;0
WireConnection;40;3;44;0
WireConnection;42;1;40;0
WireConnection;109;0;108;0
WireConnection;109;1;58;0
WireConnection;110;0;109;0
ASEEND*/
//CHKSM=B2AA3FE70F3E337DC038C95B1DF50DB38A299ED5
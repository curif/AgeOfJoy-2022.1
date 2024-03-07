// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/CRT_01"
{
	Properties
	{
		_TextureSampleScanline("Texture Sample Scanline", 2D) = "white" {}
		_TextureSampleDotMask("Texture Sample DotMask", 2D) = "white" {}
		_ScanlineTiling("ScanlineTiling", Vector) = (0,0,0,0)
		_DotmaskTiling("DotmaskTiling", Vector) = (512,256,0,0)
		_GameScreen("GameScreen", 2D) = "white" {}
		_GameScreenBrightnessWithScanlines("GameScreenBrightnessWithScanlines", Range( 0 , 10)) = 1
		_ScanlinePower("ScanlinePower", Range( 0 , 1)) = 1
		_ScanlineTransitionDistance("ScanlineTransitionDistance", Float) = 0.1
		_DotMaskTransitionDistance("DotMaskTransitionDistance", Float) = 0.05
		_ScanlineMax("ScanlineMax", Color) = (1,1,1,0)
		_ScanlineTransitionPower("ScanlineTransitionPower", Float) = 5
		_DotMaskTransitionPower("DotMaskTransitionPower", Float) = 5
		_DotMaskOffsetAdd("DotMaskOffsetAdd", Range( 0 , 2)) = 0
		_DotMaskBrighten("DotMaskBrighten", Range( 0 , 1)) = 0
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _GameScreen;
			uniform float4 _GameScreen_ST;
			uniform float _GameScreenBrightnessWithScanlines;
			uniform float4 _ScanlineMax;
			uniform sampler2D _TextureSampleScanline;
			uniform float2 _ScanlineTiling;
			uniform float _ScanlinePower;
			uniform float _DotMaskBrighten;
			uniform sampler2D _TextureSampleDotMask;
			uniform float2 _DotmaskTiling;
			uniform float _DotMaskOffsetAdd;
			uniform float _ScanlineTransitionDistance;
			uniform float _ScanlineTransitionPower;
			uniform float _DotMaskTransitionDistance;
			uniform float _DotMaskTransitionPower;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
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
				float2 uv_GameScreen = i.ase_texcoord1.xy * _GameScreen_ST.xy + _GameScreen_ST.zw;
				float4 tex2DNode11 = tex2D( _GameScreen, uv_GameScreen );
				float2 texCoord2 = i.ase_texcoord1.xy * _ScanlineTiling + float2( 0,0 );
				float4 tex2DNode1 = tex2D( _TextureSampleScanline, texCoord2 );
				float4 lerpResult18 = lerp( _ScanlineMax , tex2DNode1 , _ScanlinePower);
				float4 temp_output_17_0 = ( ( tex2DNode11 * _GameScreenBrightnessWithScanlines ) * lerpResult18 );
				float2 appendResult71 = (float2(_DotmaskTiling.x , ( _DotmaskTiling.y + _DotMaskOffsetAdd )));
				float2 texCoord61 = i.ase_texcoord1.xy * appendResult71 + float2( 0,0 );
				float clampResult24 = clamp( pow( ( distance( WorldPosition , _WorldSpaceCameraPos ) / _ScanlineTransitionDistance ) , _ScanlineTransitionPower ) , 0.0 , 1.0 );
				float4 lerpResult28 = lerp( temp_output_17_0 , tex2DNode11 , clampResult24);
				float clampResult49 = clamp( pow( ( distance( WorldPosition , _WorldSpaceCameraPos ) / _DotMaskTransitionDistance ) , _DotMaskTransitionPower ) , 0.0 , 1.0 );
				float4 lerpResult58 = lerp( ( ( temp_output_17_0 + _DotMaskBrighten ) * tex2D( _TextureSampleDotMask, texCoord61 ) ) , lerpResult28 , clampResult49);
				
				
				finalColor = lerpResult58;
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
Node;AmplifyShaderEditor.CommentaryNode;30;-2639.204,-353.0924;Inherit;False;1297.335;546.6675;Transition Scanline;6;25;26;20;27;21;22;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;25;-2501.205,-303.0924;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;26;-2589.205,-125.7583;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector2Node;10;-1871.5,600.4998;Inherit;False;Property;_ScanlineTiling;ScanlineTiling;2;0;Create;True;0;0;0;False;0;False;0,0;1,256;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DistanceOpNode;27;-2255.204,-242.4257;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-2129.171,-107.0917;Inherit;False;Property;_ScanlineTransitionDistance;ScanlineTransitionDistance;8;0;Create;True;0;0;0;False;0;False;0.1;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;46;-1850.858,-1101.13;Inherit;False;1297.335;546.6675;Transition Scanline;8;55;54;53;52;51;50;49;48;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-1636.5,593.4998;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;60;-1596.941,-1475.502;Inherit;False;Property;_DotmaskTiling;DotmaskTiling;3;0;Create;True;0;0;0;False;0;False;512,256;1,256;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;75;-1554.702,-1230.776;Inherit;False;Property;_DotMaskOffsetAdd;DotMaskOffsetAdd;16;0;Create;True;0;0;0;False;0;False;0;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-1804.537,80.90859;Inherit;False;Property;_ScanlineTransitionPower;ScanlineTransitionPower;11;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;21;-1898.537,-192.4256;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;51;-1712.859,-1051.13;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;52;-1800.859,-873.7968;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;19;-729.5,379.4998;Inherit;False;Property;_ScanlineMax;ScanlineMax;10;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;16;-797.3726,828.4147;Inherit;False;Property;_ScanlinePower;ScanlinePower;6;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-1104.218,-11.88854;Inherit;False;Property;_GameScreenBrightnessWithScanlines;GameScreenBrightnessWithScanlines;5;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;11;-1096.302,-315.0475;Inherit;True;Property;_GameScreen;GameScreen;4;0;Create;True;0;0;0;False;0;False;-1;None;1e4addacf63225541ad984df94a11789;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-1347.5,557.4999;Inherit;True;Property;_TextureSampleScanline;Texture Sample Scanline;0;0;Create;True;0;0;0;False;0;False;-1;None;1bca5357adbdec0498a30ffa7acd80d3;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-1337.702,-1370.776;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;23;-1650.868,-107.7921;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;53;-1466.859,-990.4636;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-1454.825,-812.1301;Inherit;False;Property;_DotMaskTransitionDistance;DotMaskTransitionDistance;9;0;Create;True;0;0;0;False;0;False;0.05;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;18;-410.5,536.4999;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-667.5659,-122.2461;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;71;-1201.702,-1424.776;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;65;-27.90436,17.10425;Inherit;False;132;132;Scanlines;1;64;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;24;-1480.869,-107.7921;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;54;-1110.191,-940.4641;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-1259.191,-693.1296;Inherit;False;Property;_DotMaskTransitionPower;DotMaskTransitionPower;12;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-164.5575,136.7485;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;61;-1013.941,-1388.502;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;80;63.2981,-724.7761;Inherit;False;Property;_DotMaskBrighten;DotMaskBrighten;17;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;29;117.2419,-186.2995;Inherit;False;228;210;Transition Between no scanlines and scanlines;1;28;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;33;-1124.618,-64.38861;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;48;-901.5245,-907.1309;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;64;3.429864,81.67746;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;62;-724.9406,-1424.502;Inherit;True;Property;_TextureSampleDotMask;Texture Sample DotMask;1;0;Create;True;0;0;0;False;0;False;-1;52fa22fcfb11d0e4280b2efa2ffb323b;1bca5357adbdec0498a30ffa7acd80d3;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;79;306.2981,-799.7761;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;56;-1298.453,1105.033;Inherit;False;766.0004;356.6671;Fresn;5;41;43;44;40;42;;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;28;167.2419,-136.2995;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;49;-731.5238,-907.1309;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;495.2981,-906.7761;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-1252.241,780.6385;Inherit;False;Property;_ScanlinePOW;ScanlinePOW;7;0;Create;True;0;0;0;False;0;False;1;1;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;36;-695.2407,574.6385;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-1248.453,1184.033;Inherit;False;Property;_FresnelBias;FresnelBias;13;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-1241.453,1249.034;Inherit;False;Property;_FresnelScale;FresnelScale;14;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-1228.453,1349.034;Inherit;False;Property;_FresnelPower;FresnelPower;15;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;40;-937.4532,1155.033;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;42;-678.4528,1166.133;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;37;-968.1523,758.4451;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;58;609.2234,-266.8329;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;936,-134;Float;False;True;-1;2;ASEMaterialInspector;100;5;AgeOfJoy/CRT_01;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.CommentaryNode;45;-215.0649,-1561.633;Inherit;False;100;100;Flashing - take scanline and scroll it over the scanline bit;0;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;35;-120.2406,896.6386;Inherit;False;641;279.0001;Add vignette;0;;1,1,1,1;0;0
WireConnection;27;0;25;0
WireConnection;27;1;26;0
WireConnection;2;0;10;0
WireConnection;21;0;27;0
WireConnection;21;1;20;0
WireConnection;1;1;2;0
WireConnection;73;0;60;2
WireConnection;73;1;75;0
WireConnection;23;0;21;0
WireConnection;23;1;22;0
WireConnection;53;0;51;0
WireConnection;53;1;52;0
WireConnection;18;0;19;0
WireConnection;18;1;1;0
WireConnection;18;2;16;0
WireConnection;34;0;11;0
WireConnection;34;1;31;0
WireConnection;71;0;60;1
WireConnection;71;1;73;0
WireConnection;24;0;23;0
WireConnection;54;0;53;0
WireConnection;54;1;55;0
WireConnection;17;0;34;0
WireConnection;17;1;18;0
WireConnection;61;0;71;0
WireConnection;33;0;24;0
WireConnection;48;0;54;0
WireConnection;48;1;50;0
WireConnection;64;0;17;0
WireConnection;62;1;61;0
WireConnection;79;0;17;0
WireConnection;79;1;80;0
WireConnection;28;0;64;0
WireConnection;28;1;11;0
WireConnection;28;2;33;0
WireConnection;49;0;48;0
WireConnection;76;0;79;0
WireConnection;76;1;62;0
WireConnection;36;0;37;0
WireConnection;40;1;41;0
WireConnection;40;2;43;0
WireConnection;40;3;44;0
WireConnection;42;1;40;0
WireConnection;37;0;1;0
WireConnection;37;1;38;0
WireConnection;58;0;76;0
WireConnection;58;1;28;0
WireConnection;58;2;49;0
WireConnection;0;0;58;0
ASEEND*/
//CHKSM=D31B8C1FE021CB7E07CFA75AD17E11B4D808D72A
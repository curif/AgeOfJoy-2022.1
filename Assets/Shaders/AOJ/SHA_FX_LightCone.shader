// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/FX_LightCone"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_SmokeColor("SmokeColor", Color) = (1,1,1,0)
		_PannerSpeed("PannerSpeed", Vector) = (0.1,0.05,0,0)
		_OpacityMult("OpacityMult", Range( 0 , 1)) = 1
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
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float4 _SmokeColor;
		uniform sampler2D _TextureSample0;
		uniform float2 _PannerSpeed;
		uniform float _OpacityMult;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Emission = _SmokeColor.rgb;
			float2 panner6 = ( 1.0 * _Time.y * _PannerSpeed + i.uv_texcoord);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV11 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode11 = ( 0.25 + 1.0 * pow( 1.0 - fresnelNdotV11, 1.0 ) );
			o.Alpha = ( ( ( tex2D( _TextureSample0, panner6 ).g * i.uv_texcoord.y ) * _OpacityMult ) * ( 1.0 - fresnelNode11 ) );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows 

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
				float3 worldNormal : TEXCOORD3;
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
				o.worldNormal = worldNormal;
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
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
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
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-1108,241.5;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;8;-1099,-37.49991;Inherit;False;Property;_PannerSpeed;PannerSpeed;2;0;Create;True;0;0;0;False;0;False;0.1,0.05;0.3,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;6;-767,86.83347;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.1,0.05;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;1;-543,51.5;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;a11563a4124b96c4db654a9ea7217d2c;a11563a4124b96c4db654a9ea7217d2c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-342,530.5001;Inherit;False;Property;_OpacityMult;OpacityMult;3;0;Create;True;0;0;0;False;0;False;1;0.106;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;11;-23.33337,595.167;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0.25;False;2;FLOAT;1;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-271.3333,243.1667;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;65,328.5001;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;13;250.6666,583.1669;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;15;-2525.271,3364.223;Inherit;False;1619.398;1016.496;FPS & Repeat Interval;24;157;156;155;154;153;152;151;150;149;148;147;145;144;143;142;141;140;139;60;59;58;57;55;54;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;16;-1858.812,2452.223;Inherit;False;950.6241;411.678;;6;133;132;131;130;129;56;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;17;-1858.812,2884.223;Inherit;False;949.6351;455.7397;;5;94;93;92;91;90;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;18;-818.8123,3172.223;Inherit;False;896.4589;325.9572;Time Loops;3;97;96;40;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;19;-818.8123,1348.223;Inherit;False;918.0942;446.6798;Time Node;3;81;33;32;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;20;-1810.812,1348.223;Inherit;False;943.6405;441.1418;Time Parameters Node;3;80;31;30;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;21;141.1877,1348.223;Inherit;False;918.4474;449.1042;Sin Time Node;3;61;35;34;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;22;1117.188,1348.223;Inherit;False;922.7375;451.2494;Delta Time Node;3;62;37;36;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;23;2075.043,1348.223;Inherit;False;924.8825;449.1043;Cos Time Node;3;63;39;38;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;24;-818.8123,2452.223;Inherit;False;909.1516;340.1595;Animated Scrolling;5;146;66;65;64;41;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;25;205.1877,2500.223;Inherit;False;896.4589;325.9572;Time Waves;4;121;119;67;42;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;26;-818.8123,3524.223;Inherit;False;898.1931;441.1202;Bouncing;5;103;102;95;44;43;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;27;205.1877,3380.223;Inherit;False;1652.457;450.1009;Ripples;16;158;138;137;136;135;134;85;72;71;70;69;68;48;47;46;45;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;28;205.1877,2836.223;Inherit;False;1199.863;530.9493;Artificial Internal Light Source;11;120;89;88;87;86;74;73;52;51;50;49;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;29;-818.8123,2804.223;Inherit;False;905.6292;343.4328;Animated Rotation;5;101;100;99;98;53;;0,0,0,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;293.3334,373.1669;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;64,-141.5;Inherit;False;Property;_SmokeColor;SmokeColor;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.759434,0.9514182,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StickyNoteNode;30;-1794.812,1396.223;Inherit;False;483.1442;258.2374;;;0,0,0,1;t/20$Returns the time value scaled by a factor of 0.05 (20x slower)$$t$Returns the unscaled time value.$$t*2$Returns the time value scaled by a factor of 2.$$t*3$$Returns the time value scaled by a factor of 3.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;31;-1282.812,1540.223;Inherit;False;349.7281;144.8991;;;0,0,0,1;The Time Parameters node outputs Unity internal elapsed time in seconds scaled by different constant amounts;0;0
Node;AmplifyShaderEditor.StickyNoteNode;32;-802.8123,1396.223;Inherit;False;477.7251;101.8063;Scale;;0,0,0,1;Scale factor which is multiplied by the current time value;0;0
Node;AmplifyShaderEditor.StickyNoteNode;33;-290.8123,1492.223;Inherit;False;349.7281;163.3307;;;0,0,0,1;The Time node outputs Unity internal elapsed time in seconds, which can be modified by a Scale factor.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;34;157.1877,1396.223;Inherit;False;482.5251;228.2063;;;0,0,0,1;t/8$Returns the animated sine value scaled by a factor of 0.125 (8x slower)$$t/4$Returns the animated sine value scaled by a factor of 0.25 (4x slower)$$t/2$Returns the animated sine value scaled by a factor of 0.5 (2x slower)$$t$Returns the unscaled animated sine value$;0;0
Node;AmplifyShaderEditor.StickyNoteNode;35;669.1877,1540.223;Inherit;False;352.8;164.8666;;;0,0,0,1;The Sin Time node outputs an animated sine wave with values varying between -1 and 1 using Unity internal time in seconds as its angle.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;36;1133.188,1396.223;Inherit;False;495.4536;284.2296;;;0,0,0,1;dt$Returns the real time elapsed between frames$$1/dt$Returns the inverse value of the real time elapsed between frames.$$smoothDt$Returns a smooth time elapsed between frames which takes into account and tone down big delta time variations$$1/smoothDt$Returns the inverse value of the current smooth delta$$;0;0
Node;AmplifyShaderEditor.StickyNoteNode;37;1645.188,1556.223;Inherit;False;373.6848;165.209;;;0,0,0,1;The Delta Time node outputs not only Unity internal elapsed time between frames in seconds but also a smoother value of it and also the inverted value of each one of them.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;38;2093.188,1396.223;Inherit;False;495.4536;284.2296;;;0,0,0,1;t/8$Returns the animated cosine value scaled by a factor of 0.125 (8x slower).$$t/4$Returns the animated cosine value scaled by a factor of 0.25 (4x slower).$$t/2$Returns the animated cosine value scaled by a factor of 0.5 (2x slower).$$t$Returns the unscaled animated cosine value.$;0;0
Node;AmplifyShaderEditor.StickyNoteNode;39;2605.188,1556.223;Inherit;False;355.2531;166.745;;;0,0,0,1;The Cos Time node outputs an animated cosine wave with values varying between -1 and 1 using Unity internal time in seconds as its angle.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;40;-722.8123,3300.223;Inherit;False;279.255;126.149;;;0,0,0,1;Using a Fraction node, you can turn Time into a value that goes from zero to one and then jumps back to zero again - a continous loop.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;41;-514.8123,2628.223;Inherit;False;191.6921;124.2083;;;0,0,0,1;Adding Time to UV coordinates can be used to scroll textures.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;42;461.1877,2644.223;Inherit;False;279.255;126.149;;;0,0,0,1;Using Sine Time can create a smooth, animated wave where the time value oscillates between -1 and 1.  Here we've adjusted that range to 0 to 1 so we can visualize it.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;43;-642.8123,3796.223;Inherit;False;268.2867;130.5351;;;0,0,0,1;In this example, the Sine of time gives us a value that oscilates back and forth between 1 and -1.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;44;-306.8123,3796.223;Inherit;False;353.8186;141.9221;;;0,0,0,1;Using the Absolute node, now our data goes from 1 to 0 and back to 1, so it's more like a bounce than a wave.$$The same thing could also be done with vertex position to actually make your model bounce.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;45;301.1877,3684.223;Inherit;False;282;110;Polar Coordinates node;;0,0,0,1;First we create a radial gradient with the Polar Coordinates node (U).;0;0
Node;AmplifyShaderEditor.StickyNoteNode;46;845.1877,3652.223;Inherit;False;181;104;;;0,0,0,1;Then we animate it by adding time.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;47;1245.188,3620.223;Inherit;False;235;100;;;0,0,0,1;Then we increase the contrast and expand the range to 5 * PI.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;48;1581.188,3652.223;Inherit;False;219;123;;;0,0,0,1;Finally, we use the Sine node to convert the larger range into 3 ripples or waves.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;49;237.1877,3204.223;Inherit;False;199.2596;116.8203;;;0,0,0,1;Using the Sine of Time to create a cycling animated value.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;50;461.1877,3204.223;Inherit;False;254.5267;114.4173;;;0,0,0,1;Moving point inside the object. The animated value becomes the point's X coordinate - so the point is moving back and forth.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;51;669.1877,2996.223;Inherit;False;254.5267;150.4609;;;0,0,0,1;Measuring the distance between the animated point and the object space position creates a gradient that's dark when the point is close to the pivot and bright when it's further away.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;52;1117.188,3124.223;Inherit;False;254.5267;150.4609;;;0,0,0,1;Saturating and inverting the gradient gives us a mask that looks like a light source inside the volume.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;53;-530.8123,3012.223;Inherit;False;233.255;100.149;;;0,0,0,1;Using Time as the Rotation input, you can create rotating texture effects.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;54;-1794.812,3588.223;Inherit;False;150;100;Interval;;0,0,0,1;Repeat Interval;0;0
Node;AmplifyShaderEditor.StickyNoteNode;55;-1602.812,3604.223;Inherit;False;150;100;FPS;;0,0,0,1;;0;0
Node;AmplifyShaderEditor.StickyNoteNode;56;-1810.812,2740.223;Inherit;False;150;100;Speed;;0,0,0,1;;0;0
Node;AmplifyShaderEditor.StickyNoteNode;57;-1394.812,3588.223;Inherit;False;242.3752;134.83;;;0,0,0,1;The Floor Node rounds DOWN the input to the nearest integer value of a scalar or of the individual components of vectors;0;0
Node;AmplifyShaderEditor.StickyNoteNode;58;-2450.812,4132.224;Inherit;False;150;100;Interval;;0,0,0,1;Repeat Interval;0;0
Node;AmplifyShaderEditor.StickyNoteNode;59;-2258.812,4148.224;Inherit;False;150;100;FPS;;0,0,0,1;;0;0
Node;AmplifyShaderEditor.StickyNoteNode;60;-2066.812,4132.224;Inherit;False;242.3752;134.83;;;0,0,0,1;The Floor Node rounds DOWN the input to the nearest integer value of a scalar or of the individual components of vectors;0;0
Node;AmplifyShaderEditor.SinTimeNode;61;669.1877,1396.223;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DeltaTime;62;1645.188,1396.223;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CosTime;63;2621.188,1396.223;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;64;-530.8123,2532.223;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;65;-754.8123,2628.223;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;66;-738.8123,2532.223;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;67;781.1877,2548.223;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;68;1661.188,3428.223;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;69;301.1877,3428.223;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;70;493.1877,3428.223;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;71;845.1877,3428.223;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;72;637.1877,3540.223;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;73;237.1877,2900.223;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinTimeNode;74;237.1877,3060.223;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;75;2301.188,1860.223;Inherit;False;Constant;_Color2;Color 0;0;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;76;2301.188,2036.223;Inherit;False;Constant;_Color3;Color 0;0;0;Create;True;0;0;0;False;0;False;0,0.1581101,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;77;2301.188,2212.223;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;78;397.1877,1860.223;Inherit;False;Constant;_Color0;Color 0;0;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;79;397.1877,2036.223;Inherit;False;Constant;_Color1;Color 0;0;0;Create;True;0;0;0;False;0;False;0,0.1581101,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;80;-1282.812,1396.223;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;81;-290.8123,1412.223;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;82;1357.188,1860.223;Inherit;False;Constant;_Color4;Color 0;0;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;83;1357.188,2036.223;Inherit;False;Constant;_Color5;Color 0;0;0;Create;True;0;0;0;False;0;False;0,0.1581101,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;84;1357.188,2212.223;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;85;637.1877,3428.223;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.78;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;86;669.1877,2900.223;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;87;829.1877,2900.223;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;88;989.1877,2900.223;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;89;1165.188,2900.223;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;8;False;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;90;-1746.812,3156.223;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FractNode;91;-1522.812,3204.223;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;92;-1394.812,3204.223;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-1202.812,2932.223;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;94;-1570.812,2932.223;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;-0.12;False;2;FLOAT;1.77;False;3;FLOAT;1.06;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;95;-722.8123,3572.223;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;96;-402.8123,3220.223;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;97;-722.8123,3220.223;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;98;-770.8123,2884.223;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RotatorNode;99;-530.8123,2884.223;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;100;-738.8123,3012.223;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;101;-242.8123,2884.223;Inherit;True;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;50;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;102;-530.8123,3572.223;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;103;-306.8123,3572.223;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;104;-1202.812,1876.223;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;105;-1522.812,1876.223;Inherit;False;Constant;_Color8;Color 0;0;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;106;-1522.812,2052.223;Inherit;False;Constant;_Color9;Color 0;0;0;Create;True;0;0;0;False;0;False;0,0.1581101,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;107;-1522.812,2228.223;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;108;-642.8123,1876.223;Inherit;False;Constant;_Color6;Color 0;0;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;109;-642.8123,2052.223;Inherit;False;Constant;_Color7;Color 0;0;0;Create;True;0;0;0;False;0;False;0,0.1581101,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;110;-258.8123,1876.223;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;111;-626.8123,2228.223;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;112;685.1877,1860.223;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;113;1645.188,1860.223;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;114;2589.188,1860.223;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;115;2125.188,2292.223;Inherit;False;Constant;_Float1;Float 0;0;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;116;-1714.812,2308.223;Inherit;False;Constant;_Float6;Float 0;0;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-786.8123,2308.223;Inherit;False;Constant;_Float5;Float 0;0;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;118;1181.188,2308.223;Inherit;False;Constant;_Float4;Float 0;0;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;119;461.1877,2548.223;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;120;477.1877,3060.223;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;-1;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SinTimeNode;121;253.1877,2548.223;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DeltaTime;122;1149.188,2164.223;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CosTime;123;2125.188,2148.223;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;124;-818.8123,2228.223;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;125;397.1877,2212.223;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;126;221.1877,2308.223;Inherit;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinTimeNode;127;221.1877,2148.223;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;128;-1778.812,2164.223;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;129;-1810.812,2660.223;Inherit;False;Constant;_Speed;Speed;0;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;130;-1634.812,2500.223;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;131;-1634.812,2660.223;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;132;-1410.812,2548.223;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;133;-1266.812,2548.223;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;1213.188,3428.223;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;135;1373.188,3428.223;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;136;1053.188,3428.223;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;137;1309.188,3540.223;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;138;1501.188,3428.223;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleRemainderNode;139;-1602.812,3412.223;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;32;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;140;-1810.812,3412.223;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;141;-1426.812,3412.223;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;142;-1266.812,3412.223;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;-1778.812,3508.223;Inherit;False;Constant;_Interval;Interval;2;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;144;-1138.812,3492.223;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;145;-1602.812,3524.223;Inherit;False;Constant;_FPS;FPS;2;0;Create;True;0;0;0;False;0;False;15;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;146;-242.8123,2532.223;Inherit;True;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;50;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleRemainderNode;147;-2258.812,3956.223;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;32;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;148;-2466.812,3956.223;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;149;-2082.812,3956.223;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;150;-1922.812,3956.223;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-2434.812,4052.223;Inherit;False;Constant;_Interval1;Interval;2;0;Create;True;0;0;0;False;0;False;44;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;152;-2258.812,4068.223;Inherit;False;Constant;_FPS1;FPS;2;0;Create;True;0;0;0;False;0;False;45;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;153;-2034.812,3828.223;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;154;-1810.812,4036.223;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;-1394.812,3828.223;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;156;-1586.812,3908.223;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;157;-1154.812,3828.223;Inherit;True;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;50;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;158;397.1877,3524.223;Inherit;False;Polar Coordinates;-1;;4;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;3;FLOAT2;0;FLOAT;55;FLOAT;56
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;507.6666,34.33334;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;AgeOfJoy/FX_LightCone;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;2;0
WireConnection;6;2;8;0
WireConnection;1;1;6;0
WireConnection;7;0;1;2
WireConnection;7;1;2;2
WireConnection;9;0;7;0
WireConnection;9;1;10;0
WireConnection;13;0;11;0
WireConnection;12;0;9;0
WireConnection;12;1;13;0
WireConnection;64;0;66;0
WireConnection;64;1;65;0
WireConnection;67;0;119;0
WireConnection;68;0;138;0
WireConnection;70;0;69;0
WireConnection;71;0;85;0
WireConnection;71;1;72;0
WireConnection;72;0;158;55
WireConnection;77;0;123;4
WireConnection;77;1;115;0
WireConnection;77;2;115;0
WireConnection;84;0;122;1
WireConnection;84;1;118;0
WireConnection;84;2;118;0
WireConnection;85;0;70;0
WireConnection;86;0;73;0
WireConnection;86;1;120;0
WireConnection;87;0;86;0
WireConnection;88;0;87;0
WireConnection;89;0;88;0
WireConnection;91;0;90;3
WireConnection;92;0;91;0
WireConnection;93;0;94;0
WireConnection;93;1;92;0
WireConnection;96;0;97;0
WireConnection;99;0;98;0
WireConnection;99;2;100;0
WireConnection;101;0;99;0
WireConnection;102;0;95;0
WireConnection;103;0;102;0
WireConnection;104;0;105;0
WireConnection;104;1;106;0
WireConnection;104;2;107;0
WireConnection;107;0;128;2
WireConnection;107;1;116;0
WireConnection;107;2;116;0
WireConnection;110;0;108;0
WireConnection;110;1;109;0
WireConnection;110;2;111;0
WireConnection;111;0;124;0
WireConnection;111;1;117;0
WireConnection;111;2;117;0
WireConnection;112;0;78;0
WireConnection;112;1;79;0
WireConnection;112;2;125;0
WireConnection;113;0;82;0
WireConnection;113;1;83;0
WireConnection;113;2;84;0
WireConnection;114;0;75;0
WireConnection;114;1;76;0
WireConnection;114;2;77;0
WireConnection;119;0;121;4
WireConnection;120;0;74;4
WireConnection;125;0;127;4
WireConnection;125;1;126;0
WireConnection;125;2;126;0
WireConnection;131;0;129;0
WireConnection;132;0;130;2
WireConnection;132;1;131;0
WireConnection;133;0;132;0
WireConnection;134;0;136;0
WireConnection;135;0;134;0
WireConnection;136;0;71;0
WireConnection;138;0;135;0
WireConnection;138;1;137;0
WireConnection;139;0;140;0
WireConnection;139;1;143;0
WireConnection;141;0;139;0
WireConnection;141;1;145;0
WireConnection;142;0;141;0
WireConnection;144;0;142;0
WireConnection;144;1;145;0
WireConnection;146;0;64;0
WireConnection;147;0;148;0
WireConnection;147;1;151;0
WireConnection;149;0;147;0
WireConnection;149;1;152;0
WireConnection;150;0;149;0
WireConnection;154;0;150;0
WireConnection;154;1;152;0
WireConnection;155;0;153;0
WireConnection;155;1;156;0
WireConnection;156;1;154;0
WireConnection;157;0;155;0
WireConnection;0;2;5;0
WireConnection;0;9;12;0
ASEEND*/
//CHKSM=DA693C621ECBE376337E4A468802D71CF3A5043C
// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/CRT_01"
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
		_Scanline_MipBias("Scanline_Mip Bias", Range( -1 , 1)) = 0.5
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_CRTBrightnessFlickerMax("CRT Brightness Flicker Max", Range( -0.5 , 0.5)) = 0.1
		_CRTBrightnessFlickerMin("CRT Brightness Flicker Min", Range( -0.5 , 0.5)) = 0.1
		_CRTBrightnessFlickerTime("CRT Brightness Flicker Time", Range( 0 , 50)) = 0
		_Damage_RGB_Offset("Damage_RGB_Offset", Vector) = (1,1,1,0)
		_MaskVTXRedOnly("MaskVTXRedOnly", Range( 0 , 1)) = 0
		_CRTParameters("CRTParameters", Vector) = (256,256,1,0.59158)
		_CRTFX("CRTFX", 2D) = "white" {}
		_Dirt_RGBAmount_APower("Dirt_RGBAmount_APower", Vector) = (0,1,0,0)
		_CRTTiling("CRTTiling", Vector) = (1,1,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
		//[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		//[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		
		Tags { "RenderType"="Opaque" "Queue"="Geometry" "DisableBatching"="False" }
	LOD 0

		Cull Back
		AlphaToMask Off
		ZWrite On
		ZTest LEqual
		ColorMask RGBA
		
		Blend Off
		

		CGINCLUDE
		#pragma target 3.0

		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		ENDCG

		
		Pass
		{
			
			Name "ForwardBase"
			Tags { "LightMode"="ForwardBase" }

			Blend One Zero

			CGPROGRAM
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#pragma multi_compile_instancing
			#pragma multi_compile __ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#ifndef UNITY_PASS_FORWARDBASE
				#define UNITY_PASS_FORWARDBASE
			#endif
			#include "HLSLSupport.cginc"
			#ifndef UNITY_INSTANCED_LOD_FADE
				#define UNITY_INSTANCED_LOD_FADE
			#endif
			#ifndef UNITY_INSTANCED_SH
				#define UNITY_INSTANCED_SH
			#endif
			#ifndef UNITY_INSTANCED_LIGHTMAPSTS
				#define UNITY_INSTANCED_LIGHTMAPSTS
			#endif
			#include "UnityShaderVariables.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			#include "AutoLight.cginc"

			#define ASE_NEEDS_FRAG_WORLD_POSITION

			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				#if UNITY_VERSION >= 201810
					UNITY_POSITION(pos);
				#else
					float4 pos : SV_POSITION;
				#endif
				#if defined(LIGHTMAP_ON) || (!defined(LIGHTMAP_ON) && SHADER_TARGET >= 30)
					float4 lmap : TEXCOORD0;
				#endif
				#if !defined(LIGHTMAP_ON) && UNITY_SHOULD_SAMPLE_SH
					half3 sh : TEXCOORD1;
				#endif
				#if defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS) && UNITY_VERSION >= 201810 && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					UNITY_LIGHTING_COORDS(2,3)
				#elif defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if UNITY_VERSION >= 201710
						UNITY_SHADOW_COORDS(2)
					#else
						SHADOW_COORDS(2)
					#endif
				#endif
				#ifdef ASE_FOG
					UNITY_FOG_COORDS(4)
				#endif
				float4 tSpace0 : TEXCOORD5;
				float4 tSpace1 : TEXCOORD6;
				float4 tSpace2 : TEXCOORD7;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD8;
				#endif
				float4 ase_color : COLOR;
				float4 ase_texcoord9 : TEXCOORD9;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			uniform float _MaskVTXRedOnly;
			uniform sampler2D _MainTex;
			uniform float2 _CRTTiling;
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
			uniform float _Scanline_MipBias;
			uniform float _Distance_Scanline_Near;
			uniform float _Distance_Scanline_Near_Power;
			uniform float _Scanline_Amount;
			uniform float _Dotmask_ScanlineRemoval;
			uniform float _DotMask_Saturate;
			uniform float _DotMask_Brighten;
			uniform sampler2D _TextureSampleDotMask;
			uniform float _Distance_Scanline_Far;
			uniform float _Distance_Scanline_Far_Power;
			uniform float _Metallic;
			uniform sampler2D _CRTFX;
			uniform float4 _CRTFX_ST;
			uniform float4 _Dirt_RGBAmount_APower;


			
			v2f VertexFunction (appdata v  ) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_color = v.ase_color;
				o.ase_texcoord9.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord9.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.vertex.w = 1;
				v.normal = v.normal;
				v.tangent = v.tangent;

				o.pos = UnityObjectToClipPos(v.vertex);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
				o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);

				#ifdef DYNAMICLIGHTMAP_ON
				o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				#ifdef LIGHTMAP_ON
				o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				#ifndef LIGHTMAP_ON
					#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
						o.sh = 0;
						#ifdef VERTEXLIGHT_ON
						o.sh += Shade4PointLights (
							unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
							unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
							unity_4LightAtten0, worldPos, worldNormal);
						#endif
						o.sh = ShadeSHPerVertex (worldNormal, o.sh);
					#endif
				#endif

				#if UNITY_VERSION >= 201810 && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					UNITY_TRANSFER_LIGHTING(o, v.texcoord1.xy);
				#elif defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if UNITY_VERSION >= 201710
						UNITY_TRANSFER_SHADOW(o, v.texcoord1.xy);
					#else
						TRANSFER_SHADOW(o);
					#endif
				#endif

				#ifdef ASE_FOG
					UNITY_TRANSFER_FOG(o,o.pos);
				#endif
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
					o.screenPos = ComputeScreenPos(o.pos);
				#endif
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( appdata v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.tangent = v.tangent;
				o.normal = v.normal;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, UNITY_MATRIX_M, _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			v2f DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				appdata o = (appdata) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.tangent = patch[0].tangent * bary.x + patch[1].tangent * bary.y + patch[2].tangent * bary.z;
				o.normal = patch[0].normal * bary.x + patch[1].normal * bary.y + patch[2].normal * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].normal * (dot(o.vertex.xyz, patch[i].normal) - dot(patch[i].vertex.xyz, patch[i].normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			v2f vert ( appdata v )
			{
				return VertexFunction( v );
			}
			#endif

			fixed4 frag (v2f IN 
				#ifdef _DEPTHOFFSET_ON
				, out float outputDepth : SV_Depth
				#endif
				) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				#ifdef LOD_FADE_CROSSFADE
					UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
				#endif

				#if defined(_SPECULAR_SETUP)
					SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
				#else
					SurfaceOutputStandard o = (SurfaceOutputStandard)0;
				#endif
				float3 WorldTangent = float3(IN.tSpace0.x,IN.tSpace1.x,IN.tSpace2.x);
				float3 WorldBiTangent = float3(IN.tSpace0.y,IN.tSpace1.y,IN.tSpace2.y);
				float3 WorldNormal = float3(IN.tSpace0.z,IN.tSpace1.z,IN.tSpace2.z);
				float3 worldPos = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
				#else
					half atten = 1;
				#endif
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				float lerpResult233 = lerp( 1.0 , IN.ase_color.r , _MaskVTXRedOnly);
				float2 texCoord192 = IN.ase_texcoord9.xy * float2( 1,1 ) + float2( 0,0 );
				float3 desaturateInitialColor185 = ( ( tex2D( _MainTex, ( texCoord192 * _CRTTiling ) ) * float4( _Damage_RGB_Offset , 0.0 ) ) * ( 1.0 - ( saturate( ( distance( IN.ase_texcoord9.xy , float2( 0.5,0.5 ) ) - _Damage_VIgnette_Radius ) ) / ( 1.0 - _Damage_Vignette_Hardness ) ) ) ).rgb;
				float desaturateDot185 = dot( desaturateInitialColor185, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar185 = lerp( desaturateInitialColor185, desaturateDot185.xxx, _Damage_Desaturation );
				float mulTime203 = _Time.y * _CRTBrightnessFlickerTime;
				float lerpResult196 = lerp( _CRTBrightnessFlickerMin , _CRTBrightnessFlickerMax , ( ( sin( mulTime203 ) + 1.0 ) * 0.5 ));
				float3 temp_output_126_0 = ( desaturateVar185 + lerpResult196 );
				float clampResult49 = clamp( pow( ( distance( worldPos , _WorldSpaceCameraPos ) / _Distance_DotMask ) , _Distance_DotMask_Power ) , 0.0 , 1.0 );
				float lerpResult93 = lerp( _Dotmask_GameScreenBrightness , 1.0 , clampResult49);
				float2 appendResult128 = (float2(1.0 , ( _CRTParameters.y / 16.0 )));
				float2 texCoord2 = IN.ase_texcoord9.xy * appendResult128 + float2( 0,0 );
				float clampResult214 = clamp( pow( ( distance( worldPos , _WorldSpaceCameraPos ) / _Distance_Scanline_Near ) , _Distance_Scanline_Near_Power ) , 0.0 , 1.0 );
				float4 lerpResult215 = lerp( tex2Dbias( _TextureSampleScanline_Near, float4( texCoord2, 0, 1.0) ) , tex2Dbias( _TextureSampleScanline_Far, float4( texCoord2, 0, _Scanline_MipBias) ) , clampResult214);
				float lerpResult101 = lerp( _Dotmask_ScanlineRemoval , 0.0 , clampResult49);
				float4 lerpResult18 = lerp( _Scanline_Color , lerpResult215 , ( _Scanline_Amount - lerpResult101 ));
				float4 temp_output_17_0 = ( float4( ( ( temp_output_126_0 * lerpResult93 ) * _Scanline_GameScreenBrightness ) , 0.0 ) * saturate( lerpResult18 ) );
				float3 desaturateInitialColor82 = temp_output_17_0.rgb;
				float desaturateDot82 = dot( desaturateInitialColor82, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar82 = lerp( desaturateInitialColor82, desaturateDot82.xxx, _DotMask_Saturate );
				float2 appendResult236 = (float2(_CRTParameters.x , _CRTParameters.y));
				float2 texCoord61 = IN.ase_texcoord9.xy * appendResult236 + float2( 0,0 );
				float clampResult24 = clamp( pow( ( distance( worldPos , _WorldSpaceCameraPos ) / _Distance_Scanline_Far ) , _Distance_Scanline_Far_Power ) , 0.0 , 1.0 );
				float4 lerpResult28 = lerp( temp_output_17_0 , float4( temp_output_126_0 , 0.0 ) , clampResult24);
				float4 lerpResult58 = lerp( ( float4( ( desaturateVar82 + _DotMask_Brighten ) , 0.0 ) * tex2D( _TextureSampleDotMask, texCoord61 ) ) , lerpResult28 , clampResult49);
				
				float2 uv_CRTFX = IN.ase_texcoord9.xy * _CRTFX_ST.xy + _CRTFX_ST.zw;
				float4 tex2DNode237 = tex2D( _CRTFX, uv_CRTFX );
				float3 appendResult242 = (float3(tex2DNode237.r , tex2DNode237.g , tex2DNode237.b));
				float3 appendResult244 = (float3(_Dirt_RGBAmount_APower.x , _Dirt_RGBAmount_APower.y , _Dirt_RGBAmount_APower.z));
				float3 break247 = ( appendResult242 * appendResult244 );
				float lerpResult240 = lerp( 1.0 , ( break247.x + break247.y + break247.z ) , _Dirt_RGBAmount_APower.w);
				
				o.Albedo = float3(0,0,0);
				o.Normal = fixed3( 0, 0, 1 );
				o.Emission = ( lerpResult233 * lerpResult58 ).rgb;
				#if defined(_SPECULAR_SETUP)
					o.Specular = fixed3( 0, 0, 0 );
				#else
					o.Metallic = _Metallic;
				#endif
				o.Smoothness = lerpResult240;
				o.Occlusion = 1;
				o.Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#ifdef _ALPHATEST_ON
					clip( o.Alpha - AlphaClipThreshold );
				#endif

				#ifdef _DEPTHOFFSET_ON
					outputDepth = IN.pos.z;
				#endif

				#ifndef USING_DIRECTIONAL_LIGHT
					fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				#else
					fixed3 lightDir = _WorldSpaceLightPos0.xyz;
				#endif

				fixed4 c = 0;
				float3 worldN;
				worldN.x = dot(IN.tSpace0.xyz, o.Normal);
				worldN.y = dot(IN.tSpace1.xyz, o.Normal);
				worldN.z = dot(IN.tSpace2.xyz, o.Normal);
				worldN = normalize(worldN);
				o.Normal = worldN;

				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = _LightColor0.rgb;
				gi.light.dir = lightDir;

				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.worldViewDir = worldViewDir;
				giInput.atten = atten;
				#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
					giInput.lightmapUV = IN.lmap;
				#else
					giInput.lightmapUV = 0.0;
				#endif
				#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
					giInput.ambient = IN.sh;
				#else
					giInput.ambient.rgb = 0.0;
				#endif
				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
				#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
					giInput.boxMin[0] = unity_SpecCube0_BoxMin;
				#endif
				#ifdef UNITY_SPECCUBE_BOX_PROJECTION
					giInput.boxMax[0] = unity_SpecCube0_BoxMax;
					giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
					giInput.boxMax[1] = unity_SpecCube1_BoxMax;
					giInput.boxMin[1] = unity_SpecCube1_BoxMin;
					giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
				#endif

				#if defined(_SPECULAR_SETUP)
					LightingStandardSpecular_GI(o, giInput, gi);
				#else
					LightingStandard_GI( o, giInput, gi );
				#endif

				#ifdef ASE_BAKEDGI
					gi.indirect.diffuse = BakedGI;
				#endif

				#if UNITY_SHOULD_SAMPLE_SH && !defined(LIGHTMAP_ON) && defined(ASE_NO_AMBIENT)
					gi.indirect.diffuse = 0;
				#endif

				#if defined(_SPECULAR_SETUP)
					c += LightingStandardSpecular (o, worldViewDir, gi);
				#else
					c += LightingStandard( o, worldViewDir, gi );
				#endif

				#ifdef ASE_TRANSMISSION
				{
					float shadow = _TransmissionShadow;
					#ifdef DIRECTIONAL
						float3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, shadow );
					#else
						float3 lightAtten = gi.light.color;
					#endif
					half3 transmission = max(0 , -dot(o.Normal, gi.light.dir)) * lightAtten * Transmission;
					c.rgb += o.Albedo * transmission;
				}
				#endif

				#ifdef ASE_TRANSLUCENCY
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					#ifdef DIRECTIONAL
						float3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, shadow );
					#else
						float3 lightAtten = gi.light.color;
					#endif
					half3 lightDir = gi.light.dir + o.Normal * normal;
					half transVdotL = pow( saturate( dot( worldViewDir, -lightDir ) ), scattering );
					half3 translucency = lightAtten * (transVdotL * direct + gi.indirect.diffuse * ambient) * Translucency;
					c.rgb += o.Albedo * translucency * strength;
				}
				#endif

				//#ifdef ASE_REFRACTION
				//	float4 projScreenPos = ScreenPos / ScreenPos.w;
				//	float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, WorldNormal ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
				//	projScreenPos.xy += refractionOffset.xy;
				//	float3 refraction = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _GrabTexture, projScreenPos ) * RefractionColor;
				//	color.rgb = lerp( refraction, color.rgb, color.a );
				//	color.a = 1;
				//#endif

				c.rgb += o.Emission;

				#ifdef ASE_FOG
					UNITY_APPLY_FOG(IN.fogCoord, c);
				#endif
				return c;
			}
			ENDCG
		}

		
		Pass
		{
			
			Name "ForwardAdd"
			Tags { "LightMode"="ForwardAdd" }
			ZWrite Off
			Blend One One

			CGPROGRAM
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#pragma multi_compile_instancing
			#pragma multi_compile __ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1

			#pragma vertex vert
			#pragma fragment frag
			#pragma skip_variants INSTANCING_ON
			#pragma multi_compile_fwdadd_fullshadows
			#ifndef UNITY_PASS_FORWARDADD
				#define UNITY_PASS_FORWARDADD
			#endif
			#include "HLSLSupport.cginc"
			#if !defined( UNITY_INSTANCED_LOD_FADE )
				#define UNITY_INSTANCED_LOD_FADE
			#endif
			#if !defined( UNITY_INSTANCED_SH )
				#define UNITY_INSTANCED_SH
			#endif
			#if !defined( UNITY_INSTANCED_LIGHTMAPSTS )
				#define UNITY_INSTANCED_LIGHTMAPSTS
			#endif
			#include "UnityShaderVariables.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			#include "AutoLight.cginc"

			#define ASE_NEEDS_FRAG_WORLD_POSITION

			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct v2f {
				#if UNITY_VERSION >= 201810
					UNITY_POSITION(pos);
				#else
					float4 pos : SV_POSITION;
				#endif
				#if UNITY_VERSION >= 201810 && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					UNITY_LIGHTING_COORDS(1,2)
				#elif defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if UNITY_VERSION >= 201710
						UNITY_SHADOW_COORDS(1)
					#else
						SHADOW_COORDS(1)
					#endif
				#endif
				#ifdef ASE_FOG
					UNITY_FOG_COORDS(3)
				#endif
				float4 tSpace0 : TEXCOORD5;
				float4 tSpace1 : TEXCOORD6;
				float4 tSpace2 : TEXCOORD7;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD8;
				#endif
				float4 ase_color : COLOR;
				float4 ase_texcoord9 : TEXCOORD9;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			uniform float _MaskVTXRedOnly;
			uniform sampler2D _MainTex;
			uniform float2 _CRTTiling;
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
			uniform float _Scanline_MipBias;
			uniform float _Distance_Scanline_Near;
			uniform float _Distance_Scanline_Near_Power;
			uniform float _Scanline_Amount;
			uniform float _Dotmask_ScanlineRemoval;
			uniform float _DotMask_Saturate;
			uniform float _DotMask_Brighten;
			uniform sampler2D _TextureSampleDotMask;
			uniform float _Distance_Scanline_Far;
			uniform float _Distance_Scanline_Far_Power;
			uniform float _Metallic;
			uniform sampler2D _CRTFX;
			uniform float4 _CRTFX_ST;
			uniform float4 _Dirt_RGBAmount_APower;


			
			v2f VertexFunction (appdata v  ) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_color = v.ase_color;
				o.ase_texcoord9.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord9.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.vertex.w = 1;
				v.normal = v.normal;
				v.tangent = v.tangent;

				o.pos = UnityObjectToClipPos(v.vertex);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
				o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);

				#if UNITY_VERSION >= 201810 && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					UNITY_TRANSFER_LIGHTING(o, v.texcoord1.xy);
				#elif defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if UNITY_VERSION >= 201710
						UNITY_TRANSFER_SHADOW(o, v.texcoord1.xy);
					#else
						TRANSFER_SHADOW(o);
					#endif
				#endif

				#ifdef ASE_FOG
					UNITY_TRANSFER_FOG(o,o.pos);
				#endif
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
					o.screenPos = ComputeScreenPos(o.pos);
				#endif
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( appdata v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.tangent = v.tangent;
				o.normal = v.normal;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, UNITY_MATRIX_M, _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			v2f DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				appdata o = (appdata) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.tangent = patch[0].tangent * bary.x + patch[1].tangent * bary.y + patch[2].tangent * bary.z;
				o.normal = patch[0].normal * bary.x + patch[1].normal * bary.y + patch[2].normal * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].normal * (dot(o.vertex.xyz, patch[i].normal) - dot(patch[i].vertex.xyz, patch[i].normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			v2f vert ( appdata v )
			{
				return VertexFunction( v );
			}
			#endif

			fixed4 frag ( v2f IN 
				#ifdef _DEPTHOFFSET_ON
				, out float outputDepth : SV_Depth
				#endif
				) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				#ifdef LOD_FADE_CROSSFADE
					UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
				#endif

				#if defined(_SPECULAR_SETUP)
					SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
				#else
					SurfaceOutputStandard o = (SurfaceOutputStandard)0;
				#endif
				float3 WorldTangent = float3(IN.tSpace0.x,IN.tSpace1.x,IN.tSpace2.x);
				float3 WorldBiTangent = float3(IN.tSpace0.y,IN.tSpace1.y,IN.tSpace2.y);
				float3 WorldNormal = float3(IN.tSpace0.z,IN.tSpace1.z,IN.tSpace2.z);
				float3 worldPos = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
				#else
					half atten = 1;
				#endif
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif


				float lerpResult233 = lerp( 1.0 , IN.ase_color.r , _MaskVTXRedOnly);
				float2 texCoord192 = IN.ase_texcoord9.xy * float2( 1,1 ) + float2( 0,0 );
				float3 desaturateInitialColor185 = ( ( tex2D( _MainTex, ( texCoord192 * _CRTTiling ) ) * float4( _Damage_RGB_Offset , 0.0 ) ) * ( 1.0 - ( saturate( ( distance( IN.ase_texcoord9.xy , float2( 0.5,0.5 ) ) - _Damage_VIgnette_Radius ) ) / ( 1.0 - _Damage_Vignette_Hardness ) ) ) ).rgb;
				float desaturateDot185 = dot( desaturateInitialColor185, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar185 = lerp( desaturateInitialColor185, desaturateDot185.xxx, _Damage_Desaturation );
				float mulTime203 = _Time.y * _CRTBrightnessFlickerTime;
				float lerpResult196 = lerp( _CRTBrightnessFlickerMin , _CRTBrightnessFlickerMax , ( ( sin( mulTime203 ) + 1.0 ) * 0.5 ));
				float3 temp_output_126_0 = ( desaturateVar185 + lerpResult196 );
				float clampResult49 = clamp( pow( ( distance( worldPos , _WorldSpaceCameraPos ) / _Distance_DotMask ) , _Distance_DotMask_Power ) , 0.0 , 1.0 );
				float lerpResult93 = lerp( _Dotmask_GameScreenBrightness , 1.0 , clampResult49);
				float2 appendResult128 = (float2(1.0 , ( _CRTParameters.y / 16.0 )));
				float2 texCoord2 = IN.ase_texcoord9.xy * appendResult128 + float2( 0,0 );
				float clampResult214 = clamp( pow( ( distance( worldPos , _WorldSpaceCameraPos ) / _Distance_Scanline_Near ) , _Distance_Scanline_Near_Power ) , 0.0 , 1.0 );
				float4 lerpResult215 = lerp( tex2Dbias( _TextureSampleScanline_Near, float4( texCoord2, 0, 1.0) ) , tex2Dbias( _TextureSampleScanline_Far, float4( texCoord2, 0, _Scanline_MipBias) ) , clampResult214);
				float lerpResult101 = lerp( _Dotmask_ScanlineRemoval , 0.0 , clampResult49);
				float4 lerpResult18 = lerp( _Scanline_Color , lerpResult215 , ( _Scanline_Amount - lerpResult101 ));
				float4 temp_output_17_0 = ( float4( ( ( temp_output_126_0 * lerpResult93 ) * _Scanline_GameScreenBrightness ) , 0.0 ) * saturate( lerpResult18 ) );
				float3 desaturateInitialColor82 = temp_output_17_0.rgb;
				float desaturateDot82 = dot( desaturateInitialColor82, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar82 = lerp( desaturateInitialColor82, desaturateDot82.xxx, _DotMask_Saturate );
				float2 appendResult236 = (float2(_CRTParameters.x , _CRTParameters.y));
				float2 texCoord61 = IN.ase_texcoord9.xy * appendResult236 + float2( 0,0 );
				float clampResult24 = clamp( pow( ( distance( worldPos , _WorldSpaceCameraPos ) / _Distance_Scanline_Far ) , _Distance_Scanline_Far_Power ) , 0.0 , 1.0 );
				float4 lerpResult28 = lerp( temp_output_17_0 , float4( temp_output_126_0 , 0.0 ) , clampResult24);
				float4 lerpResult58 = lerp( ( float4( ( desaturateVar82 + _DotMask_Brighten ) , 0.0 ) * tex2D( _TextureSampleDotMask, texCoord61 ) ) , lerpResult28 , clampResult49);
				
				float2 uv_CRTFX = IN.ase_texcoord9.xy * _CRTFX_ST.xy + _CRTFX_ST.zw;
				float4 tex2DNode237 = tex2D( _CRTFX, uv_CRTFX );
				float3 appendResult242 = (float3(tex2DNode237.r , tex2DNode237.g , tex2DNode237.b));
				float3 appendResult244 = (float3(_Dirt_RGBAmount_APower.x , _Dirt_RGBAmount_APower.y , _Dirt_RGBAmount_APower.z));
				float3 break247 = ( appendResult242 * appendResult244 );
				float lerpResult240 = lerp( 1.0 , ( break247.x + break247.y + break247.z ) , _Dirt_RGBAmount_APower.w);
				
				o.Albedo = float3(0,0,0);
				o.Normal = fixed3( 0, 0, 1 );
				o.Emission = ( lerpResult233 * lerpResult58 ).rgb;
				#if defined(_SPECULAR_SETUP)
					o.Specular = fixed3( 0, 0, 0 );
				#else
					o.Metallic = _Metallic;
				#endif
				o.Smoothness = lerpResult240;
				o.Occlusion = 1;
				o.Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#ifdef _ALPHATEST_ON
					clip( o.Alpha - AlphaClipThreshold );
				#endif

				#ifdef _DEPTHOFFSET_ON
					outputDepth = IN.pos.z;
				#endif

				#ifndef USING_DIRECTIONAL_LIGHT
					fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				#else
					fixed3 lightDir = _WorldSpaceLightPos0.xyz;
				#endif

				fixed4 c = 0;
				float3 worldN;
				worldN.x = dot(IN.tSpace0.xyz, o.Normal);
				worldN.y = dot(IN.tSpace1.xyz, o.Normal);
				worldN.z = dot(IN.tSpace2.xyz, o.Normal);
				worldN = normalize(worldN);
				o.Normal = worldN;

				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = _LightColor0.rgb;
				gi.light.dir = lightDir;
				gi.light.color *= atten;

				#if defined(_SPECULAR_SETUP)
					c += LightingStandardSpecular( o, worldViewDir, gi );
				#else
					c += LightingStandard( o, worldViewDir, gi );
				#endif

				#ifdef ASE_TRANSMISSION
				{
					float shadow = _TransmissionShadow;
					#ifdef DIRECTIONAL
						float3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, shadow );
					#else
						float3 lightAtten = gi.light.color;
					#endif
					half3 transmission = max(0 , -dot(o.Normal, gi.light.dir)) * lightAtten * Transmission;
					c.rgb += o.Albedo * transmission;
				}
				#endif

				#ifdef ASE_TRANSLUCENCY
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					#ifdef DIRECTIONAL
						float3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, shadow );
					#else
						float3 lightAtten = gi.light.color;
					#endif
					half3 lightDir = gi.light.dir + o.Normal * normal;
					half transVdotL = pow( saturate( dot( worldViewDir, -lightDir ) ), scattering );
					half3 translucency = lightAtten * (transVdotL * direct + gi.indirect.diffuse * ambient) * Translucency;
					c.rgb += o.Albedo * translucency * strength;
				}
				#endif

				//#ifdef ASE_REFRACTION
				//	float4 projScreenPos = ScreenPos / ScreenPos.w;
				//	float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, WorldNormal ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
				//	projScreenPos.xy += refractionOffset.xy;
				//	float3 refraction = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _GrabTexture, projScreenPos ) * RefractionColor;
				//	color.rgb = lerp( refraction, color.rgb, color.a );
				//	color.a = 1;
				//#endif

				#ifdef ASE_FOG
					UNITY_APPLY_FOG(IN.fogCoord, c);
				#endif
				return c;
			}
			ENDCG
		}

		
		Pass
		{
			
			Name "Deferred"
			Tags { "LightMode"="Deferred" }

			AlphaToMask Off

			CGPROGRAM
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#pragma multi_compile_instancing
			#pragma multi_compile __ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#pragma multi_compile_prepassfinal
			#ifndef UNITY_PASS_DEFERRED
				#define UNITY_PASS_DEFERRED
			#endif
			#include "HLSLSupport.cginc"
			#if !defined( UNITY_INSTANCED_LOD_FADE )
				#define UNITY_INSTANCED_LOD_FADE
			#endif
			#if !defined( UNITY_INSTANCED_SH )
				#define UNITY_INSTANCED_SH
			#endif
			#if !defined( UNITY_INSTANCED_LIGHTMAPSTS )
				#define UNITY_INSTANCED_LIGHTMAPSTS
			#endif
			#include "UnityShaderVariables.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"

			#define ASE_NEEDS_FRAG_WORLD_POSITION

			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				#if UNITY_VERSION >= 201810
					UNITY_POSITION(pos);
				#else
					float4 pos : SV_POSITION;
				#endif
				float4 lmap : TEXCOORD2;
				#ifndef LIGHTMAP_ON
					#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
						half3 sh : TEXCOORD3;
					#endif
				#else
					#ifdef DIRLIGHTMAP_OFF
						float4 lmapFadePos : TEXCOORD4;
					#endif
				#endif
				float4 tSpace0 : TEXCOORD5;
				float4 tSpace1 : TEXCOORD6;
				float4 tSpace2 : TEXCOORD7;
				float4 ase_color : COLOR;
				float4 ase_texcoord8 : TEXCOORD8;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#ifdef LIGHTMAP_ON
			float4 unity_LightmapFade;
			#endif
			fixed4 unity_Ambient;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			uniform float _MaskVTXRedOnly;
			uniform sampler2D _MainTex;
			uniform float2 _CRTTiling;
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
			uniform float _Scanline_MipBias;
			uniform float _Distance_Scanline_Near;
			uniform float _Distance_Scanline_Near_Power;
			uniform float _Scanline_Amount;
			uniform float _Dotmask_ScanlineRemoval;
			uniform float _DotMask_Saturate;
			uniform float _DotMask_Brighten;
			uniform sampler2D _TextureSampleDotMask;
			uniform float _Distance_Scanline_Far;
			uniform float _Distance_Scanline_Far_Power;
			uniform float _Metallic;
			uniform sampler2D _CRTFX;
			uniform float4 _CRTFX_ST;
			uniform float4 _Dirt_RGBAmount_APower;


			
			v2f VertexFunction (appdata v  ) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_color = v.ase_color;
				o.ase_texcoord8.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.vertex.w = 1;
				v.normal = v.normal;
				v.tangent = v.tangent;

				o.pos = UnityObjectToClipPos(v.vertex);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
				o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);

				#ifdef DYNAMICLIGHTMAP_ON
					o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#else
					o.lmap.zw = 0;
				#endif
				#ifdef LIGHTMAP_ON
					o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
					#ifdef DIRLIGHTMAP_OFF
						o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
						o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
					#endif
				#else
					o.lmap.xy = 0;
					#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
						o.sh = 0;
						o.sh = ShadeSHPerVertex (worldNormal, o.sh);
					#endif
				#endif
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( appdata v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.tangent = v.tangent;
				o.normal = v.normal;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, UNITY_MATRIX_M, _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			v2f DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				appdata o = (appdata) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.tangent = patch[0].tangent * bary.x + patch[1].tangent * bary.y + patch[2].tangent * bary.z;
				o.normal = patch[0].normal * bary.x + patch[1].normal * bary.y + patch[2].normal * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].normal * (dot(o.vertex.xyz, patch[i].normal) - dot(patch[i].vertex.xyz, patch[i].normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			v2f vert ( appdata v )
			{
				return VertexFunction( v );
			}
			#endif

			void frag (v2f IN 
				, out half4 outGBuffer0 : SV_Target0
				, out half4 outGBuffer1 : SV_Target1
				, out half4 outGBuffer2 : SV_Target2
				, out half4 outEmission : SV_Target3
				#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
				, out half4 outShadowMask : SV_Target4
				#endif
				#ifdef _DEPTHOFFSET_ON
				, out float outputDepth : SV_Depth
				#endif
			)
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				#ifdef LOD_FADE_CROSSFADE
					UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
				#endif

				#if defined(_SPECULAR_SETUP)
					SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
				#else
					SurfaceOutputStandard o = (SurfaceOutputStandard)0;
				#endif
				float3 WorldTangent = float3(IN.tSpace0.x,IN.tSpace1.x,IN.tSpace2.x);
				float3 WorldBiTangent = float3(IN.tSpace0.y,IN.tSpace1.y,IN.tSpace2.y);
				float3 WorldNormal = float3(IN.tSpace0.z,IN.tSpace1.z,IN.tSpace2.z);
				float3 worldPos = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				half atten = 1;

				float lerpResult233 = lerp( 1.0 , IN.ase_color.r , _MaskVTXRedOnly);
				float2 texCoord192 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float3 desaturateInitialColor185 = ( ( tex2D( _MainTex, ( texCoord192 * _CRTTiling ) ) * float4( _Damage_RGB_Offset , 0.0 ) ) * ( 1.0 - ( saturate( ( distance( IN.ase_texcoord8.xy , float2( 0.5,0.5 ) ) - _Damage_VIgnette_Radius ) ) / ( 1.0 - _Damage_Vignette_Hardness ) ) ) ).rgb;
				float desaturateDot185 = dot( desaturateInitialColor185, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar185 = lerp( desaturateInitialColor185, desaturateDot185.xxx, _Damage_Desaturation );
				float mulTime203 = _Time.y * _CRTBrightnessFlickerTime;
				float lerpResult196 = lerp( _CRTBrightnessFlickerMin , _CRTBrightnessFlickerMax , ( ( sin( mulTime203 ) + 1.0 ) * 0.5 ));
				float3 temp_output_126_0 = ( desaturateVar185 + lerpResult196 );
				float clampResult49 = clamp( pow( ( distance( worldPos , _WorldSpaceCameraPos ) / _Distance_DotMask ) , _Distance_DotMask_Power ) , 0.0 , 1.0 );
				float lerpResult93 = lerp( _Dotmask_GameScreenBrightness , 1.0 , clampResult49);
				float2 appendResult128 = (float2(1.0 , ( _CRTParameters.y / 16.0 )));
				float2 texCoord2 = IN.ase_texcoord8.xy * appendResult128 + float2( 0,0 );
				float clampResult214 = clamp( pow( ( distance( worldPos , _WorldSpaceCameraPos ) / _Distance_Scanline_Near ) , _Distance_Scanline_Near_Power ) , 0.0 , 1.0 );
				float4 lerpResult215 = lerp( tex2Dbias( _TextureSampleScanline_Near, float4( texCoord2, 0, 1.0) ) , tex2Dbias( _TextureSampleScanline_Far, float4( texCoord2, 0, _Scanline_MipBias) ) , clampResult214);
				float lerpResult101 = lerp( _Dotmask_ScanlineRemoval , 0.0 , clampResult49);
				float4 lerpResult18 = lerp( _Scanline_Color , lerpResult215 , ( _Scanline_Amount - lerpResult101 ));
				float4 temp_output_17_0 = ( float4( ( ( temp_output_126_0 * lerpResult93 ) * _Scanline_GameScreenBrightness ) , 0.0 ) * saturate( lerpResult18 ) );
				float3 desaturateInitialColor82 = temp_output_17_0.rgb;
				float desaturateDot82 = dot( desaturateInitialColor82, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar82 = lerp( desaturateInitialColor82, desaturateDot82.xxx, _DotMask_Saturate );
				float2 appendResult236 = (float2(_CRTParameters.x , _CRTParameters.y));
				float2 texCoord61 = IN.ase_texcoord8.xy * appendResult236 + float2( 0,0 );
				float clampResult24 = clamp( pow( ( distance( worldPos , _WorldSpaceCameraPos ) / _Distance_Scanline_Far ) , _Distance_Scanline_Far_Power ) , 0.0 , 1.0 );
				float4 lerpResult28 = lerp( temp_output_17_0 , float4( temp_output_126_0 , 0.0 ) , clampResult24);
				float4 lerpResult58 = lerp( ( float4( ( desaturateVar82 + _DotMask_Brighten ) , 0.0 ) * tex2D( _TextureSampleDotMask, texCoord61 ) ) , lerpResult28 , clampResult49);
				
				float2 uv_CRTFX = IN.ase_texcoord8.xy * _CRTFX_ST.xy + _CRTFX_ST.zw;
				float4 tex2DNode237 = tex2D( _CRTFX, uv_CRTFX );
				float3 appendResult242 = (float3(tex2DNode237.r , tex2DNode237.g , tex2DNode237.b));
				float3 appendResult244 = (float3(_Dirt_RGBAmount_APower.x , _Dirt_RGBAmount_APower.y , _Dirt_RGBAmount_APower.z));
				float3 break247 = ( appendResult242 * appendResult244 );
				float lerpResult240 = lerp( 1.0 , ( break247.x + break247.y + break247.z ) , _Dirt_RGBAmount_APower.w);
				
				o.Albedo = float3(0,0,0);
				o.Normal = fixed3( 0, 0, 1 );
				o.Emission = ( lerpResult233 * lerpResult58 ).rgb;
				#if defined(_SPECULAR_SETUP)
					o.Specular = fixed3( 0, 0, 0 );
				#else
					o.Metallic = _Metallic;
				#endif
				o.Smoothness = lerpResult240;
				o.Occlusion = 1;
				o.Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float3 BakedGI = 0;

				#ifdef _ALPHATEST_ON
					clip( o.Alpha - AlphaClipThreshold );
				#endif

				#ifdef _DEPTHOFFSET_ON
					outputDepth = IN.pos.z;
				#endif

				#ifndef USING_DIRECTIONAL_LIGHT
					fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				#else
					fixed3 lightDir = _WorldSpaceLightPos0.xyz;
				#endif

				float3 worldN;
				worldN.x = dot(IN.tSpace0.xyz, o.Normal);
				worldN.y = dot(IN.tSpace1.xyz, o.Normal);
				worldN.z = dot(IN.tSpace2.xyz, o.Normal);
				worldN = normalize(worldN);
				o.Normal = worldN;

				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = 0;
				gi.light.dir = half3(0,1,0);

				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.worldViewDir = worldViewDir;
				giInput.atten = atten;
				#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
					giInput.lightmapUV = IN.lmap;
				#else
					giInput.lightmapUV = 0.0;
				#endif
				#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
					giInput.ambient = IN.sh;
				#else
					giInput.ambient.rgb = 0.0;
				#endif
				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
				#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
					giInput.boxMin[0] = unity_SpecCube0_BoxMin;
				#endif
				#ifdef UNITY_SPECCUBE_BOX_PROJECTION
					giInput.boxMax[0] = unity_SpecCube0_BoxMax;
					giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
					giInput.boxMax[1] = unity_SpecCube1_BoxMax;
					giInput.boxMin[1] = unity_SpecCube1_BoxMin;
					giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
				#endif

				#if defined(_SPECULAR_SETUP)
					LightingStandardSpecular_GI( o, giInput, gi );
				#else
					LightingStandard_GI( o, giInput, gi );
				#endif

				#ifdef ASE_BAKEDGI
					gi.indirect.diffuse = BakedGI;
				#endif

				#if UNITY_SHOULD_SAMPLE_SH && !defined(LIGHTMAP_ON) && defined(ASE_NO_AMBIENT)
					gi.indirect.diffuse = 0;
				#endif

				#if defined(_SPECULAR_SETUP)
					outEmission = LightingStandardSpecular_Deferred( o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2 );
				#else
					outEmission = LightingStandard_Deferred( o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2 );
				#endif

				#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
					outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, float3(0, 0, 0));
				#endif
				#ifndef UNITY_HDR_ON
					outEmission.rgb = exp2(-outEmission.rgb);
				#endif
			}
			ENDCG
		}

		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }
			Cull Off

			CGPROGRAM
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#pragma multi_compile_instancing
			#pragma multi_compile __ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1

			#pragma vertex vert
			#pragma fragment frag
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#pragma shader_feature EDITOR_VISUALIZATION
			#ifndef UNITY_PASS_META
				#define UNITY_PASS_META
			#endif
			#include "HLSLSupport.cginc"
			#if !defined( UNITY_INSTANCED_LOD_FADE )
				#define UNITY_INSTANCED_LOD_FADE
			#endif
			#if !defined( UNITY_INSTANCED_SH )
				#define UNITY_INSTANCED_SH
			#endif
			#if !defined( UNITY_INSTANCED_LIGHTMAPSTS )
				#define UNITY_INSTANCED_LIGHTMAPSTS
			#endif
			#include "UnityShaderVariables.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			#include "UnityMetaPass.cginc"

			
			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct v2f {
				#if UNITY_VERSION >= 201810
					UNITY_POSITION(pos);
				#else
					float4 pos : SV_POSITION;
				#endif
				#ifdef EDITOR_VISUALIZATION
					float2 vizUV : TEXCOORD1;
					float4 lightCoord : TEXCOORD2;
				#endif
				float4 ase_color : COLOR;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			uniform float _MaskVTXRedOnly;
			uniform sampler2D _MainTex;
			uniform float2 _CRTTiling;
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
			uniform float _Scanline_MipBias;
			uniform float _Distance_Scanline_Near;
			uniform float _Distance_Scanline_Near_Power;
			uniform float _Scanline_Amount;
			uniform float _Dotmask_ScanlineRemoval;
			uniform float _DotMask_Saturate;
			uniform float _DotMask_Brighten;
			uniform sampler2D _TextureSampleDotMask;
			uniform float _Distance_Scanline_Far;
			uniform float _Distance_Scanline_Far_Power;


			
			v2f VertexFunction (appdata v  ) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = mul(unity_ObjectToWorld, float4( (v.vertex).xyz, 1 )).xyz;
				o.ase_texcoord4.xyz = ase_worldPos;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;
				o.ase_texcoord4.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.vertex.w = 1;
				v.normal = v.normal;
				v.tangent = v.tangent;

				#ifdef EDITOR_VISUALIZATION
					o.vizUV = 0;
					o.lightCoord = 0;
					if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
						o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
					else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
					{
						o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
						o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
					}
				#endif

				o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( appdata v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.tangent = v.tangent;
				o.normal = v.normal;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, UNITY_MATRIX_M, _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			v2f DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				appdata o = (appdata) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.tangent = patch[0].tangent * bary.x + patch[1].tangent * bary.y + patch[2].tangent * bary.z;
				o.normal = patch[0].normal * bary.x + patch[1].normal * bary.y + patch[2].normal * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].normal * (dot(o.vertex.xyz, patch[i].normal) - dot(patch[i].vertex.xyz, patch[i].normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			v2f vert ( appdata v )
			{
				return VertexFunction( v );
			}
			#endif

			fixed4 frag (v2f IN 
				#ifdef _DEPTHOFFSET_ON
				, out float outputDepth : SV_Depth
				#endif
				) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				#ifdef LOD_FADE_CROSSFADE
					UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
				#endif

				#if defined(_SPECULAR_SETUP)
					SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
				#else
					SurfaceOutputStandard o = (SurfaceOutputStandard)0;
				#endif

				float lerpResult233 = lerp( 1.0 , IN.ase_color.r , _MaskVTXRedOnly);
				float2 texCoord192 = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float3 desaturateInitialColor185 = ( ( tex2D( _MainTex, ( texCoord192 * _CRTTiling ) ) * float4( _Damage_RGB_Offset , 0.0 ) ) * ( 1.0 - ( saturate( ( distance( IN.ase_texcoord3.xy , float2( 0.5,0.5 ) ) - _Damage_VIgnette_Radius ) ) / ( 1.0 - _Damage_Vignette_Hardness ) ) ) ).rgb;
				float desaturateDot185 = dot( desaturateInitialColor185, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar185 = lerp( desaturateInitialColor185, desaturateDot185.xxx, _Damage_Desaturation );
				float mulTime203 = _Time.y * _CRTBrightnessFlickerTime;
				float lerpResult196 = lerp( _CRTBrightnessFlickerMin , _CRTBrightnessFlickerMax , ( ( sin( mulTime203 ) + 1.0 ) * 0.5 ));
				float3 temp_output_126_0 = ( desaturateVar185 + lerpResult196 );
				float3 ase_worldPos = IN.ase_texcoord4.xyz;
				float clampResult49 = clamp( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / _Distance_DotMask ) , _Distance_DotMask_Power ) , 0.0 , 1.0 );
				float lerpResult93 = lerp( _Dotmask_GameScreenBrightness , 1.0 , clampResult49);
				float2 appendResult128 = (float2(1.0 , ( _CRTParameters.y / 16.0 )));
				float2 texCoord2 = IN.ase_texcoord3.xy * appendResult128 + float2( 0,0 );
				float clampResult214 = clamp( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / _Distance_Scanline_Near ) , _Distance_Scanline_Near_Power ) , 0.0 , 1.0 );
				float4 lerpResult215 = lerp( tex2Dbias( _TextureSampleScanline_Near, float4( texCoord2, 0, 1.0) ) , tex2Dbias( _TextureSampleScanline_Far, float4( texCoord2, 0, _Scanline_MipBias) ) , clampResult214);
				float lerpResult101 = lerp( _Dotmask_ScanlineRemoval , 0.0 , clampResult49);
				float4 lerpResult18 = lerp( _Scanline_Color , lerpResult215 , ( _Scanline_Amount - lerpResult101 ));
				float4 temp_output_17_0 = ( float4( ( ( temp_output_126_0 * lerpResult93 ) * _Scanline_GameScreenBrightness ) , 0.0 ) * saturate( lerpResult18 ) );
				float3 desaturateInitialColor82 = temp_output_17_0.rgb;
				float desaturateDot82 = dot( desaturateInitialColor82, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar82 = lerp( desaturateInitialColor82, desaturateDot82.xxx, _DotMask_Saturate );
				float2 appendResult236 = (float2(_CRTParameters.x , _CRTParameters.y));
				float2 texCoord61 = IN.ase_texcoord3.xy * appendResult236 + float2( 0,0 );
				float clampResult24 = clamp( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / _Distance_Scanline_Far ) , _Distance_Scanline_Far_Power ) , 0.0 , 1.0 );
				float4 lerpResult28 = lerp( temp_output_17_0 , float4( temp_output_126_0 , 0.0 ) , clampResult24);
				float4 lerpResult58 = lerp( ( float4( ( desaturateVar82 + _DotMask_Brighten ) , 0.0 ) * tex2D( _TextureSampleDotMask, texCoord61 ) ) , lerpResult28 , clampResult49);
				
				o.Albedo = float3(0,0,0);
				o.Normal = fixed3( 0, 0, 1 );
				o.Emission = ( lerpResult233 * lerpResult58 ).rgb;
				o.Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip( o.Alpha - AlphaClipThreshold );
				#endif

				#ifdef _DEPTHOFFSET_ON
					outputDepth = IN.pos.z;
				#endif

				UnityMetaInput metaIN;
				UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
				metaIN.Albedo = o.Albedo;
				metaIN.Emission = o.Emission;
				#ifdef EDITOR_VISUALIZATION
					metaIN.VizUV = IN.vizUV;
					metaIN.LightCoord = IN.lightCoord;
				#endif
				return UnityMetaFragment(metaIN);
			}
			ENDCG
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }
			ZWrite On
			ZTest LEqual
			AlphaToMask Off

			CGPROGRAM
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#pragma multi_compile_instancing
			#pragma multi_compile __ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1

			#pragma vertex vert
			#pragma fragment frag
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#pragma multi_compile_shadowcaster
			#ifndef UNITY_PASS_SHADOWCASTER
				#define UNITY_PASS_SHADOWCASTER
			#endif
			#include "HLSLSupport.cginc"
			#ifndef UNITY_INSTANCED_LOD_FADE
				#define UNITY_INSTANCED_LOD_FADE
			#endif
			#ifndef UNITY_INSTANCED_SH
				#define UNITY_INSTANCED_SH
			#endif
			#ifndef UNITY_INSTANCED_LIGHTMAPSTS
				#define UNITY_INSTANCED_LIGHTMAPSTS
			#endif
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityShaderVariables.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"

			
			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				V2F_SHADOW_CASTER;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#ifdef UNITY_STANDARD_USE_DITHER_MASK
				sampler3D _DitherMaskLOD;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			

			
			v2f VertexFunction (appdata v  ) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.vertex.w = 1;
				v.normal = v.normal;
				v.tangent = v.tangent;

				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( appdata v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.tangent = v.tangent;
				o.normal = v.normal;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, UNITY_MATRIX_M, _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			v2f DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				appdata o = (appdata) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.tangent = patch[0].tangent * bary.x + patch[1].tangent * bary.y + patch[2].tangent * bary.z;
				o.normal = patch[0].normal * bary.x + patch[1].normal * bary.y + patch[2].normal * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].normal * (dot(o.vertex.xyz, patch[i].normal) - dot(patch[i].vertex.xyz, patch[i].normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			v2f vert ( appdata v )
			{
				return VertexFunction( v );
			}
			#endif

			fixed4 frag (v2f IN 
				#ifdef _DEPTHOFFSET_ON
				, out float outputDepth : SV_Depth
				#endif
				#if !defined( CAN_SKIP_VPOS )
				, UNITY_VPOS_TYPE vpos : VPOS
				#endif
				) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				#ifdef LOD_FADE_CROSSFADE
					UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
				#endif

				#if defined(_SPECULAR_SETUP)
					SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
				#else
					SurfaceOutputStandard o = (SurfaceOutputStandard)0;
				#endif

				
				o.Normal = fixed3( 0, 0, 1 );
				o.Occlusion = 1;
				o.Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_SHADOW_ON
					if (unity_LightShadowBias.z != 0.0)
						clip(o.Alpha - AlphaClipThresholdShadow);
					#ifdef _ALPHATEST_ON
					else
						clip(o.Alpha - AlphaClipThreshold);
					#endif
				#else
					#ifdef _ALPHATEST_ON
						clip(o.Alpha - AlphaClipThreshold);
					#endif
				#endif

				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif

				#ifdef UNITY_STANDARD_USE_DITHER_MASK
					half alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy*0.25,o.Alpha*0.9375)).a;
					clip(alphaRef - 0.01);
				#endif

				#ifdef _DEPTHOFFSET_ON
					outputDepth = IN.pos.z;
				#endif

				SHADOW_CASTER_FRAGMENT(IN)
			}
			ENDCG
		}
		
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CommentaryNode;89;-3442.326,416.804;Inherit;False;2502;953.5816;Scanlines;15;236;205;104;215;98;100;99;16;1;2;18;19;102;103;130;;0.1536936,1,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;190;-3496.94,-495.9763;Inherit;False;558.502;338.8665;Damage Desaturation;2;186;185;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;189;-4301.94,-519.5765;Inherit;False;603.7002;397.2666;Damage RGB;2;187;188;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;130;-3056.968,663.22;Inherit;False;471;230.3333;Scanlines technically appearing every other line;2;128;127;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;46;-2599.804,-1052.528;Inherit;False;1297.335;546.6675;Transition DotMask;8;55;54;53;52;51;50;49;48;;0,0.8417869,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;103;-3386.326,883.8041;Inherit;False;260;210;Vertical Screen res / 8;1;235;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;90;-4816.853,-692.6508;Inherit;False;363.3334;277;GameScreen;1;11;;1,0,0.9828625,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;94;-1190.249,-630.0359;Inherit;False;620;541;Boost Texture when Dotmask Present;5;91;95;93;92;96;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;102;-1635.793,1072.348;Inherit;False;228;210;A=dotmask, B=nodotmask;1;101;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;30;-2639.204,-353.0924;Inherit;False;1297.335;546.6675;Transition Scanline;8;25;26;20;27;21;22;24;23;;0,0.8326645,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;97;-904.7473,91.75391;Inherit;False;614.1814;256.0048;Boost Texture when Scanlines present;2;31;34;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;87;-1646.941,-1524.502;Inherit;False;1235.334;406.3926;DotMask;2;61;62;;1,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;86;-750.9811,-1019.187;Inherit;False;673.8616;357.9644;Dotmask Desaturation;4;79;80;82;83;;1,0.7044024,0.7044024,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;65;84.48536,76.33668;Inherit;False;273;157;Frame and Scanlines;1;64;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;88;238.744,-999.3019;Inherit;False;228;186.3333;Multiply Dotmask by Frame;1;76;;1,0.7610062,0.7610062,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;29;250.8946,-222.7503;Inherit;False;415;219;A = Frame and Scanlines, B = Raw Frame;1;28;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;136;-5651.142,-81.46181;Inherit;False;1558;351;SphereMask;10;157;158;179;180;156;155;154;153;152;151;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;206;-3238.498,1647.25;Inherit;False;1297.335;546.6675;Transition Scanlines Near;8;214;213;212;211;210;209;208;207;;0,0.8417869,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-1173.237,-473.562;Inherit;False;Property;_Dotmask_GameScreenBrightness;Dotmask_GameScreenBrightness;18;0;Create;True;0;0;0;False;0;False;1;2.24;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;25;-2501.205,-303.0924;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;26;-2589.205,-125.7583;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;19;-1616.324,466.804;Inherit;False;Property;_Scanline_Color;Scanline_Color;12;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;92;-911.4401,-244.3924;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;93;-874.7485,-602.4357;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;27;-2255.204,-242.4257;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-753.2373,-184.8619;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-854.7473,235.092;Inherit;False;Property;_Scanline_GameScreenBrightness;Scanline_GameScreenBrightness;7;0;Create;True;0;0;0;False;0;False;2.5;1.5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;18;-1359.324,614.8042;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;21;-1898.537,-192.4256;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-468.5659,141.7539;Inherit;False;2;2;0;FLOAT3;1,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;23;-1650.868,-107.7921;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-212.6577,136.7485;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-699.1408,-782.6078;Inherit;False;Property;_DotMask_Saturate;DotMask_Saturate;17;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;24;-1480.869,-107.7921;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-700.9811,-927.5185;Inherit;False;Property;_DotMask_Brighten;DotMask_Brighten;16;0;Create;True;0;0;0;False;0;False;0.273;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;82;-287.12,-797.5563;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;61;-1013.941,-1388.502;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;33;-1124.618,-64.38861;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;64;115.8195,140.9099;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;91;-842.5527,-362.1745;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;79;-274.2234,-969.1874;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;28;300.8942,-172.7503;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;288.744,-949.3019;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;58;679.7466,-651.7343;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;62;-724.9406,-1424.502;Inherit;True;Property;_TextureSampleDotMask;Texture Sample DotMask;5;0;Create;True;0;0;0;False;0;False;-1;52fa22fcfb11d0e4280b2efa2ffb323b;52fa22fcfb11d0e4280b2efa2ffb323b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;126;-2738.403,-519.7662;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;131;-2692.291,-1035.823;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DistanceOpNode;151;-5379.142,-17.46176;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;152;-5187.141,-17.46176;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;153;-5027.142,-17.46176;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;154;-5027.142,94.53832;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;155;-4835.143,-17.46176;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;156;-4691.142,-17.46176;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;180;-5635.142,-17.46176;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;179;-5602.142,115.5385;Inherit;False;Constant;_SphereCenter;Sphere Center;0;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleDivideOpNode;127;-2958.795,701.1768;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;16;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-2491.094,666.093;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;128;-2788.664,697.8539;Inherit;False;FLOAT2;4;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;157;-5409.227,199.1412;Inherit;False;Property;_Damage_Vignette_Hardness;Damage_Vignette_Hardness;0;0;Create;True;0;0;0;False;0;False;1;0.596;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;158;-5379.142,94.53832;Inherit;False;Property;_Damage_VIgnette_Radius;Damage_VIgnette_Radius;2;0;Create;True;0;0;0;False;0;False;0;0.435;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;187;-4251.94,-305.9766;Inherit;False;Property;_Damage_RGB_Offset;Damage_RGB_Offset;26;0;Create;True;0;0;0;False;0;False;1,1,1;1,1,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;188;-3876.24,-469.5765;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;184;-3629.251,-482.074;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;186;-3446.94,-269.7765;Inherit;False;Property;_Damage_Desaturation;Damage_Desaturation;1;0;Create;True;0;0;0;False;0;False;0;0;-0.5;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;185;-3148.438,-445.9763;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;196;-2807.776,-151.4734;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;201;-3131.301,143.6211;Inherit;False;ConstantBiasScale;-1;;3;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;203;-3503.516,138.4411;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;204;-3274.515,168.4411;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;197;-3337.776,-35.47342;Inherit;False;Property;_CRTBrightnessFlickerMax;CRT Brightness Flicker Max;23;0;Create;True;0;0;0;False;0;False;0.1;0.02;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;125;-3345.055,-137.4754;Inherit;False;Property;_CRTBrightnessFlickerMin;CRT Brightness Flicker Min;24;0;Create;True;0;0;0;False;0;False;0.1;0.015;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-2236.325,645.8042;Inherit;True;Property;_TextureSampleScanline_Far;Texture Sample Scanline_Far;4;0;Create;True;0;0;0;False;0;False;-1;2fd07d7dd5fe8b046a2eaaf43291b55e;2fd07d7dd5fe8b046a2eaaf43291b55e;True;0;False;white;Auto;False;Object;-1;MipBias;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;51;-2461.805,-1002.528;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;52;-2549.805,-825.1956;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DistanceOpNode;53;-2215.807,-941.8624;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;54;-1859.139,-891.8629;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-2008.139,-644.5283;Inherit;False;Property;_Distance_DotMask_Power;Distance_DotMask_Power;14;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;48;-1650.472,-858.5297;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;49;-1480.472,-858.5297;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;207;-3100.499,1697.25;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;208;-3188.499,1874.582;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DistanceOpNode;209;-2854.501,1757.916;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;211;-2497.833,1807.915;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;213;-2289.167,1841.248;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;214;-2119.167,1841.248;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;210;-2842.467,1936.249;Inherit;False;Property;_Distance_Scanline_Near;Distance_Scanline_Near;10;0;Create;True;0;0;0;False;0;False;0.05;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;212;-2646.833,2055.25;Inherit;False;Property;_Distance_Scanline_Near_Power;Distance_Scanline_Near_Power;15;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1827.197,804.719;Inherit;False;Property;_Scanline_Amount;Scanline_Amount;8;0;Create;True;0;0;0;False;0;False;1;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;99;-1508.597,801.6913;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;101;-1587.793,1123.348;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-1943.597,1132.691;Inherit;False;Property;_Dotmask_ScanlineRemoval;Dotmask_ScanlineRemoval;19;0;Create;True;0;0;0;False;0;False;0;0.124;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;98;-1121.597,620.6912;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;215;-1875.659,567.4342;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;205;-2235.679,951.5669;Inherit;True;Property;_TextureSampleScanline_Near;Texture Sample Scanline_Near;3;0;Create;True;0;0;0;False;0;False;-1;e03c6b81d5a1d1b40879ce22e711aed5;e03c6b81d5a1d1b40879ce22e711aed5;True;0;False;white;Auto;False;Object;-1;MipBias;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;20;-2125.171,-56.0917;Inherit;False;Property;_Distance_Scanline_Far;Distance_Scanline_Far;9;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-2107.537,31.90859;Inherit;False;Property;_Distance_Scanline_Far_Power;Distance_Scanline_Far_Power;13;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;218;1677.291,-592.7473;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;233;1431.291,-720.9138;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;231;926.291,-706.7473;Inherit;False;Property;_MaskVTXRedOnly;MaskVTXRedOnly;27;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;217;1072.291,-1036.747;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;234;1146.291,-1159.914;Inherit;False;Constant;_Float1;Float 1;34;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;216;1653.291,-961.7469;Inherit;False;Constant;_AlbedoColor;AlbedoColor;34;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;236;-3083.761,944.0881;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;200;-3784.776,126.5267;Inherit;False;Property;_CRTBrightnessFlickerTime;CRT Brightness Flicker Time;25;0;Create;True;0;0;0;False;0;False;0;35;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-2203.773,-763.5289;Inherit;False;Property;_Distance_DotMask;Distance_DotMask;11;0;Create;True;0;0;0;False;0;False;0.15;0.15;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;117;1483.097,-400.5663;Inherit;False;Property;_Metallic;Metallic;21;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;118;1490.097,-294.5663;Inherit;False;Property;_Smooth;Smooth;22;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;235;-3345.761,924.0881;Inherit;False;Property;_CRTParameters;CRTParameters;28;0;Create;True;0;0;0;False;0;False;256,256,1,0.59158;256,256,1,0.59158;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;244;1682.639,216.5506;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;242;1679.435,37.10436;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector4Node;241;1396.379,266.7527;Inherit;False;Property;_Dirt_RGBAmount_APower;Dirt_RGBAmount_APower;30;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,0,0,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;240;2370.515,-23.77918;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;238;1851.408,100.1243;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;247;2024.442,69.14873;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;245;2161.16,59.5355;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;237;1354.225,7.133547;Inherit;True;Property;_CRTFX;CRTFX;29;0;Create;True;0;0;0;False;0;False;-1;3b4bc173790b8f34e918d52865e6eae0;3b4bc173790b8f34e918d52865e6eae0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;192;-5615.66,-606.2523;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;248;-5154.237,-597.1586;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;249;-5423.237,-346.1585;Inherit;False;Property;_CRTTiling;CRTTiling;31;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;11;-4769.454,-642.6509;Inherit;True;Property;_MainTex;MainTex;6;0;Create;True;0;0;0;False;0;False;-1;8f939eab460ea6041bc9d72da92c13a6;8f939eab460ea6041bc9d72da92c13a6;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;104;-2690.115,916.6333;Inherit;False;Property;_Scanline_MipBias;Scanline_Mip Bias;20;0;Create;True;0;0;0;False;0;False;0.5;0.5;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;111;1109.069,-724.2061;Float;False;False;-1;2;ASEMaterialInspector;0;4;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ExtraPrePass;0;0;ExtraPrePass;6;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;2;False;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;113;1109.069,-724.2061;Float;False;False;-1;2;ASEMaterialInspector;0;4;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ForwardAdd;0;2;ForwardAdd;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;2;False;0;False;True;4;1;False;;1;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;True;1;LightMode=ForwardAdd;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;114;1109.069,-724.2061;Float;False;False;-1;2;ASEMaterialInspector;0;4;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;Deferred;0;3;Deferred;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Deferred;True;2;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;115;1109.069,-724.2061;Float;False;False;-1;2;ASEMaterialInspector;0;4;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;Meta;0;4;Meta;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;116;1109.069,-724.2061;Float;False;False;-1;2;ASEMaterialInspector;0;4;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ShadowCaster;0;5;ShadowCaster;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;112;1863.069,-596.2061;Float;False;True;-1;2;ASEMaterialInspector;0;4;AgeOfJoy/CRT_01;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ForwardBase;0;1;ForwardBase;18;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;2;False;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;40;Workflow,InvertActionOnDeselection;1;0;Surface;0;0;  Blend;0;0;  Refraction Model;0;0;  Dither Shadows;1;0;Two Sided;1;0;Deferred Pass;1;0;Transmission;0;0;  Transmission Shadow;0.5,False,;0;Translucency;0;0;  Translucency Strength;1,False,;0;  Normal Distortion;0.5,False,;0;  Scattering;2,False,;0;  Direct;0.9,False,;0;  Ambient;0.1,False,;0;  Shadow;0.5,False,;0;Cast Shadows;1;0;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;Ambient Light;1;0;Meta Pass;1;0;Add Pass;1;0;Override Baked GI;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Fwd Specular Highlights Toggle;0;0;Fwd Reflections Toggle;0;0;Disable Batching;0;0;Vertex Position,InvertActionOnDeselection;1;0;0;6;False;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.CommentaryNode;45;-215.0649,-1561.633;Inherit;False;514;215;Rolling - can be moving or stationary - take scanline and scroll it over the scanline bit;0;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;84;-194.3853,-1840.56;Inherit;False;514;215;Color Blobs;0;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;85;-216.3853,-2094.561;Inherit;False;514;215;Color Mismatch;0;;1,1,1,1;0;0
WireConnection;92;0;126;0
WireConnection;93;0;96;0
WireConnection;93;2;49;0
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
WireConnection;23;0;21;0
WireConnection;23;1;22;0
WireConnection;17;0;34;0
WireConnection;17;1;98;0
WireConnection;24;0;23;0
WireConnection;82;0;17;0
WireConnection;82;1;83;0
WireConnection;61;0;131;0
WireConnection;33;0;24;0
WireConnection;64;0;17;0
WireConnection;91;0;126;0
WireConnection;79;0;82;0
WireConnection;79;1;80;0
WireConnection;28;0;64;0
WireConnection;28;1;91;0
WireConnection;28;2;33;0
WireConnection;76;0;79;0
WireConnection;76;1;62;0
WireConnection;58;0;76;0
WireConnection;58;1;28;0
WireConnection;58;2;49;0
WireConnection;62;1;61;0
WireConnection;126;0;185;0
WireConnection;126;1;196;0
WireConnection;131;0;236;0
WireConnection;151;0;180;0
WireConnection;151;1;179;0
WireConnection;152;0;151;0
WireConnection;152;1;158;0
WireConnection;153;0;152;0
WireConnection;154;0;157;0
WireConnection;155;0;153;0
WireConnection;155;1;154;0
WireConnection;156;0;155;0
WireConnection;127;0;235;2
WireConnection;2;0;128;0
WireConnection;128;1;127;0
WireConnection;188;0;11;0
WireConnection;188;1;187;0
WireConnection;184;0;188;0
WireConnection;184;1;156;0
WireConnection;185;0;184;0
WireConnection;185;1;186;0
WireConnection;196;0;125;0
WireConnection;196;1;197;0
WireConnection;196;2;201;0
WireConnection;201;3;204;0
WireConnection;203;0;200;0
WireConnection;204;0;203;0
WireConnection;1;1;2;0
WireConnection;1;2;104;0
WireConnection;53;0;51;0
WireConnection;53;1;52;0
WireConnection;54;0;53;0
WireConnection;54;1;55;0
WireConnection;48;0;54;0
WireConnection;48;1;50;0
WireConnection;49;0;48;0
WireConnection;209;0;207;0
WireConnection;209;1;208;0
WireConnection;211;0;209;0
WireConnection;211;1;210;0
WireConnection;213;0;211;0
WireConnection;213;1;212;0
WireConnection;214;0;213;0
WireConnection;99;0;16;0
WireConnection;99;1;101;0
WireConnection;101;0;100;0
WireConnection;101;2;49;0
WireConnection;98;0;18;0
WireConnection;215;0;205;0
WireConnection;215;1;1;0
WireConnection;215;2;214;0
WireConnection;205;1;2;0
WireConnection;218;0;233;0
WireConnection;218;1;58;0
WireConnection;233;0;234;0
WireConnection;233;1;217;1
WireConnection;233;2;231;0
WireConnection;236;0;235;1
WireConnection;236;1;235;2
WireConnection;244;0;241;1
WireConnection;244;1;241;2
WireConnection;244;2;241;3
WireConnection;242;0;237;1
WireConnection;242;1;237;2
WireConnection;242;2;237;3
WireConnection;240;1;245;0
WireConnection;240;2;241;4
WireConnection;238;0;242;0
WireConnection;238;1;244;0
WireConnection;247;0;238;0
WireConnection;245;0;247;0
WireConnection;245;1;247;1
WireConnection;245;2;247;2
WireConnection;248;0;192;0
WireConnection;248;1;249;0
WireConnection;11;1;248;0
WireConnection;112;0;216;0
WireConnection;112;2;218;0
WireConnection;112;4;117;0
WireConnection;112;5;240;0
ASEEND*/
//CHKSM=0B176AFEC08887FE9694FB423890C7811B3BA38F
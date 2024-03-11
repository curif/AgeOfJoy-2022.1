// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AgeOfJoy/PhoneGlass"
{
	Properties
	{
		_Opacity("Opacity", Range( 0 , 1)) = 0.2173913
		_MRAmount("MRAmount", Range( 0 , 0)) = 0
		_GlassColor("GlassColor", Color) = (0.9150943,0.8989999,0.4287706,0)
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha exclude_path:deferred 
		struct Input
		{
			half filler;
		};

		uniform float4 _GlassColor;
		uniform float _Metallic;
		uniform float _Smoothness;
		uniform float _Opacity;
		uniform float _MRAmount;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _GlassColor.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = ( _Opacity - _MRAmount );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.RangedFloatNode;3;-515,467.5001;Inherit;False;Property;_Opacity;Opacity;0;0;Create;True;0;0;0;False;0;False;0.2173913;0.4960435;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-510.5935,604.5898;Inherit;False;Property;_MRAmount;MRAmount;1;0;Create;True;0;0;0;False;0;False;0;0.4960435;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-443,-35.50006;Inherit;False;Property;_GlassColor;GlassColor;2;0;Create;True;0;0;0;False;0;False;0.9150943,0.8989999,0.4287706,0;0.9150943,0.8989999,0.4287705,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;4;-511.5935,158.0898;Inherit;False;Property;_Metallic;Metallic;3;0;Create;True;0;0;0;False;0;False;0;0.7951739;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-506.5935,247.0898;Inherit;False;Property;_Smoothness;Smoothness;4;0;Create;True;0;0;0;False;0;False;0;0.6086956;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;7;-137.5935,398.5898;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;1;195,25;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;AgeOfJoy/PhoneGlass;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Front;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;0;3;0
WireConnection;7;1;6;0
WireConnection;1;0;2;0
WireConnection;1;3;4;0
WireConnection;1;4;5;0
WireConnection;1;9;7;0
ASEEND*/
//CHKSM=2EC08650735A29D86F7475739089FACA3121B31B
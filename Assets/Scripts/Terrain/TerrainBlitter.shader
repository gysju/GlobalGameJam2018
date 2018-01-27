// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TerrainBlitter"
{
	Properties
	{
		[HideInInspector]_Normal_Alpha("Normal_Alpha", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal_Alpha;
		uniform float4 _Normal_Alpha_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Normal = float3(0,0,1);
			float2 uv_Normal_Alpha = i.uv_texcoord * _Normal_Alpha_ST.xy + _Normal_Alpha_ST.zw;
			float4 tex2DNode2 = tex2D( _Normal_Alpha, uv_Normal_Alpha );
			float3 temp_cast_0 = (tex2DNode2.a).xxx;
			o.Emission = temp_cast_0;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=13801
650;257;1316;656;1070.656;301.0384;1.417173;True;True
Node;AmplifyShaderEditor.SamplerNode;1;-731,-42;Float;True;Property;_MainTex;_MainTex;0;1;[HideInInspector];None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;5;-241.6091,107.1075;Float;False;FLOAT3;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.SamplerNode;2;-734,187;Float;True;Property;_Normal_Alpha;Normal_Alpha;1;1;[HideInInspector];None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TerrainBlitter;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Masked;0.5;True;True;0;False;TransparentCutout;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;2;1
WireConnection;5;1;2;2
WireConnection;5;2;2;3
WireConnection;0;15;2;4
ASEEND*/
//CHKSM=251A4CC44C688418FDF07A9A443B1BD25D8B17E7
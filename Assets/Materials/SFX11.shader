// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SFX/SFX11"
{
	Properties
	{
		_Contrast("Contrast", Range( 0 , 2)) = 0
		_Percomponentvalues("Per component values", Float) = 16
		_Center("Center", Range( 0 , 1)) = 0
		_Desaturate("Desaturate", Range( 0 , 1)) = 0
		_Scale("Scale", Range( 1 , 20)) = 0
		_RGBmask("RGBmask", 2D) = "white" {}
		_RGBMask("RGBMask", Range( 0 , 2)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float4 screenPos;
		};

		uniform sampler2D _GrabTexture;
		uniform float _Scale;
		uniform float _Desaturate;
		uniform float _Center;
		uniform float _Contrast;
		uniform float _Percomponentvalues;
		uniform sampler2D _RGBmask;
		uniform float _RGBMask;

		inline fixed4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return fixed4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 appendResult22 = (float2(_ScreenParams.x , _ScreenParams.y));
			float2 temp_output_30_0 = ( ( float2( 1,1 ) / appendResult22 ) * _Scale );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPos16 = ase_screenPos;
			#if UNITY_UV_STARTS_AT_TOP
			float scale16 = -1.0;
			#else
			float scale16 = 1.0;
			#endif
			float halfPosW16 = ase_screenPos16.w * 0.5;
			ase_screenPos16.y = ( ase_screenPos16.y - halfPosW16 ) * _ProjectionParams.x* scale16 + halfPosW16;
			ase_screenPos16.xyzw /= ase_screenPos16.w;
			float2 temp_output_24_0 = (ase_screenPos16).xy;
			float2 temp_output_34_0 = fmod( temp_output_24_0 , temp_output_30_0 );
			float4 screenColor20 = tex2D( _GrabTexture, ( ( ( temp_output_30_0 * float2( 0.5,0.5 ) ) + temp_output_24_0 ) - temp_output_34_0 ) );
			float3 desaturateVar69 = lerp( screenColor20.rgb,dot(screenColor20.rgb,float3(0.299,0.587,0.114)).xxx,_Desaturate);
			float3 temp_cast_1 = (_Center).xxx;
			float2 temp_output_46_0 = step( fmod( temp_output_24_0 , ( temp_output_30_0 * float2( 2,2 ) ) ) , temp_output_34_0 );
			float temp_output_48_0 = (temp_output_46_0).x;
			float temp_output_49_0 = (temp_output_46_0).y;
			float4 lerpResult76 = lerp( float4(1,1,1,0) , tex2D( _RGBmask, ( temp_output_24_0 * ( appendResult22 / _Scale ) ) ) , _RGBMask);
			o.Emission = ( ( floor( ( float4( ( max( ( ( ( desaturateVar69 - temp_cast_1 ) * _Contrast ) + _Center ) , float3( 0,0,0 ) ) * _Percomponentvalues ) , 0.0 ) + ( float4(0.4926471,0.4926471,0.4926471,0) * ( ( temp_output_48_0 - temp_output_49_0 ) * ( temp_output_49_0 - temp_output_48_0 ) ) ) ) ) * ( 1.0 / _Percomponentvalues ) ) * lerpResult76 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14001
1927;-208;2546;1374;1383.282;536.2027;1;True;False
Node;AmplifyShaderEditor.ScreenParams;21;-2273.801,-288.0964;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;22;-2044.986,-258.7726;Float;False;FLOAT2;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-2411.126,-97.4907;Float;False;Property;_Scale;Scale;2;0;0;1;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;33;-1882.861,-282.0217;Float;False;2;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GrabScreenPosition;16;-2344.814,107.2737;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-1671.802,-200.6178;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;24;-2046.901,113.9037;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-1427.755,-223.0857;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-1260.305,-139.1417;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FmodOpNode;34;-1305.063,107.2335;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;35;-1071.26,-18.59765;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;70;-1257.341,-277.9226;Float;False;Property;_Desaturate;Desaturate;1;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;20;-891.0421,-28.8232;Float;False;Global;_GrabScreen0;Grab Screen 0;-1;0;Object;-1;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-1487.035,395.5572;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DesaturateOpNode;69;-498.3413,-255.9226;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FmodOpNode;44;-1254.335,299.5572;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-1230.861,-394.3043;Float;False;Property;_Center;Center;1;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;46;-1008.835,225.1572;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-1069.861,-581.3044;Float;False;Property;_Contrast;Contrast;1;0;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;64;-922.8611,-417.3043;Float;False;2;0;FLOAT3;0,0,0,0;False;1;FLOAT;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;48;-830.8553,187.0691;Float;False;True;False;False;False;1;0;FLOAT2;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-713.8611,-556.3044;Float;False;2;2;0;FLOAT3;0,0,0,0;False;1;FLOAT;0.0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;49;-826.8553,284.0691;Float;False;False;True;False;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;51;-537.8553,291.0691;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;50;-537.8553,139.0691;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;66;-534.8611,-547.3044;Float;False;2;2;0;FLOAT3;0,0,0,0;False;1;FLOAT;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-644.0408,-268.8159;Float;False;Property;_Percomponentvalues;Per component values;1;0;16;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;54;-543.8553,-142.9309;Float;False;Constant;_Color0;Color 0;1;0;0.4926471,0.4926471,0.4926471,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;67;-375.8611,-609.3044;Float;False;2;0;FLOAT3;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-328.8553,217.0691;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;73;-1984.026,639.684;Float;False;2;0;FLOAT2;0.0;False;1;FLOAT;0.0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-197.8553,-14.9309;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-205.0408,-518.8162;Float;False;2;2;0;FLOAT3;0,0,0,0;False;1;FLOAT;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1826.026,622.684;Float;False;2;2;0;FLOAT2;0.0,0;False;1;FLOAT2;0.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;53;-69.85535,-309.9309;Float;False;2;2;0;FLOAT3;0,0,0,0;False;1;COLOR;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;41;-337.0408,-275.8159;Float;False;2;0;FLOAT;1.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;78;-64.28223,533.7973;Float;False;Property;_RGBMask;RGBMask;6;0;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;77;-80.28223,88.7973;Float;False;Constant;_Color1;Color 1;6;0;1,1,1,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FloorOpNode;39;56.9648,-389.4428;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;71;-785.0632,586.2516;Float;True;Property;_RGBmask;RGBmask;5;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;76;129.7178,277.7973;Float;False;3;0;COLOR;0.0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;256.9648,-254.4428;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;208.7177,33.7973;Float;False;2;2;0;COLOR;0.0;False;1;COLOR;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;562,-120;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;SFX/SFX11;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Translucent;0.5;True;True;0;False;Opaque;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;22;0;21;1
WireConnection;22;1;21;2
WireConnection;33;1;22;0
WireConnection;30;0;33;0
WireConnection;30;1;25;0
WireConnection;24;0;16;0
WireConnection;57;0;30;0
WireConnection;59;0;57;0
WireConnection;59;1;24;0
WireConnection;34;0;24;0
WireConnection;34;1;30;0
WireConnection;35;0;59;0
WireConnection;35;1;34;0
WireConnection;20;0;35;0
WireConnection;45;0;30;0
WireConnection;69;0;20;0
WireConnection;69;1;70;0
WireConnection;44;0;24;0
WireConnection;44;1;45;0
WireConnection;46;0;44;0
WireConnection;46;1;34;0
WireConnection;64;0;69;0
WireConnection;64;1;63;0
WireConnection;48;0;46;0
WireConnection;65;0;64;0
WireConnection;65;1;61;0
WireConnection;49;0;46;0
WireConnection;51;0;49;0
WireConnection;51;1;48;0
WireConnection;50;0;48;0
WireConnection;50;1;49;0
WireConnection;66;0;65;0
WireConnection;66;1;63;0
WireConnection;67;0;66;0
WireConnection;52;0;50;0
WireConnection;52;1;51;0
WireConnection;73;0;22;0
WireConnection;73;1;25;0
WireConnection;55;0;54;0
WireConnection;55;1;52;0
WireConnection;36;0;67;0
WireConnection;36;1;37;0
WireConnection;74;0;24;0
WireConnection;74;1;73;0
WireConnection;53;0;36;0
WireConnection;53;1;55;0
WireConnection;41;1;37;0
WireConnection;39;0;53;0
WireConnection;71;1;74;0
WireConnection;76;0;77;0
WireConnection;76;1;71;0
WireConnection;76;2;78;0
WireConnection;38;0;39;0
WireConnection;38;1;41;0
WireConnection;75;0;38;0
WireConnection;75;1;76;0
WireConnection;0;2;75;0
ASEEND*/
//CHKSM=44333FDD376246B7C2E52D4FAE05B5A2FE31A10A
// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Button"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.05
		[Header(Refraction)]
		_ChromaticAberration("Chromatic Aberration", Range( 0 , 0.3)) = 0.1
		_Outertranslucencymin("Outer translucency min", Range( 0 , 1)) = 0
		_Outertranslucencymax("Outer translucency max", Range( 0 , 1)) = 0
		_Innertranslucencymin("Inner translucency min", Range( 0 , 1)) = 0
		_Innertranslucencymax("Inner translucency max", Range( 0 , 1)) = 0
		_Softness("Softness", Range( 0 , 1)) = 0
		_IOR("IOR", Range( 0.5 , 1.5)) = 1
		[HDR]_Rimlight("Rim light", Color) = (0,0,0,0)
		_Emissiveboost("Emissive boost", Range( 0 , 10)) = 0
		_Color("Color", Color) = (0,0,0,0)
		_emissiveramp("emissive ramp", Range( 0.25 , 10)) = 0
		_NormalFactor("NormalFactor", Float) = 0
		_CutThreshold("CutThreshold", Float) = 0
		_Size("Size", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ }
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile _ALPHAPREMULTIPLY_ON
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			INTERNAL_DATA
			float3 worldNormal;
			float4 screenPos;
		};

		uniform float _Size;
		uniform float _NormalFactor;
		uniform float _CutThreshold;
		uniform float4 _Color;
		uniform float4 _Rimlight;
		uniform float _emissiveramp;
		uniform float _Emissiveboost;
		uniform float _Softness;
		uniform float _Outertranslucencymin;
		uniform float _Outertranslucencymax;
		uniform float _Innertranslucencymin;
		uniform float _Innertranslucencymax;
		uniform sampler2D _GrabTexture;
		uniform float _ChromaticAberration;
		uniform float _IOR;
		uniform float _Cutoff = 0.05;

		inline float4 Refraction( Input i, SurfaceOutputStandardSpecular o, float indexOfRefraction, float chomaticAberration ) {
			float3 worldNormal = o.Normal;
			float4 screenPos = i.screenPos;
			#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
			#else
				float scale = 1.0;
			#endif
			float halfPosW = screenPos.w * 0.5;
			screenPos.y = ( screenPos.y - halfPosW ) * _ProjectionParams.x * scale + halfPosW;
			#if SHADER_API_D3D9 || SHADER_API_D3D11
				screenPos.w += 0.00000000001;
			#endif
			float2 projScreenPos = ( screenPos / screenPos.w ).xy;
			float3 worldViewDir = normalize( UnityWorldSpaceViewDir( i.worldPos ) );
			float3 refractionOffset = ( ( ( ( indexOfRefraction - 1.0 ) * mul( UNITY_MATRIX_V, float4( worldNormal, 0.0 ) ) ) * ( 1.0 / ( screenPos.z + 1.0 ) ) ) * ( 1.0 - dot( worldNormal, worldViewDir ) ) );
			float2 cameraRefraction = float2( refractionOffset.x, -( refractionOffset.y * _ProjectionParams.x ) );
			float4 redAlpha = tex2D( _GrabTexture, ( projScreenPos + cameraRefraction ) );
			float green = tex2D( _GrabTexture, ( projScreenPos + ( cameraRefraction * ( 1.0 - chomaticAberration ) ) ) ).g;
			float blue = tex2D( _GrabTexture, ( projScreenPos + ( cameraRefraction * ( 1.0 + chomaticAberration ) ) ) ).b;
			return float4( redAlpha.r, green, blue, redAlpha.a );
		}

		void RefractionF( Input i, SurfaceOutputStandardSpecular o, inout fixed4 color )
		{
			#ifdef UNITY_PASS_FORWARDBASE
			color.rgb = color.rgb + Refraction( i, o, _IOR, _ChromaticAberration ) * ( 1 - color.a );
			color.a = 1;
			#endif
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			o.Normal = float3(0,0,1);
			float2 _Vector1 = float2(-0.2,0.5);
			float lerpResult75 = lerp( (0.0 + (i.uv_texcoord.x - 0.0) * (0.5 - 0.0) / (0.2 - 0.0)) , 0.5 , step( 0.2 , i.uv_texcoord.x ));
			float lerpResult80 = lerp( (-0.2 + (i.uv_texcoord.x - 0.5) * (1.0 - -0.2) / (1.0 - 0.5)) , lerpResult75 , step( i.uv_texcoord.x , 0.8 ));
			float2 appendResult81 = (float2(lerpResult80 , i.uv_texcoord.y));
			float2 appendResult53 = (float2(( appendResult81.x + 0.01 ) , appendResult81.y));
			float smoothstepResult97 = smoothstep( _Vector1.x , _Vector1.y , ( distance( appendResult53 , float2( 0.5,0 ) ) - _Size ));
			float temp_output_36_0 = distance( appendResult81 , float2( 0.5,0.5 ) );
			float temp_output_37_0 = ( temp_output_36_0 - _Size );
			float smoothstepResult96 = smoothstep( _Vector1.x , _Vector1.y , temp_output_37_0);
			float2 appendResult54 = (float2(appendResult81.x , ( appendResult81.y + 0.01 )));
			float smoothstepResult98 = smoothstep( _Vector1.x , _Vector1.y , ( distance( appendResult54 , float2( 0.5,0 ) ) - _Size ));
			float3 appendResult61 = (float3(( ( ( smoothstepResult97 - smoothstepResult96 ) * _NormalFactor ) + 0.5 ) , ( ( ( smoothstepResult98 - smoothstepResult96 ) * _NormalFactor ) + 0.5 ) , 1.0));
			float3 normalizeResult60 = normalize( appendResult61 );
			float3 appendResult105 = (float3(( sin( normalizeResult60.x ) * ( 0.5 * UNITY_PI ) ) , ( sin( normalizeResult60.y ) * ( 0.5 * UNITY_PI ) ) , normalizeResult60.z));
			float4 appendResult85 = (float4(appendResult105 , step( max( temp_output_37_0 , 0.0 ) , _CutThreshold )));
			float4 Normal_Alpha84 = appendResult85;
			float4 _Vector0 = float4(0,1,-1,1);
			float4 temp_cast_0 = (_Vector0.x).xxxx;
			float4 temp_cast_1 = (_Vector0.y).xxxx;
			float4 temp_cast_2 = (_Vector0.z).xxxx;
			float4 temp_cast_3 = (_Vector0.w).xxxx;
			float4 temp_output_14_0 = (temp_cast_2 + (Normal_Alpha84 - temp_cast_0) * (temp_cast_3 - temp_cast_2) / (temp_cast_1 - temp_cast_0));
			o.Normal = temp_output_14_0.xyz;
			float4 appendResult35 = (float4(_Color.r , _Color.g , _Color.b , ( 1.0 - temp_output_36_0 )));
			float4 Color_Distance39 = appendResult35;
			float4 temp_output_86_0 = Color_Distance39;
			o.Albedo = temp_output_86_0.xyz;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float4 temp_cast_6 = (_Vector0.x).xxxx;
			float4 temp_cast_7 = (_Vector0.y).xxxx;
			float4 temp_cast_8 = (_Vector0.z).xxxx;
			float4 temp_cast_9 = (_Vector0.w).xxxx;
			float fresnelNDotV10 = dot( WorldNormalVector( i , temp_output_14_0.xyz ), ase_worldViewDir );
			float fresnelNode10 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNDotV10, 2.0 ) );
			o.Emission = ( ( fresnelNode10 * _Rimlight ) + ( Color_Distance39 * pow( Color_Distance39.w , _emissiveramp ) * _Emissiveboost ) ).rgb;
			o.Smoothness = _Softness;
			float smoothstepResult9 = smoothstep( _Outertranslucencymin , _Outertranslucencymax , Color_Distance39.w);
			float smoothstepResult31 = smoothstep( _Innertranslucencymin , _Innertranslucencymax , Color_Distance39.w);
			o.Alpha = max( ( 1.0 - smoothstepResult9 ) , smoothstepResult31 );
			clip( Normal_Alpha84.w - _Cutoff );
			o.Normal = o.Normal + 0.00001 * i.screenPos * i.worldPos;
		}

		ENDCG
		CGPROGRAM
		#pragma only_renderers d3d9 d3d11 d3d11_9x 
		#pragma surface surf StandardSpecular keepalpha finalcolor:RefractionF fullforwardshadows exclude_path:deferred 

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
			# include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 screenPos : TEXCOORD7;
				float4 tSpace0 : TEXCOORD1;
				float4 tSpace1 : TEXCOORD2;
				float4 tSpace2 : TEXCOORD3;
				float4 texcoords01 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				fixed3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.texcoords01 = float4( v.texcoord.xy, v.texcoord1.xy );
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord.xy = IN.texcoords01.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
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
Version=13801
280;487;1906;734;2386.97;285.9284;2.809376;True;False
Node;AmplifyShaderEditor.TexCoordVertexDataNode;76;-5057.327,-749.9965;Float;False;0;2;0;5;FLOAT2;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;77;-4729.925,-337.0761;Float;False;Constant;_Float0;Float 0;17;0;0.5;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.StepOpNode;72;-4761.848,-540.9197;Float;False;2;0;FLOAT;0.2;False;1;FLOAT;0.2;False;1;FLOAT
Node;AmplifyShaderEditor.TFHCRemapNode;73;-4773.229,-911.2713;Float;False;5;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.2;False;3;FLOAT;0.0;False;4;FLOAT;0.5;False;1;FLOAT
Node;AmplifyShaderEditor.TFHCRemapNode;74;-4779.597,-734.9335;Float;False;5;0;FLOAT;0.0;False;1;FLOAT;0.5;False;2;FLOAT;1.0;False;3;FLOAT;-0.2;False;4;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.StepOpNode;78;-4745.764,-121.0721;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.8;False;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;75;-4501.692,-772.4619;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;80;-4226.589,-597.5949;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;81;-3937.2,-586.5398;Float;False;FLOAT2;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.BreakToComponentsNode;82;-3967.837,-377.6662;Float;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;40;-3591.775,-306.3084;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.01;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;41;-3590.742,-204.2617;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.01;False;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;54;-3467.854,-203.5108;Float;False;FLOAT2;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.DynamicAppendNode;53;-3471.016,-310.0764;Float;False;FLOAT2;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.DistanceOpNode;36;-3308.677,-612.7645;Float;False;2;0;FLOAT2;0,0,0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT
Node;AmplifyShaderEditor.DistanceOpNode;52;-3341.496,-210.2826;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0;False;1;FLOAT
Node;AmplifyShaderEditor.DistanceOpNode;51;-3345.145,-312.4241;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;83;-3556.9,-513.2327;Float;False;Property;_Size;Size;17;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.Vector2Node;95;-3232.31,-24.57996;Float;False;Constant;_Vector1;Vector 1;16;0;-0.2,0.5;0;3;FLOAT2;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleSubtractOpNode;37;-3142.632,-612.7646;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.5;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleSubtractOpNode;46;-3199.566,-212.0702;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.5;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleSubtractOpNode;44;-3201.173,-311.7026;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.5;False;1;FLOAT
Node;AmplifyShaderEditor.SmoothstepOpNode;96;-3040.31,-469.58;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SmoothstepOpNode;98;-3049.31,-219.58;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SmoothstepOpNode;97;-3048.31,-337.58;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleSubtractOpNode;47;-2883.697,-207.3885;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleSubtractOpNode;42;-2885.796,-316.221;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;57;-3024.547,-75.33345;Float;False;Property;_NormalFactor;NormalFactor;15;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2735.187,-305.2935;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;-2732.043,-201.5368;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;65;-2572.404,-197.576;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.5;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;64;-2573.994,-296.1455;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.5;False;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;61;-2422.949,-273.1534;Float;False;FLOAT3;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.NormalizeNode;60;-2255.655,-274.2085;Float;False;1;0;FLOAT3;0,0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.BreakToComponentsNode;100;-2286.256,-174.6574;Float;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SinOpNode;103;-2029.256,-116.6574;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.PiNode;102;-2107.256,61.34259;Float;False;1;0;FLOAT;0.5;False;1;FLOAT
Node;AmplifyShaderEditor.SinOpNode;99;-2027.256,-179.6574;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;-1862.256,-176.6574;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;-1860.256,-87.65741;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;66;-2749.27,-558.8514;Float;False;Property;_CutThreshold;CutThreshold;16;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMaxOpNode;38;-2992.987,-606.6149;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.StepOpNode;67;-2560.919,-518.8476;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;105;-1709.256,-155.6574;Float;False;FLOAT3;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.OneMinusNode;108;-3063.569,-694.2797;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;34;-3092.482,-849.1587;Float;False;Property;_Color;Color;14;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;35;-2806.443,-739.8604;Float;False;FLOAT4;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT4
Node;AmplifyShaderEditor.DynamicAppendNode;85;-2089.743,-288.882;Float;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT4
Node;AmplifyShaderEditor.GetLocalVarNode;86;-1364.613,-490.7264;Float;False;39;0;1;FLOAT4
Node;AmplifyShaderEditor.RegisterLocalVarNode;84;-1920.806,-282.8816;Float;False;Normal_Alpha;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.RegisterLocalVarNode;39;-2645.15,-728.5834;Float;False;Color_Distance;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.Vector4Node;15;-1622.942,341.2868;Float;False;Constant;_Vector0;Vector 0;9;0;0,1,-1,1;0;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;88;-1544.667,38.23089;Float;False;84;0;1;FLOAT4
Node;AmplifyShaderEditor.BreakToComponentsNode;87;-1386.402,-418.0392;Float;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;27;-1201.847,97.60309;Float;False;Property;_emissiveramp;emissive ramp;14;0;0;0.25;10;0;1;FLOAT
Node;AmplifyShaderEditor.TFHCRemapNode;14;-1238.942,252.2868;Float;False;5;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;1,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;1,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.RangedFloatNode;8;-1143.686,452.8109;Float;False;Property;_Outertranslucencymax;Outer translucency max;5;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;7;-959.6863,354.8109;Float;False;Property;_Outertranslucencymin;Outer translucency min;4;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;20;-712.8474,-265.3969;Float;False;Property;_Rimlight;Rim light;11;1;[HDR];0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;29;-897.5141,615.2697;Float;False;Property;_Innertranslucencymin;Inner translucency min;6;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;30;-1080.514,713.2697;Float;False;Property;_Innertranslucencymax;Inner translucency max;7;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.FresnelNode;10;-727.9424,-444.7132;Float;False;Tangent;4;0;FLOAT3;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;2.0;False;1;FLOAT
Node;AmplifyShaderEditor.SmoothstepOpNode;9;-632.6863,345.8109;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;24;-696.8474,55.60309;Float;False;Property;_Emissiveboost;Emissive boost;13;0;0;0;10;0;1;FLOAT
Node;AmplifyShaderEditor.PowerNode;26;-871.8474,-31.39691;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SmoothstepOpNode;31;-570.5141,606.2697;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-346.8474,-17.39691;Float;False;3;3;0;FLOAT4;0,0,0,0;False;1;FLOAT;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.OneMinusNode;22;-364.8474,312.6031;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-401.8474,-281.3969;Float;False;2;2;0;FLOAT;0.0;False;1;COLOR;0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;6;-187.686,740.8109;Float;False;Property;_IOR;IOR;9;0;1;0.5;1.5;0;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;4;96.58533,-374.7168;Float;False;Property;_Color0;Color 0;3;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMaxOpNode;28;-103.3097,367.7737;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;25;-137.2134,-6.004021;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.BreakToComponentsNode;89;-1574.266,117.9198;Float;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;5;-226.6863,131.8109;Float;False;Property;_Softness;Softness;8;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;238,-82;Float;False;True;2;Float;ASEMaterialInspector;0;0;StandardSpecular;Custom/Button;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Custom;0.05;True;True;0;False;TransparentCutout;Transparent;ForwardOnly;True;True;False;False;False;False;True;False;False;False;False;False;False;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;-1;-1;0;-1;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;72;1;76;1
WireConnection;73;0;76;1
WireConnection;74;0;76;1
WireConnection;78;0;76;1
WireConnection;75;0;73;0
WireConnection;75;1;77;0
WireConnection;75;2;72;0
WireConnection;80;0;74;0
WireConnection;80;1;75;0
WireConnection;80;2;78;0
WireConnection;81;0;80;0
WireConnection;81;1;76;2
WireConnection;82;0;81;0
WireConnection;40;0;82;0
WireConnection;41;0;82;1
WireConnection;54;0;82;0
WireConnection;54;1;41;0
WireConnection;53;0;40;0
WireConnection;53;1;82;1
WireConnection;36;0;81;0
WireConnection;52;0;54;0
WireConnection;51;0;53;0
WireConnection;37;0;36;0
WireConnection;37;1;83;0
WireConnection;46;0;52;0
WireConnection;46;1;83;0
WireConnection;44;0;51;0
WireConnection;44;1;83;0
WireConnection;96;0;37;0
WireConnection;96;1;95;1
WireConnection;96;2;95;2
WireConnection;98;0;46;0
WireConnection;98;1;95;1
WireConnection;98;2;95;2
WireConnection;97;0;44;0
WireConnection;97;1;95;1
WireConnection;97;2;95;2
WireConnection;47;0;98;0
WireConnection;47;1;96;0
WireConnection;42;0;97;0
WireConnection;42;1;96;0
WireConnection;55;0;42;0
WireConnection;55;1;57;0
WireConnection;56;0;47;0
WireConnection;56;1;57;0
WireConnection;65;0;56;0
WireConnection;64;0;55;0
WireConnection;61;0;64;0
WireConnection;61;1;65;0
WireConnection;60;0;61;0
WireConnection;100;0;60;0
WireConnection;103;0;100;1
WireConnection;99;0;100;0
WireConnection;101;0;99;0
WireConnection;101;1;102;0
WireConnection;104;0;103;0
WireConnection;104;1;102;0
WireConnection;38;0;37;0
WireConnection;67;0;38;0
WireConnection;67;1;66;0
WireConnection;105;0;101;0
WireConnection;105;1;104;0
WireConnection;105;2;100;2
WireConnection;108;0;36;0
WireConnection;35;0;34;1
WireConnection;35;1;34;2
WireConnection;35;2;34;3
WireConnection;35;3;108;0
WireConnection;85;0;105;0
WireConnection;85;3;67;0
WireConnection;84;0;85;0
WireConnection;39;0;35;0
WireConnection;87;0;86;0
WireConnection;14;0;88;0
WireConnection;14;1;15;1
WireConnection;14;2;15;2
WireConnection;14;3;15;3
WireConnection;14;4;15;4
WireConnection;10;0;14;0
WireConnection;9;0;87;3
WireConnection;9;1;7;0
WireConnection;9;2;8;0
WireConnection;26;0;87;3
WireConnection;26;1;27;0
WireConnection;31;0;87;3
WireConnection;31;1;29;0
WireConnection;31;2;30;0
WireConnection;23;0;86;0
WireConnection;23;1;26;0
WireConnection;23;2;24;0
WireConnection;22;0;9;0
WireConnection;18;0;10;0
WireConnection;18;1;20;0
WireConnection;28;0;22;0
WireConnection;28;1;31;0
WireConnection;25;0;18;0
WireConnection;25;1;23;0
WireConnection;89;0;88;0
WireConnection;0;0;86;0
WireConnection;0;1;14;0
WireConnection;0;2;25;0
WireConnection;0;4;5;0
WireConnection;0;8;6;0
WireConnection;0;9;28;0
WireConnection;0;10;89;3
ASEEND*/
//CHKSM=1822C0C768B350C219EAE5C11C278E3DC6A895E4
// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Terrain"
{
	Properties
	{
		[Header(Refraction)]
		_ChromaticAberration("Chromatic Aberration", Range( 0 , 0.3)) = 0.1
		_Normal_Alpha("Normal_Alpha", 2D) = "white" {}
		_Color_Distance("Color_Distance", 2D) = "white" {}
		_Outertranslucencymin("Outer translucency min", Range( 0 , 1)) = 0
		_Outertranslucencymax("Outer translucency max", Range( 0 , 1)) = 0
		_Innertranslucencymin("Inner translucency min", Range( 0 , 1)) = 0
		_Innertranslucencymax("Inner translucency max", Range( 0 , 1)) = 0
		_Softness("Softness", Range( 0 , 1)) = 0
		_IOR("IOR", Range( 0.5 , 1.5)) = 1
		_Cutoff( "Mask Clip Value", Float ) = 0.05
		[HDR]_Rimlight("Rim light", Color) = (0,0,0,0)
		_Emissiveboost("Emissive boost", Range( 0 , 10)) = 0
		_emissiveramp("emissive ramp", Range( 0.25 , 10)) = 0
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

		uniform sampler2D _Normal_Alpha;
		uniform float4 _Normal_Alpha_ST;
		uniform sampler2D _Color_Distance;
		uniform float4 _Color_Distance_ST;
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
			float2 uv_Normal_Alpha = i.uv_texcoord * _Normal_Alpha_ST.xy + _Normal_Alpha_ST.zw;
			float4 tex2DNode2 = tex2D( _Normal_Alpha, uv_Normal_Alpha );
			float4 _Vector0 = float4(0,1,-1,1);
			float4 temp_cast_0 = (_Vector0.x).xxxx;
			float4 temp_cast_1 = (_Vector0.y).xxxx;
			float4 temp_cast_2 = (_Vector0.z).xxxx;
			float4 temp_cast_3 = (_Vector0.w).xxxx;
			float4 temp_output_14_0 = (temp_cast_2 + (tex2DNode2 - temp_cast_0) * (temp_cast_3 - temp_cast_2) / (temp_cast_1 - temp_cast_0));
			o.Normal = temp_output_14_0.rgb;
			float2 uv_Color_Distance = i.uv_texcoord * _Color_Distance_ST.xy + _Color_Distance_ST.zw;
			float4 tex2DNode1 = tex2D( _Color_Distance, uv_Color_Distance );
			o.Albedo = tex2DNode1.rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float4 temp_cast_6 = (_Vector0.x).xxxx;
			float4 temp_cast_7 = (_Vector0.y).xxxx;
			float4 temp_cast_8 = (_Vector0.z).xxxx;
			float4 temp_cast_9 = (_Vector0.w).xxxx;
			float fresnelNDotV10 = dot( WorldNormalVector( i , temp_output_14_0.rgb ), ase_worldViewDir );
			float fresnelNode10 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNDotV10, 2.0 ) );
			o.Emission = ( ( fresnelNode10 * _Rimlight ) + ( tex2DNode1 * pow( tex2DNode1.a , _emissiveramp ) * _Emissiveboost ) ).rgb;
			o.Smoothness = _Softness;
			float smoothstepResult9 = smoothstep( _Outertranslucencymin , _Outertranslucencymax , tex2DNode1.a);
			float smoothstepResult31 = smoothstep( _Innertranslucencymin , _Innertranslucencymax , tex2DNode1.a);
			o.Alpha = max( ( 1.0 - smoothstepResult9 ) , smoothstepResult31 );
			clip( tex2DNode2.a - _Cutoff );
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
7;29;1906;1124;1399.627;92.47052;1;True;True
Node;AmplifyShaderEditor.SamplerNode;2;-1728,36;Float;True;Property;_Normal_Alpha;Normal_Alpha;1;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector4Node;15;-1622.942,341.2868;Float;False;Constant;_Vector0;Vector 0;9;0;0,1,-1,1;0;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;27;-1201.847,97.60309;Float;False;Property;_emissiveramp;emissive ramp;14;0;0;0.25;10;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;1;-1392.773,-203.0756;Float;True;Property;_Color_Distance;Color_Distance;2;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.TFHCRemapNode;14;-1238.942,252.2868;Float;False;5;0;COLOR;0.0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;1,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;7;-959.6863,354.8109;Float;False;Property;_Outertranslucencymin;Outer translucency min;4;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;8;-1143.686,452.8109;Float;False;Property;_Outertranslucencymax;Outer translucency max;5;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.PowerNode;26;-871.8474,-31.39691;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;24;-696.8474,55.60309;Float;False;Property;_Emissiveboost;Emissive boost;13;0;0;0;10;0;1;FLOAT
Node;AmplifyShaderEditor.FresnelNode;10;-727.9424,-444.7132;Float;False;Tangent;4;0;FLOAT3;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;2.0;False;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;20;-712.8474,-265.3969;Float;False;Property;_Rimlight;Rim light;11;1;[HDR];0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;30;-1080.514,713.2697;Float;False;Property;_Innertranslucencymax;Inner translucency max;7;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;29;-897.5141,615.2697;Float;False;Property;_Innertranslucencymin;Inner translucency min;6;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SmoothstepOpNode;9;-632.6863,345.8109;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-346.8474,-17.39691;Float;False;3;3;0;COLOR;0.0;False;1;FLOAT;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SmoothstepOpNode;31;-570.5141,606.2697;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-401.8474,-281.3969;Float;False;2;2;0;FLOAT;0.0;False;1;COLOR;0.0;False;1;COLOR
Node;AmplifyShaderEditor.OneMinusNode;22;-364.8474,312.6031;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;5;-226.6863,131.8109;Float;False;Property;_Softness;Softness;8;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;6;-187.686,740.8109;Float;False;Property;_IOR;IOR;9;0;1;0.5;1.5;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;21;-1278.847,-404.3969;Float;False;Property;_Fresnelpower;Fresnel power;12;0;3;0;10;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMaxOpNode;28;-80.84741,400.6031;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;4;96.58533,-374.7168;Float;False;Property;_Color0;Color 0;3;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;25;-153.8474,-13.39691;Float;False;2;2;0;COLOR;0.0;False;1;COLOR;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;238,-82;Float;False;True;2;Float;ASEMaterialInspector;0;0;StandardSpecular;Custom/Terrain;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Custom;0.05;True;True;0;False;TransparentCutout;Transparent;ForwardOnly;True;True;False;False;False;False;True;False;False;False;False;False;False;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;10;1;0;-1;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;14;0;2;0
WireConnection;14;1;15;1
WireConnection;14;2;15;2
WireConnection;14;3;15;3
WireConnection;14;4;15;4
WireConnection;26;0;1;4
WireConnection;26;1;27;0
WireConnection;10;0;14;0
WireConnection;9;0;1;4
WireConnection;9;1;7;0
WireConnection;9;2;8;0
WireConnection;23;0;1;0
WireConnection;23;1;26;0
WireConnection;23;2;24;0
WireConnection;31;0;1;4
WireConnection;31;1;29;0
WireConnection;31;2;30;0
WireConnection;18;0;10;0
WireConnection;18;1;20;0
WireConnection;22;0;9;0
WireConnection;28;0;22;0
WireConnection;28;1;31;0
WireConnection;25;0;18;0
WireConnection;25;1;23;0
WireConnection;0;0;1;0
WireConnection;0;1;14;0
WireConnection;0;2;25;0
WireConnection;0;4;5;0
WireConnection;0;8;6;0
WireConnection;0;9;28;0
WireConnection;0;10;2;4
ASEEND*/
//CHKSM=2897BCECF3EA02586661B756CE4CD559F73F4402
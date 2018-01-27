Shader "Skybox/NewSkybox"
{
	Properties
	{
		_Tex("Cubemap", Cube) = "white" {}
		_MainTex("Ramp", 2D) = "white" {}
		_Rotation("Rotation", Range(0,6.28)) = 0
		_RotationSpeed("Rotation speed", Range(-1,1)) = 0
		_IntensityBoost("Intensity boost", Float) = 20.0
		_Smoothness("Smoothness", Range(0,300)) = 100
	}

	SubShader
	{

		Tags{ "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
		Cull Off ZWrite Off Fog{ Mode Off }

		Pass
		{

				CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 5.0

		#include "UnityCG.cginc"
		#include "Lighting.cginc"

				samplerCUBE _Tex;
			sampler2D _MainTex;
			half4 _Tex_HDR;
			float _Rotation;
			float _IntensityBoost;
			float _RotationSpeed;

			float _Smoothness;

			struct appdata_t
			{
				float4 vertex : POSITION;
				float3 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
				half3 color : COLOR0;
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				float s, c;

				half3 sunDir = normalize(_WorldSpaceLightPos0.xyz);


				float3 zAxis = float3(0,0,1);
				float3 xAxis = normalize(cross(float3(0, 1, 0), zAxis));
				float3 yAxis = cross(zAxis, xAxis);

				float4x4 followMatrix = { xAxis.x, yAxis.x, zAxis.x, 0.0f,
					xAxis.y, yAxis.y, zAxis.y, 0.0f,
					xAxis.z, yAxis.z, zAxis.z, 0.0f,
					0.0f, 0.0f, 0.0f, 1.0f };

				float rotation = _Rotation + _RotationSpeed*_Time.x;
				sincos(rotation, s, c);
				float2x2 m = float2x2(c, -s, s, c);
				o.vertex = float4(mul(m, v.vertex.xz), v.vertex.yw).xzyw;
				o.vertex = mul(followMatrix, o.vertex);
				o.vertex = UnityObjectToClipPos(o.vertex);
				o.texcoord = v.texcoord;
				return o;
			}

			float mathSoftMax(float a, float b, float k = 300.0f)
			{
				return log(exp(k*a) + exp(k*b)) / k;
			}

			float mathSoftMin(float a, float b, float k = 300.0f)
			{
				return -mathSoftMax(-a, -b, k);
			}

			fixed4 frag(v2f i) : SV_Target
			{

				float rX = 0.5f + _RotationSpeed*_Time.x*2;
				float rY = 0;
				float srX = sin(rX);
				float crX = cos(rX);
				float srY = sin(rY);
				float crY = cos(rY);

				float3x3 mat;

				mat._11_21_31 = float3(crX*crY, -crX*srY, srX);
				mat._12_22_32 = float3(srY, crY, 0);
				mat._13_23_33 = float3(-srX*crY, srX*srY, crX);

				float tex1 = texCUBElod(_Tex, float4(i.texcoord,1)).r*abs(sin(_Time.x*6));
				float tex2 = texCUBElod(_Tex, float4(mul(mat, i.texcoord), 1)).g*abs(sin((_Time.x+2) * 8));
				float tex3 = texCUBElod(_Tex, float4(mul(-mat,i.texcoord), 1)).b*abs(sin((_Time.x+4) * 10));

				float ref = mathSoftMax(tex3, mathSoftMax(tex1, tex2, _Smoothness), _Smoothness);

				float4 color = tex2D(_MainTex, float2(ref, 0.5f));

				return color;
			}

			ENDCG
		}
	}
}

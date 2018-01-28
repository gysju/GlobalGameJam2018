Shader "Skybox/NewSkybox2"
{
	Properties
	{
		_Tex("Cubemap", Cube) = "white" {}
		_MainTex("Ramp", 2D) = "white" {}
		_Rotation("Rotation", Range(0,6.28)) = 0
		_RotationSpeed("Rotation speed", Range(-1,1)) = 0
		_IntensityBoost("Intensity boost", Float) = 20.0
		_Smoothness("Smoothness", Range(0,300)) = 100
			_Color("Color", Color) = (1,1,1,1)
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
			
			float4 _Color;

			// 1D random numbers
			float rand(float n)
			{
				return frac(sin(n) * 43758.5453123);
			}

			// 2D random numbers
			float2 rand2(in float2 p)
			{
				return frac(float2(sin(p.x * 591.32 + p.y * 154.077), cos(p.x * 391.32 + p.y * 49.077)));
			}

			// 1D noise
			float noise1(float p)
			{
				float fl = floor(p);
				float fc = frac(p);
				return lerp(rand(fl), rand(fl + 1.0), fc);
			}

			// rotate position around axis
			float2 rotate(float2  p, float a)
			{
				return float2 (p.x * cos(a) - p.y * sin(a), p.x * sin(a) + p.y * cos(a));
			}

			float voronoi(in float2 x)
			{
				float2 p = floor(x);
				float2 f = frac(x);

				float2 res = 8.0.xx;
				for (int j = -1; j <= 1; j++)
				{
					for (int i = -1; i <= 1; i++)
					{
						float2 b = float2(i, j);
						float2 r = float2(b) - f + rand2(p + b);

						// chebyshev distance, one of many ways to do this
						//float d = max(abs(r.x), abs(r.y));
						float d = length(r);

						if (d < res.x)
						{
							res.y = res.x;
							res.x = d;
						}
						else if (d < res.y)
						{
							res.y = d;
						}
					}
				}
				return res.y - res.x;
			}


			fixed4 frag(v2f i) : SV_Target
			{
				float2 pos = i.vertex.xy / _ScreenParams.xy;
				pos.y *= _ScreenParams.y / _ScreenParams.x;
				pos *= 2.0f;
				pos *= 0.6 + sin(_Time.x* 0.1) * 0.4;
				pos = rotate(pos, sin(_Time.x * 0.3) * 1.0);
				pos += _Time.x * 0.4;

				float v = 0.0;
				float a = 0.6, f = 1.0;

				for (int i = 0; i < 2; i++) 
				{
					float v1 = voronoi(pos * f + 5.0);
					float v2 = 0.0;

					// make the moving electrons-effect for higher octaves
					if (i > 0)
					{
						// of course everything based on voronoi
						v2 = voronoi(pos * f * 0.5 + 50.0 + _Time.x*5);

						float va = 0.0, vb = 0.0;
						va = 1.0 - smoothstep(0.0, 0.1, v1);
						vb = 1.0 - smoothstep(0.0, 0.08, v2);
						v += a * pow(va * (0.5 + vb), 2.0);
					}

					// make sharp edges
					v1 = 1.0 - smoothstep(0.0, 0.3, v1);

					// noise is used as intensity map
					v2 = a * (noise1(v1 * 1.5 + 0.1));

					// octave 0's intensity changes a bit
					if (i == 0)
						v += v2 * 0.5;
					else
						v += v2;

					f *= 3.0;
					a *= 0.7;
				}



				//float rX = 0.5f + _RotationSpeed*_Time.x*2;
				//float rY = 0;
				//float srX = sin(rX);
				//float crX = cos(rX);
				//float srY = sin(rY);
				//float crY = cos(rY);

				//float3x3 mat;

				//mat._11_21_31 = float3(crX*crY, -crX*srY, srX);
				//mat._12_22_32 = float3(srY, crY, 0);
				//mat._13_23_33 = float3(-srX*crY, srX*srY, crX);

				//float tex1 = texCUBElod(_Tex, float4(i.texcoord,1)).r*abs(sin(_Time.x*6));
				//float tex2 = texCUBElod(_Tex, float4(mul(mat, i.texcoord), 1)).g*abs(sin((_Time.x+2) * 8));
				//float tex3 = texCUBElod(_Tex, float4(mul(-mat,i.texcoord), 1)).b*abs(sin((_Time.x+4) * 10));

				//float ref = mathSoftMax(tex3, mathSoftMax(tex1, tex2, _Smoothness), _Smoothness);

				float4 color = tex2D(_MainTex, float2(1-v, 0.5f));

				//float4 color = float4(v,v, v, 1);

				return color * _Color;
			}

			ENDCG
		}
	}
}

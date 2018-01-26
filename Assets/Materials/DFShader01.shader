Shader "Custom/DFShader01" 
{
	Properties
	{
		_BackColor("Back color", Color) = (1,1,1,1)
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
			_NormalAmplification("Normal Amplification", Range(0,50)) = 10.0
			_CutLimit("Cut limit", Range(0,1)) = 0.0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input 
			{
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			float4 _BackColor;
			float4 _Color;
			float _NormalAmplification;
			float _CutLimit;

			void surf(Input IN, inout SurfaceOutputStandard o) 
			{
				// Albedo comes from a texture tinted by color
				float4 c = tex2D(_MainTex, IN.uv_MainTex);
				float height = c.g;
				float dhdx = ddx(height);
				float dhdy = ddy(height);
				float obj = smoothstep(_CutLimit, _CutLimit+0.05, height);

				//o.Albedo = lerp(_BackColor, _Color, obj);
				o.Albedo = _Color;
				clip(obj-0.5);

				o.Normal = float3(dhdx, dhdy, 0)*_NormalAmplification;
				o.Normal.z = sqrt(1 - o.Normal.x*o.Normal.x - o.Normal.y * o.Normal.y);

				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}

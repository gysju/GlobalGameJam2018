Shader "Custom/BasicTest"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	_Color_Distance("_Color_Distance", 2D) = "white" {}
	_Normal_Alpha("_Normal_Alpha", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _Color_Distance;
				sampler2D _Normal_Alpha;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 col2 = tex2D(_Color_Distance, i.uv);
				fixed4 col3 = tex2D(_Normal_Alpha, i.uv);
				// just invert the colors
				col.rgb = 1 - col.rgb;
				return float4(col2.rgb*col3.a,1);
			}
			ENDCG
		}
	}
}

﻿// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/MakeVisible" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_TransparencyMult("Transparency Mulitplier", Range(0,1)) = 1
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
			Tags {
				"RenderType" = "Transparent"
				"Queue" = "Transparent"
			}
			LOD 200

			ZWrite Off

			

			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows alpha:fade
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			half _TransparencyMult;

			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o) {
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;

				//Multiply alpha by value between 0 and 1
				o.Alpha = c.a * _TransparencyMult;
			}
			ENDCG
		}
}
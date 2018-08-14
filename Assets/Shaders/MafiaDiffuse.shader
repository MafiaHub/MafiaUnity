Shader "Mafia/Diffuse" {
	
	Properties {
		_MainTex ("Diffuse", 2D) = "white" {}
		_LightTex ("Lightmap", 2D) = "white" {}
	}
	
	SubShader {
		Tags { "RenderType" = "Opaque" }

		CGPROGRAM
		#pragma surface surf Lambert
	
		struct Input {
			float2 uv_MainTex;
			float2 uv_LightTex;
		};
	
		sampler2D _MainTex;
		sampler2D _LightTex;
	
		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
			o.Albedo *= tex2D(_LightTex, IN.uv_LightTex).rgb;
		}
		ENDCG

	} 
    Fallback "Diffuse"

}
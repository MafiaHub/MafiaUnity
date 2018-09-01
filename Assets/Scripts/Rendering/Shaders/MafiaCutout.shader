Shader "Mafia/Cutout" {
	
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
        _Cutoff ("Cutoff", float) = 0.5
		_MainTex ("Diffuse", 2D) = "white" {}
		_LightTex ("Lightmap", 2D) = "white" {}
	}
	
	SubShader {
		Tags {
            "RenderType"="Opaque"
            }
        
        LOD 400

        Cull Off

		CGPROGRAM
        #pragma surface surf Lambert
	
		struct Input {
			float2 uv_MainTex;
		};
	
		float4 _Color;
        float _Cutoff;
		sampler2D _MainTex;
		sampler2D _LightTex;
	
		void surf (Input IN, inout SurfaceOutput o) {
            float4 col = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = col.rgb;
			o.Albedo *= tex2D(_LightTex, IN.uv_MainTex).rgb;
			o.Albedo *= _Color.rgb;
            o.Alpha = _Color.a*col.a;
            clip(_Color.a*col.a - _Cutoff);
		}
		ENDCG

	} 
    Fallback "Legacy Shaders/Transparent/Cutout/Diffuse"

}
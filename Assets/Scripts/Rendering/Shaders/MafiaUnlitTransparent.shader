Shader "Mafia/Unlit Transparent" {
	
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Diffuse", 2D) = "white" {}
	}
	
	SubShader {
		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
            "RenderType"="Transparent"
            }
        
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_TARGET
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col*_Color;
            }

            ENDCG
        }
	} 
    Fallback "Diffuse"

}
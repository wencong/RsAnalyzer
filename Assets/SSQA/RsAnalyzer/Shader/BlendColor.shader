// Compiled shader for PC, Mac & Linux Standalone, uncompressed size: 0.4KB

Shader "BlendColor" {
	Properties {
		_Color ("Main Color", Color) = (0, 0, 0, 0)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader {    
		Pass {		
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM        
			#pragma vertex vert        
			#pragma fragment frag        
			#include "UnityCG.cginc"

			fixed4 _Color;

			float4 vert (float4 v:POSITION) : SV_POSITION        
			{            
				return mul (UNITY_MATRIX_MVP, v);        
			}

			fixed4 frag() : COLOR        
			{            
				return _Color;
			}

			ENDCG
		}
	}
}
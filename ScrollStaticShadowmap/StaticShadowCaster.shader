Shader "Unlit/StaticShadowCaster"
{
	Properties
	{
		 
	}
	SubShader
	{
		 
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
 			};

			struct v2f
			{   float4 vertex : SV_POSITION;
				 float2 depth : TEXCOORD0;
			};
	 
		 uniform float _shadowmapBias;
		 uniform float _shadowmapNormalBias;
 
			v2f vert (appdata v)
			{
				v2f o;
				float4 wpos=mul(unity_ObjectToWorld,v.vertex);
				float3 wnormal= UnityObjectToWorldNormal(v.normal);
				float nl=dot(wnormal,normalize(_WorldSpaceCameraPos.xyz-wpos));
		 float4 vpos = mul(UNITY_MATRIX_V,wpos-float4(_shadowmapNormalBias*0.15*wnormal,0));
		 vpos.z-=_shadowmapBias+lerp(0.1,0,pow(nl,0.5));
		 o.vertex= mul(UNITY_MATRIX_P,vpos);
	      o.depth= o.vertex.zw;
			return o;
			}
			
			fixed frag (v2f i) : SV_DEPTH
			{
  
            return   (i.depth.x/i.depth.y*0.5+0.5);
		  
       
			}
			ENDCG
		}
	}
}

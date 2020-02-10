Shader "Hidden/MergeDepth"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
		 
		ztest always
		 
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

			sampler2D _MainTex;
		uniform	sampler2D _rtTex;
	 
			 
 	    uniform	int _rtID;
    //uniform	int _rtTexID;
			float4 _MainTex_ST;
  			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
			//o.vertex.xy*=0.5;
			//o.vertex.xy+=_rtTexID*0.5;
			 
			half2	pos=o.vertex.xy/o.vertex.w;
			 
			pos=pos*0.5+0.5;
				 pos.xy*= lerp(0.8,0.2, sign(_rtID)); 
  	if(_rtID>0){
  
  		 
  			 if (_rtID < 5)
			{
				 pos.x += 0.8;
				pos.y += (_rtID-1) / 5.0;
			}
			else
			{
			 pos.x += (_rtID-5)/ 5.0;
				pos.y += 0.8;
				 
				
			}
			//o.vertex.y-=0.2;
  			}
				 
				 
					 o.vertex.xy=(pos*2-1)*o.vertex.w;
					  o.vertex.xy=clamp(o.vertex.xy,-1,1);
					 
				o.uv = v.uv;//TRANSFORM_TEX(, _MainTex);
 				return o;
			}
			
			float frag (v2f i) : SV_DEPTH
			{
			 
			  
					 
			 return  tex2D(_rtTex,i.uv).r;
				 
			}
			ENDCG
		}
	}
}

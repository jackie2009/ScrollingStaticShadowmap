using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

namespace com.jackie2009.scrollStaticShadowmap
{
	


public class StaticShadowCaster : MonoBehaviour
{
 	public bool renderUpdateMode;
	public Shader castShader;
	public Shader screenSpaceShadowsShader;
	public LightShadowResolution shadowResolution;
	private Camera cmr;
	public RenderTexture shadowmap;
[Range(0,2)]
	public float bias=0.05f;
	[Range(0,3)]
	public float normalBias=0.4f;
 	// Use this for initialization
    private int currentRenderIndex=0;
    private Matrix4x4[] lightProjecionMatrixs=new Matrix4x4[10];
    private void Awake()
    {
	     cmr = GetComponent<Camera>();
		print( SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap));
		print(SystemInfo.supports2DArrayTextures);
		print((SystemInfo.copyTextureSupport & CopyTextureSupport.DifferentTypes) != 0);
		print((SystemInfo.copyTextureSupport & CopyTextureSupport.Copy3D) != 0);
	
		GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows,screenSpaceShadowsShader);
	}



	private void OnEnable()
	{
		castShadow();
	}

	private void OnDisable()
	{
		cmr.targetTexture = null;
		shadowmap.Release();
		shadowmap = null;
		Shader.SetGlobalFloat("_shadowmapEnable", 0);
		Shader.SetGlobalTexture("_shadowmap",null);
		foreach (var renderer in FindObjectsOfType<Renderer>())
		{
					 
			if(renderer.gameObject.CompareTag("staticShadowmap"))
				renderer.shadowCastingMode = ShadowCastingMode.On;
		}
	}

	void castShadow()
	{
		/*
		rtArray.dimension = TextureDimension.Tex2DArray;
		rtArray.volumeDepth = 9;
		rtArray.Create();
		*/
		if (shadowResolution == LightShadowResolution.FromQualitySettings)
			shadowResolution = (LightShadowResolution)QualitySettings.shadowResolution;
		int rtSize = (1 << (int)shadowResolution) * 512;
		#if SHADOWMAP_ATLAS
		rtSize *= 2;
			//shadowmap=RenderTexture.GetTemporary(4096,4096,16, RenderTextureFormat.Shadowmap,RenderTextureReadWrite.Default,1);//  new RenderTexture(4096, 4096, 16/*depth*/, RenderTextureFormat.Shadowmap, RenderTextureReadWrite.Linear);
			shadowmap= new RenderTexture(rtSize, rtSize, 16/*depth*/, RenderTextureFormat.Shadowmap, RenderTextureReadWrite.Linear);
			
			
			
			 
			
		//shadowmap.dimension = TextureDimension.Tex2D;
 
		#else
		shadowmap= new RenderTexture(rtSize, rtSize, 16/*depth*/, RenderTextureFormat.Shadowmap, RenderTextureReadWrite.Linear);
	 
 		shadowmap.dimension = TextureDimension.Tex2DArray;
		shadowmap.volumeDepth = 10;
#endif
	 	shadowmap.useMipMap = false;
	 shadowmap.autoGenerateMips = false;
	 	shadowmap.filterMode = FilterMode.Point;
		 shadowmap.name = "StaticShadowmap";
		 shadowmap.Create();
		
		cmr.enabled = false;
		 cmr.targetTexture = shadowmap;
		 
        Shader.SetGlobalTexture("_shadowmap",shadowmap);
        Shader.SetGlobalFloat("_shadowmapEnable", 1);
		 foreach (var renderer in FindObjectsOfType<Renderer>())
		 {
			 //一定要留至少一个对象投射阴影 否则整个阴影流程会被引擎 优化掉 直接跳过我们的静态阴影
		 if(renderer.gameObject.CompareTag("staticShadowmap"))
			  renderer.shadowCastingMode = ShadowCastingMode.Off;
		 }


		 renderShadow();
	}

	private void renderShadow()
	{
		Shader.SetGlobalFloat("_shadowmapBias", bias);
		Shader.SetGlobalFloat("_shadowmapNormalBias", normalBias);

		     cmr.RenderWithShader(castShader,"");
        		 UpdateMatrix();
	}

	private void OnDestroy()
	{
		Shader.SetGlobalFloat("_shadowmapEnable", 0);
		Shader.SetGlobalTexture("_shadowmap",null);
	}

	private void Update()
	{
		if (renderUpdateMode&& shadowmap!=null)
		{
			renderShadow();
		 
		}
		
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		//;
		if (shadowmap == null) return;
#if SHADOWMAP_ATLAS
		 
		int destX = 0;
		int destY = 0;
		if (currentRenderIndex > 0)
		{
			//destX = shadowmap.width / 2 + shadowmap.width*((currentRenderIndex+1)%2) / 4;
			//destY = shadowmap.height/4 * ((currentRenderIndex-1)/2);
			if (currentRenderIndex < 5)
			{
				destX = shadowmap.width * 4 / 5;
				destY = shadowmap.height*(currentRenderIndex-1) / 5;
			}
			else
			{
				destX = shadowmap.width *(currentRenderIndex-5)/ 5;
				destY = shadowmap.height * 4 / 5;
				
			}
		}
 print(currentRenderIndex+","+src.width+","+src.height+","+destX+","+destY);

		 Graphics.CopyTexture(src,0,0,0,0,src.width,src.height,shadowmap,0,0,destX,destY);
	 

#else
		Graphics.CopyTexture(src,0,shadowmap,currentRenderIndex+1);
#endif
	}

	private void UpdateMatrix()
	{
	
		Matrix4x4 worldToView = cmr.worldToCameraMatrix;
		Matrix4x4 projection  = GL.GetGPUProjectionMatrix(cmr.projectionMatrix, false);
		Matrix4x4 lightProjecionMatrix =  projection * worldToView;
		//Shader.SetGlobalMatrix ("_LightProjection", lightProjecionMatrix);
#if SHADOWMAP_ATLAS
		lightProjecionMatrixs[currentRenderIndex] = lightProjecionMatrix;		
#else

		lightProjecionMatrixs[currentRenderIndex + 1] = lightProjecionMatrix;
#endif
		Shader.SetGlobalMatrixArray("_LightProjections", lightProjecionMatrixs);
		print(currentRenderIndex);
		 
 	}


	public void renderWithIndex(int index,Vector3 pos)
	{
		if (shadowmap == null)
		{
			return;
		}

		 
		pos.y = 0;
		transform.parent.position = pos;
		currentRenderIndex = index;
#if SHADOWMAP_ATLAS
		int rtSize = shadowmap.width*4/5;
		if (index != 0) rtSize = shadowmap.width/5;
		if (cmr.targetTexture != null)
		{
		//	var tempRT = cmr.targetTexture;
			//RenderTexture.ReleaseTemporary(tempRT);
		}

		var tempRT =cmr.targetTexture = RenderTexture.GetTemporary(rtSize, rtSize, 16, shadowmap.format);		
#endif
		renderShadow();
		
#if SHADOWMAP_ATLAS
		 
		  cmr.targetTexture =null;
		  RenderTexture.ReleaseTemporary(tempRT);
#endif
		
	}
}
}
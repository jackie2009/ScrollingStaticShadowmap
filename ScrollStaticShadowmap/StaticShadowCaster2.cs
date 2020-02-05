using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class StaticShadowCaster2 : MonoBehaviour
{
 	public bool renderUpdateMode;
	public Shader castShader;
	public Shader screenSpaceShadowsShader;
  
	private Camera cmr;
	public RenderTexture shadowmap;
[Range(0,2)]
	public float bias=0.05f;
	[Range(0,3)]
	public float normalBias=0.4f;
 	// Use this for initialization
	void Start () {
		  cmr = GetComponent<Camera>();
		print( SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap));
		print(SystemInfo.supports2DArrayTextures);
		print((SystemInfo.copyTextureSupport & CopyTextureSupport.DifferentTypes) != 0);
		print((SystemInfo.copyTextureSupport & CopyTextureSupport.Copy3D) != 0);
	
		GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows,screenSpaceShadowsShader);
	}

	private void OnGUI()
	{
		 
		GUI.skin.button.fontSize = 36;
		if (shadowmap == null)
		{
			if (GUI.Button(new Rect(Screen.width / 2 - 200, Screen.height / 2, 800, 40), "启用静态shadowmap观察drawcall和性能"))
			{

				castShadow();
			 
			}
			
		}
		else
		{
			if (GUI.Button(new Rect(Screen.width / 2 - 200, Screen.height / 2, 800, 40), "启动普通shadowmap观察drawcall和性能"))
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
		}
	}
	void castShadow()
	{
		/*
		rtArray.dimension = TextureDimension.Tex2DArray;
		rtArray.volumeDepth = 9;
		rtArray.Create();
		*/
		shadowmap= new RenderTexture(4096, 4096, 16/*depth*/, RenderTextureFormat.Shadowmap, RenderTextureReadWrite.Linear);
		shadowmap.useMipMap = false;
 		shadowmap.dimension = TextureDimension.Tex2DArray;
		shadowmap.volumeDepth = 9;
		shadowmap.autoGenerateMips = false;
		shadowmap.filterMode = FilterMode.Point;
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
		//Graphics.Blit(src,dest);
		 
		Graphics.CopyTexture(src,0,shadowmap,1);
  
	}

	private void UpdateMatrix()
	{
	
		Matrix4x4 worldToView = cmr.worldToCameraMatrix;
		Matrix4x4 projection  = GL.GetGPUProjectionMatrix(cmr.projectionMatrix, false);
		Matrix4x4 lightProjecionMatrix =  projection * worldToView;
		Shader.SetGlobalMatrix ("_LightProjection", lightProjecionMatrix);
 	}

	 
}

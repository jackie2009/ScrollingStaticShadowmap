using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace com.jackie2009.scrollStaticShadowmap
{
	public class ScrollStaticShadowmap : MonoBehaviour
	{

 
		public float cellSize=10;
        public int cellCountSqrt = 8;
		private StaticShadowCaster _shadowCaster;
        private GameObject testItem;
          public bool showdebugRect = true;
          public float avgHeight =0;

          private HashSet<int> capturedSet;//(z+5000)*10000+x+5000

          private int lastCenterKey;

          public Matrix4x4 lightSpace;
          public Matrix4x4 lightSpaceInverse;

 
          private Vector2 cellsizeInLightSpace;
        // Use this for initialization
        void Start()
        {
            	        
			_shadowCaster = GetComponent<StaticShadowCaster>();
          



            testItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
              testItem.layer = LayerMask.NameToLayer("Water");
              testItem.GetComponent<Collider>().enabled = false;
            testItem.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);

            testItem.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            testItem.GetComponent<Renderer>().material.color = Color.red;
            testItem.GetComponent<Renderer>().enabled = showdebugRect;
            testItem.transform.position = new Vector3(0, avgHeight, 0);




            float cosLightDir = Vector3.Dot(Vector3.up, -transform.forward);
            GetComponent<Camera>().aspect = 1 /cosLightDir ;
            GetComponent<Camera>().orthographicSize = cellSize / 2 * cosLightDir + (1-cosLightDir)*3+0.2f;//* Mathf.Sqrt(2);
      
             // _shadowCaster.enabled = true;
              capturedSet=new HashSet<int>();

              lightSpace = transform.worldToLocalMatrix;
              lightSpace.m03 = lightSpace.m13 = lightSpace.m23 = 0;
           

              lightSpaceInverse = transform.localToWorldMatrix;
              lightSpaceInverse.m03 = lightSpaceInverse.m13 = lightSpaceInverse.m23 = 0;
              Shader.SetGlobalMatrix("StaticShadowlightSpace", lightSpace);
               
              cellsizeInLightSpace=new Vector2(cellSize, cellSize * Mathf.Abs(transform.forward.y));
              Shader.SetGlobalVector("StaticShadowCellSize", cellsizeInLightSpace);

        }

       

        private void OnGUI()
		{

			GUI.skin.button.fontSize = 36;
			if (_shadowCaster.enabled == false)
			{
				if (GUI.Button(new Rect(0, 0, 800, 40), "启用静态shadowmap观察drawcall和性能"))
				{
			 
					capturedSet.Clear();
					_shadowCaster.enabled = true;

				}

			}
			else
			{
				if (GUI.Button(new Rect(0, 0, 800, 40), "启动普通shadowmap观察drawcall和性能"))
				{

					_shadowCaster.enabled = false;
				}
			}
		}
       

        private void Update()
        {
 
	        renderShadow(Time.frameCount);
	         
        }

        private int getPosSetKey(int x, int z)
        {
	        return (z+5000) * 10000 + (x+5000);
        }
        private int getDistanceForKey(int key1, int key2)
        {
	        return Mathf.Max(Mathf.Abs(key1/10000-key2/10000), Mathf.Abs(key1%10000-key2%10000));
        }
       
        // Update is called once per frame
		void renderShadow(int index)
		{
            _shadowCaster.cellCountSqrt = cellCountSqrt;
            if (_shadowCaster.enabled == false) return;
            
            
            int renderIndex = index % (cellCountSqrt * cellCountSqrt);
            int offsetX = renderIndex % cellCountSqrt-cellCountSqrt/2;
            int offsetY = renderIndex / cellCountSqrt-cellCountSqrt/2;
            Vector3 cmrPosRot = lightSpace.MultiplyPoint3x4(Camera.main.transform.position);
            var centerPos = new Vector2(cmrPosRot.x / cellsizeInLightSpace.x,cmrPosRot.y / cellsizeInLightSpace.y);
            int centerX = (int)Mathf.Floor(centerPos.x);
            int centerY = (int)Mathf.Floor(centerPos.y);
           
            //判断是否需要重拍阴影
            int centerKey = getPosSetKey(centerX, centerY);
            //删除capturedSet中需要重新绘制的
            if (lastCenterKey != centerKey)
            {
	            List<int> delItems=new List<int>();
	            foreach (var k in capturedSet)
	            {
		            if (getDistanceForKey(k, centerKey) > cellCountSqrt / 2)delItems.Add(k); 
	            }

	            foreach (var item in delItems)
	            {
		            capturedSet.Remove(item);
	            }

	            lastCenterKey = centerKey;

            }

            int newKey = getPosSetKey(centerX + offsetX, centerY + offsetY);
           if (capturedSet.Contains(newKey)) return;
            capturedSet.Add(newKey);
            
            var pos =Vector3.zero;
            
            pos.x = (offsetX + centerX) * cellsizeInLightSpace.x+cellsizeInLightSpace.x /2;
            pos.y = (offsetY + centerY) * cellsizeInLightSpace.y + cellsizeInLightSpace.y  / 2;

            Vector3 wpos = lightSpaceInverse.MultiplyPoint3x4(pos);
               float cmrAdjust= (wpos.y - avgHeight)/transform.forward.y;
               pos.z -= cmrAdjust;
               pos= lightSpaceInverse.MultiplyPoint3x4(pos);
            
            

            testItem.transform.position = pos;
            
            //计算世界坐标下 固定图集位置 ，不应该完全根据相对相机位置来计算 否则会覆盖其他正在使用的像素 这句含义只有上帝和我清楚
            int renderIndexX = renderIndex % cellCountSqrt;
            int renderIndexY = renderIndex / cellCountSqrt;
            renderIndexX = (renderIndexX + centerX%cellCountSqrt+cellCountSqrt) % cellCountSqrt;
            renderIndexY = (renderIndexY + centerY%cellCountSqrt+cellCountSqrt) % cellCountSqrt;
            
            renderIndex = renderIndexY * cellCountSqrt + renderIndexX;
         _shadowCaster.renderWithIndex(renderIndex,pos);
         print("renderIndex:"+renderIndex);



        }
	}
}
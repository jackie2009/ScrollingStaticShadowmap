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
            Vector4 StaticShadowLightDir = transform.forward;
            StaticShadowLightDir.w = cellSize;
             Shader.SetGlobalVector("StaticShadowLightDir", StaticShadowLightDir);
             Shader.SetGlobalFloat("StaticShadowAvgHeight", avgHeight);
             // _shadowCaster.enabled = true;
              capturedSet=new HashSet<int>();
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
            int offsetZ = renderIndex / cellCountSqrt-cellCountSqrt/2;
            var centerPos = Camera.main.transform.position / cellSize;
            int centerX = (int)Mathf.Floor(centerPos.x);
            int centerZ = (int)Mathf.Floor(centerPos.z);
           
            //判断是否需要重拍阴影
            int centerKey = getPosSetKey(centerX, centerZ);
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

            int newKey = getPosSetKey(centerX + offsetX, centerZ + offsetZ);
            if (capturedSet.Contains(newKey)) return;
            capturedSet.Add(newKey);
            
            var pos = testItem.transform.position;
            pos.x = (offsetX + centerX) * cellSize+cellSize /2;
            pos.z = (offsetZ + centerZ) * cellSize + cellSize  / 2;
            testItem.transform.position = pos;
            
            //计算世界坐标下 固定图集位置 ，不应该完全根据相对相机位置来计算 否则会覆盖其他正在使用的像素 这句含义只有上帝和我清楚
            int renderIndexX = renderIndex % cellCountSqrt;
            int renderIndexZ = renderIndex / cellCountSqrt;
            renderIndexX = (renderIndexX + centerX%cellCountSqrt+cellCountSqrt) % cellCountSqrt;
            renderIndexZ = (renderIndexZ + centerZ%cellCountSqrt+cellCountSqrt) % cellCountSqrt;
            
            renderIndex = renderIndexZ * cellCountSqrt + renderIndexX;
         _shadowCaster.renderWithIndex(renderIndex,pos);
         print("renderIndex:"+renderIndex);



        }
	}
}
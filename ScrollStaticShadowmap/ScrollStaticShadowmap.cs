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
         public float renderStep = 0.1f;
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
            testItem.transform.position = new Vector3(0, 1.4f, 0);




            float cosLightDir = Vector3.Dot(Vector3.up, -transform.forward);
            GetComponent<Camera>().aspect = 1 /cosLightDir ;
            GetComponent<Camera>().orthographicSize = cellSize / 2 * cosLightDir + (1-cosLightDir)*3+0.2f;//* Mathf.Sqrt(2);
            //StartCoroutine(loopRender());
             //_shadowCaster.enabled = true;
		}

       

        private void OnGUI()
		{

			GUI.skin.button.fontSize = 36;
			if (_shadowCaster.enabled == false)
			{
				if (GUI.Button(new Rect(0, 0, 800, 40), "启用静态shadowmap观察drawcall和性能"))
				{
					 
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
        private IEnumerator loopRender()
        {
	        int index = 0;
	        while (true)
	        {
             yield return new WaitForSeconds(renderStep);
             renderShadow(index);
		        index++;
	        }
        }

        private void Update()
        {
	        renderShadow(Time.frameCount);
	        print(   GetComponent<Camera>().aspect );
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



        }
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace com.jackie2009.scrollStaticShadowmap
{
	public class ScrollStaticShadowmap : MonoBehaviour
	{

		private ShadowmapCell[] cells;
		public float cellSize=10;
		private StaticShadowCaster _shadowCaster;

		// Use this for initialization
		void Start()
		{
			_shadowCaster = GetComponent<StaticShadowCaster>();
			cells = new ShadowmapCell[3 * 3];
			for (int i = 0; i < cells.Length; i++)
			{
				int r = i / 3;
				int c = i % 3;
				var testItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
				testItem.GetComponent<Collider>().enabled = false;
				testItem.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);

				testItem.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
				testItem.GetComponent<Renderer>().material.color = new Color((float)r/3,(float)c/3 ,
					0);
				testItem.transform.position = new Vector3(0, 1.4f, 0);
				cells[i] = new ShadowmapCell(c - 1, r - 1, testItem.transform,cellSize);
			}

			GetComponent<Camera>().orthographicSize = cellSize / 2 * Mathf.Sqrt(2);
		}

		private void OnGUI()
		{

			GUI.skin.button.fontSize = 36;
			if (_shadowCaster.enabled == false)
			{
				if (GUI.Button(new Rect(0, 0, 800, 40), "启用静态shadowmap观察drawcall和性能"))
				{
					foreach (var c in cells)
					{
						c.resetCurrentPos();
					}
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


		// Update is called once per frame
		void Update()
		{
			if(_shadowCaster.enabled )
			cells[Time.frameCount % cells.Length].updateDraw(_shadowCaster);

		}
	}
}
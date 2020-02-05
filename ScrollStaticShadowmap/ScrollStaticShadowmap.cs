using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollStaticShadowmap : MonoBehaviour
{
	 
	private ShadowmapCell []cells;
	// Use this for initialization
	void Start () {
		cells=new ShadowmapCell[3*3];
		for (int i = 0; i < cells.Length; i++)
		{
			int r = i / 3;
			int c = i % 3;
			var testItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
			testItem.GetComponent<Collider>().enabled = false;
			testItem.transform.localScale=new Vector3(100,1,100);
			testItem.GetComponent<Renderer>().material.color=new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f));
			testItem.transform.position=new Vector3(0,1.4f,0);
			cells[i]=new ShadowmapCell(c-1,r-1,testItem.transform);
		}
	}

	

	// Update is called once per frame
	void Update () {
		cells[Time.frameCount%cells.Length].updateDraw();
		
	}
}

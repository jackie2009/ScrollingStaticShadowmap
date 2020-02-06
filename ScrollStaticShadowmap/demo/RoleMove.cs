using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleMove : MonoBehaviour
{
	public float speed = 10;
	 
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 movDir=Vector2.zero;
		movDir.x = Input.GetAxis("Horizontal");
		movDir.z = Input.GetAxis("Vertical");
		transform.Translate(movDir.normalized*Time.deltaTime*speed,Camera.main.transform);
	}
}

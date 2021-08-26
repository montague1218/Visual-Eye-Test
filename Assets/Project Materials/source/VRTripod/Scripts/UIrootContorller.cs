using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIrootContorller : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		this.transform.rotation = Quaternion.Euler(
            transform.eulerAngles.x,
			Camera.main.transform.eulerAngles.y,
            transform.eulerAngles.z);
	}
}

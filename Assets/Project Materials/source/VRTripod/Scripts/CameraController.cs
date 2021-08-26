using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using System.Linq;
using System;

public class CameraController : MonoBehaviour {
    public float moveSpeed = 2.0f;
    public float backSpeed = 1.0f;
    public float upSpeed = 1.0f;
    public float downSpeed = 0.7f;
	public GameObject tripod;

	void Update()
    {


        transform.Translate(Vector3.forward * Time.deltaTime);
        transform.Translate(Vector3.up * Time.deltaTime, Space.World);
		//Vector3 cameraPos = transform.position;
		//cameraPos.z -= moveSpeed * Time.deltaTime;
    }

	public void goUp()
    {
        //public float upSpeed = 1.0f;
        this.transform.Translate(new Vector3(0, Time.deltaTime * this.upSpeed, 0), Space.World);
    }

    public void goDown()
    {
        //public float downSpeed = 0.7f;
        this.transform.Translate(new Vector3(0, Time.deltaTime * -this.downSpeed, 0), Space.World);
    }

	public void goForward()
	{   
		transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
		//var forward = this.transform.forward;
		//this.transform.Translate(Time.deltaTime * this.moveSpeed * forward, Space.World);
	}

    public void goBack()
    {
        //public float downSpeed = 0.7f;
        var forward = this.transform.forward;
        var dir = new Vector3(-forward.x, 0, -forward.z);
        this.transform.Translate(Time.deltaTime * this.backSpeed * dir, Space.World);
        //this.transform.Translate(new Vector3(0, 0, Time.deltaTime * -this.backSpeed), Space.Self);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class tripodController : MonoBehaviour
{
	//Click move
	public Transform vrCamera;
    private bool started = false;
	//Click move
	

	[Header("")]
    public float speed = 3.0f;
    public float upSpeed = 1.0f;
    public float downSpeed = 1.0f;
    public float backSpeed = 3.0f;
    private bool isMove = false;
    private bool isGoingUp = false;
	private bool isGoingDown = false;
    private bool isGoingBack = false;

    //public GameObject buttons;
    //public float smoothSpeed = 0.125f;
    //public float startSpeed = 1.0f;
    //public float startPos = 38.0f;

    void FixedUpdate()
	{
        MoveCamera();

    }
	
    public void MoveCamera() {
        //this.transform.rotation = Quaternion.Euler(
        //transform.eulerAngles.x,
        //vrCamera.eulerAngles.y,
        //transform.eulerAngles.z);

        //if (started == true && transform.position.z <= startPos) 
        //{
        //  Vector3 desiredPos = transform.position;
        //  desiredPos.z += startSpeed;

        //  Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);
        //  transform.position = smoothedPos;

        //  if (transform.position.z >= startPos -0.1) 
        //  {
        //      started = false;
        //      buttons.SetActive(true);
        //  }
        //}
        if (this.isGoingUp)
        {
            this.goUp();
        }
        if (this.isGoingDown)
        {
            this.goDown();
        }
        if (this.isGoingBack)
        {
            this.goBack();
        }
        if (this.isMove)
        {
            this.Move();
        }
    }

    /*-----------START-------------*/
    public void MovingForward()
	{
		started = true;
	}

	/*-----------MOVE-------------*/
	public void startMoving()
	{
		isMove = true;
	}
	public void stopMoving()
	{
		isMove = false;
	}
	public void Move()
    {
        transform.position = transform.position + vrCamera.transform.forward * speed * Time.deltaTime;
    }

    /*-----------UP-------------*/
	public void startGoingUp()
    {
        this.isGoingUp = true;
        //if (upSound != null) upSound.tweenTo(1);
    }
	public void stopGoingUp()
    {
        this.isGoingUp = false;
        //if (upSound != null) upSound.tweenTo(0);
    }
	public void goUp()
    {
		//public float upSpeed = 1.0f;
		//transform.position = transform.position + transform.up * upSpeed * Time.deltaTime;
        this.transform.Translate(new Vector3(0, Time.deltaTime * this.upSpeed, 0), Space.World);
    }

	/*-----------DOWN-------------*/   
	public void startGoingDown()
    {
        this.isGoingDown = true;
        //if (downSound != null) downSound.tweenTo(1);
    }

    public void stopGoingDown()
    {
        this.isGoingDown = false;
        //if (downSound != null) downSound.tweenTo(0);
    }
	public void goDown()
    {
        //public float downSpeed = 0.7f;
        this.transform.Translate(new Vector3(0, Time.deltaTime * -this.downSpeed, 0), Space.World);
    }

	/*-----------BACK-------------*/
	public void startGoingBack()
    {
        this.isGoingBack = true;
        //if (backSound != null) backSound.tweenTo(1);
    }   
    public void stopGoingBack()
    {
        this.isGoingBack = false;
        //if (backSound != null) backSound.tweenTo(0);
    }
	public void goBack()
    {
		//public float backSpeed = 1.0f;

		var forward = vrCamera.forward;
		var dir = new Vector3(-forward.x, 0, -forward.z);
  
        this.transform.Translate(Time.deltaTime * this.backSpeed * dir, Space.World);
		//this.transform.Translate(new Vector3(0, 0, Time.deltaTime * -this.backSpeed), Space.World);
    }
}


using UnityEngine;
using System.Collections;
using System;
public class SnookerBallController : MonoBehaviour {

    public bool onMove;
	public bool stoped;
	public Transform _direction;
	public float _force;
	public int id;
	public string type;
	public bool pocketed;
	
	Vector3 curPos;
	Vector3 lastPos;
	
	public AudioClip poolballhit;
	
	
	
	Rigidbody rigidbody;
	
	void Start() {
		GetComponent<Rigidbody>().sleepThreshold = 0.15f;
		rigidbody = GetComponent<Rigidbody>();
		
	}
	
	void Awake()
	{
	  lastPos = transform.position;
	  
	  stoped = true;
	}

	void Update()
	{
	    if (PoolNetworkManager.instance.startedGame) 
		{
		  float speed = rigidbody.velocity.magnitude;
		  
	      if(speed > 0)
	      { 
		     PoolNetworkManager.instance.EmitBallPosition(id,transform.position,transform.rotation);
		  
		  }
		  
		  if(speed < 0.01)
		  {
		     stoped = true;
		  }
		  else
		  {
		     stoped = false;
		  }
		}
	
			  
	}
	void FixedUpdate () {
	
	try{
		rigidbody = GetComponent<Rigidbody>();
		if (rigidbody.velocity.y > 0) {
			var velocity = rigidbody.velocity;
			velocity.y *= 0.3f;
			rigidbody.velocity = velocity;
		}
		
		if(onMove)
		{
		  MoveCueBall();
		  onMove = false;
		}
		
	    if (PoolNetworkManager.instance.myTurn && PoolNetworkManager.instance.startedGame) 
		{
         
		     curPos = transform.position;
			 
             lastPos = curPos;
			 
	    }
	}
	catch(Exception e)
	{
		  Debug.Log(e.ToString());
		  PoolCanvasManager.instance.txtLog.text = e.ToString();
	}
		
		
		
		
	}
	
	public void MoveCueBall()
    {
	   try{
		   GetComponent<Rigidbody>().AddForce(_direction.TransformDirection(Vector3.forward) *-1* _force);
		   }
		catch(Exception e)
		{
		  Debug.Log(e.ToString());
		  PoolCanvasManager.instance.txtLog.text = e.ToString();
		}
		   
				
   }
   
   public void NetworkMove(Vector3 _position,Quaternion _rot) 
	{

		transform.position = new Vector3(_position.x,transform.position.y,_position.z);
		transform.rotation = _rot;
	}
	
	
	
	
	
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PoolServerModule;

public class CueController : MonoBehaviour
{
    public float force;
	
    public float maxForce;
	
	public float minForce;

	public Vector3 strikeDirection;
	
	public GameObject circleRendererPref;
	
	public GameObject circleRenderer;
	
	public bool canMove;

	public const float MIN_DISTANCE = 27.5f;
	
	public const float MAX_DISTANCE = 32f;
	
	public GameObject cue;
	
	public GameObject cueBall;
	
	public GameObject startLineTrans;
	
	public float forceAmplitude;
	
	LineRenderer cueLine;
	
	public Vector3 cueOffset;
	
	int obstacleMask;
	
	Quaternion curRot;
	
	Quaternion lastRot;
	
	public enum state : int {idle,aim,strike};

	public state currentState;
	   
	private Vector3 touchMousePos;
	  
	Quaternion rot;
	  
	private RaycastHit hitInfo = new RaycastHit();
	
    private Ray ray = new Ray();
	
	public float radius;
	
	Vector3 endPosition;
	
	
    // Start is called before the first frame update
    void Awake()
    {
	    cueLine = GetComponent <LineRenderer> ();
		strikeDirection = Vector3.forward;
		obstacleMask = LayerMask.GetMask ("Obstacle");
		cueLine.enabled = false;
        
		
		if (PoolNetworkManager.instance.udpClient.serverRunning) 
		{
		  //get original cue ball
		  cueBall = PoolGameManager.instance.cueBall;
		}
		else
		{
		   //get network clone cue ball
		   cueBall = PoolGameManager.instance.networkCueBall;
		}
		  
	
    }

	public void SetUpDefaultPosition()
	{
	  transform.position = new Vector3(cueBall.transform.position.x - cueOffset.x,transform.position.y,cueBall.transform.position.z - cueOffset.z);
	}
	
   void Update() {
	 
	 if (PoolNetworkManager.instance.myTurn)
	 {	  
		curRot = transform.rotation;
        if(curRot!= lastRot) {
		
		  UpdateStatusToServer ();
		}
		
        lastRot = curRot;
	  		  
	    switch (currentState)
        {
		
		  case state.idle:
		   DisableEffects ();
		  break;
          case state.aim:
		  
		  canMove = true;
		  GetComponent<Renderer>().enabled = true;
		  cueLine.enabled = true;
		  RaycastHit hit;	
		  float minDistance = Mathf.Infinity;
		  
		  ray.origin = startLineTrans.transform.position;
		  ray.direction = startLineTrans.transform.TransformDirection(Vector3.forward);
		  radius = cueBall.GetComponent<SphereCollider>().radius * cueBall.transform.localScale.x;
				 
		   if (Physics.SphereCast(ray, radius, out hit, Mathf.Infinity, obstacleMask))
		   {
		      cueLine.enabled = true;
					
			  if(circleRenderer!=null)
		      {
				 Destroy(circleRenderer);
			  }
			  
			  endPosition = ray.origin + (ray.direction.normalized * hit.distance);     
			  circleRenderer = GameObject.Instantiate (circleRendererPref,
			  new Vector3(endPosition.x,circleRendererPref.transform.position.y,endPosition.z),
			  circleRendererPref.transform.rotation);
	            
			  if(circleRenderer!=null)
			  {
				  cueLine.SetPosition (0, startLineTrans.transform.position);
				  cueLine.SetPosition (1, circleRenderer.transform.position);
			  }
					  
			}//END_IF
		    else
		    {
			    if(circleRenderer!=null)
				{
				    Destroy(circleRenderer);
				}
			}
			
			Debug.DrawRay(startLineTrans.transform.position, startLineTrans.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
			break;
			
		    case  state.strike:
			  
			  Destroy(circleRenderer);
			 
			  if (PoolNetworkManager.instance.udpClient.serverRunning)
			  {
			     PoolGameManager.instance.Strike();
			     PoolGameManager.instance.PlayPoolBallHitSound();
			  }
			  else
			  {
			  
			    float sliderValue = 100 - PoolCanvasManager.instance.powerSlider.value;
				PoolCanvasManager.instance.powerSlider.value = 100;
			    PoolNetworkManager.instance.EmitCueForce(sliderValue);
				
			  }
			  
			  PoolGameManager.instance.HideCue();
			  currentState = state.idle;
			  break;
				 
			}//END_SWITCH
			 
			}//END_IF
		  
	 }
		
	 public void DisableEffects ()
     {
         cueLine.enabled = false;
		 GetComponent<Renderer>().enabled = false;
        
     }
		 
	 public void EnableEffects ()
     {
         cueLine.enabled = true;
		 GetComponent<Renderer>().enabled = true;
        
     }
		
     void MoveCueBall()
     {
		    
		float totalForce = PoolCanvasManager.instance.powerSlider.value/100*maxForce;
		
		if(totalForce>0)
		{
		   forceAmplitude = totalForce - minForce;
		}
		else
		{
		   forceAmplitude = 0;
		}
		   
		var relativeDistance = (Vector3.Distance(transform.position, cueBall.transform.position) - MIN_DISTANCE) / (MAX_DISTANCE - MIN_DISTANCE);
			
		force = forceAmplitude * relativeDistance + minForce;
		startLineTrans.transform.parent = null;
			
		if (PoolNetworkManager.instance.udpClient.serverRunning) 
		{
			 cueBall.GetComponent<SnookerBallController>()._direction = startLineTrans.transform;
	         cueBall.GetComponent<SnookerBallController>()._force = force;
			 cueBall.GetComponent<SnookerBallController>().onMove = true;
		}
		else
		{
			 PoolNetworkManager.instance.EmitCueForce(force);
		}
					
    }
		
		
	void UpdateStatusToServer ()
	{
	  PoolNetworkManager.instance.EmitCuePosAndRot(transform.position,transform.rotation);
	}
  
   void OnMouseDrag()
   {
      Turning();
   }
  
    /// <summary>
	/// rotate cue aroud ball
	/// </summary>
	void Turning()
	{
	
	   float angle = 0;
       
	   Quaternion rotation = transform.rotation;

       if (Mathf.Abs(Input.GetAxis("Mouse X")) > Mathf.Abs(Input.GetAxis("Mouse Y")))
       {
           angle = Input.GetAxis("Mouse X");
           
		   if (Camera.main.ScreenToWorldPoint(Input.mousePosition).z < startLineTrans.transform.position.z)
           {
             angle = -angle;          
           }
		  
        }
       else
       {
          angle = Input.GetAxis("Mouse Y");
		  
          if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x > startLineTrans.transform.position.x)
           {
               angle = -angle;  
           }
       }
	   
       float factor = Vector2.Distance(touchMousePos, Camera.main.ScreenToWorldPoint(Input.mousePosition));
       
	   factor *= 300.0f;

       if(factor < 1.5f)
	   {
	      factor = 1.5f;
	   }
     
       factor = factor * 0.05f;
       
       if(factor > 6.0f)
	   {
	      factor = 6.0f;
	   }
	  
       angle *=  factor;

       touchMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
           
       rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y+ angle, rotation.eulerAngles.z );
	   
	   if(cueBall!=null)
	   {
	     cue.transform.RotateAround(new Vector3(cueBall.transform.position.x,transform.position.y,cueBall.transform.position.z), Vector3.up,angle);
	   }
      	
	}
	
	/// <summary>
	/// Sets the cue state.
	/// </summary>
	public void SetState(string _state)
	{
	   switch (_state)
       {
		 case "idle":
		 currentState = state.idle;
		 break;
		 case "aim":
		 SetUpDefaultPosition();
		 currentState = state.aim;
		 break;
		 case "strike":
		 currentState = state.strike;
		 break;
	   }//END_SWITCH
	}

	/// <summary>
	/// Gets the cue state.
	/// </summary>
	/// <returns>The state in string format.</returns>
	public string GetState()
	{
	   switch (currentState)
       {
		 case state.idle:
		 return "idle";
		 break;
		 case state.aim:
		 return "aim";
		 break;
		 case state.strike:
		  return "strike";
		 break;
	   }//END_SWITCH
		return string.Empty;
	}
  
}

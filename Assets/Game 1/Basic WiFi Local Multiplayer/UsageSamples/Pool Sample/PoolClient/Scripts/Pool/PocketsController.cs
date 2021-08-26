using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PoolServerModule;

public class PocketsController : MonoBehaviour {
	public GameObject redBalls;
	public GameObject cueBall;
	

	private Vector3 originalCueBallPosition;
	
	public float maxDistance = 1f;
	
	ArrayList ballsToRemove;


	void Start() {
		
		ballsToRemove = new ArrayList();
		
	}
	
	
	public void Update()
	{
		OnCustomCollider ();
	}
	
	/// <summary>
	/// method for detecting the proximity of the pocket
	/// </summary>
	void OnCustomCollider()
	{
	
		   foreach(KeyValuePair<int, SnookerBallController> entry in PoolGameManager.instance.balls)
		   {
		     Vector3 meToBall = transform.position - entry.Value.gameObject.transform.position;
			 //check if ball is near pocket
			 if (meToBall.sqrMagnitude < maxDistance) 
			 { 
			   if(entry.Value.id!=0&&!entry.Value.pocketed)
				  {
				   
				   entry.Value.pocketed = true;
			
				   ballsToRemove.Add(entry.Value.id);
				
				   PoolGameManager.instance.BallPocketed(entry.Value.id);
				   PoolGameManager.instance.PlayPocketSound();
				   GameObject.Destroy(entry.Value.gameObject);
				  }
				  else
				  {
				    PoolGameManager.instance.cueBall.transform.position =  PoolGameManager.instance.originalCueBallPosition;
				  }
				

			 }
		   }
		   
		   foreach(int i in ballsToRemove)
		   {
		     PoolGameManager.instance.balls.Remove (i);
		   }
		
	}

	
}

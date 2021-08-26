using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PoolServerModule;

public class PoolGameManager : MonoBehaviour
{
   public static PoolGameManager instance;
   
   public GameObject cuePref;
   
   public Vector3 cueOffset;
   
   public GameState currentState { get; set; }
   
   public string gameState;
   
   public float forceAmplitude;
   
    public float maxForce;
	
	public float minForce;
	
	public float force = 0f;
	
	public Vector3 originalCueBallPosition;
	
	bool waitingDelay;
	
	public bool onStrike;

	public const float MIN_DISTANCE = 27.5f;
	
	public const float MAX_DISTANCE = 32f;
	
	public GameObject cue;
	
	public GameObject ballsContainer;
	
	public GameObject [] ballsPref;
	
	public GameObject [] netBallsPref;
	
	public GameObject networkCueBallPref;
	
	public float networkSliderValue;
	
	public GameObject networkCueBall;
	
	public GameObject networkCuePref;
	
	public GameObject networkCue;
	
	public GameObject cueBallPref;
	
	public GameObject cueBall;
	
	public Dictionary<int, SnookerBallController> balls = new Dictionary<int, SnookerBallController>();
	
	public Dictionary<int, NetworkBall> netBalls = new Dictionary<int, NetworkBall>();
	
	public bool startedGame;
	
	public bool timeOut;
	
    public int pocketedBalls;
   
    public int pocketedSolidBalls;
	
	public int pocketedStripeBalls;
	
	public int totalBalls;
	
	public AudioClip pocketSound;
	
	public AudioClip poolballhit;
	
	public AudioClip ballColisionSound;
	
	
	
	void Awake()
    {
        if (instance == null)
        {
            instance = this;
			originalCueBallPosition = cueBallPref.transform.position;
			currentState = GameState.WaitingForStrikeState;
			gameState = "WaitingForStrikeState";
			
        }
        else
        {
            DestroyImmediate(this);
        }
    }
	
	public void StartGame()
	{
	   currentState = GameState.WaitingForStrikeState;
	   totalBalls = PoolGameManager.instance.ballsPref.Length;
	   pocketedBalls = 0;
	   gameState = "WaitingForStrikeState";
	   SpawnCueBall();
	   SpawnBalls();
	  
	}
	

	public void SpawnCueBall()
	{
	   
	   cueBall = GameObject.Instantiate (cueBallPref,cueBallPref.transform.position,cueBallPref.transform.rotation);
	   cueBall.name = "CueBall";
	   balls[0] = cueBall.GetComponent<SnookerBallController> ();
	}
	
	public void SpawnBalls()
	{
	   foreach(GameObject ball in ballsPref)
	   {
		   SnookerBallController new_ball = GameObject.Instantiate (ball,ball.transform.position,ball.transform.rotation).
		   GetComponent<SnookerBallController> ();
		   new_ball.gameObject.name = "Ball"+new_ball.id;
		   balls[new_ball.id] = new_ball;
	  
	  }
	}
	
	public void SpawnNetworkCueBall()
	{ 
	   networkCueBall = GameObject.Instantiate (networkCueBallPref,networkCueBallPref.transform.position,networkCueBallPref.transform.rotation);
	   netBalls[0] = networkCueBall.GetComponent<NetworkBall> ();	  
	   networkCueBall.name = "NetCueBall";
	}
	
	
	
	public void SpawnNetworkBalls()
	{
	  foreach(GameObject ball in netBallsPref)
	  {
		   NetworkBall new_ball = GameObject.Instantiate (ball,ball.transform.position,ball.transform.rotation).
		   GetComponent<NetworkBall> ();
		   new_ball.gameObject.name = "Ball"+new_ball.id;
		   netBalls[new_ball.id] = new_ball;
	  }
	}
	
	public void SpawnCue()
	{
	   if(cue!=null)
	   {
	     Destroy(cue);
	   }
	  	if (PoolNetworkManager.instance.udpClient.serverRunning) {
		
		 cue = Instantiate (cuePref,cuePref.transform.position,cuePref.transform.rotation);
		
	   }
	   else
	   {
	      cue = Instantiate (cuePref,cuePref.transform.position,cuePref.transform.rotation);
	   }
	  
	}
	
	public void HideCue()
	{
	  
	 cue.GetComponent<CueController>().DisableEffects();
	
	}
	
	public void ActiveCue()
	{
	 
		cue.GetComponent<CueController>().EnableEffects();
	}
	public void ActiveNetworkCue()
	{
	 
	   if(networkCue!=null)
	   {
		 networkCue.GetComponent<Renderer>().enabled = true;
	   }
	   else
	   {
	      SpawnNetworkCue();
	   }
	}
	
	public void HideNetworkCue()
	{
	  networkCue.GetComponent<Renderer>().enabled = false;
	}
	
	public void DestroyCue()
	{
	   if(cue!=null)
	   {
	     Destroy(cue);
	   }
	}
	
	public void SpawnNetworkCue()
	{
	   if(networkCue!=null)
	   {
	     Destroy(networkCue);
	   }
	   networkCue = Instantiate (networkCuePref,networkCuePref.transform.position,networkCuePref.transform.rotation);
	   
	   
	}
	
	public void ReleaseCueStrike()
	{
	  
	  if(  cue.GetComponent<CueController>().GetState().Equals("aim"))
	  {
	     cue.GetComponent<CueController>().SetState("strike");
	  }
	
	}
	
	
	public void DestroyNetworkCue()
	{
	   if(networkCue!=null)
	   {
	    Destroy(networkCue);
	   }
	   
	}

	public void Strike()
	{
	   if(PoolNetworkManager.instance.myTurn)
	   {
	   
	     float sliderValue = 100 - PoolCanvasManager.instance.powerSlider.value;
	     float totalForce = sliderValue/100*maxForce;
		 PoolCanvasManager.instance.powerSlider.value = 100;
		 if(totalForce>0)
		 {
			  forceAmplitude = totalForce - minForce;
		 }
		 else
		 {
		    forceAmplitude = 0;
		 }
	   
		 var relativeDistance = (Vector3.Distance(cue.transform.position, cueBall.transform.position) - MIN_DISTANCE) / (MAX_DISTANCE - MIN_DISTANCE);
			
		 force = forceAmplitude * relativeDistance + minForce;
		 GameObject _startLineTrans = cue.GetComponent<CueController>().startLineTrans;
		 _startLineTrans.transform.parent = null;
		 cueBall.GetComponent<SnookerBallController>()._direction = _startLineTrans.transform;
	     cueBall.GetComponent<SnookerBallController>()._force = force;
	     cueBall.GetComponent<SnookerBallController>().onMove = true;
		 _startLineTrans.transform.parent = cue.transform;
		 currentState = GameState.StrikingState;
	   }
	   else
	   {
		  float totalForce = networkSliderValue/100*maxForce;
		  if(totalForce>0)
		  {
			forceAmplitude = totalForce - minForce;
		  }
		  else
		  {
			forceAmplitude = 0;
		  }
		  
		  var relativeDistance = (Vector3.Distance(networkCue.transform.position, cueBall.transform.position) - MIN_DISTANCE) / (MAX_DISTANCE - MIN_DISTANCE);
		  force = forceAmplitude * relativeDistance + minForce;
		  GameObject _startLineTrans = networkCue.GetComponent<NetworkCue>().startLineTrans;
		  _startLineTrans.transform.parent = null;
		  cueBall.GetComponent<SnookerBallController>()._direction = _startLineTrans.transform;
	      cueBall.GetComponent<SnookerBallController>()._force = force;
		  cueBall.GetComponent<SnookerBallController>().onMove = true;
		  _startLineTrans.transform.parent = networkCue.transform;
		  currentState = GameState.StrikingState;
	   }
	        
	}
	
	public void BallPocketed(int ballNumber) {
	
	   if(PoolNetworkManager.instance.udpClient.serverRunning)
	   {
	    PoolNetworkManager.instance.EmitDestroyBall(ballNumber);
	   }
	   
	}
	
	 // Update is called once per frame
    void Update()
    {
       	
	   switch (currentState)
       {
		
		 case GameState.WaitingForStrikeState:
	      gameState = "WaitingForStrikeState";
         break;
		  
		 case GameState.StrikingState:  
		   timeOut = false;
		   currentState = GameState.StrikeState;
		   gameState = "StrikeState";
		   StartCoroutine (WaitDelay());
		 break;
			 
		case GameState.StrikeState:
	     if(timeOut)
		 {
		   WaitResult();
		 }
		break;
		
	 }//END_SWITCH
	  	
		  
    }
	
	 public IEnumerator WaitDelay()
	{
		if (waitingDelay)
		{
			yield break;
		}
	
		waitingDelay = true;

		yield return new WaitForSeconds(4f);

		timeOut = true;
		
		waitingDelay = false;
	}
	
	
	void WaitResult()
	{
	
	   int cont = 0;
	   bool allStopped = true;
	   gameState = "waiting result";
	   foreach(KeyValuePair<int, SnookerBallController> entry in balls)
	   {
				
		 if (entry.Value.gameObject!=null&&!entry.Value.stoped )
		 {
			allStopped = false;
			break;
		 }
	   }
		
	   if(allStopped)
	   {
		 currentState = GameState.WaitingForStrikeState;
		 gameState = "WaitingForStrikeState";	  
		 PoolNetworkManager.instance.EmitChangeTurn(); 
	   }
		    
	}
	
	/// <summary>
	/// Resets the game.
	/// </summary>
	public void ResetGame()
	{
       
	   PoolCanvasManager.instance.ShowMessage("YOU WIN!!!");

	   PoolCanvasManager.instance.PlayAudio (PoolCanvasManager.instance.victorySound);

	   PoolCanvasManager.instance.OpenScreen(0);

	   PoolNetworkManager.instance.SetPlayerType("none");

	   ClearGame();
	  
	}

	//reset the game for the losing player
	public void ResetGameForLoserPlayer()
	{
	   PoolCanvasManager.instance.ShowMessage("YOU LOSE :(");
	 
	   PoolCanvasManager.instance.PlayAudio (PoolCanvasManager.instance.fallSound);

	   PoolCanvasManager.instance.OpenScreen(0);

	   ClearGame();

	   PoolNetworkManager.instance.SetPlayerType("none");
	}
	
	void ClearGame()
	{
	    foreach(KeyValuePair<int, SnookerBallController> entry in balls)
		{
			Destroy(entry.Value.gameObject);
		}
		foreach(KeyValuePair<int, NetworkBall> entry in netBalls)
		{
			Destroy(entry.Value.gameObject);
		}
		balls.Clear ();
		netBalls.Clear ();
		DestroyCue();
	    DestroyNetworkCue();
		
	}
	
	public void PlayPoolBallHitSound()
	{
		GetComponent<AudioSource>().PlayOneShot(poolballhit);
	}
	public void PlayPocketSound()
	{
		GetComponent<AudioSource>().PlayOneShot(pocketSound);
	}
	public void PlayBallColisionSound()
	{
		GetComponent<AudioSource>().PlayOneShot(ballColisionSound);
	}
	
}

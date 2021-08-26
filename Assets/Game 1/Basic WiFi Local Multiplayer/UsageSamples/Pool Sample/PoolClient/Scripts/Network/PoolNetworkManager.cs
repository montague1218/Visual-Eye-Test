using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UDPCore;
using PoolServerModule;

public class PoolNetworkManager : MonoBehaviour {

    //from UDP Core API
	public UDPComponent udpClient;

	//Variable that defines ":" character as separator
	static private readonly char[] Delimiter = new char[] {':'};

	//useful for any gameObject to access this class without the need of instances her or you declare her
	public static PoolNetworkManager instance;

	//flag which is determined the player is logged in the arena
	public bool onLogged = false;

	//store localPlayer
	public GameObject myPlayer;

	//local player id
	public string myId = string.Empty;
	
	//local player id
	public string local_player_id;

	public int serverPort = 3310;

	public int clientPort = 3000;

	public bool waitingAnswer;

	public bool serverFound;

	public bool waitingSearch;

	public List<string> _localAddresses { get; private set; }

	public enum PlayerType { NONE,SOLID,STRIPE}; 

	public PlayerType playerType;
	
	public bool myTurn;
	
	public string myType;
	
	public bool startedGame;
	
	public bool serverAlreadyStarted;
	
	// Use this for initialization
	void Start () {
	
	 // if don't exist an instance of this class
	 if (instance == null) {

		//it doesn't destroy the object, if other scene be loaded
		DontDestroyOnLoad (this.gameObject);

		instance = this;// define the class as a static variable
		
		udpClient = gameObject.GetComponent<UDPComponent>();
		
		//find any  server in others hosts
		ConnectToUDPServer();	
	 }
	 else
	 {
		//it destroys the class if already other class exists
		Destroy(this.gameObject);
	 }
		
	}
	
	
	/// <summary>
	/// Connect client to PoolServer.cs
	/// </summary>
	public void ConnectToUDPServer()
	{


		if (udpClient.GetServerIP () != string.Empty) {

		    int randomPort = UnityEngine.Random.Range(3001, 3310);
			//connect to PoolServer
			udpClient.connect (udpClient.GetServerIP (), serverPort, randomPort);

			udpClient.On ("PONG", OnPrintPongMsg);

			udpClient.On ("JOIN_SUCCESS", OnJoinGame);
			
			udpClient.On("START_GAME", OnStartGame);
			
			udpClient.On("CHANGE_TURN", OnChangeTurn);
			
			udpClient.On ("UPDATE_BALL_POS", OnUpdateBallPosition);
			
			udpClient.On ("UPDATE_CUE_POS_AND_ROT", OnUpdateCuePosAndRot);
			
			udpClient.On ("UPDATE_CUE_FORCE", OnUpdateCueForce);
			
			udpClient.On ("UPDATE_DESTROY_BALL", OnDestroyBall);
			
			udpClient.On("GAME_OVER", OnGameOver);
			
			udpClient.On ("USER_DISCONNECTED", OnUserDisconnected);

		}


	}

	void Update()
	{

		// if there is no wifi network
		if (udpClient.noNetwork) {

		
			PoolCanvasManager.instance.txtSearchServerStatus.text = "Please Connect to Wifi Hotspot";

			serverFound = false;

			PoolCanvasManager.instance.ShowLoadingImg ();
		}


		//if it was not found a server
		if (!serverFound) {

			PoolCanvasManager.instance.txtSearchServerStatus.text = string.Empty;

			// if there is a wifi connection but the server has not been started
			if (!udpClient.noNetwork) {
				
				//PoolCanvasManager.instance.txtSearchServerStatus.text = "Please start the server ";

			}
			else
			{
				
				PoolCanvasManager.instance.txtSearchServerStatus.text = "Please Connect to Wifi Hotspot ";
			}
		
			// start routine to detect a server on wifi network
			StartCoroutine ("PingPong");
		}
		//found server
		else
		{

		}

	}

	/// <summary>
	/// corroutine called  of times in times to send a ping to the server
	/// </summary>
	/// <returns>The pong.</returns>
	private IEnumerator PingPong()
	{

		if (waitingSearch)
		{
			yield break;
		}

		waitingSearch = true;

		//sends a ping to server
		EmitPing ();
		
		// wait 1 seconds and continue
		yield return new WaitForSeconds(1);
		
		waitingSearch = false;

	}



	//it generates a random id for the local player
	public string generateID()
	{
		string id = Guid.NewGuid().ToString("N");

		//reduces the size of the id
		id = id.Remove (id.Length - 15);

		return id;
	}

	/// <summary>
	///  receives an answer of the server.
	/// from  void OnReceivePing(string [] pack,IPEndPoint anyIP ) in server
	/// </summary>
	public void OnPrintPongMsg(UDPEvent data)
	{

		/*
		 * data.pack[0]= CALLBACK_NAME: "PONG"
		 * data.pack[1]= "pong!!!!"
		*/

		Debug.Log("receive pong");
		
		serverFound = true;
		
		if(serverAlreadyStarted)
		{
		   EmitJoinGame();
		   serverAlreadyStarted = false;
		}

		//arrow the located text in the inferior part of the game screen
		//PoolCanvasManager.instance.txtSearchServerStatus.text = "------- server is running -------";

	}

	// <summary>
	/// sends ping message to PoolServer.
	///     case "PING":
	///     OnReceivePing(pack,anyIP);
	///     break;
	/// take a look in PoolServer.cs script
	/// </summary>
	public void EmitPing() {

		//hash table <key, value>	
		Dictionary<string, string> data = new Dictionary<string, string>();

		//JSON package
		data["callback_name"] = "PING";

		//store "ping!!!" message in msg field
		data["msg"] = "ping!!!!";

		//The Emit method sends the mapped callback name to  the server
		udpClient.EmitToServer (data["callback_name"] ,data["msg"]);
		

	}
	

	/// <summary>
	/// Emits the join game to PoolServer.
	/// case "JOIN_GAME":
	///   OnReceiveJoinGame(pack,anyIP);
	///  break;
	/// take a look in PoolServer.cs script
	/// </summary>
	public void EmitJoinGame()
	{
	
	  try{
	  
		// check if there is a wifi connection
		if (!udpClient.noNetwork) {
		
			// check if there is a server running
		   if (serverFound) {
		
		      Dictionary<string, string> data = new Dictionary<string, string>();

		      data["callback_name"] = "JOIN_GAME";
		
		      //it is already verified an id was generated
		      if (myId.Equals (string.Empty)) {

			    myId = generateID ();
               
			    data ["player_id"] = myId;
		      }
		      else
		      {
			    data ["player_id"] = myId;
		      }
		    
		      string msg = data["player_id"];
		     
		      udpClient.EmitToServer (data["callback_name"] ,msg);
		    }
			
			else
			{
			   serverAlreadyStarted = true;
				   
			   PoolServer.instance. CreateServer();
				
			}
			
		}
		
		else
		{
		  PoolCanvasManager.instance.ShowAlertDialog("Please Connect to Wifi Hotspot");
		}
		
     }
     catch ( Exception e ){

       PoolCanvasManager.instance.txtLog.text =  e.ToString();	
     }
	}


	/// <summary>
	/// Raises the join game event from PoolServer.
	/// only the first player to connect gets this feedback from the server
	/// </summary>
	/// <param name="data">Data.</param>
	void OnJoinGame(UDPEvent data)
	{
		Debug.Log ("\n joining ...\n");

		// open game screen only for the first player, as the second has not logged in yet
		PoolCanvasManager.instance.OpenScreen(1);
		
		// set solid to the first player to connect
		SetPlayerType("solid");
		 
		PoolCanvasManager.instance.txtFooter .text = "connected! \n Waiting for another player";

		Debug.Log ("\n first player SOLID joined...\n");

	}


	/// <summary>
	/// Raises the start game event.
	/// both players receive this response from the server
	/// </summary>
	/// <param name="data">Data.</param>
	void OnStartGame(UDPEvent data)
	{
		Debug.Log ("\n game is runing...\n");
		

		startedGame = true;
		
		if (udpClient.serverRunning) {
	
		   PoolGameManager.instance.StartGame();
		  
		}
		else
		{
		    PoolGameManager.instance.SpawnNetworkCueBall();
			PoolGameManager.instance.SpawnNetworkBalls();
		}


		// check if it's the first player to connect
		if(GetPlayerType().Equals("solid"))
		{
		  // define as first to play	
		  myTurn = true;
		  PoolCanvasManager.instance.ShowMessage("Your balls are Solid");
		  PoolCanvasManager.instance.SetUIBallsSlots("solid");
		  PoolCanvasManager.instance.txtFooter .text = "Your move";
		  PoolGameManager.instance.SpawnCue();
		  PoolGameManager.instance.cue.GetComponent<CueController>().SetState("aim");
		}
		else// if you are the second player
		{

		   myTurn = false;
		   SetPlayerType("stripe");
		   PoolCanvasManager.instance.ShowMessage("Your balls are Stripe");
		   PoolCanvasManager.instance.SetUIBallsSlots("stripe");
		   PoolGameManager.instance.SpawnNetworkCue();
		   PoolCanvasManager.instance.txtFooter.text = "Opponent move";
           PoolCanvasManager.instance.OpenScreen(1);

			
		}

		Debug.Log ("\n game loaded...\n");

	}


	/// <summary>
	/// Emits change turn event to PoolServer
	/// </summary>
	public void EmitChangeTurn()
	{
		Dictionary<string, string> data = new Dictionary<string, string>();//pacote JSON

		data["callback_name"] = "UPDATE_CHANGE_TURN";
		
		data["player_id"] = myId;
		
		data["player_type"] = GetPlayerType();
		
		string msg = data["player_id"];
	
		//sends to the server through socket UDP the jo package 
		udpClient.EmitToServer (data ["callback_name"], msg);
		
		Debug.Log("change turn sended");

	}



	/// <summary>
	/// updates change turn event with information from PoolServer
	/// </summary>

	void OnChangeTurn(UDPEvent data)
	{
	
	    /*
		 * data.data.pack[0] = CALLBACK_NAME: "CHANGE_TURN" from server
		 * data.data.pack[1] = id of the opponent who made the last move
		*/

		// how the server message is transmitted to both players,
		// we should check if we are the next player to play, message target
		//data.pack[1] stores the id of the player who finished his move
		
		Debug.Log("receive change turn");
		
		
		if(data.pack[1].Equals(myId))
		{
		   PoolCanvasManager.instance.txtFooter.text = "Your move";
		   PoolGameManager.instance.SpawnCue();
		   PoolGameManager.instance.cue.GetComponent<CueController>().SetState("aim");
		   PoolGameManager.instance.HideNetworkCue();
		   myTurn = true;
		}
		else
		{
		   PoolGameManager.instance.HideCue();
		   PoolGameManager.instance.SpawnNetworkCue();
		   PoolCanvasManager.instance.txtFooter.text = "Opponent move";
		   myTurn = false;
			
		}
	
		
	}
	
	/// <summary>
	/// Emits the local ball position to server.
	/// </summary>
	public void EmitBallPosition(int id, Vector3 _pos,Quaternion _rot)
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data["callback_name"] = "SEND_BALL_POS";

		data["id"] = myId.ToString();
		
		data["ball_id"] = id.ToString();

		Vector3 position = new Vector3( _pos.x,_pos.y,_pos.z );

		data["position"] = position.z+":"+position.x;
		
		data["rotation"] =  _rot.x+":"+_rot.y+":"+_rot.z+":"+_rot.w;
	
	   
		//send the position point to server
		string msg = data["id"]+":"+data["ball_id"]+":"+data["position"]+":"+data["rotation"];

		//sends to the server through socket UDP the jo package 
		udpClient.EmitToServer (data["callback_name"] ,msg);

	}

	/// <summary>
	///  Update the ball position to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdateBallPosition(UDPEvent data)
	{

		/*
		 * data.pack[0] = UPDATE_MOVE_BALL
		 * data.pack[1] = id (ball id)
		 * data.pack[2] = position.x
		 * data.pack[3] = position.y
		 * data.pack[4] = position.z
		*/

		//it reduces to zero the accountant meaning that answer of the server exists to this moment
		
		if (PoolGameManager.instance.netBalls [int.Parse(data.pack [1])] != null) {
		
		
			NetworkBall ball = PoolGameManager.instance.netBalls [int.Parse(data.pack [1])];
		
		 if(data.pack [4].ToLower().Contains(','))
		 {
		    CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ",";
			
			
			Vector4 rot = new Vector4 (
				             float.Parse (data.pack [4],NumberStyles.Any,ci), float.Parse (data.pack [5],NumberStyles.Any,ci), 
							 float.Parse (data.pack [6],NumberStyles.Any,ci),
				             float.Parse (data.pack [7],NumberStyles.Any,ci));// atualiza a posicao
			
			
			ball.UpdatePosAndRot (new Vector3 (
				float.Parse ( data.pack [3],NumberStyles.Any,ci), 0,
				float.Parse ( data.pack [2],NumberStyles.Any,ci)),new Quaternion (rot.x, rot.y, rot.z, rot.w));
		 }
		 else
		 {
		    CultureInfo ci2 = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci2.NumberFormat.CurrencyDecimalSeparator = ".";
			
			
			Vector4 rot = new Vector4 (
				             float.Parse (data.pack [4],NumberStyles.Any,ci2), float.Parse (data.pack [5],NumberStyles.Any,ci2), 
							 float.Parse (data.pack [6],NumberStyles.Any,ci2),
				             float.Parse (data.pack [7],NumberStyles.Any,ci2));// atualiza a posicao
			
			
			ball.UpdatePosAndRot (new Vector3 (
				float.Parse ( data.pack [3],NumberStyles.Any,ci2), 0,
				float.Parse ( data.pack [2],NumberStyles.Any,ci2)),new Quaternion (rot.x, rot.y, rot.z, rot.w));
		 }//END_ELSE
			
	   }//END_IF
		 
	   }
	
	/// <summary>
	/// Emits the cue position and rotation to server.
	/// </summary>
	//responsible method for transmitting to the server the movement of the player associated to this client
	public void EmitCuePosAndRot(Vector3 _pos,Quaternion _rot)
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data["callback_name"] = "SEND_POS_AND_ROT";

		data["local_player_id"] = myId;

		Vector3 position = new Vector3( _pos.x,_pos.y,_pos.z );

		data["position"] = position.x+":"+position.z;
		
		data["rotation"] =  _rot.x+":"+_rot.y+":"+_rot.z+":"+_rot.w;
		

		//send the player id, position and rotation to server
		string msg = data["local_player_id"]+":"+data["position"]+":"+data["rotation"];

		//sends to the server through socket UDP the jo package 
		udpClient.EmitToServer (data["callback_name"] ,msg);

	}
	
	
	/// <summary>
	/// Update the network player cue position and rotation to local player.
	/// </summary>
	void OnUpdateCuePosAndRot(UDPEvent data)
	{
		
		/*
		 * data.pack[0] = UPDATE_CUE_POS_AND_ROT
		 * data.pack[1] = id (network player id)
		 * data.pack[2] = rotation.x
		 * data.pack[3] = rotation.y
		 * data.pack[4] = rotation.z
		 * data.pack[5] = rotation.w
		*/

		
		if (PoolGameManager.instance.networkCue!= null) {
		
		  NetworkCue netCue = PoolGameManager.instance.networkCue.GetComponent<NetworkCue>();
		
		 if(data.pack [1].ToLower().Contains(','))
		 {
		    CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ",";
			
			//update with the new position
			netCue.UpdatePosition (new Vector3 (
				float.Parse (data.pack [1],NumberStyles.Any,ci), netCue.gameObject.transform.position.y, float.Parse (data.pack [2],NumberStyles.Any,ci)));

		
		    Vector4 rot = new Vector4 (
				             float.Parse (data.pack [3],NumberStyles.Any,ci), float.Parse (data.pack [4],NumberStyles.Any,ci),
							 float.Parse (data.pack [5],NumberStyles.Any,ci),
				             float.Parse (data.pack [6],NumberStyles.Any,ci));// atualiza a posicao
			netCue.UpdateRotation (new Quaternion (rot.x, rot.y, rot.z, rot.w));
		 }
		 else
		 {
		   CultureInfo ci2 = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci2.NumberFormat.CurrencyDecimalSeparator = ".";
			
			
			
			//update with the new position
			netCue.UpdatePosition (new Vector3 (
				float.Parse (data.pack [1],NumberStyles.Any,ci2), netCue.gameObject.transform.position.y, 
				float.Parse (data.pack [2],NumberStyles.Any,ci2)));

		
		    Vector4 rot = new Vector4 (
				             float.Parse (data.pack [3],NumberStyles.Any,ci2), float.Parse (data.pack [4],NumberStyles.Any,ci2),
							 float.Parse (data.pack [5],NumberStyles.Any,ci2),
				             float.Parse (data.pack [6],NumberStyles.Any,ci2));// atualiza a posicao
			netCue.UpdateRotation (new Quaternion (rot.x, rot.y, rot.z, rot.w));
		 }
		
			
		}
	

	}

	
	/// <summary>
	/// Emits the local player cue force to server.
	/// </summary>
	public void EmitCueForce(float _force)
	{
	    Debug.Log("try to emit cue force");
	    PoolGameManager.instance.PlayPoolBallHitSound();
		 
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data["callback_name"] = "SEND_CUE_FORCE";

		data["local_player_id"] = myId;

		data["force"] = _force.ToString();
		
		//send the position point to server
		string msg = data["local_player_id"]+":"+data["force"];

		//sends to the server through socket UDP the jo package 
		udpClient.EmitToServer (data["callback_name"] ,msg);

		 Debug.Log(" cue force sended");
	

	}
	
	/// <summary>
	///  Update the network player cue Force to local player.
	/// </summary>
	void OnUpdateCueForce(UDPEvent data)
	{

		/*
		 * data.pack[0] = UPDATE_CUE_FORCE
		 * data.pack[1] = force
		*/
        try{

		Debug.Log("client received: "+data.pack [1]);
		PoolGameManager.instance.PlayPoolBallHitSound();
		 if(data.pack [1].ToLower().Contains(','))
		 {
		  	CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ",";
			
	        PoolGameManager.instance.networkSliderValue = float.Parse(data.pack [1],NumberStyles.Any,ci);
		 }
		 else
		 {
		    CultureInfo ci2 = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci2.NumberFormat.CurrencyDecimalSeparator = ".";
			
		    PoolGameManager.instance.networkSliderValue = float.Parse(data.pack [1],NumberStyles.Any,ci2);
		 }
		
		
		 try{
		 
		  PoolGameManager.instance.Strike();
		}
		 catch ( Exception e ){

          Debug.LogError(e.ToString());	
        }
		}
		catch(Exception e)
		{
            Debug.LogError(e.ToString());	
		}
		

	}
	
	
	public void EmitDestroyBall(int ball_id)
	{
		Dictionary<string, string> data = new Dictionary<string, string>();//pacote JSON

		PoolCanvasManager.instance.txtFooter.text = " ";
				
		data["callback_name"] = "DESTROY_BALL";
		
		data["player_id"] = myId;
		
		data["ball_id"] = ball_id.ToString();

		string msg = data["player_id"]+":"+data["ball_id"];

		//sends to the server through socket UDP the msg package 
		udpClient.EmitToServer (data ["callback_name"], msg);

	}

	void OnDestroyBall(UDPEvent data)
	{
	
	    /*
		 * data.data.pack[0] = CALLBACK_NAME: "UPDATE_DESTROY_BALL"
		 * data.data.pack[1] = ball_id
		
		*/
		  
	    if(!udpClient.serverRunning)
	    {
	
	      PoolGameManager.instance.PlayPocketSound();
	   
	      int ball_id = -1;
	  
	      if (PoolGameManager.instance.netBalls [int.Parse(data.pack [1])] != null) {
	  
	         ball_id = int.Parse(data.pack [1]);
		
			//find network ball
			NetworkBall ball = PoolGameManager.instance.netBalls [int.Parse(data.pack [1])];
			
			PoolGameManager.instance.BallPocketed(ball.id);
			
			Destroy(ball.gameObject);
			
		 }
			
		if(ball_id!=-1)
		{
		 PoolGameManager.instance.netBalls.Remove (ball_id);
		}
		
	}//END_IF
	
	PoolCanvasManager.instance.HideUIBallSlot(int.Parse(data.pack [1]));
			
		
	}


	/// <summary>
	/// Send a message to the server to notify you that the next player has lost the game.
	/// </summary>
	public void EmitGameOver()
	{
		Dictionary<string, string> data = new Dictionary<string, string>();

		myTurn = false;

		PoolCanvasManager.instance.txtFooter.text = " ";
			
		
		data["callback_name"] = "UPDATE_GAME_OVER";
		
		data["player_id"] = myId;

		string msg = data["player_id"];

	
		//sends to the server through socket UDP the msg package 
		udpClient.EmitToServer (data ["callback_name"], msg);

	}


	void OnGameOver(UDPEvent data)
	{
	
	    /*
		 * data.data.pack[0] = CALLBACK_NAME: "GAME_OVER" from server
		 * data.data.pack[1] = player_id
		
		*/

		// how the server message is transmitted to both players,
		// we should check if we are the next player to play, the loser
		//data.pack[1] stores the id of the player who won the match
    
		if(GetPlayerType().Equals(data.pack[1]))
		{
		   PoolGameManager.instance.ResetGame();
		}
		else
		{
		 PoolGameManager.instance.ResetGameForLoserPlayer();
		}
		
		myTurn = false;
	
	}


	/// <summary>
	/// Emits the disconnect to server
	/// </summary>
	void EmitDisconnect()
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string> ();

		data ["callback_name"] = "disconnect";

		data ["local_player_id"] = myId;

		if (udpClient.serverRunning) {

			data ["isMasterServer"] = "true";
		} else {
			data ["isMasterServer"] = "false";
		}


		string msg = data ["local_player_id"]+":"+data ["isMasterServer"];

		Debug.Log ("emit disconnect");

		udpClient.EmitToServer (data ["callback_name"], msg);

		

		if (udpClient != null) {

			udpClient.disconnect ();


		}
	}

	/// <summary>
	/// inform the local player to destroy offline network player
	/// </summary>
	void OnUserDisconnected(UDPEvent data )
	{

		/*
		 * data.pack[0]  = USER_DISCONNECTED
		 * data.pack[1] = id (network player id)
		 * data.pack[2] = isMasterServer
		*/
		Debug.Log ("disconnect!");

		// check if the disconnected player was the master server
		if (bool.Parse (data.pack [2])) {


			RestartGame ();
		}
		else
		{

			BoardManager.instance.ResetGameForWOPlayer();
		
		     myTurn = false;

		}


	}

	public void RestartGame()
	{
		
	    serverFound = false;

		PoolCanvasManager.instance.OpenScreen (0);

	}


	void CloseApplication()
	{

		if (udpClient != null) {

			EmitDisconnect ();

			udpClient.disconnect ();

		}
	}


	void OnApplicationQuit() {

		Debug.Log("Application ending after " + Time.time + " seconds");

		CloseApplication ();

	}


	public string GetPlayerType()
	{
		switch (playerType) {

		case PlayerType.NONE:
			return "none";
		break;
		case PlayerType.SOLID:
			return "solid";
		break;
		case PlayerType.STRIPE:
			return "stripe";
		break;
		
		}
		return string.Empty;
	}


	/// <summary>
	/// Sets the type of the user.
	/// </summary>
	/// <param name="_userType">User type.</param>
	public void SetPlayerType(string _playerType)
	{
		switch (_playerType) {

		case "none":
			playerType = PlayerType.NONE;	
		break;
		case "solid":
			playerType = PlayerType.SOLID;	
		break;
		case "stripe":
			playerType = PlayerType.STRIPE;	
		break;
		}
	}
	
	
	
}

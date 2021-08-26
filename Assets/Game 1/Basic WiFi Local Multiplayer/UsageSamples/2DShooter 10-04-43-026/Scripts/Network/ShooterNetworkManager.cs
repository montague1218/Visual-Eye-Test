using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using  UDPCore;



public class ShooterNetworkManager : MonoBehaviour
{
   
    //from UDP Socket API
	private UDPComponent udpClient;


	//Variable that defines comma character as separator
	static private readonly char[] Delimiter = new char[] {':'};

	//useful for any gameObject to access this class without the need of instances her or you declare her
	public static ShooterNetworkManager instance;

	//flag which is determined the player is logged in the arena
	public bool onLogged = false;

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

	public enum PlayerType { NONE,GIRL,BOY}; 

	public PlayerType playerType;
	
	public bool myTurn;
	
	public string myType;
	
	public bool startedGame;
	
	public bool serverAlreadyStarted;
	
	//store localPlayer
	public GameObject myPlayer;
	
	//store all players in game
	public Dictionary<string, Player2DManager> networkPlayers = new Dictionary<string, Player2DManager>();
	
	ArrayList playersNames;
	
	//store the local players' models
	public GameObject[] localPlayersPrefabs;
	

	//store the networkplayers' models
	public GameObject[] networkPlayerPrefabs;
	
	public GameObject txtPlayerNamePref;
	
	//stores the spawn points 
	public Transform[] spawnPoints;
	
	public Camera2DFollow cameraFollow;
	
	public bool isGameOver;

	
	
	// Use this for initialization
	void Start () {
	
	 // if don't exist an instance of this class
	 if (instance == null) {

		//it doesn't destroy the object, if other scene be loaded
		DontDestroyOnLoad (this.gameObject);

		instance = this;// define the class as a static variable
		
		//instantiates the UDPComponent library in the udpClient variable
		udpClient = gameObject.GetComponent<UDPComponent>();
		
		playersNames = new ArrayList();
		
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
	/// Connect client to ShooterServer.cs
	/// </summary>
	public void ConnectToUDPServer()
	{


		if (udpClient.GetServerIP () != string.Empty) {

		    //generates a random port between 3001 and 3310
		    int randomPort = UnityEngine.Random.Range(3001, 3310);
			
			//connect to ShooterServer
			udpClient.connect (udpClient.GetServerIP (), serverPort, randomPort);

			//receives a "PONG" notification from the server
			udpClient.On ("PONG", OnPrintPongMsg);

			//receives a "JOIN_SUCCESS" notification from the server
			udpClient.On ("JOIN_SUCCESS", OnJoinGame);

			//receives a "JOIN_SUCCESS" notification from the server
			udpClient.On ("RESPAWN_PLAYER", OnRespawnPlayer);

			//receives a "SPAWN_PLAYER" notification from the server
			udpClient.On ("SPAWN_PLAYER", OnSpawnPlayer);
			
			//receives a "UPDATE_POS_AND_ROT" notification from the server
			udpClient.On ("UPDATE_POS_AND_ROT",OnUpdatePosAndRot);
			
			//receives a "UPDATE_JUMP" notification from the server
			udpClient.On ("UPDATE_JUMP",OnUpdateJump);
			
			//receives a "UPDATE_PLAYER_DAMAGE" notification from the server
			udpClient.On ("UPDATE_PLAYER_DAMAGE",OnUpdatePlayerDamage);
			
			//receives a "GAME_OVER" notification from the server
			udpClient.On ("GAME_OVER",OnGameOver);
			
			//receives a "UPDATE_PLAYER_ANIMATOR" notification from the server
			udpClient.On ("UPDATE_PLAYER_ANIMATOR",OnUpdateAnim);
			
			//receives a "USER_DISCONNECTED" notification from the server
			udpClient.On ("USER_DISCONNECTED",OnUserDisconnected);
			

			
		
		}


	}

	void Update()
	{

		// if there is no wifi network
		if (udpClient.noNetwork) {

		
			ShooterCanvasManager.instance.txtSearchServerStatus.text = "Please Connect to Wifi Hotspot";

			serverFound = false;

			ShooterCanvasManager.instance.ShowLoadingImg ();
		}


		//if it was not found a server
		if (!serverFound) {

			ShooterCanvasManager.instance.txtSearchServerStatus.text = string.Empty;

			// if there is a wifi connection but the server has not been started
			if (!udpClient.noNetwork) {
				
				//PoolCanvasManager.instance.txtSearchServerStatus.text = "Please start the server ";

			}
			else
			{
				
				ShooterCanvasManager.instance.txtSearchServerStatus.text = "Please Connect to Wifi Hotspot ";
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
		ShooterCanvasManager.instance.txtSearchServerStatus.text = "------- server is running -------";

	}

	// <summary>
	/// sends ping message to UDPServer.
	///     case "PING":
	///     OnReceivePing(pack,anyIP);
	///     break;
	/// take a look in TicTacttoeServer.cs script
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
	/// Emits the join game to Server.
	/// case "JOIN_GAME":
	///   OnReceiveJoinGame(pack,anyIP);
	///  break;
	/// take a look in Server.cs script
	/// </summary>
	public void EmitJoinGame()
	{
	
	  try{
	  
		
		//checks if the player was already logged in and suffered a game over, or if you just opened the application
		if (!isGameOver) {
		
		 // check if there is a wifi connection
		if (!udpClient.noNetwork) {
		
			// check if there is a server running
		   if (serverFound) {
		
		      Dictionary<string, string> data = new Dictionary<string, string>();//pacote JSON

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
		    
			  data["player_name"] = ShooterCanvasManager.instance.inputLogin.text;
			  
			  data["avatar"] = ButtonChooseManager.instance.currentAvatar.ToString();

		      string msg = data["player_id"]+":"+data["player_name"]+":"+data["avatar"];
		     
		      udpClient.EmitToServer (data["callback_name"] ,msg);
		    }
			
			else
			{
			   serverAlreadyStarted = true;
				   
			   ShooterServer.instance.CreateServer();
				
			}
			
		}
		
		else
		{
		  ShooterCanvasManager.instance.ShowAlertDialog("Please Connect to Wifi Hotspot");
		}
			
		}
		else
		{ //if the player was already in play and suffered GameOver
		  
		   Dictionary<string, string> data = new Dictionary<string, string>();

		   data["callback_name"] = "RESPAWN";
		
		   data["player_name"] = ShooterCanvasManager.instance.inputLogin.text;
			  
		   data["avatar"] = ButtonChooseManager.instance.currentAvatar.ToString();

		   string msg = myId+":"+data["player_name"]+":"+data["avatar"];
		     
		   udpClient.EmitToServer (data["callback_name"] ,msg);
		   
				
		}
		
     }
     catch ( Exception e ){

       ShooterCanvasManager.instance.txtLog.text =  e.ToString();	
     }
	}


	/// <summary>
	/// Raises the join game event from Server.
	/// only the first player to connect gets this feedback from the server
	/// </summary>
	/// <param name="data">Data.</param>
	void OnJoinGame(UDPEvent data)
	{
		Debug.Log ("\n joining ...\n");
		
		
		/*
		 * data.data.pack[0] = CALLBACK_NAME: "JOIN_SUCCESS" from server
		 * data.data.pack[1] = id (local player id)
		 * data.data.pack[2]= name (local player name)
		 * data.data.pack[3] = avatar
		*/

		Debug.Log("Login successful, joining game");
    try{

		if (!myPlayer) {

			// take a look in Player2DManager.cs script
			Player2DManager newPlayer;
			
			// newPlayer = GameObject.Instantiate( local player avatar or model, spawn position, spawn rotation)
			newPlayer = GameObject.Instantiate (localPlayersPrefabs [int.Parse(data.pack[3])],spawnPoints[int.Parse(data.pack[4])].position,
			Quaternion.identity).GetComponent<Player2DManager> ();
			  
			newPlayer.id = data.pack [1];

			//this is local player
			newPlayer.isLocalPlayer = true;

			//now local player online in the arena
			newPlayer.isOnline = true;
			
			newPlayer.gameObject.name = data.pack [1];
			  
			//puts the local player on the list
			networkPlayers [data.pack [1]] = newPlayer;

			myPlayer = networkPlayers [data.pack[1]].gameObject;
			
			cameraFollow.SetTarget(newPlayer.gameObject.transform);
			
			ShooterCanvasManager.instance.localPlayerImg.sprite =  ShooterCanvasManager.instance.
			spriteFacesPref[int.Parse(data.pack[3])].GetComponent<SpriteRenderer> ().sprite;
	
		    ShooterCanvasManager.instance.txtLocalPlayerName.text = data.pack[2];
				
		    ShooterCanvasManager.instance.txtLocalPlayerHealth.text = "100";
			  
			ShooterCanvasManager.instance.OpenScreen(1);
			  
			GameObject txtName = GameObject.Instantiate (txtPlayerNamePref,new Vector3(0f,0f,-0.1f), Quaternion.identity) as GameObject;
			txtName.name = data.pack[2];
			txtName.GetComponent<PlayerName> ().setName (data.pack[2]);
			txtName.GetComponent<Follow> ().SetTarget (newPlayer.gameObject.transform);
			
			playersNames.Add(txtName);
			
			Debug.Log("player instantiated");

			
			Debug.Log("player in game");
		}
	}
	catch(Exception e)
	{
		Debug.LogError(e.ToString());
	}

	}
	
	/// <summary>
	/// Raises the spawn player event.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnSpawnPlayer(UDPEvent data)
	{

		/*
		 * data.pack[0] = SPAWN_PLAYER
		 * data.pack[1] = id (network player id)
		 * data.pack[2]= name
		 * data.pack[3] = avatar
		*/
		
		
		Debug.Log ("\n spawning network player ...\n");

			bool alreadyExist = false;

			//verify all players to  prevents copies
			foreach(KeyValuePair<string, Player2DManager> entry in networkPlayers)
			{
				// same id found ,already exist!!! 
				if (entry.Value.id== data.pack [1])
				{
					alreadyExist = true;
				}
			}
			if (!alreadyExist) {

				Debug.Log("creating a new player");

				Player2DManager newPlayer;

				// newPlayer = GameObject.Instantiate( network player avatar or model, spawn position, spawn rotation)
				newPlayer =  GameObject.Instantiate (networkPlayerPrefabs [int.Parse(data.pack[3])],spawnPoints[int.Parse(data.pack[4])].position,
			  Quaternion.identity).GetComponent<Player2DManager> ();
			  
			 

				//it is not the local player
				newPlayer.isLocalPlayer = false;

				//network player online in the arena
				newPlayer.isOnline = true;

				newPlayer.gameObject.name = data.pack [1];

				//puts the local player on the list
				networkPlayers [data.pack [1]] = newPlayer;
				
				ShooterCanvasManager.instance.networkPlayerImg.sprite =  ShooterCanvasManager.instance.spriteFacesPref[int.Parse(data.pack[3])].GetComponent<SpriteRenderer> ().sprite;
	
		        ShooterCanvasManager.instance.txtNetworkPlayerName.text = data.pack[2];
				
		        ShooterCanvasManager.instance.txtNetworkPlayerHealth.text = "100";
				
			
				GameObject txtName = GameObject.Instantiate (txtPlayerNamePref,new Vector3(0f,0f,-0.1f), Quaternion.identity) as GameObject;
				txtName.name = data.pack[2];
				txtName.GetComponent<PlayerName> ().setName (data.pack[2]);
				txtName.GetComponent<Follow> ().SetTarget (newPlayer.gameObject.transform);
				
				playersNames.Add(txtName);
				
			}

	}

	/// <summary>
	/// method to handle notification that arrived from the server.
	/// </summary>
	/// <remarks>
	/// respawn local player .
	/// </remarks>
	/// <param name="data">received package from server.</param>
	void OnRespawnPlayer(UDPEvent data)
	{   
		/*
		 * data.pack[0] = RESPAWN_PLAYER
		 * data.pack[1] = id 
		 * data.pack[2]= name
		 * data.pack[3] = "position.x;position.y;position.z"
		 
		*/
		Debug.Log("respawn received");
        try{
		Debug.Log("Respawn successful, joining game");
		
		 ShooterCanvasManager.instance.OpenScreen(1);
		
		onLogged = true;
		
		isGameOver = false;

		if (myPlayer == null) {

			// take a look in Player2DManager.cs script
			Player2DManager newPlayer;
			
			// newPlayer = GameObject.Instantiate( local player avatar or model, spawn position, spawn rotation)
			newPlayer = GameObject.Instantiate (localPlayersPrefabs [int.Parse(data.pack[3])],
			spawnPoints[int.Parse(data.pack[4])].position,
			Quaternion.identity).GetComponent<Player2DManager> ();
			  
			newPlayer.id = data.pack [1];

			//this is local player
			newPlayer.isLocalPlayer = true;

			//now local player online in the arena
			newPlayer.isOnline = true;
			
			newPlayer.gameObject.name = data.pack [1];
			  
			//puts the local player on the list
			networkPlayers [data.pack [1]] = newPlayer;

			myPlayer = networkPlayers [data.pack[1]].gameObject;
			
			cameraFollow.SetTarget(newPlayer.gameObject.transform);
			
			ShooterCanvasManager.instance.localPlayerImg.sprite =  ShooterCanvasManager.instance.
			spriteFacesPref[int.Parse(data.pack[3])].GetComponent<SpriteRenderer> ().sprite;
	
		    ShooterCanvasManager.instance.txtLocalPlayerName.text = data.pack[2];
				
		    ShooterCanvasManager.instance.txtLocalPlayerHealth.text = "100";
			  
			ShooterCanvasManager.instance.OpenScreen(1);
			  
			GameObject txtName = GameObject.Instantiate (txtPlayerNamePref,new Vector3(0f,0f,-0.1f), Quaternion.identity) as GameObject;
			txtName.name = data.pack[2];
			txtName.GetComponent<PlayerName> ().setName (data.pack[2]);
			txtName.GetComponent<Follow> ().SetTarget (newPlayer.gameObject.transform);
			
			playersNames.Add(txtName);

		}//END_IF
		}//END_TRY
		catch(Exception e)
		{
		  Debug.LogError(e.ToString());
		}

	}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////PLAYER POSITION AND ROTATION UPDATES///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//send local player position and rotation to server
	public void EmitPosAndRot(string _data)
	{
	  //Identifies with the name "POS_AND_ROT", the notification to be transmitted to the server,
	  //and send to the server the player's position and rotation
	   udpClient.EmitToServer ("POS_AND_ROT" ,_data);
	}

	/// <summary>
	/// Update the network player position and rotation to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdatePosAndRot(UDPEvent data)
	{
		/*
		 * data.pack[0] = UPDATE_MOVE_AND_ROTATE
		 * data.pack[1] = id (network player id)
		 * data.pack[2] = "position.x;position.y;posiiton.z"
		 * data.pack[3] = "rotation.x; rotation.y; rotation.z; rotation.w"
		*/
		
		if (networkPlayers [data.pack [1]] != null)
		{
		  
			Player2DManager netPlayer = networkPlayers[data.pack[1]];
			
			

			//update with the new position
			netPlayer.UpdatePosition(UtilsClass.StringToVector3(data.pack[2]));

			Vector4 rot = UtilsClass.StringToVector4(data.pack[3]);// convert string to Vector4

			//update new player rotation
			netPlayer.UpdateRotation(new Quaternion (rot.x,rot.y,rot.z,rot.w));
			

		}
	}
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////



////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////PLAYER JUMP UPDATES///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//send local player jump to server
	public void EmitJump()
	{
	  //hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();
		
		//Identifies with the name "JUMP", the notification to be transmitted to the server
		data["callback_name"] = "JUMP";

		string msg = myPlayer.GetComponent<Player2DManager>().id;

		
	   udpClient.EmitToServer (data["callback_name"] ,msg);
	}
	
	/// <summary>
	///  Update the network player jump to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdateJump(UDPEvent data)
	{
		/*
		 * data.pack[0] = UPDATE_JUMP
		 * data.pack[1] = id (network player id)
		
		*/
		
		//find network player by your id
		Player2DManager netPlayer = networkPlayers[data.pack[1]];

		//updates current animation
		netPlayer.UpdateJump();

	}
	
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////ANIMATION UPDATES/////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Emits the local player animation to Server.js.
	/// </summary>
	/// <param name="_animation">Animation.</param>
	public void EmitAnimation(string _animation)
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();
		
		//Identifies with the name "ANIMATION", the notification to be transmitted to the server
		data["callback_name"] = "ANIMATION";

		data["local_player_id"] = myPlayer.GetComponent<Player2DManager>().id;

		data ["animation"] = _animation;

		//message to be sent to the server concatenated by ":"
		string msg = data["local_player_id"]+":"+data ["animation"];

		//sends the current animation from the player to the server
		udpClient.EmitToServer (data["callback_name"] ,msg);

	}

	/// <summary>
	///  Update the network player animation to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdateAnim(UDPEvent data)
	{
		/*
		 * data.pack[0] = UPDATE_PLAYER_ANIMATOR
		 * data.pack[1] = id (network player id)
		 * data.pack[2] = animation (network player animation)
		*/
		
		//find network player by your id
		Player2DManager netPlayer = networkPlayers[data.pack[1]];

		//updates current animation
		netPlayer.UpdateAnimator(data.pack[2]);

	}
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////DAMAGE UPDATES/////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  public void EmitPlayerDamage(string _networkPlayerID)
  {
    //hash table <key, value>
	Dictionary<string, string> data = new Dictionary<string, string>();
		
	//Identifies with the name "DAMAGE", the notification to be transmitted to the server
	data["callback_name"] = "DAMAGE";

	data["local_player_id"] = _networkPlayerID;

	//message to be sent to the server concatenated by ":"
	string msg = data["local_player_id"];


	//sends the current animation from the player to the server
	udpClient.EmitToServer (data["callback_name"] ,msg);
	
  }
  
  /// <summary>
	///  Update the network player animation to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdatePlayerDamage(UDPEvent data)
	{
		/*
		 * data.pack[0] = UPDATE_PLAYER_DAMAGE
		 * data.pack[1] = id (network player id)
		
		*/
		
		//find network player by your id
		Player2DManager netPlayer = networkPlayers[data.pack[1]];

		if(networkPlayers[data.pack[1]].isLocalPlayer)
		{
		    ShooterCanvasManager.instance.txtLocalPlayerHealth.text = data.pack[2];
		}
		else
		{
		    ShooterCanvasManager.instance.txtNetworkPlayerHealth.text = data.pack[2];
		}
		//updates current animation
		netPlayer.UpdateAnimator("OnDamage");

	}
	
	
	  /// <summary>
	///  Update the network player animation to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnGameOver(UDPEvent data)
	{
		 /*
		 * data.data.pack[0] = CALLBACK_NAME: "GAME_OVER" from server
		 * data.data.pack[1] = looser player id
		
		*/
		
		isGameOver = true;
		
		//find network player by your id
		Player2DManager netPlayer = networkPlayers[data.pack[1]];

		if(networkPlayers[data.pack[1]].isLocalPlayer)
		{
		    ResetGameForLoserPlayer();
		}
		else
		{
		     ResetGame();
		}
		
		foreach(GameObject name in playersNames)
		{
		  Destroy(name);
		}
		
		playersNames.Clear();
		
	}
	
	void ResetGame()
	{
	    
		myPlayer = null;
		
		//send answer in broadcast
		foreach (KeyValuePair<string, Player2DManager> entry in networkPlayers) {

		  Destroy(entry.Value.gameObject);
		  
		}//END_FOREACH
		
		networkPlayers.Clear();
		
	
		ShooterCanvasManager.instance.ShowAlertDialog("YOU WIN!!!");
		
		ShooterCanvasManager.instance.txtLocalPlayerHealth.text = "100";
	
	    ShooterCanvasManager.instance.txtNetworkPlayerHealth.text = "100";
		
		ShooterCanvasManager.instance.OpenScreen(0);
		
		
	}
	
	void ResetGameForLoserPlayer()
	{
	    myPlayer = null;
		
		
		//send answer in broadcast
		foreach (KeyValuePair<string, Player2DManager> entry in networkPlayers) {

		  Destroy(entry.Value.gameObject);
		  
		}//END_FOREACH
		
		networkPlayers.Clear();
		
	
	    ShooterCanvasManager.instance.txtLocalPlayerHealth.text = "100";
	
	    ShooterCanvasManager.instance.txtNetworkPlayerHealth.text = "100";
		
		ShooterCanvasManager.instance.OpenScreen(0);
		
		ShooterCanvasManager.instance.ShowAlertDialog("YOU LOSE!!!");
	}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////




	void OnApplicationQuit() {

		Debug.Log("Application ending after " + Time.time + " seconds");

		EmitDisconnect ();
			
	}
	
	void EmitDisconnect()
	{
		if(myPlayer)
		{
			//hash table <key, value>
			Dictionary<string, string> data = new Dictionary<string, string>();

			//JSON package
			data["callback_name"] = "disconnect";

			data ["local_player_id"] = local_player_id;

			if (udpClient.serverRunning) {

				data ["isMasterServer"] = "true";
			}
			else 
			{
				data ["isMasterServer"] = "false";
			}
				
			//send the position point to server
			string msg = data["local_player_id"]+":"+data ["isMasterServer"];

			//Debug.Log ("emit disconnect");

			//we make four attempts of similar sending of preventing the loss of packages
			udpClient.EmitToServer (data["callback_name"] ,msg);

			udpClient.EmitToServer (data["callback_name"] ,msg);

			udpClient.EmitToServer (data["callback_name"] ,msg);

			udpClient.EmitToServer (data["callback_name"] ,msg);
		}

		if (udpClient != null) {

			udpClient.disconnect ();


		}
	}


	/// <summary>
	/// inform the local player to destroy offline network player
	/// </summary>
	/// <param name="_msg">Message.</param>
	//disconnect network player
	void OnUserDisconnected(UDPEvent data )
	{

		/*
		 * data.pack[0]  = USER_DISCONNECTED
		 * data.pack[1] = id (network player id)
		 * data.pack[2] = isMasterServer
		*/
		Debug.Log ("disconnect!");
		
        //if this instance is master serve
		if (bool.Parse (data.pack [2])) {
			isGameOver = true;
			
		}
		else
		{
		  serverFound = false;
		}
		
		foreach(GameObject name in playersNames)
		{
		  Destroy(name);
		}
		
		playersNames.Clear();
		
		ResetGame();


	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////HELPERS////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	Vector3 StringToVector3(string target ){

		Vector3 newVector;
		string[] newString = Regex.Split(target,";");
		newVector = new Vector3( float.Parse(newString[0]), float.Parse(newString[1]),float.Parse(newString[2]));

		return newVector;

	}
	
	Vector4 StringToVector4(string target ){

		Vector4 newVector;
		string[] newString = Regex.Split(target,";");
		newVector = new Vector4( float.Parse(newString[0]), float.Parse(newString[1]),float.Parse(newString[2]),float.Parse(newString[3]));

		return newVector;

	}
	
	string Vector3ToString(Vector3 vet ){

		return  vet.x+";"+vet.y+";"+vet.z;

	}
	
	string Vector4ToString(Vector4 vet ){

		return  vet.x+";"+vet.y+";"+vet.z+";"+vet.w;

	}
	
	public string GetPlayerType()
	{
		switch (playerType) {

		case PlayerType.NONE:
			return "none";
		break;
		case PlayerType.GIRL:
			return "girl";
		break;
		case PlayerType.BOY:
			return "boy";
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
		case "boy":
			playerType = PlayerType.BOY;	
		break;
		case "girl":
			playerType = PlayerType.GIRL;	
		break;
		}
	}
	
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


		
}

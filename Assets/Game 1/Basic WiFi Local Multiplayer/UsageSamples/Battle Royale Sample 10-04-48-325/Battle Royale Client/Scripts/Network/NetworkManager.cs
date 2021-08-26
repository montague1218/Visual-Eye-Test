using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using UDPCore;
using UDPServerModule;

public class NetworkManager : MonoBehaviour {

	//from UDP Socket API
	private UDPComponent udpClient;

	//Variable that defines comma character as separator
	static private readonly char[] Delimiter = new char[] {':'};

	//useful for any gameObject to access this class without the need of instances her or you declare her
	public static NetworkManager instance;

	//flag which is determined the player is logged in the arena
	public bool onLogged = false;

	//store localPlayer
	public GameObject myPlayer;

	//local player id
	public string myId = string.Empty;

	//local player id
	public string local_player_id;

	//store all players in game
	public Dictionary<string, PlayerManager> networkPlayers = new Dictionary<string, PlayerManager>();

	//store the local players' models
	public GameObject[] localPlayersPrefabs;

	//store the networkplayers' models
	public GameObject[] networkPlayerPrefabs;

	//stores the spawn points 
	public Transform[] spawnPoints;

	//camera prefab
	public GameObject camRigPref;

	public GameObject camRig;

	public int serverPort = 3310;
	
	public int clientPort = 3000;

	public bool tryJoinServer;

	public bool waitingAnswer;

	public bool serverFound;

	public bool waitingSearch;

	public bool gameIsRunning;

	public int maxReconnectTimes = 10;

	public int contTimes;

	public float maxTimeOut;

	public float timeOut;

	public List<string> _localAddresses { get; private set; }


	// Use this for initialization
	void Start () {

		// if don't exist an instance of this class
		if (instance == null) {

			//it doesn't destroy the object, if other scene be loaded
			DontDestroyOnLoad (this.gameObject);

			instance = this;// define the class as a static variable
			
			udpClient = gameObject.GetComponent<UDPComponent>();
			
			int randomPort = UnityEngine.Random.Range(3001, 3310);

			
		
			//find any  server in others hosts
			ConnectToUDPServer(serverPort, randomPort);

			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

			string address = string.Empty;

			string subAddress = string.Empty;

			_localAddresses = new List<string>();

		}
		else
		{
			//it destroys the class if already other class exists
			Destroy(this.gameObject);
		}
		
	}





	/// <summary>
	/// Connect client to any UDP server.
	/// </summary>
	public void ConnectToUDPServer(int _serverPort, int _clientPort)
	{


		if (udpClient.GetServerIP () != string.Empty) {

			//connect to udp server
			udpClient.connect (udpClient.GetServerIP (), _serverPort, _clientPort);

			//The On method in simplistic terms is used to map a method name to an annonymous function.
			udpClient.On ("PONG", OnPrintPongMsg);

			udpClient.On ("JOIN_SUCCESS", OnJoinGame);

			udpClient.On ("SPAWN_PLAYER", OnSpawnPlayer);

			udpClient.On ("UPDATE_POS_AND_ROT", OnUpdatePosAndRot);

			udpClient.On ("UPDATE_ATTACK",OnUpdateAttack);

			udpClient.On ("UPDATE_PLAYER_DAMAGE",OnUpdatePlayerDamage);

			udpClient.On ("UPDATE_PLAYER_ANIMATOR", OnUpdateAnim);

			udpClient.On ("USER_DISCONNECTED", OnUserDisconnected);

		
		}


	}

	void Update()
	{



		//if it was not found a server
		if (!serverFound) {
			
			//tries to obtain a "pong" of some local server
			StartCoroutine ("PingPong");
		}
		//found server
		else
		{
			//if the player is already in game
			if (gameIsRunning)
			{
				//maintain a connection with the server to detect disconnection
				StartCoroutine ("PingPong");


				/*************** verifies the disconnection of some player ***************/
				List<string> keys = new List<string> (networkPlayers.Keys);
			
				foreach (string key in keys) {

					if (networkPlayers.ContainsKey (key)) {

						if (networkPlayers [key] != null) {

							//increases the time of wait
							networkPlayers [key].timeOut += Time.deltaTime;

							//the client is verified exceeded the time limits of wait
							if (networkPlayers [key].timeOut >= maxTimeOut) {
						
							
								//destroy network player by your id
							//	Destroy (networkPlayers [key].gameObject);

								//remove from the dictionary
							//	networkPlayers.Remove (networkPlayers [key].id);
							
							}
						}
					}

				}//END_FOREACH
			/*************************************************************************/	

			}
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

		//important to verify the server it is connected
		if (gameIsRunning)
		{
			//number of pings sent to the server without answer
			contTimes++;
		}


		// wait 1 seconds and continue
		yield return new WaitForSeconds(1);

		//if contTimes arrived to the maximum value of attempts means that the server is not more answering or it disconnected
		if (contTimes > maxReconnectTimes )
		{
			contTimes = 0;

			//restarts the game so that a new server is created
			RestartGame ();
		}

		waitingSearch = false;

	}

	//function to help to detect flaw in the connection
	public IEnumerator WaitAnswer()
	{
		if (waitingAnswer)
		{
			yield break;
		}
	
		tryJoinServer = true;

		waitingAnswer = true;

		CanvasManager.instance.ShowLoadingImg ();

		yield return new WaitForSeconds(5f);

		CanvasManager.instance.CloseLoadingImg ();

		waitingAnswer = false;
	   
		//if true we lost the package the servant didn't answer
		//take a look in public void OnJoinGame(UDPEvent data) function
		if (tryJoinServer) {
			
			tryJoinServer = false;

			CanvasManager.instance.CloseLoadingImg();

		}


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

		serverFound = true;

		contTimes = 0;

		//arrow the located text in the inferior part of the game screen
		CanvasManager.instance.txtSearchServerStatus.text = "------- server is running -------";
	
	}




	// <summary>
	/// sends ping message to server.
	/// to  case "PING":
	///     OnReceivePing(pack,anyIP);
	///     break;
	/// take a look in UDPServer.cs script
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
	///  tries to put the player in game
	/// </summary>
	public void EmitJoin()
	{
		// verifies WiFi connection
		if (!udpClient.noNetwork) {


			if (serverFound) {

				//tries to put the player in game
				TryJoinServer ();
				
			}
			else
			{
				if (udpClient.serverRunning)
				{
					
					TryJoinServer ();
				} 
				else 
				{
					CanvasManager.instance.ShowAlertDialog ("PLEASE START THE SERVER");
				}
			}

		}//END_IF
		else
		{
			
			if (udpClient.noNetwork) {
				
				CanvasManager.instance.ShowAlertDialog ("PLEASE CONNECT TO ANY WIFI NETWORK");
			}

			else
			{
				if (serverFound) {

					TryJoinServer ();
				}

				else
				{
					CanvasManager.instance.ShowAlertDialog ("THERE NO ARE SERVER RUNNING ON NETWORK!");
				}

			}
		}

	}

	/// <summary>
	/// Tries the join server.
	///  case "JOIN":
	///  OnReceiveJoin(pack,anyIP);
	///  break;
	///  take a look in UDPServer.cs script
	/// </summary>
	public void TryJoinServer()
	{
		

		//hash table <key, value>	
		Dictionary<string, string> data = new Dictionary<string, string> ();

		data ["callback_name"] = "JOIN";//set up callback name


		data["player_name"] = CanvasManager.instance.inputLogin.text;

		//it is already verified an id was generated
		if (myId.Contains (string.Empty)) {

			myId = generateID ();

			data ["player_id"] = myId;
		}
		else
		{
			data ["player_id"] = myId;
		}

		//makes the draw of a point for the player to be spawn
		//int index = Random.Range (0, spawnPoints.Length);

		int index = 0;

	
		data["position"] = UtilsClass.Vector3ToString(spawnPoints[index].position);

	
		//send the position point to server
		string msg =  data["player_id"]+":"+data["player_name"]+ ":"+ data["position"];

		//sends to the server through socket UDP the jo package 
		udpClient.EmitToServer (data ["callback_name"], msg);

		//we waited for a time to verify the connection
		StartCoroutine (WaitAnswer ());
	}

	/// <summary>
	/// Joins the local player in game.
	/// </summary>
	/// <param name="_data">Data.</param>
	public void OnJoinGame(UDPEvent data)
	{

		/*
		 * data.data.pack[0] = CALLBACK_NAME: "JOIN_SUCCESS" from server
		 * data.data.pack[1] = id (local player id)
		 * data.data.pack[2]= name (local player name)
		 * data.data.pack[3] = position.x (local player position x)
		 * data.data.pack[4] = position.y (local player position ...)
		 * data.data.pack[5] = position.z
		 * data. data.pack[6] = rotation.x
		 * data.data.pack[7] = rotation.y
		 * data.data.pack[8] = rotation.z
		 * data.data.pack[9] = rotation.w
		*/

		Debug.Log("Login successful, joining game");


		if (!myPlayer) {

			// take a look in PlayerManager.cs script
			PlayerManager newPlayer;
			
			if(udpClient.serverRunning)
			{
			  // newPlayer = GameObject.Instantiate( local player avatar or model, spawn position, spawn rotation)
			  newPlayer = GameObject.Instantiate (localPlayersPrefabs [0],
			UtilsClass.StringToVector3(data.pack[3]),Quaternion.identity).GetComponent<PlayerManager> ();

			}
			else
			{
			  // newPlayer = GameObject.Instantiate( local player avatar or model, spawn position, spawn rotation)
			  newPlayer = GameObject.Instantiate (localPlayersPrefabs [0],
				UtilsClass.StringToVector3(data.pack[3]),Quaternion.identity).GetComponent<PlayerManager> ();

			}
			

			Debug.Log("player instantiated");

			newPlayer.id = data.pack [1];

			//this is local player
			newPlayer.isLocalPlayer = true;

			//now local player online in the arena
			newPlayer.isOnline = true;

			//set local player's 3D text with his name
			newPlayer.Set3DName(data.pack[2]);

			//puts the local player on the list
			networkPlayers [data.pack [1]] = newPlayer;

			myPlayer = networkPlayers [data.pack[1]].gameObject;

			local_player_id =  data.pack [1];

			//spawn cam
			camRig = GameObject.Instantiate (camRigPref, new Vector3 (0f, 0f, 0f), Quaternion.identity);

			//set local player how  being MultipurposeCameraRig target to follow him
			camRig.GetComponent<CameraFollow> ().SetTarget (myPlayer.transform, newPlayer.cameraTotarget);

			CanvasManager.instance.healthSlider.value = newPlayer.gameObject.GetComponent<PlayerHealth>().health;

			CanvasManager.instance.txtHealth.text = "HP " + newPlayer.gameObject.GetComponent<PlayerHealth>().health + " / " +
				newPlayer.gameObject.GetComponent<PlayerHealth>().maxHealth;
			
			//hide the lobby menu (the input field and join buton)
			CanvasManager.instance.OpenScreen(3);

			CanvasManager.instance.CloseLoadingImg ();

			CanvasManager.instance.lobbyCamera.GetComponent<Camera> ().enabled = false;

			gameIsRunning = true;

			CanvasManager.instance.CloseLoadingImg();

			//take a look in public IEnumerator WaitAnswer()
			tryJoinServer = false;

			// the local player now is logged
			onLogged = true;

			Debug.Log("player in game");
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
		 * data.pack[3] = position.x
		 * data.pack[4] = position.y
		 * data.pack[5] = position.z
		 * data.pack[6] = rotation.x
		 * data.pack[7] = rotation.y
		 * data.pack[8] = rotation.z
		 * data.pack[9] = rotation.w
		*/

		 Debug.Log("try spawn player");
		if (onLogged ) {

		
			bool alreadyExist = false;

			//verify all players to  prevents copies
			foreach(KeyValuePair<string, PlayerManager> entry in networkPlayers)
			{
				// same id found ,already exist!!! 
				if (entry.Value.id== data.pack [1])
				{
					alreadyExist = true;
				}
			}
			if (!alreadyExist) {

				Debug.Log("creating a new player");

				PlayerManager newPlayer;

				// newPlayer = GameObject.Instantiate( network player avatar or model, spawn position, spawn rotation)
				newPlayer = GameObject.Instantiate (networkPlayerPrefabs [0],
					UtilsClass.StringToVector3(data.pack[3]),Quaternion.identity).GetComponent<PlayerManager> ();

				//it is not the local player
				newPlayer.isLocalPlayer = false;

				//network player online in the arena
				newPlayer.isOnline = true;

				//set the network player 3D text with his name
				newPlayer.Set3DName(data.pack[2]);

				newPlayer.gameObject.name = data.pack [1];

				//puts the local player on the list
				networkPlayers [data.pack [1]] = newPlayer;
			}

		}

	}

	/// <summary>
	///  Update the network player position to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdatePosAndRot(UDPEvent data)
	{

		/* data.pack[0] = UPDATE_POS_AND_ROT
		 * data.pack[1] = id (network player id)
		 * data.pack[2] = "position.x;position.y;posiiton.z"
		 * data.pack[3] = "rotation.x; rotation.y; rotation.z; rotation.w"
		*/

        try{
		
		//it reduces to zero the accountant meaning that answer of the server exists to this moment
		contTimes = 0;

		if (networkPlayers [data.pack [1]] != null)
		{
			//find network player
			PlayerManager netPlayer = networkPlayers [data.pack [1]];
			netPlayer.timeOut = 0f;

			//update with the new position
			netPlayer.UpdatePosition(UtilsClass.StringToVector3(data.pack[2]));

			//update new player rotation
			netPlayer.UpdateRotation(new Quaternion (netPlayer.transform.rotation.x,float.Parse(data.pack[3]),netPlayer.transform.rotation.z,netPlayer.transform.rotation.w));

		}
		}//END_TRY
		catch(Exception e)
		{
		  Debug.LogError(e.ToString());
		}


	}


	/// <summary>
	/// method to send local player position and rotation update to the server.
	/// </summary>
	/// <param name="id">local player id.</param>
	/// <param name="_pos">local player position.</param>
	/// <param name="_rot">local player rotation.</param>
	public void EmitPosAndRot(Vector3 _pos, string rotY)
	{
	  //Identifies with the name "POS_AND_ROT", the notification to be transmitted to the server,
	  //and send to the server the player's position and rotation

	  //hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data["local_player_id"] =  myPlayer.GetComponent<PlayerManager>().id;

		data["position"] = UtilsClass.Vector3ToString(_pos);
		
		data["rotation"] = rotY;
		
		string msg = data["local_player_id"]+":"+data["position"]+":"+data["rotation"];


	     udpClient.EmitToServer ("POS_AND_ROT" ,msg);
	}

	



	/// <summary>
	/// Emits the local player attack to server.
	/// </summary>
	/// <param name="callback_name">Callback name.</param>
	/// <param name="_data">Data.</param>
	public void EmitAttack(string callback_name,string _data)
	{

		//sends to the server through socket UDP the _data package 
		udpClient.EmitToServer(callback_name ,_data);



	}

	/// <summary>
	/// Update the network player attack to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdateAttack(UDPEvent data)
	{
		/*
		 * data.pack[0] = UPDATE_ATACK
		 * data.pack[1] = id (network player id)

		*/

		if (networkPlayers [data.pack [1]] != null)
		{
			PlayerManager netPlayer = networkPlayers[data.pack[1]];

			netPlayer.UpdateAnimator ("IsAtack");

		}


	}
		

	/// <summary>
	/// Emits the local player phisicst damage to server.
	/// </summary>
	/// <param name="_shooterId">Shooter identifier.</param>
	/// <param name="_targetId">Target identifier.</param>
	public void EmitPhisicstDamage(string _shooterId, string _targetId)
	{

		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data ["shooterId"] = _shooterId;
		data ["targetId"] = _targetId;

		//join info
		string msg = data ["shooterId"]+":"+data ["targetId"];

		//sends to the server through socket UDP the jo package 
		udpClient.EmitToServer("PHISICS_DAMAGE" ,msg );

	}


	//it updates the suffered damage to local player
	void OnUpdatePlayerDamage (UDPEvent data)
	{

		/*
		 * data.pack[0] = UPDATE_PHISICS_DAMAGE
		 * data.pack[1] = attacker.id or shooter.id (network player id)
		 * data.pack[2] = target.id (network player id)
		 * data.pack[3] = target.health
		 */



		if (networkPlayers [data.pack [2]] != null) 
		{

			PlayerManager PlayerTarget = networkPlayers[data.pack [2]];
			PlayerTarget. GetComponent<PlayerHealth> ().TakeDamage ();


			if (PlayerTarget.isLocalPlayer)// if i am target
			{
				CanvasManager.instance.healthSlider.value = float.Parse(data.pack [3],CultureInfo.CurrentCulture);
				CanvasManager.instance.txtHealth.text = "HP " + data.pack [3] + " / " 
					+ PlayerTarget. GetComponent<PlayerHealth> ().maxHealth;

			}



		}

		if (networkPlayers [data.pack [1]] != null) 
		{

			PlayerManager PlayerShooter = networkPlayers[data.pack [1]];

		}


	}

	/// <summary>
	/// Emits the local player animation to Server.js.
	/// </summary>
	/// <param name="_animation">Animation.</param>
	public void EmitAnimation(string _animation)
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		//JSON package
		data["callback_name"] = "ANIMATION";//preenche com o id da callback receptora que está no servidor

		data["local_player_id"] = myPlayer.GetComponent<PlayerManager>().id;

		data ["animation"] = _animation;

		//send the position point to server
		string msg = data["local_player_id"]+":"+data ["animation"];

		//sends to the server through socket UDP the jo package 
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

		contTimes = 0;

		//find network player by your id
		PlayerManager netPlayer = networkPlayers[data.pack[1]];
		netPlayer.timeOut = 0f;
		//updates current animation
		netPlayer.UpdateAnimator(data.pack[2]);

	}


	void GameOver()
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

		if (bool.Parse (data.pack [2])) {
			
			RestartGame ();
		}
		else
		{
			
				if (networkPlayers [data.pack [1]] != null) {
				
					//destroy network player by your id
					Destroy (networkPlayers [data.pack [1]].gameObject);

					//remove from the dictionary
					networkPlayers.Remove (data.pack [1]);
				}
		}


	}

	public void RestartGame()
	{
		CanvasManager.instance.txtSearchServerStatus.text = "PLEASE START SERVER";

		Destroy (camRig.gameObject);
		foreach(KeyValuePair<string, PlayerManager> entry in networkPlayers)
		{
			if (networkPlayers [entry.Key] != null) {
				Destroy (networkPlayers [entry.Key].gameObject);
			}
		}

		networkPlayers.Clear ();

		gameIsRunning = false;

		serverFound = false;

		myId = string.Empty;

		CanvasManager.instance.OpenScreen (0);

	}



	void OnApplicationQuit() {

		Debug.Log("Application ending after " + Time.time + " seconds");

		GameOver ();
			
	}
		
}

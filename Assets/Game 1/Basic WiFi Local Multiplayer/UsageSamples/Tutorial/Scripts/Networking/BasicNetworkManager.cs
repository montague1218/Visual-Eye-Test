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
using UDPCore;

public class BasicNetworkManager : MonoBehaviour
{

	public static BasicNetworkManager instance;

	//local player id
	public string local_player_id;


	public GameObject[] cubePrefabs;

	public Transform[] spawnPoints;

	//store localPlayer
	public GameObject myPlayer;

	UDPComponent udpClient;

	public int serverPort;

	public bool serverFound;

	bool waitingSearch;

	public bool serverAlreadyStarted;



	//store all players in game <id, cubeManager>
	public Dictionary<string, CubeManager> networkPlayers = new Dictionary<string, CubeManager>();



    // Start is called before the first frame update
    void Start()
    {
		
		// if don't exist an instance of this class
		if (instance == null) {

			//it doesn't destroy the object, if other scene be loaded
			DontDestroyOnLoad (this.gameObject);

			instance = this;// define the class as a static variable

			//instantiates the UDPComponent library in the udpClient variable
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


	void Update()
	{
		//if it was not found a server
		if (!serverFound) {
			
			// start routine to detect a server on wifi network
			StartCoroutine ("PingPong");
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
		yield return new WaitForSeconds(3);

		waitingSearch = false;

		if(!serverFound)
		{
			EmitJoinGame();
		}



	}


	public void ConnectToUDPServer()
	{
		if (udpClient.GetServerIP () != string.Empty) {

			//generates a random port between 3001 and 3310
			int randomPort = UnityEngine.Random.Range(3001, 3310);

			//connect to Server
			udpClient.connect (udpClient.GetServerIP (), serverPort, randomPort);

			//receives a "PONG" notification from the server
			udpClient.On ("PONG", OnPrintPongMsg);

			//receives a "JOIN_SUCCESS" notification from the server
			udpClient.On ("JOIN_SUCCESS", OnJoinGame);

			//receives a "SPAWN_PLAYER" notification from the server
			udpClient.On ("SPAWN_PLAYER", OnSpawnPlayer);

			//receives a "UPDATE_POS_AND_ROT" notification from the server
			udpClient.On ("UPDATE_POS_AND_ROT",OnUpdatePosAndRot);

			//receives a "USER_DISCONNECTED" notification from the server
			udpClient.On ("USER_DISCONNECTED",OnUserDisconnected);





		}
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


	}

	public void EmitPing()
	{
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
		
		// check if there is a server running
		if (!serverFound) {

			//create a  new server
			BasicServer.instance.CreateServer();

			Dictionary<string, string> data = new Dictionary<string, string>();

			data["callback_name"] = "JOIN_GAME";

			local_player_id = generateID ();

			data ["player_id"] = local_player_id;

			string msg = data["player_id"];

			//sends to the server 
			udpClient.EmitToServer (data["callback_name"] ,msg);

		}



		serverAlreadyStarted = true;

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
	/// Raises the join game event from Server.
	/// only the first player to connect gets this feedback from the server
	/// </summary>
	/// <param name="data">Data.</param>
	void OnJoinGame(UDPEvent data)
	{
	   /*
		 * data.data.pack[0] = CALLBACK_NAME: "JOIN_SUCCESS" from server
		 * data.data.pack[1] = id (local player id)

		*/

		Debug.Log("Login successful, joining game");


		if (!myPlayer) {

			int randomPoint = UnityEngine.Random.Range(0, spawnPoints.Length);


			// take a look in Player2DManager.cs script
			CubeManager newPlayer;

			// newPlayer = GameObject.Instantiate( local player avatar or model, spawn position, spawn rotation)
			newPlayer = GameObject.Instantiate (cubePrefabs [0],spawnPoints[randomPoint].position,
				Quaternion.identity).GetComponent<CubeManager> ();

			newPlayer.id = data.pack [1];

			//this is local player
			newPlayer.isLocalPlayer = true;

			//now local player online in the arena
			newPlayer.isOnline = true;

			newPlayer.gameObject.name = data.pack [1];

			//puts the local player on the list
			networkPlayers [data.pack [1]] = newPlayer;

			myPlayer = networkPlayers [data.pack[1]].gameObject;

			Debug.Log("player instantiated");


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
		*/


		Debug.Log ("\n spawning network player ...\n");

		bool alreadyExist = false;

		//verify all players to  prevents copies
		foreach(KeyValuePair<string, CubeManager> entry in networkPlayers)
		{
			// same id found ,already exist!!! 
			if (entry.Value.id== data.pack [1])
			{
				alreadyExist = true;
			}
		}

		if(local_player_id.Equals(data.pack [1]))
		{

			alreadyExist = true;
		}
		if (!alreadyExist) {

			Debug.Log("creating a new player");

			int randomPoint = UnityEngine.Random.Range(0, spawnPoints.Length);


			CubeManager newPlayer;

			// newPlayer = GameObject.Instantiate( network player avatar or model, spawn position, spawn rotation)
			newPlayer = GameObject.Instantiate (cubePrefabs [1],spawnPoints[randomPoint].position,
				Quaternion.identity).GetComponent<CubeManager> ();


            newPlayer.id = data.pack [1];

			//it is not the local player
			newPlayer.isLocalPlayer = false;

			//network player online in the arena
			newPlayer.isOnline = true;

			newPlayer.gameObject.name = data.pack [1];

			//puts the local player on the list
			networkPlayers [data.pack [1]] = newPlayer;


		}

	}


	////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////PLAYER POSITION AND ROTATION UPDATES///////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
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

		data["local_player_id"] =  myPlayer.GetComponent<CubeManager>().id;

		data["position"] = UtilsClass.Vector3ToString(_pos);
		
		data["rotation"] = rotY;
		
		string msg = data["local_player_id"]+":"+data["position"]+":"+data["rotation"];


	     udpClient.EmitToServer ("POS_AND_ROT" ,msg);
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
		 * data.pack[3] = "rotation.y"
		*/

		if (networkPlayers [data.pack [1]] != null)
		{
			//find network player
		    CubeManager netPlayer = networkPlayers[data.pack[1]];

			//update with the new position
			netPlayer.UpdatePosition(UtilsClass.StringToVector3(data.pack[2]));

			//update new player rotation
			netPlayer.UpdateRotation(new Quaternion (netPlayer.transform.rotation.x,float.Parse(data.pack[3]),netPlayer.transform.rotation.z,netPlayer.transform.rotation.w));

		}
		
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

		//send answer in broadcast
		foreach (KeyValuePair<string, CubeManager> entry in networkPlayers) {

			if(entry.Value.id.Equals(data.pack[1]))
				Destroy(entry.Value.gameObject);

		}//END_FOREACH


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



  

}

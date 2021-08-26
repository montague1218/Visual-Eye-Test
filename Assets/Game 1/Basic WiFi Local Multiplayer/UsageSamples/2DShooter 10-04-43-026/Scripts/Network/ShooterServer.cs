using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UDPCore;

public class ShooterServer : MonoBehaviour
{

    public static ShooterServer instance;

    //from UDP Client Module API
	private UDPComponent udpClient;

	public bool tryCreateServer;

	public bool waitingAnswer;

	public int onlinePlayers;

	public float maxTimeOut;
		
	//store all players in game
	public Dictionary<string, Client> connectedClients = new Dictionary<string, Client>();

	public float cont;

	public class Client
	{
			public string  id;

			public string name;
			
			public string avatar;

			public string position;

			public string rotation;

			public string animation;

			public int kills = 0;

			public int health = 100;

			public int maxHealth = 100;

			public bool isDead;

			public float timeOut = 0f;

			public IPEndPoint remoteEP;

	}

	

		// Use this for initialization
		void Start () {

			// if don't exist an instance of this class
			if (instance == null) {

				//it doesn't destroy the object, if other scene be loaded
				DontDestroyOnLoad (this.gameObject);

				instance = this;// define the class as a static variable

				udpClient = gameObject.GetComponent<UDPComponent>();

				 udpClient.On ("PING", OnReceivePing);
				 udpClient.On ("JOIN_GAME", OnReceiveJoinGame);
				 //receives a "RESPAWN" notification from client
			     udpClient.On ("RESPAWN", OnReceiveRespawn);
				 udpClient.On ( "POS_AND_ROT", OnReceivePosAndRot);
				 udpClient.On ("JUMP", OnReceiveJump);
				 udpClient.On ("DAMAGE", OnReceiveDamage);
				 udpClient.On ("ANIMATION", OnReceiveAnimation);
				 udpClient.On ("disconnect", 	OnReceiveDisconnect);

			}
			else
			{
				//it destroys the class if already other class exists
				Destroy(this.gameObject);
			}

		}
		
	
		/// <summary>
		/// Creates a UDP Server in in the associated client
		/// called method when the button "start" on HUDCanvas is pressed
		/// </summary>
		public void CreateServer()
		{
			if (udpClient.GetServerIP()!= string.Empty) {

				if (ShooterNetworkManager.instance.serverFound && !udpClient.serverRunning) {} 

				else
				{
					if (!udpClient.serverRunning) {
						
						//start server
			            udpClient.StartServer();

						Debug.Log ("UDP Server listening on IP " + udpClient.GetServerIP () + " and port " + udpClient.serverPort);

						Debug.Log ("------- server is running -------");
						
					}
					
				}
					
			}//END_IF
			else
			{
				ShooterCanvasManager.instance.ShowAlertDialog ("PLEASE CONNECT TO A WIFI NETWORK");
			}

		}


		public string generateID()
		{
			return Guid.NewGuid().ToString("N");
		}

        /// <summary>
	    /// Receive ping request.
	    /// </summary>
	    /// <param name="data">received package from client.</param>
		void  OnReceivePing(UDPEvent data )
		{
			/*
		       * data.pack[0]= CALLBACK_NAME: "PONG"
		       * data.pack[1]= "ping"
		    */

			Debug.Log("receive ping from game client");

		 try
	    {
			
			Dictionary<string, string> send_pack = new Dictionary<string, string> ();
					  	
		    //JSON package
		    send_pack ["callback_name"] = "PONG";
			    	
		    //store  player info in msg field
		    send_pack ["msg"] = "pong!!!";
				
		    var response = string.Empty;

		    byte[] msg = null;
	
		    response = send_pack ["callback_name"] + ':' + send_pack ["msg"];

		     msg = Encoding.ASCII.GetBytes (response);

		     //send answer to client that called server
	        udpClient.EmitToClient( msg, data.anyIP);

		     Debug.Log ("[INFO] PONG message sended to connected player");

		}//END_TRY
	    catch(Exception e) {
		 Debug.LogError(e.ToString());
		
	    }

	}




////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////[JOIN] [SPAWN AND RESPAWN] FUNCTIONS///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Receive the player's name and position.
	/// </summary>
	/// <param name="data">received package from client.</param>
	void OnReceiveJoinGame(UDPEvent data )
	{
			
		/*
		 * data.pack[0] = CALLBACK_NAME: "JOIN"
		 * data.pack[1] = player id
		 * data.pack[2] = player position
		 * data.pack[3] = player avatar
		*/

	 try
	{
		if (!connectedClients.ContainsKey (data.pack [1])) {
		
	
			Client client = new Client ();

			client.id = data.pack [1];//set client id
			
			client.name = data.pack [2];//set client name
			
			client.avatar = data.pack [3];
			  
			//set  clients's port and ip address
			client.remoteEP = data.anyIP;

			Debug.Log ("[INFO] player " + client.id + ": logged!");
			
			//add client in search engine
			connectedClients.Add (client.id.ToString (), client);

			onlinePlayers = connectedClients.Count;
				
			Debug.Log ("[INFO] Total players: " + connectedClients.Count);
		
			IPEndPoint playerIP = data.anyIP;

			EmitResponse(client,data.anyIP); // sends feedback to the player

			if(onlinePlayers <= 2) // already exist a connected player waiting
			{
			 	
		      EmitPlayersToPlayer(client,playerIP); // spawn all game clients to the player

		      EmitPlayerInBroadcast(client); // spawn the current player for all online players
			}
			 	
		}//END_IF
				
	}//END_TRY
	catch(Exception e) {
		Debug.LogError(e.ToString());
		
	}

}

    
	  /// <summary>
	/// sends feedback to the game client that opened the communicationt.
	/// </summary>
	void EmitResponse(Client client, IPEndPoint playerIP )
	{
     
		Dictionary<string, string> send_pack = new Dictionary<string, string> ();

		//JSON package
		send_pack ["callback_name"] = "JOIN_SUCCESS";
			  
		if(onlinePlayers== 1)
		{
			send_pack ["spawnPoint"] = "0";
		}
		else
		{
			send_pack ["spawnPoint"] = "1";
		}
		//store  player info in msg field
			 
			  
		 send_pack ["msg"] = client.id + ":" + client.name + ":" + client.avatar+":"+ send_pack ["spawnPoint"];
			
		 var response = string.Empty;

		 byte[] msg = null;
		  
		 //format the data with the sifter comma for they be send from turn to udp client
		 response = send_pack ["callback_name"] + ':' + send_pack ["msg"];

		 msg = Encoding.ASCII.GetBytes (response);

		 //send answer to client that called me 
	     udpClient.EmitToClient( msg, playerIP);

		 Debug.Log ("[INFO]sended to connected player : JOIN_SUCCESS");
			
	}

     /// <summary>
	/// sends all online players to the game client that opened the communicationt.
	/// </summary>
	void EmitPlayersToPlayer(Client client, IPEndPoint playerIP)
	{

		Dictionary<string, string> send_pack = new Dictionary<string, string> ();

		var response = string.Empty;

	    byte[] msg = null;
	  
	
		//sends the game clients for the client sender
		foreach (KeyValuePair<string, Client> entry in connectedClients) {

		    // same id found ,already exist!!!
			if (entry.Value.id != client.id) {

				
				//JSON package
				send_pack ["callback_name"] = "SPAWN_PLAYER";

				send_pack ["spawnPoint"] = "0";
			           
				send_pack ["msg"] = entry.Value.id + ":" + entry.Value.name + ":" + entry.Value.avatar+
				":"+send_pack["spawnPoint"];
 
				//format the data with the sifter '':' character for they be send from turn to udp client
				response = send_pack  ["callback_name"] + ':' + send_pack  ["msg"];

				msg = Encoding.ASCII.GetBytes (response);

				//send answer to client that called server
				udpClient.EmitToClient( msg, playerIP);

			}

		}
		Debug.Log ("[INFO]sended all online players to connected player : SPAWN_PLAYER");

	}

     /// <summary>
	/// sends the game client that opened the communicationt to all online players.
	/// </summary>
	void EmitPlayerInBroadcast(Client client)
	{

		Dictionary<string, string> send_pack = new Dictionary<string, string> ();

		var response = string.Empty;

	    byte[] msg = null;

		//JSON package
		send_pack  ["callback_name"] = "SPAWN_PLAYER";

		send_pack ["spawnPoint"] = "1";

		
		send_pack ["msg"] = client.id + ":" + client.name + ":" + client.avatar+":"+send_pack ["spawnPoint"];

		//sends the client sender to all clients in game
		foreach (KeyValuePair<string, Client> entry in connectedClients) {

			if (entry.Value.id != client.id) {

				Debug.Log("sending second player");
				//format the data with the sifter comma for they be send from turn to udp client
				response = send_pack  ["callback_name"] + ':' + send_pack  ["msg"];

				msg = Encoding.ASCII.GetBytes (response);

				//send answer to all clients in connectClients list
				udpClient.EmitToClient( msg,entry.Value.remoteEP);

			}//END_IF

		}//END_FOREACH
	
	}

	void OnReceiveRespawn(UDPEvent data  )
	{
			
		/*
		 * pack[0] = CALLBACK_NAME: "RESPAWN"
		 * pack[1] = player id
		 * pack[2] = player avatar
		*/

	 try
	{
		if (connectedClients.ContainsKey (data.pack [1])) {
		
		
		    connectedClients [data.pack [1]].isDead = false;
            
		    connectedClients [data.pack [1]].health = connectedClients [data.pack [1]].maxHealth;
		      
			string playerID = data.pack[1];

			connectedClients [data.pack [1]].name = data.pack [2];//set client name
			
			connectedClients [data.pack [1]].avatar = data.pack [3];

			IPEndPoint playerIP = data.anyIP;

			Client player = connectedClients [data.pack [1]];

			EmitRespawnPlayer(playerID,connectedClients [data.pack [1]].name,
			connectedClients [data.pack [1]].avatar,
			 playerIP );

			EmitPlayerInBroadcast(player);
		
			
		}//END_IF
				
	}//END_TRY
	catch(Exception e) {
			Debug.Log(e.ToString());
	}
}

 /// <summary>
	/// method to handle notification that arrived from any client.
	/// </summary>
	/// <remarks>
	///  respawn player in client machine.
	/// </remarks>
	void  EmitRespawnPlayer(string playerID,string name,string avatar, IPEndPoint playerIP )
	{
     
		Dictionary<string, string> send_pack = new Dictionary<string, string> ();
					  	
		 //JSON package
		 send_pack ["callback_name"] = "RESPAWN_PLAYER";
			  	  	
		 send_pack ["spawnPoint"] = "0";
			           
		 send_pack ["msg"] = playerID + ":" + name + ":" + avatar+
				":"+send_pack["spawnPoint"];
				
		 var response = string.Empty;

	     byte[] msg = null;
		 //format the data with the sifter : for they be send from turn to udp client
	     response = send_pack ["callback_name"] + ':' + send_pack ["msg"];

		  msg = Encoding.ASCII.GetBytes (response);

		  //send answer to client that called me 
		   udpClient.EmitToClient( msg,playerIP);

		   Debug.Log("send respawn player");
			
	}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////



////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////// PLAYER's POSITION AND ROTATION UPDATES ///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// receive the network player position and rotation and send in broadcast to all online players.
	/// </summary>
	/// <param name="data">received package from any client.</param>
	void OnReceivePosAndRot(UDPEvent data )
	{
			
		/*
		 * data.pack[0] = CALLBACK_NAME: "POS_AND_ROT"
	     * data.pack[1] = id (network player id)
		 * data.pack[2] = "position.x;position.y;posiiton.z"
		 * data.pack[3] = "rotation.x; rotation.y; rotation.z; rotation.w"
	    */
	    
	  try
	  {
	  
		if (connectedClients.ContainsKey (data.pack [1])) {

			connectedClients [data.pack [1]].position = data.pack[2];
		
		    connectedClients [data.pack [1]].rotation = data.pack[3];

	        BroadcastEmitPlayerPosAndRot(data.pack [1]);
		}
	  
	   }//END_TRY
		catch(Exception e) {
			Debug.LogError(e.ToString());
		}
		
	}
	

	 /// <summary>
	/// // send the current player position and rotation for all online players.
	/// </summary>
	/// <param name="playerID"> current player ID.</param>
	void BroadcastEmitPlayerPosAndRot(string playerID)
	{

		Dictionary<string, string> send_pack = new Dictionary<string, string> ();
			
	    //JSON package
		send_pack  ["callback_name"] = "UPDATE_POS_AND_ROT";
	    
		//store "pong!!!" message in msg field
		send_pack  ["msg"] = connectedClients [playerID].id + ":" +
		 connectedClients [playerID].position + ":" + connectedClients [playerID].rotation;
		
		var response = string.Empty;

		byte[] msg = null;
		
		
		//sends the client sender to all clients in game
		foreach (KeyValuePair<string, Client> entry in connectedClients) {

			if(!entry.Value.id.Equals(connectedClients [playerID].id ))
			{
			  //format the data with the sifter comma for they be send from turn to udp client
			  response = send_pack  ["callback_name"] + ':' + send_pack  ["msg"];

			  msg = Encoding.ASCII.GetBytes (response);

			  //send answer to all clients in connectClients list
			  udpClient.EmitToClient( msg,entry.Value.remoteEP);
			}
			
		}//END_FOREACH
	
	}
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////// PLAYER's POSITION AND ROTATION UPDATES ///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// receive the network player position and rotation and send in broadcast to all online players.
	/// </summary>
	/// <param name="data">received package from any client.</param>
	void OnReceiveJump(UDPEvent data )
	{
			
		/*
		 * data.pack[0] = CALLBACK_NAME: "JUMP"
		 * data.pack[1] = playerId
		*/
	    
	  try
	  {
	  
		if (connectedClients.ContainsKey (data.pack [1])) {

		
	        BroadcastEmitPlayerJump(data.pack [1]);
		}
	  
	   }//END_TRY
		catch(Exception e) {
			Debug.LogError(e.ToString());
		}
		
	}
	

	 /// <summary>
	/// // send the current player position and rotation for all online players.
	/// </summary>
	/// <param name="playerID"> current player ID.</param>
	void BroadcastEmitPlayerJump(string playerID)
	{

		Dictionary<string, string> send_pack = new Dictionary<string, string> ();
			
	    //JSON package
		send_pack  ["callback_name"] = "UPDATE_JUMP";
	    
		//store "pong!!!" message in msg field
		send_pack  ["msg"] = connectedClients [playerID].id ;
		
		var response = string.Empty;

		byte[] msg = null;
		
		
		//sends the client sender to all clients in game
		foreach (KeyValuePair<string, Client> entry in connectedClients) {

			if(!entry.Value.id.Equals(connectedClients [playerID].id ))
			{
			  //format the data with the sifter comma for they be send from turn to udp client
			  response = send_pack  ["callback_name"] + ':' + send_pack  ["msg"];

			  msg = Encoding.ASCII.GetBytes (response);

			  //send answer to all clients in connectClients list
			  udpClient.EmitToClient( msg,entry.Value.remoteEP);
			}
			
		}//END_FOREACH
	
	}
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////



////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////// PLAYERS ANIMATION HANDLER////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


	/// <summary>
	/// Receive the player's position and rotation.
	/// </summary>
	/// <param name="data">received package from client.</param>
	void OnReceiveAnimation(UDPEvent data )
	{
			
		/*
		 * data.pack[0] = CALLBACK_NAME: ANIMATION"
		 * data.pack[1] = player id
		 * data.pack[2] = player animation
	   */
	
	  try
	  {
	  
	    connectedClients [data.pack [1]].animation = data.pack[2];
		
	 
	    Dictionary<string, string> send_pack = new Dictionary<string, string> ();
			
	    //JSON package
		send_pack  ["callback_name"] = "UPDATE_PLAYER_ANIMATOR";
	    
		send_pack  ["msg"] = connectedClients [data.pack [1]].id + ":" + connectedClients [data.pack [1]].animation;
		
		var response = string.Empty;

		byte[] msg = null;

		
		//sends the client sender to all clients in game
		foreach (KeyValuePair<string, Client> entry in connectedClients) {

			if(!entry.Value.id.Equals(connectedClients [data.pack [1]].id ))
			{
			  //format the data with the sifter comma for they be send from turn to udp client
			  response = send_pack  ["callback_name"] + ':' + send_pack  ["msg"];

			  msg = Encoding.ASCII.GetBytes (response);

			  //send answer to all clients in connectClients list
			  udpClient.EmitToClient( msg,entry.Value.remoteEP);
			}
			

			

		}//END_FOREACH
		
	  
	   }//END_TRY
		catch(Exception e) {
			Debug.LogError(e.ToString());
		}
		
	}
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////



////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////// PLAYERS DAMAGE HANDLER////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	 /// <summary>
	/// receives player damage notification.
	/// </summary>
    /// <param name="data">received package from client.</param>
	void OnReceiveDamage(UDPEvent data )
	{
			
		/*
		 * data.pack[0] = CALLBACK_NAME: "DAMAGE"
	     * data.pack[1] = playerId
		*/
	   try{

       string targetID = data.pack [1];
	  
	   if (connectedClients.ContainsKey (targetID)) {
	   
	   
	      var target = connectedClients [targetID];
		 
	      int _damage= 10;
		  
		   // if the target has not lost all of its health
		     if(target.health - _damage > 0)
			 {
			   
			   target.health -=_damage; //health decrement
			 }
			 
			 else
			 { //target death

			    //if not dead already
                if(!target.isDead)
               {				
			   
			     target.isDead = true;// target now is dead
				 
				 target.kills = 0;

				 EmitTargetDeathInBroadcast(targetID);
		
			   }//END_ if    
			 }//END_ELSE
	   
	        EmitTargetDamageInBroadcast(targetID);
	
	  }//END_IF
	    
	}//END_TRY
	catch(Exception e) {
			Debug.LogError(e.ToString());
	}
}

	void EmitTargetDeathInBroadcast(string targetID)
	{

        Dictionary<string, string> send_pack = new Dictionary<string, string> ();
		   
		var response = string.Empty;
			
		byte[] msg = null;
		
		 //player connectedClients [pack [1]] is dead
		 send_pack ["callback_name"] = "GAME_OVER";
			
		 send_pack  ["msg"] = targetID;
		  
		foreach (KeyValuePair<string, Client> entry in connectedClients) {

			
			  //format the data with the sifter comma for they be send from turn to udp client
			  response = send_pack  ["callback_name"] + ':' + send_pack  ["msg"];

			  msg = Encoding.ASCII.GetBytes (response);

			  //send answer to all clients in connectClients list
			  udpClient.EmitToClient( msg,entry.Value.remoteEP);
			
		 }//END_FOREACH

	}

	void EmitTargetDamageInBroadcast(string targetID)
	{

	   Dictionary<string, string> send_pack = new Dictionary<string, string> ();
		   
		var response = string.Empty;
			
		byte[] msg = null;

        send_pack ["callback_name"] = "UPDATE_PLAYER_DAMAGE";

		send_pack ["player_id"] = targetID;
				  
		send_pack ["player_health"] = connectedClients [targetID].health.ToString();
					
        send_pack ["msg"] = send_pack ["player_id"]+":"+send_pack ["player_health"];

		 
		//sends the client sender to all clients in game
		foreach (KeyValuePair<string, Client> entry in connectedClients) {

			
		    response = send_pack  ["callback_name"] + ':' + send_pack  ["msg"];

			  msg = Encoding.ASCII.GetBytes (response);

			  //send answer to all clients in connectClients list
			  udpClient.EmitToClient( msg,entry.Value.remoteEP);
			

		 }//END_FOREACH
	}
	

		void OnReceiveDisconnect(UDPEvent data)
		{
			/*
		     * data.pack[0]= CALLBACK_NAME: "disconnect"
		     * data.pack[1]= player_id
		     * data.pack[2]= isMasterServer (true or false)
		    */


			Dictionary<string, string> send_pack = new Dictionary<string, string>();

		
			var response = string.Empty;

			byte[] msg = null;
	

			 //JSON package
			 send_pack ["callback_name"] = "USER_DISCONNECTED";

			 send_pack ["msg"] =  data.pack[1];

			 send_pack ["isMasterServer"] = data.pack [2];

			 response = send_pack ["callback_name"] + ':' + send_pack ["msg"]+':'+send_pack ["isMasterServer"] ;

			 msg = Encoding.ASCII.GetBytes (response);

			//sends the client sender to all clients in game
			foreach (KeyValuePair<string, Client> entry in connectedClients) {

				Debug.Log("send disconnect");

				//send answer to all clients in connectClients list
				udpClient.EmitToClient( msg,entry.Value.remoteEP);

			}//END_FOREACH

			connectedClients.Clear();//clear the players list

		}

}

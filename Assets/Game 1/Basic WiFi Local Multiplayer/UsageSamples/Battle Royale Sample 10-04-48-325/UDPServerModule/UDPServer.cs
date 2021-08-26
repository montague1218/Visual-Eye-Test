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
using  UDPCore;


namespace UDPServerModule
{

	public class UDPServer : MonoBehaviour {

	    public static UDPServer instance;

		//from UDP Core Module API
		private UDPComponent udpClient;

		static private readonly char[] Delimiter = new char[] {':'};

		public string[] pack;

		public int onlinePlayers;

		public float maxTimeOut;
		//store all players in game
		public Dictionary<string, Client> connectedClients = new Dictionary<string, Client>();

		public float cont;

		public class Client
		{
			public string  id;

			public string name;

			public string position;

			public string rotation;

			public string animation;

			public int kills = 0;

			public int health = 100;

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
				udpClient.On ("JOIN", OnReceiveJoin);
				udpClient.On ( "POS_AND_ROT", OnReceivePosAndRot);
				udpClient.On ("ATTACK",OnReceiveAttack);
				udpClient.On ("PHISICS_DAMAGE", OnReceiveDamage);
				udpClient.On ("ANIMATION", OnReceiveAnimation);
				udpClient.On ("disconnect", OnReceiveDisconnect);

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
			if (udpClient.GetIP()!= string.Empty) {

				if (NetworkManager.instance.serverFound && !udpClient.serverRunning)
				{
					CanvasManager.instance.ShowAlertDialog ("SERVER ALREADY RUNNING ON NETWORK!");
				}

				else
				{
					if (!udpClient.serverRunning) {

						udpClient.StartServer();

						Debug.Log ("UDP Server listening on IP " + udpClient.GetIP () + " and port " + udpClient.serverPort);

						Debug.Log ("------- server is running -------");

						//CanvasManager.instance.inputLogin.text = "Master";
						CanvasManager.instance.inputLogin.text = CanvasManager.instance.inputHostName.text;
					}

					NetworkManager.instance.EmitJoin ();
				}

			}
			else
			{
				CanvasManager.instance.ShowAlertDialog ("PLEASE CONNECT TO A WIFI NETWORK");
			}


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
	void OnReceiveJoin(UDPEvent data )
	{
			
		/*
		 * data.pack[0] = CALLBACK_NAME: "JOIN"
		 * data.pack[1] = player_name
		 * data.pack[2] = player id
		 * data.pack[3] = position.x
		 * data.pack[4] = position.y
		 * data.pack[5] = position.z
		*/
			

	 try
	{
		if (!connectedClients.ContainsKey (data.pack [1])) {
		
	
			Client client = new Client ();

			client.id = data.pack [1];//set client id
			
			client.name = data.pack [2];//set client name
			
			client.position = data.pack[3];
			
			//set  clients's port and ip address
			client.remoteEP = data.anyIP;

			Debug.Log ("[INFO] player " + client.name + ": logged!");
			
			//add client in search engine
			connectedClients.Add (client.id.ToString (), client);

			onlinePlayers = connectedClients.Count;
				
			Debug.Log ("[INFO] Total players: " + connectedClients.Count);
		
			IPEndPoint playerIP = data.anyIP;

			EmitResponse(client,data.anyIP); // sends feedback to the player

		    EmitPlayersToPlayer(client,playerIP); // spawn all game clients to the player

		    EmitPlayerInBroadcast(client); // spawn the current player for all online players
				
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

		//store  player info in msg field
		send_pack ["msg"] = client.id + ":" + client.name + ":" + client.position;
					
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

				send_pack ["callback_name"] = "SPAWN_PLAYER";

				send_pack ["msg"] = entry.Value.id + ":" + entry.Value.name + ":" + entry.Value.position;

				//format the data with the sifter comma for they be send from turn to udp client
				response = send_pack ["callback_name"] + ':' +send_pack ["msg"];

				msg = Encoding.ASCII.GetBytes (response);

				//send answer to client that called server
				udpClient.EmitToClient( msg, playerIP);
				Debug.Log ("[INFO] player sended");

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
		send_pack ["callback_name"] = "SPAWN_PLAYER";

		
		send_pack ["msg"] = client.id + ":" + client.name + ":" + client.position;

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

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////




////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////// PLAYER's POSITION AND ROTATION UPDATES ///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// receive the network player position  and send in broadcast to all online players.
	/// </summary>
	/// <param name="data">received package from any client.</param>
	void OnReceivePosAndRot(UDPEvent data )
	{
		
		/*
		 * data.pack[0] = CALLBACK_NAME: POS_AND_ROT"
		 * data.pack[1] = player id
		 * data.pack[2] = player position
		 * data.pack[3] = player rotation
	   */
	    

	  try
	  {
	  
		if (connectedClients.ContainsKey (data.pack [1])) {

            connectedClients [data.pack [1]].timeOut = 0f;
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
		send_pack ["callback_name"] = "UPDATE_POS_AND_ROT";

		send_pack  ["msg"] = playerID+ ":" +
		 connectedClients [playerID].position + ":" + connectedClients [playerID].rotation;
		

		var response = string.Empty;

		byte[] msg = null;
		
		
		//sends the client sender to all clients in game
		foreach (KeyValuePair<string, Client> entry in connectedClients) {

			if(!entry.Value.id.Equals(connectedClients [playerID].id ))
			{
			  //format the data with the ':' for they be send from turn to udp client
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
///////////////////////////////////////// PLAYERS ATTACK AND DAMAGE HANDLER////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

     /// <summary>
	/// receives player attack notification.
	/// </summary>
    /// <param name="data">received package from client.</param>
    void OnReceiveAttack(UDPEvent data )
	{
			
		/*
		 * data.pack[0] = CALLBACK_NAME: "ATTACK"
		 * data.pack[1] = shooterId

		*/
	   try{

	   if (connectedClients.ContainsKey (data.pack [1])) {
	     
		  string playerID =  connectedClients [data.pack [1]].id;

		  EmitPlayerAttackInBroadcast(playerID);
 
	    }//END_IF
	    
	   }//END_TRY
		catch(Exception e) {
			Debug.LogError(e.ToString());
		}
	}

	void EmitPlayerAttackInBroadcast(string playerID)
	{
        Dictionary<string, string> send_pack = new Dictionary<string, string> ();
		   
		var response = string.Empty;

	    send_pack  ["callback_name"] = "UPDATE_ATTACK";
		   
		send_pack  ["msg"] = playerID;
		
		byte[] msg = null;
		
		
		//sends the client sender to all clients in game
		foreach (KeyValuePair<string, Client> entry in connectedClients) {

			  //format the data with the : character for they be send from turn to udp client
			  response = send_pack  ["callback_name"] + ':' + send_pack  ["msg"];

			  msg = Encoding.ASCII.GetBytes (response);

			  //send answer to all clients in connectClients list
			  udpClient.EmitToClient( msg,entry.Value.remoteEP);
				

		 }//END_FOREACH
	}
	
	 /// <summary>
	/// receives player damage notification.
	/// </summary>
    /// <param name="data">received package from client.</param>
	void OnReceiveDamage(UDPEvent data )
	{
			
		/*
	     * data.pack[0] = CALLBACK_NAME: "PHISICS_DAMAGE"
		 * data.pack[1] = shooterId
	     * data.pack[2] = targetId
        */

	   try{
		  
	   string shooterID = data.pack [1];

       string targetID = data.pack [2];
	  
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

			    //player is dead
				//restore player health
				connectedClients [data.pack [2]].health = 100;
  
			 }//END_ELSE
	   
	        EmitTargetDamageInBroadcast(shooterID,targetID);
	
	  }//END_IF
	    
	}//END_TRY
	catch(Exception e) {
			Debug.LogError(e.ToString());
	}
	}

	
	void EmitTargetDamageInBroadcast(string shooterID, string targetID)
	{

	   Dictionary<string, string> send_pack = new Dictionary<string, string> ();
		   
		var response = string.Empty;
			
		byte[] msg = null;

        	send_pack ["callback_name"] = "UPDATE_PLAYER_DAMAGE";

			send_pack ["shooter_id"] = connectedClients [shooterID].id;

			send_pack ["target_id"] = connectedClients [targetID].id;

			send_pack ["target_health"] = connectedClients [targetID].health.ToString();

			send_pack ["msg"] = send_pack ["shooter_id"]+ ":" +send_pack ["target_id"]+ ":" +send_pack ["target_health"];

		 
		//sends the client sender to all clients in game
		foreach (KeyValuePair<string, Client> entry in connectedClients) {

			
		    response = send_pack  ["callback_name"] + ':' + send_pack  ["msg"];

			  msg = Encoding.ASCII.GetBytes (response);

			  //send answer to all clients in connectClients list
			  udpClient.EmitToClient( msg,entry.Value.remoteEP);
			

		 }//END_FOREACH
	}
	
	

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
		 * data.pack[0] = CALLBACK_NAME: "ANIMATION"
		 * data.pack[1] = playerId
		 * data.pack[2] = animation (IDLE OR WALK)

		*/
	
	  try
	  {
	  
	    connectedClients [data.pack [1]].animation = data.pack[2];
		
	 
	    Dictionary<string, string> send_pack = new Dictionary<string, string> ();
			
	    connectedClients [data.pack [1]].animation = data.pack [2];

		send_pack ["callback_name"] = "UPDATE_PLAYER_ANIMATOR";

		send_pack ["player_id"] = connectedClients [data.pack [1]].id;

		send_pack ["animation"] = connectedClients [data.pack [1]].animation;

		send_pack ["msg"] = send_pack ["player_id"]+ ":" +send_pack ["animation"];

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
//////////////////////////////////////////////DISCONNECTION FUNCTION HANDLER////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	 /// <summary>
	/// Receive the player's disconnection.
	/// </summary>
	/// <param name="data">received package from client.</param>
	void OnReceiveDisconnect(UDPEvent data )
	{
		/*
		     * data.pack[0]= CALLBACK_NAME: "disconnect"
		     * data.pack[1]= player_id
		     * data.pack[2]= isMasterServer (true or false)
		    */

	   
	   if (connectedClients.ContainsKey (data.pack [1])) {
	   
	    Debug.Log("user: "+connectedClients [data.pack [1]].name+" try to disconnect");
		
	    Dictionary<string, string> send_pack = new Dictionary<string, string> ();
			
	   //JSON package
		send_pack ["callback_name"] = "USER_DISCONNECTED";
			
		send_pack ["msg"] = connectedClients [data.pack [1]].id.ToString ();

		send_pack ["isMasterServer"] = data.pack [2];

		var response = string.Empty;

		byte[] msg = null;
		
		//sends the client sender to all clients in game
		foreach (KeyValuePair<string, Client> entry in connectedClients) {

			if (entry.Value.id != connectedClients [data.pack [1]].id ) {

			  
				//format the data with the sifter comma for they be send from turn to udp client
			
		       response = send_pack ["callback_name"] + ':' + send_pack ["msg"]+':'+send_pack ["isMasterServer"] ;

				msg = Encoding.ASCII.GetBytes (response);

				//send answer to all clients in connectClients list
				udpClient.EmitToClient( msg,entry.Value.remoteEP);

			}//END_IF

		}//END_FOREACH
		
		
		connectedClients.Remove (data.pack [1]);
		
		
	   }//END_IF
		
	}


	
	}

}

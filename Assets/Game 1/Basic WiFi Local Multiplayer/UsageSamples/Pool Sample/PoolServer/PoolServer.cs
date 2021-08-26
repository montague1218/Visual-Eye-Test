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


namespace PoolServerModule
{

 public class PoolServer : MonoBehaviour {

  public static PoolServer instance;

  //from UDP Client Module API
  private UDPComponent udpClient;
		
  public int serverSocketPort;

  UdpClient udpServer;

  private readonly object udpServerLock = new object();

  private readonly object connectedClientsLock = new object();

  static private readonly char[] Delimiter = new char[] {':'};

  private const int bufSize = 8 * 1024;

  private State state = new State();

  private IPEndPoint endPoint;

  private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);

  private AsyncCallback recv = null;

  public enum UDPServerState {DISCONNECTED,CONNECTED,ERROR,SENDING_MESSAGE};

  public UDPServerState udpServerState;

  public string[] pack;

  private Thread tListenner;

  public string serverHostName;

  string receivedMsg = string.Empty;

  private bool stopServer = false;

  public int serverPort = 3310;

  public bool tryCreateServer;

  public bool waitingAnswer;

  public bool serverRunning;

	
  public int onlinePlayers;
  
  public string playerOne;
  
  public string playerTwo;
  
  public string currentPlayer;


  
  //store all players in game
  public Dictionary<string, Client> connectedClients = new Dictionary<string, Client>();
  
  public Dictionary<string, string> ballsPocketed = new Dictionary<string, string>();


   public class Client
	{
		public string  id;

		public string type;
		
		public int score;
		
		public Vector3 position;

		public Quaternion rotation;

		public float timeOut = 0f;

		public IPEndPoint remoteEP;

	}

	public class State
	{
		public byte[] buffer = new byte[bufSize];
    }
	
	public void Awake()
	{
		udpServerState = UDPServerState.DISCONNECTED;

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
			udpClient.On ( "UPDATE_CHANGE_TURN", OnReceiveChangeTurn);
			udpClient.On ("SEND_POS_AND_ROT", OnReceiveCuePosAndRot);
			udpClient.On ("SEND_BALL_POS",OnReceiveBallPos);
			udpClient.On ("SEND_CUE_FORCE",  OnReceiveCueForce);
			udpClient.On ("DESTROY_BALL", OnReceiveDestroyBall);
			udpClient.On ("UPDATE_GAME_OVER", OnReceiveGameOver);
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
			udpClient.StartServer();
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
		 * pack[0] = CALLBACK_NAME: "JOIN_GAME"
		 * pack[1] = player id
		*/
			

	 try
	{
		if (!connectedClients.ContainsKey (data.pack [1])) {
		
	
			Client client = new Client ();

			client.id = data.pack [1];//set client id
			  
			//set  clients's port and ip address
			client.remoteEP = data.anyIP;

			Debug.Log ("[INFO] player " + client.id + ": logged!");

			//add client in search engine
			connectedClients.Add (client.id.ToString (), client);

			onlinePlayers = connectedClients.Count;
				
			Debug.Log ("[INFO] Total players: " + connectedClients.Count);
		
			IPEndPoint playerIP = data.anyIP;

		
			//first player connected
			if(onlinePlayers == 1)
			{
			   playerOne = client.id;
			   
			   currentPlayer = playerOne;
			   
			   connectedClients[client.id].type = "solid";

			   EmitResponse(client,data.anyIP); // sends feedback to the player

            }//END_IF
			else if(onlinePlayers <= 2) // already exist a connected player waiting
			{
				  	
		     EmitStartGameInBroadcast(client); // spawn the current player for all online players

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
			  
		 send_pack ["myType"] = "solid";
				
		 //store  player info in msg field
		 send_pack ["msg"] = "player joined!";
		 
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
	/// sends the game client that opened the communicationt to all online players.
	/// </summary>
	void EmitStartGameInBroadcast(Client client)
	{

		Dictionary<string, string> send_pack = new Dictionary<string, string> ();

		var response = string.Empty;

	    byte[] msg = null;

		send_pack ["callback_name"] = "START_GAME";
			  
		connectedClients[client.id].type = "stripe";
			  
		playerTwo = client.id;
			   
		//store  player info in msg field
		send_pack ["msg"] = "starting game for 2 players connected!";

		//sends the client sender to all clients in game
		foreach (KeyValuePair<string, Client> entry in connectedClients) {

			
			//format the data with the sifter comma for they be send from turn to udp client
			response = send_pack  ["callback_name"] + ':' + send_pack  ["msg"];

			msg = Encoding.ASCII.GetBytes (response);

			//send answer to all clients in connectClients list
			udpClient.EmitToClient( msg,entry.Value.remoteEP);

		}//END_FOREACH
	
	}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    
	/// <summary>
	/// Receive change request.
	/// </summary>
	/// <param name="data">received package from client.</param>
	void OnReceiveChangeTurn(UDPEvent data )
	{
			
			/*
		        * pack[0] = CALLBACK_NAME: "CHANGE_TURN"
		        * pack[1] = player_id
				* pack[2] = player_type
				* pack[3] = i
				* pack[4] = j
		    */
          try{
			Debug.Log("receive change turn");
			
			if(ballsPocketed.Count > 0)
			{
			  Debug.Log("ball pocked");
			
			  foreach(KeyValuePair<string, string> entry in ballsPocketed)
			  {
			  
				//solid ball
				if (int.Parse(entry.Value)<= 7 && connectedClients[entry.Key].type.Equals("solid"))
				{
					currentPlayer = entry.Key;//keep the player
					Debug.Log("keep player");
					
				}
				
				if (int.Parse(entry.Value)<= 7 && !connectedClients[entry.Key].type.Equals("solid"))
				{
					 if(currentPlayer.Equals(playerOne))
			         {
			            currentPlayer = playerTwo;
						Debug.Log("player two");
			         }
			         else
			         {
					   Debug.Log("player one");
			           currentPlayer = playerOne;
			         }
					
				
				}
				
				
				//stripe ball
				if (int.Parse(entry.Value)>=9  && connectedClients[entry.Key].type.Equals("stripe"))
				{
					currentPlayer = entry.Key;//keep the player	
					
				}
				
				//stripe ball
				if (int.Parse(entry.Value)>=9  && !connectedClients[entry.Key].type.Equals("stripe"))
				{
					 if(currentPlayer.Equals(playerOne))
			         {
			            currentPlayer = playerTwo;
			         }
			         else
			         {
			           currentPlayer = playerOne;
			         }
					
				}
			  }
			}
			else
			{
			  if(currentPlayer.Equals(playerOne))
			  {
			   Debug.Log("now is player 2");
			   currentPlayer = playerTwo;
			  }
			  else
			 {
			   currentPlayer = playerOne;
			   Debug.Log("now is player 1");
			 }
			}
			
			
			ballsPocketed.Clear();
			Dictionary<string, string> send_pack = new Dictionary<string, string>();
			
			var response = string.Empty;
			
			byte[] msg = null;

			
			//JSON package
			send_pack ["callback_name"] = "CHANGE_TURN";

			send_pack ["player_id"] = currentPlayer;
			
			//sends the client sender to all clients in game
			foreach (KeyValuePair<string, Client> entry in connectedClients) {

				Debug.Log("send change turn in broadcast to both players");
				
				//format the data with the sifter comma for they be send from turn to udp client
			    response = send_pack ["callback_name"] +':'+send_pack ["player_id"];

				msg = Encoding.ASCII.GetBytes (response);

				 //send answer to all clients in connectClients list
			    udpClient.EmitToClient( msg,entry.Value.remoteEP);
					
			}//END_FOREACH
		  }
		  catch(Exception e)
		  {
			  Debug.Log(e.ToString());
		  }
		
	}
	

    /// <summary>
	/// receive ball position and send in broadcast to all online players.
	/// </summary>
	/// <param name="data">received package from any client.</param>
		void OnReceiveBallPos(UDPEvent data )
		{
			/*
		      * data.pack[0] = CALLBACK_NAME: "MOVE"
		      * data.pack[1] = player_id
		      * data.pack[2] = position.x
		      * data.pack[3] = position.y
		      * data.pack[4] = possition.z

		    */


			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			Dictionary<string, string> data2 = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;
			

			//JSON package
			send_pack ["callback_name"] = "UPDATE_BALL_POS";
					
			send_pack ["id "] = data.pack [2];

			send_pack ["position"] = data.pack [3] + ":" + data.pack [4] ;
					
			send_pack ["rotation"] = data.pack [5] + ":" + data.pack [6]+":"+data.pack [7]+":"+data.pack [8] ;
					
			send_pack ["msg"] = send_pack ["id "] +":"+ send_pack ["position"]+":"+send_pack ["rotation"];

			response = send_pack ["callback_name"] + ':' + send_pack ["msg"];

		    msg = Encoding.ASCII.GetBytes (response);
			
			//send answer in broadcast
			foreach (KeyValuePair<string, Client> entry in connectedClients) {

				if(!entry.Value.id.Equals(connectedClients [data.pack [1]].id))
				 {
				  //send answer to all clients in connectClients list
			      udpClient.EmitToClient( msg,entry.Value.remoteEP);
				 }

				}//END_FOREACH

		  
		}
		
		 /// <summary>
	     /// receive destroy ball notification.
	    /// </summary>
	    /// <param name="data">received package from any client.</param>
		void OnReceiveDestroyBall(UDPEvent data )
		{
			/*
		      * data.pack[0] = CALLBACK_NAME: "MOVE"
		      * data.pack[1] = player_id
		      * data.pack[2] = position.x
		      * data.pack[3] = position.y
		      * data.pack[4] = possition.z

		    */
			
			ballsPocketed.Add (currentPlayer, data.pack [2]);
			Debug.Log("add ball: "+ballsPocketed[currentPlayer]+" from: "+currentPlayer);
			 //player solid wins
			 Dictionary<string, string> send_pack = new Dictionary<string, string>();
			int ballNumber = int.Parse(data.pack [2]);
			var response = string.Empty;
			
			byte[] msg = null;

		   if(ballNumber<8)
	       {
		     
		     foreach (KeyValuePair<string, Client> entry in connectedClients) {

				 if(entry.Value.type.Equals("solid"))
				  {
				    entry.Value.score+=1;
				  }
		     }
			 
			
			 PoolGameManager.instance.pocketedSolidBalls+=1;
	    
	       }
	       else if(ballNumber>8)
	       {
		     foreach (KeyValuePair<string, Client> entry in connectedClients) {

				 if(entry.Value.type.Equals("stripe"))
				  {
				    entry.Value.score+=1;
				  }
			 }
			 PoolGameManager.instance.pocketedStripeBalls+=1;
	    
	       }
		   
		   
		   if(ballNumber == 8)
		   {
		      //player solid wins
			   Dictionary<string, string> game_over_pack = new Dictionary<string, string>();
			
			   //JSON package
			   game_over_pack ["callback_name"] = "UPDATE_GAME_OVER";
			
			   
		       if(connectedClients [data.pack [1]].type.Equals("solid")&& PoolGameManager.instance.pocketedSolidBalls ==7)
			   {
	
				  game_over_pack ["winner"] = "solid";

			
			     //sends the client sender to all clients in game
			     foreach (KeyValuePair<string, Client> entry in connectedClients) {

					  Debug.Log("send game over");
					  //format the data with the sifter comma for they be send from turn to udp client
					  response = game_over_pack ["callback_name"]+':'+game_over_pack ["winner"];

					  msg = Encoding.ASCII.GetBytes (response);

			          //send answer to all clients in connectClients list
			          udpClient.EmitToClient( msg,entry.Value.remoteEP);

				}//END_FOREACH
				
				connectedClients.Clear();//clear the players list
			 }
			 else if(connectedClients[data.pack [1]].type.Equals("solid")&& PoolGameManager.instance.pocketedSolidBalls <7)
			 {
			   // player solid loses
			    game_over_pack ["winner"] = "stripe";

			
			     //sends the client sender to all clients in game
			     foreach (KeyValuePair<string, Client> entry in connectedClients) {


	
					  Debug.Log("send game over");
					  //format the data with the sifter comma for they be send from turn to udp client
					  response =  game_over_pack ["callback_name"]+':'+ game_over_pack ["winner"];

					  msg = Encoding.ASCII.GetBytes (response);

					  
			          //send answer to all clients in connectClients list
			          udpClient.EmitToClient( msg,entry.Value.remoteEP);

				}//END_FOREACH
				
				connectedClients.Clear();//clear the players list
			 }
			 else if(connectedClients[data.pack [1]].type.Equals("stripe")&& 
			 PoolGameManager.instance.pocketedStripeBalls == 7)
			 {
			   // player stripe wins
			    game_over_pack ["winner"] = "stripe";

			
			     //sends the client sender to all clients in game
			     foreach (KeyValuePair<string, Client> entry in connectedClients) {


	
					  Debug.Log("send game over");
					  //format the data with the sifter comma for they be send from turn to udp client
					  response =  game_over_pack ["callback_name"]+':'+ game_over_pack ["winner"];

					  msg = Encoding.ASCII.GetBytes (response);
 
			         //send answer to all clients in connectClients list
			         udpClient.EmitToClient( msg,entry.Value.remoteEP);

				}//END_FOREACH
				
				connectedClients.Clear();//clear the players list
			 }
			  else if(connectedClients[data.pack [1]].type.Equals("stripe")&& PoolGameManager.instance.pocketedSolidBalls <7)
			 {
			   // player stripe loses
			    game_over_pack ["winner"] = "solid";

			
			     //sends the client sender to all clients in game
			     foreach (KeyValuePair<string, Client> entry in connectedClients) {


					  Debug.Log("send game over");
					  //format the data with the sifter comma for they be send from turn to udp client
					  response =  game_over_pack ["callback_name"]+':'+ game_over_pack ["winner"];

					  msg = Encoding.ASCII.GetBytes (response);

					  
			          //send answer to all clients in connectClients list
			          udpClient.EmitToClient( msg,entry.Value.remoteEP);

				}//END_FOREACH
				
				connectedClients.Clear();//clear the players list
			 }
			 
			  PoolGameManager.instance.pocketedSolidBalls = 0;
			  
			  PoolGameManager.instance.pocketedStripeBalls = 0;
			 
		   }
		   else
		   {
		     
			Dictionary<string, string> data2 = new Dictionary<string, string>();

			
			//JSON package
			send_pack ["callback_name"] = "UPDATE_DESTROY_BALL";
					
			send_pack ["ball_id"] = data.pack [2];
					
			send_pack ["score_solid"] = PoolGameManager.instance.pocketedSolidBalls.ToString();
					
			send_pack ["score_stripe"] = PoolGameManager.instance.pocketedStripeBalls.ToString();
	
			send_pack ["msg"] = send_pack ["ball_id"] +":"+send_pack ["score_solid"] +":"+send_pack ["score_stripe"];

			response = send_pack ["callback_name"] + ':' + send_pack ["msg"];

			msg = Encoding.ASCII.GetBytes (response);
			


			//send answer in broadcast
			foreach (KeyValuePair<string, Client> entry in connectedClients) {
				
			  //send answer to all clients in connectClients list
			  udpClient.EmitToClient( msg,entry.Value.remoteEP);
				  

			}//END_FOREACH
		}

			

		  
		}

	/// <summary>
	/// Receive game over request.
	/// </summary>
	/// <param name="data">received package from client.</param>
	void OnReceiveGameOver(UDPEvent data )
	{
			
			/*
		        * pack[0] = CALLBACK_NAME: "GAME_OVER"
		        * pack[1] = player_id
		    */

			Debug.Log("receive game over");
			
			Dictionary<string, string> send_pack = new Dictionary<string, string>();
			
			send_pack ["player_id"] = data.pack[1];
			
			var response = string.Empty;
			
			byte[] msg = null;

			
			//JSON package
			send_pack ["callback_name"] = "GAME_OVER";

			
			//sends the client sender to all clients in game
			foreach (KeyValuePair<string, Client> entry in connectedClients) {


	
				Debug.Log("send game over");
				//format the data with the sifter comma for they be send from turn to udp client
				response = send_pack ["callback_name"]+':'+send_pack ["player_id"];

				msg = Encoding.ASCII.GetBytes (response);

				//send answer to all clients in connectClients list
			    udpClient.EmitToClient( msg,entry.Value.remoteEP); 

				}//END_FOREACH
				
				connectedClients.Clear();//clear the players list
			
		
	}
	
	
	
	/// <summary>
	/// Receive cue position.
	/// </summary>
	/// <param name="data">received package from client.</param>
		void OnReceiveCuePos(UDPEvent data)
		{
			/*
		      * data.pack[0] = CALLBACK_NAME: "MOVE"
		      * data.pack[1] = player_id
		      * data.pack[2] = position.x
		      * data.pack[3] = position.y
		      * data.pack[4] = possition.z

		    */


			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			Dictionary<string, string> data2 = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;
		
				if (connectedClients.ContainsKey (data.pack [1])) {

					connectedClients [data.pack [1]].timeOut = 0f;
					
					//JSON package
					send_pack ["callback_name"] = "UPDATE_CUE_POS";

				
					send_pack ["position"] = data.pack [2] + ":" +data.pack [3]+ ":" + data.pack [4];

					send_pack ["msg"] =   send_pack ["position"];

					response = send_pack ["callback_name"] + ':' + send_pack ["msg"];

					msg = Encoding.ASCII.GetBytes (response);
				}


				//send answer in broadcast
				foreach (KeyValuePair<string, Client> entry in connectedClients) {
				
				  if(!entry.Value.id.Equals(connectedClients [data.pack [1]].id))
				  {
				       //send answer to all clients in connectClients list
			           udpClient.EmitToClient( msg,entry.Value.remoteEP); 
				  }

				 

				}//END_FOREACH

		}
		
	
	/// <summary>
	/// Receive cue force.
	/// </summary>
	/// <param name="data">received package from client.</param>
		void OnReceiveCueForce(UDPEvent data )
		{
			/*
		      * data.pack[0] = CALLBACK_NAME: "MOVE"
		      * data.pack[1] = player_id
		      * data.pack[2] = position.x
		      * data.pack[3] = position.y
		      * data.pack[4] = possition.z

		    */
          try{

			  Debug.Log("receive cue force");

			 

			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			Dictionary<string, string> data2 = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;
		
				if (connectedClients.ContainsKey (data.pack [1])) {

					
					//JSON package
					send_pack ["callback_name"] = "UPDATE_CUE_FORCE";

			
					send_pack ["force"] = data.pack [2];
					
					
					send_pack ["msg"] =   send_pack ["force"];

					response = send_pack ["callback_name"] + ':' + send_pack ["msg"];

					msg = Encoding.ASCII.GetBytes (response);
				}


				//send answer in broadcast
				foreach (KeyValuePair<string, Client> entry in connectedClients) {
				
				  if(!entry.Value.id.Equals(connectedClients [data.pack [1]].id))
				  {
					
				    //send answer to all clients in connectClients list
			        udpClient.EmitToClient( msg,entry.Value.remoteEP);
					 Debug.Log("server cue force sended");
				  }

				 

				}//END_FOREACH
		  }
		  catch(Exception e)
		  {
			  Debug.Log(e.ToString());
		  }

		}


			
		 /// <summary>
	 /// receive the cue position and rotation and send in broadcast to all online players.
    /// </summary>
    /// <param name="data">received package from any client.</param>
		void OnReceiveCuePosAndRot(UDPEvent data )
		{
			/*
		      * data.pack[0] = CALLBACK_NAME: "ROTATE"
		      * data.pack[1] = player_id
		      * data.pack[2] = rotation.x
		      * data.pack[3] = rotation.y
		      * data.pack[4] = rotation.z
		      * data.pack[5] = rotation.w

		    */

			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			Dictionary<string, string> data2 = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;

			if (connectedClients.ContainsKey (data.pack [1])) {

				connectedClients [data.pack [1]].timeOut = 0f;
				
				connectedClients [data.pack [1]].position = new Vector3 (float.Parse (data.pack [2]), 0
				, float.Parse (data.pack [3]));
						
						
				connectedClients [data.pack [1]].rotation = new Quaternion (float.Parse (data.pack [4]),
				 float.Parse (data.pack [5])
				, float.Parse (data.pack [6]), float.Parse (data.pack [7]));

				//JSON package
				send_pack ["callback_name"] = "UPDATE_CUE_POS_AND_ROT";
				
					Vector3 position = new Vector3 (connectedClients [data.pack [1]].position.x,
						                   connectedClients [data.pack [1]].position.y, 
										   connectedClients [data.pack [1]].position.z);
										   
										   
			  send_pack ["position"] = data.pack [2] + ":"+ data.pack [3];

			
				send_pack ["rotation"] = data.pack [4] + ":" +
				data.pack [5] +
				":" + data.pack [6] + ":" + data.pack [7];
				//store "pong!!!" message in msg field
				
				}
				send_pack ["msg"] =  send_pack ["position"]+":"+send_pack ["rotation"];

				response = send_pack ["callback_name"] + ':' + send_pack ["msg"];

				msg = Encoding.ASCII.GetBytes (response);

				//send answer in broadcast
				foreach (KeyValuePair<string, Client> entry in connectedClients) {

				 if(!entry.Value.id.Equals(connectedClients [data.pack [1]].id))
				  {
				   
				    //send answer to all clients in connectClients list
			        udpClient.EmitToClient( msg,entry.Value.remoteEP);
				  }

				}//END_FOREACH

		}


    
	/// <summary>
	/// Receive disconnection request.
	/// </summary>
	/// <param name="data">received package from client.</param>
		void OnReceiveDisconnect(UDPEvent data )
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

}

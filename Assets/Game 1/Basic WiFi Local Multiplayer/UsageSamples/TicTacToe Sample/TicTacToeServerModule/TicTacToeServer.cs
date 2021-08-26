using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using  UDPCore;


namespace TicTacToeServerModule
{

	public class TicTacToeServer : MonoBehaviour
	{


		public static TicTacToeServer instance;

		//from UDP Client Module API
		private UDPComponent udpClient;

		string receivedMsg = string.Empty;

		private bool stopServer = false;

		public int serverPort = 3310;

		public bool tryCreateServer;

		public bool waitingAnswer;

		public bool serverRunning;

		public int onlinePlayers;

		//store all players in game
		public Dictionary<string, Client> connectedClients = new Dictionary<string, Client>();


		public class Client
		{
			public string id;

			public string type;

			public float timeOut = 0f;

			public IPEndPoint remoteEP;

		}



		// Use this for initialization
		void Start()
		{

			// if don't exist an instance of this class
			if (instance == null)
			{

				//it doesn't destroy the object, if other scene be loaded
				DontDestroyOnLoad(this.gameObject);

				instance = this;// define the class as a static variable

				udpClient = gameObject.GetComponent<UDPComponent>();
				udpClient.On("PING", OnReceivePing);
				udpClient.On("JOIN_GAME", OnReceiveJoinGame);
				udpClient.On("SERVER_UPDATE_BOARD", OnReceiveUpdateBoard);
				udpClient.On("SERVER_GAME_OVER", OnReceiveGameOver);
				udpClient.On("disconnect", OnReceiveDisconnect);
				udpClient.On("DISPLAY_ANSWER", OnDisplayAnswer);
				udpClient.On("UPDATE_SPOT", OnUpdateSpotLocation);
				udpClient.On("UPDATE_PATTERN", OnUpdateEyePattern);
				udpClient.On("UPDATE_UI", OnUpdateUI);
				udpClient.On("AB", OnAB);
				udpClient.On("UI", OnUI);
			}
			else
			{
				//it destroys the class if already other class exists
				Destroy(this.gameObject);
			}

		}

		public void OnAB(UDPEvent data)
		{
			string response = FormatResponse(data.pack[0], data.pack[1]);
			byte[] msg = Encoding.ASCII.GetBytes(response);

			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				//sends the client sender to all clients in game
				udpClient.EmitToClient(msg, entry.Value.remoteEP);
			}
		}

		public void OnUI(UDPEvent data)
		{
			string response = FormatResponse(data.pack[0], data.pack[1]);
			byte[] msg = Encoding.ASCII.GetBytes(response);

			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				//sends the client sender to all clients in game
				udpClient.EmitToClient(msg, entry.Value.remoteEP);
			}
		}


		/// <summary>
		/// Creates a UDP Server in in the associated client
		/// called method when the button "start" on HUDCanvas is pressed
		/// </summary>
		public void CreateServer()
		{
			if (udpClient.GetIP() != string.Empty)
			{

				if (TicTacNetworkManager.instance.serverFound && !udpClient.serverRunning)
				{
					TicTacCanvasManager.instance.ShowAlertDialog("There are server already running on the network!");

				}

				else
				{
					if (!udpClient.serverRunning)
					{

						//start server
						udpClient.StartServer();
						Debug.Log("UDP Server listening on IP " + udpClient.GetIP() + " and port " + udpClient.serverPort);

						Debug.Log("------- server is running -------");

						TicTacCanvasManager.instance.txtSearchServerStatus.text = "Server is running";
					}
					else
					{
						TicTacCanvasManager.instance.ShowAlertDialog("Server already running on the network!");
					}

				}//END_ELSE

			}
			else
			{
				TicTacCanvasManager.instance.ShowAlertDialog("Please connect to a Wifi network");
			}


		}

		private string FormatResponse(string callback_name, string content)
		{
			//format the data with the sifter comma for they be send from turn to udp client
			return string.Format("{0}:{1}", callback_name, content);
		}

		public string generateID()
		{
			return Guid.NewGuid().ToString("N");
		}

		private void OnDisplayAnswer(UDPEvent data)
        {
			string response = FormatResponse(data.pack[0], data.pack[1]);
			byte[] msg = Encoding.ASCII.GetBytes(response);
			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				//sends the client sender to all clients in game
				udpClient.EmitToClient(msg, entry.Value.remoteEP);
			}
		}

		private void OnUpdateIntro(UDPEvent data)
		{
			/*
				* pack[0] = CALLBACK_NAME: "UPDATE_UI"
				* pack[1] = intro number
			*/

			string response = FormatResponse(data.pack[0], data.pack[1]);
			byte[] msg = Encoding.ASCII.GetBytes(response);
			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				//sends the client sender to all clients in game
				udpClient.EmitToClient(msg, entry.Value.remoteEP);
			}
		}


		private void OnUpdateUI(UDPEvent data)
		{
			/*
				* pack[0] = CALLBACK_NAME: "UPDATE_UI"
				* pack[1] = UI_name
			*/

			string response = FormatResponse(data.pack[0], data.pack[1]);
			byte[] msg = Encoding.ASCII.GetBytes(response);

			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				//sends the client sender to all clients in game
				udpClient.EmitToClient(msg, entry.Value.remoteEP);
			}
		}

		private void OnUpdateSpotLocation(UDPEvent data)
		{

			/*
		        * pack[0] = CALLBACK_NAME: "UPDATE_SPOT"
		        * pack[1] = x
				* pack[2] = y
				* pack[3] = z
				* pack[4] = visible (bool)
		    */

			//QuizManager.instance.logger.text = "receive spot:" + pack[1] + "," + pack[2] + "," + pack[3];

			string content = string.Format("{0}:{1}:{2}:{3}:{4}", data.pack[1], data.pack[2], data.pack[3], data.pack[4], data.pack[5]);
			string response = FormatResponse("UPDATE_SPOT", content);
			byte[] msg = Encoding.ASCII.GetBytes(response);

			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				//sends the client sender to all clients in game
				udpClient.EmitToClient(msg, entry.Value.remoteEP);
			}
		}

		private void OnUpdateEyePattern(UDPEvent data)
        {
			string response = FormatResponse(data.pack[0], data.pack[1]);
			byte[] msg = Encoding.ASCII.GetBytes(response);

			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				//sends the client sender to all clients in game
				udpClient.EmitToClient(msg, entry.Value.remoteEP);
			}
		}

		/// <summary>
		/// Receive ping request.
		/// </summary>
		/// <param name="data">received package from client.</param>
		void OnReceivePing(UDPEvent data)
		{
			/*
		       * data.pack[0]= CALLBACK_NAME: "PONG"
		       * data.pack[1]= "ping"
		    */

			Debug.Log("receive ping from game client");

			try
			{

				Dictionary<string, string> send_pack = new Dictionary<string, string>();

				//JSON package
				send_pack["callback_name"] = "PONG";

				//store  player info in msg field
				send_pack["msg"] = "pong!!!";

				var response = string.Empty;

				byte[] msg = null;

				response = send_pack["callback_name"] + ':' + send_pack["msg"];

				msg = Encoding.ASCII.GetBytes(response);

				//send answer to client that called server
				udpClient.EmitToClient(msg, data.anyIP);

				Debug.Log("[INFO] PONG message sended to connected player");

			}//END_TRY
			catch (Exception e)
			{
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
		void OnReceiveJoinGame(UDPEvent data)
		{

			/*
			 * pack[0] = CALLBACK_NAME: "JOIN_GAME"
			 * pack[1] = player id
			*/

			try
			{
				if (!connectedClients.ContainsKey(data.pack[1]))
				{


					Client client = new Client();

					client.id = data.pack[1];//set client id

					//set  clients's port and ip address
					client.remoteEP = data.anyIP;

					Debug.Log("[INFO] player " + client.id + ": logged!");

					//add client in search engine
					connectedClients.Add(client.id.ToString(), client);

					onlinePlayers = connectedClients.Count;

					Debug.Log("[INFO] Total players: " + connectedClients.Count);

					IPEndPoint playerIP = data.anyIP;

					QuizModule.DebugPanel.instance.SetLogger(1, "RE JOIN " + onlinePlayers);
					//first player connected
					if (onlinePlayers == 1)
					{

						EmitResponse(data.anyIP); // sends feedback to the player

					}//END_IF
					else if (onlinePlayers <= 2) // already exist a connected player waiting
					{

						EmitStartGameInBroadcast(client); // spawn the current player for all online players

					}


				}//END_IF

			}//END_TRY
			catch (Exception e)
			{
				Debug.LogError(e.ToString());

			}
		}


		/// <summary>
		/// sends feedback to the game client that opened the communicationt.
		/// </summary>
		void EmitResponse(IPEndPoint playerIP)
		{

			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			//JSON package
			send_pack["callback_name"] = "JOIN_SUCCESS";

			//store  player info in msg field
			send_pack["msg"] = "player joined!";

			var response = string.Empty;

			byte[] msg = null;

			//format the data with the sifter comma for they be send from turn to udp client
			response = send_pack["callback_name"] + ':' + send_pack["msg"];

			msg = Encoding.ASCII.GetBytes(response);

			//send answer to client that called me 
			udpClient.EmitToClient(msg, playerIP);

			Debug.Log("[INFO]sended to connected player : JOIN_SUCCESS");

		}



		/// <summary>
		/// sends the game client that opened the communicationt to all online players.
		/// </summary>
		void EmitStartGameInBroadcast(Client client)
		{

			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;

			send_pack["callback_name"] = "START_GAME";

			//store  player info in msg field
			send_pack["msg"] = "starting game for 2 players connected!";


			//sends the client sender to all clients in game
			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{


				//format the data with the sifter comma for they be send from turn to udp client
				response = send_pack["callback_name"] + ':' + send_pack["msg"];

				msg = Encoding.ASCII.GetBytes(response);

				//send answer to all clients in connectClients list
				udpClient.EmitToClient(msg, entry.Value.remoteEP);

			}//END_FOREACH

		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////




		void OnReceiveUpdateBoard(UDPEvent data)
		{

			/*
		        * pack[0] = CALLBACK_NAME: "UPDATE_BOARD"
		        * pack[1] = player_id
				* pack[2] = player_type
				* pack[3] = i
				* pack[4] = j
		    */

			Debug.Log("receive update board");

			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;


			//JSON package
			send_pack["callback_name"] = "UPDATE_BOARD";

			send_pack["player_id"] = data.pack[1];

			send_pack["player_type"] = data.pack[2];

			send_pack["i"] = data.pack[3];

			send_pack["j"] = data.pack[4];

			//sends the client sender to all clients in game
			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{


				Debug.Log("send update board");

				//format the data with the sifter comma for they be send from turn to udp client
				response = send_pack["callback_name"] + ':' + send_pack["player_id"] + ':' +
																	 send_pack["player_type"] + ':' + send_pack["i"] + ':' + send_pack["j"];

				msg = Encoding.ASCII.GetBytes(response);

				//send answer to all clients in connectClients list
				udpClient.EmitToClient(msg, entry.Value.remoteEP);

			}//END_FOREACH



		}


		void OnReceiveGameOver(UDPEvent data)
		{

			/*
		        * pack[0] = CALLBACK_NAME: "GAME_OVER"
		        * pack[1] = player_id
		    */

			Debug.Log("receive game over");

			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			send_pack["player_id"] = data.pack[1];

			var response = string.Empty;

			byte[] msg = null;


			//JSON package
			send_pack["callback_name"] = "GAME_OVER";


			//sends the client sender to all clients in game
			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{

				Debug.Log("send game over");
				//format the data with the sifter comma for they be send from turn to udp client
				response = send_pack["callback_name"] + ':' + send_pack["player_id"];

				msg = Encoding.ASCII.GetBytes(response);

				//send answer to all clients in connectClients list
				udpClient.EmitToClient(msg, entry.Value.remoteEP);

			}//END_FOREACH

			connectedClients.Clear();//clear the players list
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
			send_pack["callback_name"] = "USER_DISCONNECTED";

			send_pack["msg"] = data.pack[1];

			send_pack["isMasterServer"] = data.pack[2];

			response = send_pack["callback_name"] + ':' + send_pack["msg"] + ':' + send_pack["isMasterServer"];

			msg = Encoding.ASCII.GetBytes(response);

			//sends the client sender to all clients in game
			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{

				Debug.Log("send disconnect");

				//send answer to all clients in connectClients list
				udpClient.EmitToClient(msg, entry.Value.remoteEP);

			}//END_FOREACH

			connectedClients.Clear();//clear the players list


		}
	}
}

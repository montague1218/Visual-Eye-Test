using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using UDPCore;
using PingServerModule;

public class PingNetworkManager : MonoBehaviour {
//from UDP Socket API
	private UDPComponent udpClient;

	//useful for any gameObject to access this class without the need of instances her or you declare her
	public static PingNetworkManager instance;

	public int serverPort = 3310;

	public int clientPort = 3000;

	public List<string> _localAddresses { get; private set; }


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
	/// Connect client to Server.cs
	/// </summary>
	public void ConnectToUDPServer()
	{
		if (udpClient.GetServerIP () != string.Empty) {

			//connect to Server
			udpClient.connect (udpClient.GetServerIP (), serverPort, clientPort);

			udpClient.On ("PONG", OnPrintPongMsg);

		}


	}

	void Update(){}



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

		Debug.Log("Receive pong from server");

		PingCanvasManager.instance.ShowAlertDialog("Receive "+data.pack[1]+" from server");


	}




	// <summary>
	/// sends ping message to UDPServer.
	///     case "PING":
	///     OnReceivePing(pack,anyIP);
	///     break;
	/// take a look in Server.cs script
	/// </summary>
	public void EmitPing() {


			// check if there is a server running
			if (udpClient.serverRunning) {

				//hash table <key, value>	
				Dictionary<string, string> data = new Dictionary<string, string>();

				//JSON package
				data["callback_name"] = "PING";

				//store "ping!!!" message in msg field
				data["msg"] = "ping!!!!";

				//The Emit method sends the mapped callback name to  the server
				udpClient.EmitToServer (data["callback_name"] ,data["msg"]);


			}

			else
			{
				PingCanvasManager.instance.ShowAlertDialog("please start the server");
			}

	}


   


	void CloseApplication()
	{

		if (udpClient != null) {

			udpClient.disconnect ();

		}
	}


	void OnApplicationQuit() {

		Debug.Log("Application ending after " + Time.time + " seconds");

		CloseApplication ();

	}
}

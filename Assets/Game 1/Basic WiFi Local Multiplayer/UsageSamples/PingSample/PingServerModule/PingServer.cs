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


namespace PingServerModule
{

	public class PingServer : MonoBehaviour {

		public static PingServer instance;

		//from UDP Client Module API
		private UDPComponent udpClient;

	

		// Use this for initialization
		void Start () {

			// if don't exist an instance of this class
			if (instance == null) {

				//it doesn't destroy the object, if other scene be loaded
				DontDestroyOnLoad (this.gameObject);

				instance = this;// define the class as a static variable

				udpClient = gameObject.GetComponent<UDPComponent>();
				//receives a "PING" notification from client
			    udpClient.On ("PING", OnReceivePing);

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
		   //start server
			udpClient.StartServer();
			if(udpClient.serverRunning)
			{
			  PingCanvasManager.instance.txtSearchServerStatus.text = "------- server is running -------";
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
		
		}

	

	}

}

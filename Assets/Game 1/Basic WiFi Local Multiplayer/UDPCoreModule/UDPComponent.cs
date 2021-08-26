using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

/// <summary>
/// UDP client component.
/// </summary>
namespace UDPCore
{
	public class UDPComponent : MonoBehaviour {

		private string  serverURL;

		public int serverPort;
		
		int clientPort;

		private UdpClient udpClient;

		private readonly object udpClientLock = new object();

		static private readonly char[] Delimiter = new char[] {':'};

		string receivedMsg = string.Empty;

		private Dictionary<string, List<Action<UDPEvent>>> handlers;

		private Queue<UDPEvent> eventQueue;

		private object eventQueueLock;

		private IPEndPoint endPoint;

		private string listenerInput = string.Empty;

		public enum UDPSocketState {DISCONNECTED,CONNECTED,ERROR,SENDING_MESSAGE};

		public UDPSocketState udpSocketState;

		private Thread tListenner;

		public string serverIP = string.Empty;

		public bool noNetwork;

		public string localNetworkIP;

		/***************************** SERVER VARIABLES **********************************************************/
	UdpClient udpServer;

  private readonly object udpServerLock = new object();

  private const int bufSize = 8 * 1024;

  private State state = new State();

  private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);

  private AsyncCallback recv = null;

  public enum UDPServerState {DISCONNECTED,CONNECTED,ERROR,SENDING_MESSAGE};

  public UDPServerState udpServerState;

  string[] pack;

  private Thread serverListenner;

  private bool stopServer = false;

  public bool serverRunning;

	public class State
	{
		public byte[] buffer = new byte[bufSize];
    }
	 
	/*********************************************************************************************************/


		public void Awake()
		{
			handlers = new Dictionary<string, List<Action<UDPEvent>>>();

			eventQueueLock = new object();

			eventQueue = new Queue<UDPEvent>();

	
			udpSocketState = UDPSocketState.DISCONNECTED;
		}




		/// <summary>
		/// open a connection with the specific server using the server URL (IP) and server Port.
		/// </summary>
		/// <param name="_serverURL">Server IP.</param>
		/// <param name="_serverPort">Server port.</param>
		/// <param name="_clientPort">Client port.</param>
		public void connect(string _serverURL, int _serverPort, int _clientPort) {

		//	Debug.Log ("try connect to server");
			if ( tListenner != null && tListenner.IsAlive) {
				
				disconnect();

				while (tListenner != null && tListenner.IsAlive) {


				}
			}

			//host udp server
			this.serverURL = _serverURL;

			//server port
			this.serverPort = _serverPort;

			//client port
			this.clientPort = _clientPort;
			
			// start  listener thread
			tListenner = new Thread(
				new ThreadStart(OnListeningServer));
			
			tListenner.IsBackground = true;

			tListenner.Start();



		}


		public void  OnListeningServer()
		{

			try
			{
				
				lock ( udpClientLock) {
					
					udpClient = new UdpClient ();

					udpClient.ExclusiveAddressUse = false;

					udpClient.Client.SetSocketOption(
						SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

					IPEndPoint localEp = new IPEndPoint(IPAddress.Any,clientPort);

					udpClient.Client.Bind(localEp);

					udpSocketState = UDPSocketState.CONNECTED;

					udpClient.BeginReceive (new AsyncCallback (OnWaitPacketsCallback), null);


				}

			}
			catch
			{
				
				throw;
			}
		}



		public void OnWaitPacketsCallback(IAsyncResult res)
		{

			lock (udpClientLock) {

				byte[] recPacket = udpClient.EndReceive (res, ref endPoint);
				MessageReceived(recPacket, endPoint.Address.ToString(), endPoint.Port);

				if (recPacket != null && recPacket.Length > 0) {
					lock (eventQueueLock) {

						//decode the received bytes vector in string fotmat
						//receivedMsg = "callback_name,param 1,param 2,param n, etc."
						receivedMsg = Encoding.UTF8.GetString (recPacket);

						//separates the items contained in the package using the two points ":" as sifter
						//and it puts them separately in the vector package []
						/*
		                  * package[0]= callback_name: e.g.: "PONG"
		                  * package[1]= message: e.g.: "pong!!!"
		                  * package[2]=  other message for example!
			            */

						var package = receivedMsg.Split (Delimiter);

						//enqueue
						eventQueue.Enqueue(new UDPEvent(package [0], receivedMsg));

						receivedMsg = string.Empty;	
					}//END_LOCK
				}//END_IF
					
				udpClient.BeginReceive (new AsyncCallback (OnWaitPacketsCallback), null);

			}//END_LOCK
		}


		private void InvokEvent(UDPEvent ev)
		{

			if (!handlers.ContainsKey(ev.name)) { return; }

			foreach (Action<UDPEvent> handler in this.handlers[ev.name]) {
				
				try{

					handler(ev);
				   } 
				catch(Exception ex){}
			}
		}


		public void MessageReceived(byte[] data, string ipHost, int portHost)
		{

			//Debug.Log(string.Format("Received data:: {0} of IP:: {1} and Port:: {2}", Encoding.UTF8.GetString (data), ipHost, portHost));

		}

		/// <summary>
		/// listening server messages.
		/// </summary>
		/// <param name="ev">Callback name</param>
		/// <param name="callback">Callback function.</param>
		public void On(string ev, Action<UDPEvent> callback)
		{
			if (!handlers.ContainsKey(ev)) {
				
				handlers[ev] = new List<Action<UDPEvent>>();
			}
			else
		    {
		 	  Debug.LogError("you already defined a key called "+ev);
			  Debug.LogError("please create e new key with other name");
		    }

			handlers[ev].Add(callback);
		}

		/// <summary>
		/// Emit the pack or message to server.
		/// </summary>
		/// <param name="callbackID">Callback ID.</param>
		/// <param name="_pack">message</param>
		public void EmitToServer(string callbackID, string _pack)
		{

			try{

				if(udpSocketState == UDPSocketState.CONNECTED)
				{
					lock ( udpClientLock) {
						
						if(udpClient == null)
						{
							udpClient = new UdpClient ();

							udpClient.ExclusiveAddressUse = false;

							udpClient.Client.SetSocketOption(
								SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

							IPEndPoint localEp = new IPEndPoint(IPAddress.Any, clientPort);

							udpClient.Client.Bind(localEp);
						}


						udpSocketState = UDPSocketState.SENDING_MESSAGE;

						string new_pack = callbackID+":"+_pack;

						byte[] data = Encoding.UTF8.GetBytes (new_pack.ToString ()); //convert to bytes

						#if UNITY_ANDROID && !UNITY_EDITOR
						
						string broadcastAddress = CaptiveReality.Jni.Util.StaticCall<string>("GetServerIP", "Invalid Response From JNI", "com.rio3dstudios.basicwifilocalmultiplayerplugin.IPManager");
						string subAddress = broadcastAddress.Remove(broadcastAddress.LastIndexOf('.'));

				        serverIP = subAddress + "." + 255;
						var endPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
                     
						#else
					
						var endPoint = new IPEndPoint(IPAddress.Parse(GetServerIP()), serverPort);
                       
						#endif
						
						try{
						
						 udpClient.EnableBroadcast = true;
   	                     
						 udpClient.Send(data, data.Length,endPoint);
   
                         }
                         catch ( Exception e ){

                          Console.WriteLine(e.ToString());	
                         }
						

						udpSocketState = UDPSocketState.CONNECTED;
					}
				}
			}
			catch(Exception e) {
				Debug.Log(e.ToString());
			}
		}


		//get local server ip address
		public string GetServerIP() {

			serverIP = string.Empty;

			string address = string.Empty;

			string subAddress = string.Empty;

			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

			//search WiFI Local Network
			foreach (IPAddress ip in host.AddressList) {
				
				if (ip.AddressFamily == AddressFamily.InterNetwork) {

					if (!ip.ToString ().Contains ("127.0.0.1")) {
						address = ip.ToString ();
					}
						
				}
			}
				
			if (address == string.Empty)
			{
				
				noNetwork = true;

				return string.Empty;
			}
			else
			{
				noNetwork = false;

				subAddress = address.Remove(address.LastIndexOf('.'));

				serverIP = subAddress + "." + 255;

				return subAddress + "." + 255;
			}
			return string.Empty;


		}





		private void OnDestroy() {
			
			disconnect ();
		}

		public void Update()
		{
			lock(eventQueueLock){ 
				
				while(eventQueue.Count > 0)
				{

					InvokEvent(eventQueue.Dequeue());
				}
			}




		}

		

		/// <summary>
		/// Disconnect this client.
		/// </summary>
		public void disconnect() {


			lock (udpClientLock) {
				
				if (udpClient != null) {
					
					udpClient.Close();

					udpClient = null;
				}

			}//END_LOCK

			if (tListenner!=null) {
				
				tListenner.Abort ();
			}

		}


		/************************************ SERVER CODE ***************************************************************/
	public string GetServerStatus()
		{
			switch (udpServerState)
			{
			    case  UDPServerState.DISCONNECTED:
				 return "DISCONNECTED";
				break;

			    case  UDPServerState.CONNECTED:
				 return "CONNECTED";
				break;

			    case  UDPServerState.SENDING_MESSAGE:
				 return "SENDING_MESSAGE";
				break;

			    case  UDPServerState.ERROR:
				 return "ERROR";
				break;
			}

			return string.Empty;
		}

		//get local server ip address
		public string GetIP() {

		
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

			string address = string.Empty;

			string subAddress = string.Empty;

			//search WiFI Local Network
			foreach (IPAddress ip in host.AddressList) {
				
				if (ip.AddressFamily == AddressFamily.InterNetwork) {


					if (!ip.ToString ().Contains ("127.0.0.1")) {
						address = ip.ToString ();
					}

				
				}
			}


			if (address==string.Empty) {
				
				return string.Empty;
			}

			else
			{
				
			    subAddress = address.Remove(address.LastIndexOf('.'));
			
				return subAddress + "." + 255;
			}

			return string.Empty;

		}

		/// <summary>
		/// Creates a UDP Server in in the associated client
		/// called method when the button "start" on HUDCanvas is pressed
		/// </summary>
		public void StartServer()
		{
			
			if (!serverRunning) {
						
			 if ( serverListenner != null && serverListenner.IsAlive) {
				
				CloseServer();

				while (serverListenner != null && serverListenner.IsAlive) {}

			  }
				
			   // start  listener thread
			  serverListenner = new Thread(
				new ThreadStart(OnListeningClients));
			
			   serverListenner.IsBackground = true;

			   serverListenner.Start();

			   serverRunning = true;

			}
							
		}

		
		/// <summary>
		/// Raises the listening clients event.
		/// </summary>
		public void  OnListeningClients()
		{

			try{
			udpServer = new UdpClient (serverPort);

			udpServer.Client.ReceiveTimeout = 300; // msec

		
			while (!stopServer) {
				try {

					udpServerState = UDPServerState.CONNECTED;

					IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

					byte[] data = udpServer.Receive(ref anyIP);

					string text = Encoding.ASCII.GetString(data);

					receivedMsg  = text;

					 lock (eventQueueLock) {
				
					
					//separates the items contained in the package using the two points ":" as sifter
					//and it puts them separately in the vector package []
					/*
		            * package[0]= callback_name: e.g.: "PONG"
		            * package[1]= message: e.g.: "pong!!!"
		            * package[2]=  other message for example!
			        */
					pack = receivedMsg.Split (Delimiter);
					
		            //enqueue
				    eventQueue.Enqueue(new UDPEvent(pack [0], receivedMsg,anyIP));
					receivedMsg = string.Empty;	
				   }



				}//END_TRY
				catch (Exception err)
				{
					//print(err.ToString());
				}
			}//END_WHILE
			}
			catch(Exception e)
			{
				Debug.Log(e.ToString());
			}
		}

		public void EmitToClient(byte[] msg, IPEndPoint remoteEP)
		{
		  try{
		   #if UNITY_STANDALONE_OSX  
		    
			
			udpServer.Send (msg, msg.Length,new IPEndPoint( remoteEP.Address.MapToIPv4(),clientPort));
			
		   #else	
		  
			udpServer.Send (msg, msg.Length, remoteEP); // echo
			
			#endif
			}
			catch(Exception e)
			{
			  Debug.LogError(e.ToString());
			}
		}

		/**
     *  DISCONNECTS SERVER
     */
		public void CloseServer() {

			udpServerState = UDPServerState.DISCONNECTED;	

			stopServer = true;


			if (udpServer != null) 
			{
				udpServer.Close ();
				udpServer = null;
			}

			if (serverListenner!=null) {
				
				serverListenner.Abort ();
			}

		}

		void OnApplicationQuit() {

			CloseServer();

			disconnect ();

		}


	}
}

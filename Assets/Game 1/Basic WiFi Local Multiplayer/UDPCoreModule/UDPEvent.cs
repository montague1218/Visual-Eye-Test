using System.Text.RegularExpressions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
namespace  UDPCore
{
	
	public class UDPEvent
	{
		public string name { get; set; }
		public string[] pack { get; set; }
		public byte[] byteArray { get; set; }
		public IPEndPoint anyIP{ get; set; }
		static private readonly char[] Delimiter = new char[] {':'};

		public UDPEvent(string name) : this(name, null,null,null) { }
		public UDPEvent(string name,string data) : this(name,data, null,null) { }
		public UDPEvent(string name,byte[] bytes) : this(name,null, bytes,null) { }
		public UDPEvent(string name,byte[] bytes,IPEndPoint anyIP) : this(name,null,bytes, anyIP) { }
		public UDPEvent(string name, string data, IPEndPoint anyIP) : this(name,data,null, anyIP) { }
		
		public UDPEvent(string name, string data,byte[] bytes,IPEndPoint anyIP)
		{
			 
			this.name = name;
			if(data!=null)
			{
			 this.pack= data.Split (Delimiter);
			}
			this.byteArray = bytes;
			this.anyIP = anyIP;
		}
		
		
		public override string ToString()
		{
			return string.Format("[UDPNetEvent: name={0}, data={1}]", name, pack.ToString());
		}
	}
}

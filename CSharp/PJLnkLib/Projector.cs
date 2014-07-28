using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using UniversalEditor;
using UniversalEditor.Accessors;
using UniversalEditor.IO;

namespace PJLink
{
	public class Projector
	{
		private TcpClient mvarClient = null;
		private NetworkStream mvarStream = null;
		
		private StreamAccessor mvarAccessor = null;
		private Reader mvarReader = null;
		private Writer mvarWriter = null;
		
		private IPEndPoint mvarEndpoint = null;
		public IPEndPoint Endpoint { get { return mvarEndpoint; } }
		
		private Projector(TcpClient client, IPEndPoint endpoint)
		{
			mvarClient = client;
			mvarEndpoint = endpoint;
		}
		
		public static Projector FromAddress(string hostname)
		{
			int port = 4352;
			string realHostName = hostname;
			if (hostname.Contains (":"))
			{
				string[] hostnameParts = hostname.Split (new string[] { ":" });
				realHostName = hostnameParts[0];
				port = Int32.Parse (hostnameParts[1]);
			}
			return FromAddress (realHostName, port);
		}
		public static Projector FromAddress(string hostname, int port)
		{
			IPAddress addr = null;
			if (IPAddress.TryParse (hostname, out addr))
			{
				return FromAddress(addr, port);
			}
			else
			{
				return FromAddress(Dns.GetHostAddresses(hostname)[0], port);
			}
		}
		public static Projector FromAddress(IPAddress address, int port)
		{
			return FromAddress (new IPEndPoint(address, port));
		}
		public static Projector FromAddress(IPEndPoint ep)
		{
			TcpClient client = new TcpClient();
			return new Projector(client, ep);
		}
		
		private string ByteToHex(byte[] array)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte val in array)
			{
				sb.Append(val.ToString ("X").PadLeft(2, '0').ToUpper ());
			}
			return sb.ToString();
		}
		
		public bool Connect(string password)
		{
			mvarClient.Connect (mvarEndpoint);
			if (mvarClient.Connected)
			{
				mvarStream = mvarClient.GetStream();
				mvarAccessor = new StreamAccessor(mvarStream);
				mvarReader = new Reader(mvarAccessor);
				mvarWriter = new Writer(mvarAccessor);
				
				string w = mvarReader.ReadFixedLengthString (9);
				if (w.Substring (0, 7) != "PJLINK ")
				{
					throw new ProtocolViolationException("Invalid PJLINK response");
				}
				switch (w[7])
				{
					case '0':
					{
						return true;
					}
					case '1':
					{
						w += mvarReader.ReadFixedLengthString (9);
						if (w[8] != ' ')
						{
							throw new ProtocolViolationException("Expected space between header and salt");
						}
						string salt = w.Substring (9, w.Length - 10);
						
						System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create ();
						string md5hash = ByteToHex(md5.ComputeHash (System.Text.Encoding.ASCII.GetBytes (salt + password)));
					
						Request req = new Request("POWR", "?");
						string resp = md5hash + req.ToString ();
						mvarWriter.WriteFixedLengthString(resp);
						mvarWriter.Flush ();
					
						string r = mvarReader.ReadFixedLengthString (7);
						if (r == "PJLINK ")
						{
							r += mvarReader.ReadFixedLengthString (5);
							if (r == "PJLINK ERRA\r")
							{
								throw new System.Security.SecurityException("The projector did not accept the given password");
							}
						}
						r += mvarReader.ReadFixedLengthString (2);
					
						bool b = (mvarClient.Available > 0);
						return true;
					}
					default:
					{
						throw new ProtocolViolationException("Security must either be None (0) or Password (1)");
					}
				}
			}
			return false;
		}
		
		public void AwaitPowerState(PowerState state)
		{
			do
			{
				System.Threading.Thread.Sleep (500);
			}
			while (PowerState != state);
		}
		
		public PowerState PowerState
		{
			get
			{
				Response resp = Send (new Request("POWR", "?"));
				switch (resp.Parameters)
				{
					case "0": return PowerState.Off;
					case "1": return PowerState.On;
					case "2": return PowerState.Cooling;
					case "3": return PowerState.WarmUp;
					case "": return PowerState.Unknown;
				}
				throw new InvalidOperationException();
			}
			set
			{
				Console.WriteLine ("pjlink: sending power state " + value.ToString () );
				switch (value)
				{
					case PowerState.Off:
					{
						Response resp = Send (new Request("POWR", "0"));
						break;
					}
					case PowerState.On:
					{
						Response resp = Send (new Request("POWR", "1"));
						if (resp.Parameters == "ERR3")
						{
							throw new InvalidOperationException("Cannot send power command; is projector powering down?");
						}
						break;
					}
					default:
					{
						throw new InvalidOperationException("You cannot set this as a power state!");
					}
				}
			}
		}
		
		public bool Mute
		{
			get
			{
				Response resp = Send (new Request("AVMT", "?"));
				return resp.Parameters != "30";
			}
			set
			{
				Console.WriteLine ("pjlink: sending a/v mute request " + value.ToString() );
				if (value)
				{
					Send (new Request("AVMT", "31"));
				}
				else
				{
					Send (new Request("AVMT", "30"));
				}
			}
		}
		
		private string SlowReadUntil(Reader reader, char untilWhat)
		{
			StringBuilder sb = new StringBuilder();
			while (true)
			{
				string s = reader.ReadFixedLengthString (1);
				if (s[0] == untilWhat) return sb.ToString ();
				sb.Append (s);
			}
			return sb.ToString ();
		}
		
		public Response Send(Request request)
		{
			mvarWriter.Write (request.ToString ());
			mvarWriter.Flush ();
			
			while (mvarClient.Available == 0)
			{
				System.Threading.Thread.Sleep (50);
			}
			
			if (mvarClient.Available > 0)
			{
				string data = SlowReadUntil(mvarReader, '\r');
				return new Response(data);
			}
			else
			{
				return Response.Empty;
			}
		}
	}
}


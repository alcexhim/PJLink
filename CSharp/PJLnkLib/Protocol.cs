using System;

using UniversalEditor.IO;

namespace PJLink
{
	public class Protocol
	{
		private char mvarHeader = '%';
		private char mvarVersion = '1';
		
		private Reader mvarReader = null;
		private Writer mvarWriter = null;
		
		public Protocol(Reader reader, Writer writer)
		{
			mvarReader = reader;
			mvarWriter = writer;
		}
		
		public Response Receive(string data = "")
		{
			if (data.Length < 7)
			{
				data += mvarReader.ReadFixedLengthString(7 - data.Length);
			}
			
			if (data[0] != mvarHeader)
			{
				throw new FormatException("Header should be '%'");
			}
			if (data[1] != mvarVersion)
			{
				throw new NotSupportedException("Only version 1 is supported");
			}
			
			string body = data.Substring (2, 6);
			// commands are case-insensitive, but let's turn them upper case anyway
			// this will avoid the rest of our code from making this mistake
			// FIXME: AFAIR this takes the current locale into consideration, it shouldn't.
			body = body.ToUpper();
			
			char separator = data[6];
			if (separator != '=')
			{
			}
			
			string param = mvarReader.ReadUntil("\r");
			return new Response(body, param);
		}
		
		public void Send(string body, string param)
		{
			Send (new Request(body, param));
		}
		public void Send(Request request)
		{
			string data = request.ToString();
			mvarWriter.Write (data);
			mvarWriter.Flush ();
		}
	}
}


using System;
namespace PJLink
{
	public class Response
	{
		private static Response mvarEmpty = new Response();
		public static Response Empty { get { return mvarEmpty; } }
		
		private bool mvarIsEmpty = false;
		public bool IsEmpty { get { return mvarIsEmpty; } }
		
		private Response()
		{
			mvarIsEmpty = true;
		}
		
		private string mvarCommand = String.Empty;
		public string Command { get { return mvarCommand; } }
		
		private string mvarParameters = String.Empty;
		public string Parameters { get { return mvarParameters; } }
		
		public Response(string rawdata)
		{
			if (rawdata[0] != '%')
			{
				throw new FormatException("data does not begin with '%'");
			}
			if (rawdata[1] != '1')
			{
				throw new FormatException("data version is not 1");
			}
			
			mvarCommand = rawdata.Substring (2, 4).ToUpper();
			char sep = rawdata[6];
			if (sep != '=') ;
			
			mvarParameters = rawdata.Substring (7);
		}
		public Response(string command, string parameters)
		{
			mvarCommand = command;
			mvarParameters = parameters;
		}
	}
}


using System;
using System.Text;

namespace PJLink
{
	public class Request
	{
		public Request(string command, string parameters, char separator = ' ')
		{
			Command = command;
			Parameters = parameters;
			Separator = separator;
		}
		
		private string mvarCommand = String.Empty;
		public string Command
		{
			get { return mvarCommand; }
			set
			{
				if (value.Length != 4) throw new ArgumentException("command string must be exactly 4 characters");
				mvarCommand = value.ToUpper();
			}
		}
		
		private char mvarSeparator = ' ';
		public char Separator { get { return mvarSeparator; } set { mvarSeparator = value; } }
		
		private string mvarParameters = String.Empty;
		public string Parameters
		{
			get { return mvarParameters; }
			set
			{
				if (value.Length > 128) throw new ArgumentException("parameter string must be 128 characters or less");
				mvarParameters = value;
			}
		}
		
		private string mvarHeader = "%";
		private string mvarVersion = "1";
		private string mvarFooter = "\r";
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append (mvarHeader);
			sb.Append (mvarVersion);
			sb.Append (mvarCommand);
			sb.Append (mvarSeparator);
			sb.Append (mvarParameters);
			sb.Append (mvarFooter);
			return sb.ToString ();
		}
	}
}


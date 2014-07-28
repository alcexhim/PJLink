using System;

namespace PJLink.ConsoleApplication
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Projector proj = Projector.FromAddress("192.168.1.22");
			proj.Connect("panasonic");
			
			proj.PowerState = PowerState.On;
			proj.AwaitPowerState(PowerState.On);
			
			proj.Mute = true;
			
			proj.PowerState = PowerState.Off;
		}
	}
}

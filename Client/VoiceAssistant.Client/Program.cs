using Microsoft.Extensions.Hosting;

namespace VoiceAssistant.Client
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			var builder = Host.CreateApplicationBuilder();
		}
	}
}
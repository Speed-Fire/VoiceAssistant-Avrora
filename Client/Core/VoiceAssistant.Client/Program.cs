using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using VoiceAssistant.Client.Audio.Extensions;
using VoiceAssistant.Client.Workers;

namespace VoiceAssistant.Client
{
	internal static class Program
	{
		static readonly IHost _host = CreateHost();
		static readonly NotifyIcon _trayIcon = CreateTrayIcon();

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			_host.RunAsync();
			Application.Run();
		}

		static IHost CreateHost()
		{
			var builder = Host.CreateApplicationBuilder();

			builder.Services
				.AddHostedService<UICommandsWorker>()
				.AddHostedService<VoiceCommandsWorker>();

			builder.Services
				.RegisterAudio();

			return builder.Build();
		}

		static NotifyIcon CreateTrayIcon()
		{
			var trayIcon = new NotifyIcon
			{
				Icon = SystemIcons.Application,
				Visible = true,
				Text = "Avrora"
			};

			var menu = new ContextMenuStrip();
			menu.Items.Add("Open", null, (s, e) => OpenUI());
			menu.Items.Add("Exit", null, async (s, e) => await Shutdown());

			trayIcon.ContextMenuStrip = menu;

			return trayIcon;
		}

		private static async Task Shutdown()
		{
			await _host.StopAsync();
			Application.Exit();
		}

		private static void OpenUI()
		{
			throw new NotImplementedException();
		}
	}
}
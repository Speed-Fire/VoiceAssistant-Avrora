using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistant.Client.Workers
{
	internal class UICommandsWorker : BackgroundService
	{
		private readonly ILogger _logger;

		public UICommandsWorker(ILogger<UICommandsWorker> logger)
		{
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					using var pipe = new NamedPipeServerStream("SomeName", PipeDirection.InOut);
					pipe.ReadMode = PipeTransmissionMode.Message;
					await pipe.WaitForConnectionAsync(stoppingToken);

					stoppingToken.ThrowIfCancellationRequested();

					using var reader = new StreamReader(pipe);
					using var writer = new StreamWriter(pipe);

					while (pipe.IsConnected)
					{
						var command = await reader.ReadLineAsync(stoppingToken);

						stoppingToken.ThrowIfCancellationRequested();

						string result = await HandleCommand(command, stoppingToken);

						stoppingToken.ThrowIfCancellationRequested();

						await writer.WriteLineAsync(result);

						stoppingToken.ThrowIfCancellationRequested();
					}
				}

				stoppingToken.ThrowIfCancellationRequested();
			}
			catch (OperationCanceledException)
			{
				_logger.LogInformation("Stopped.");
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex, "", []);
			}
		}

		private async Task<string> HandleCommand(string? message, CancellationToken stoppingToken)
		{
			throw new NotImplementedException();
		}
	}
}

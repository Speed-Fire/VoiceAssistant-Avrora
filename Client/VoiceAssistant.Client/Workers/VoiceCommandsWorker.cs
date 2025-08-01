using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceAssistant.Client.Abstractions;

namespace VoiceAssistant.Client.Workers
{
	internal class VoiceCommandsWorker : BackgroundService
	{
		private readonly IKeyPhraseListener _keyPhraseListener;
		private readonly IVoiceRecorder _voiceRecorder;
		private readonly IServerCommunicator _serverCommunicator;
		private readonly ISpeechSynthesizer _speechSynthesizer;
		private readonly IVoiceCommandHandler _voiceCommandHandler;
		private readonly ILogger _logger;

		public VoiceCommandsWorker(
			IKeyPhraseListener keyPhraseListener,
			IVoiceRecorder voiceRecorder,
			IServerCommunicator serverCommunicator,
			ISpeechSynthesizer speechSynthesizer,
			IVoiceCommandHandler voiceCommandHandler,
			ILogger<VoiceCommandsWorker> logger)
		{
			_keyPhraseListener = keyPhraseListener;
			_voiceRecorder = voiceRecorder;
			_serverCommunicator = serverCommunicator;
			_speechSynthesizer = speechSynthesizer;
			_voiceCommandHandler = voiceCommandHandler;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					await _keyPhraseListener.ListenAsync(stoppingToken);

					stoppingToken.ThrowIfCancellationRequested();

					await _speechSynthesizer.SpeakAsync("Слушаю вас");

					stoppingToken.ThrowIfCancellationRequested();

					var audio = await _voiceRecorder.RecordAsync(stoppingToken);

					stoppingToken.ThrowIfCancellationRequested();

					using var answer = 
						await _serverCommunicator.SendCommandAsync(audio, stoppingToken);

					stoppingToken.ThrowIfCancellationRequested();

					await _voiceCommandHandler.HandleAsync(answer);

					stoppingToken.ThrowIfCancellationRequested();
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
				throw;
			}
		}
	}
}

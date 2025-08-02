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
		private readonly ICommandHandler _commandHandler;
		private readonly ISpeechSynthesizer _speechSynthesizer;
		private readonly Abstractions.ICommandExecutor _commandExecutor;
		private readonly ILogger _logger;

		public VoiceCommandsWorker(
			IKeyPhraseListener keyPhraseListener,
			IVoiceRecorder voiceRecorder,
			ICommandHandler serverCommunicator,
			ISpeechSynthesizer speechSynthesizer,
			Abstractions.ICommandExecutor voiceCommandHandler,
			ILogger<VoiceCommandsWorker> logger)
		{
			_keyPhraseListener = keyPhraseListener;
			_voiceRecorder = voiceRecorder;
			_commandHandler = serverCommunicator;
			_speechSynthesizer = speechSynthesizer;
			_commandExecutor = voiceCommandHandler;
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

					await _speechSynthesizer.SpeakAsync("Слушаю вас", stoppingToken);

					stoppingToken.ThrowIfCancellationRequested();

					var audio = await _voiceRecorder.RecordAsync(stoppingToken);

					stoppingToken.ThrowIfCancellationRequested();

					await foreach(var command in _commandHandler.HandleAsync(audio, stoppingToken))
					{
						stoppingToken.ThrowIfCancellationRequested();

						await _commandExecutor.ExecuteAsync(command);
					}

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

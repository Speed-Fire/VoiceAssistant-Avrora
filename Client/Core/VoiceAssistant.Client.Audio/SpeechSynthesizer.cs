using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceAssistant.Client.Abstractions;

namespace VoiceAssistant.Client.Audio
{
	internal class SpeechSynthesizer : ISpeechSynthesizer
	{
		private readonly System.Speech.Synthesis.SpeechSynthesizer _synthesizer;

		public SpeechSynthesizer()
		{
			_synthesizer = new();

		}

		public Task SpeakAsync(string text, CancellationToken stoppingToken)
		{
			return Task.Run(() => Speak(text, stoppingToken), stoppingToken);
		}

		private void Speak(string text, CancellationToken stoppingToken)
		{
			var prompt = _synthesizer.SpeakAsync(text);

			while (!prompt.IsCompleted)
			{
				if (stoppingToken.IsCancellationRequested)
				{
					_synthesizer.Pause();
					break;
				}
			}
		}

		public void Dispose()
		{
			_synthesizer.Dispose();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;
using VoiceAssistant.Client.Abstractions;

namespace VoiceAssistant.Client.Audio
{
	internal class KeyPhraseListener : IKeyPhraseListener
	{
		private readonly SpeechRecognitionEngine _speechRecognitionEngine;

		private volatile bool _detected = false;

		public KeyPhraseListener()
		{
			_speechRecognitionEngine = new(new System.Globalization.CultureInfo("en-US"));
			_speechRecognitionEngine.SetInputToDefaultAudioDevice();

			var grammarStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream("VoiceAssistant.Client.Audio.Grammars.Grammar_en-us.xml");
			var grammar = new Grammar(grammarStream);

			_speechRecognitionEngine.LoadGrammar(grammar);
			_speechRecognitionEngine.SpeechRecognized += SpeechRecognized;
		}

		public Task ListenAsync(CancellationToken stoppingToken)
		{
			return Task.Run(() => Listen(stoppingToken), stoppingToken);
		}

		private void Listen(CancellationToken stoppingToken)
		{
			_detected = false;
			_speechRecognitionEngine.RecognizeAsync();

			while (!stoppingToken.IsCancellationRequested)
			{
				if (_detected)
					break;
			}

			_speechRecognitionEngine.RecognizeAsyncStop();
		}

		private void SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
		{
			_detected = true;
		}

		public void Dispose()
		{
			_speechRecognitionEngine.Dispose();
		}
	}
}

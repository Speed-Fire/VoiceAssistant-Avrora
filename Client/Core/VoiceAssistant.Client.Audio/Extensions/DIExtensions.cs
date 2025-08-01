using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceAssistant.Client.Abstractions;

namespace VoiceAssistant.Client.Audio.Extensions
{
	public static class DIExtensions
	{
		public static IServiceCollection RegisterAudio(this IServiceCollection services)
		{
			services
				.AddSingleton<ISpeechSynthesizer, SpeechSynthesizer>()
				.AddSingleton<IVoiceRecorder, VoiceRecorder>()
				.AddSingleton<IKeyPhraseListener, KeyPhraseListener>();

			return services;
		}
	}
}

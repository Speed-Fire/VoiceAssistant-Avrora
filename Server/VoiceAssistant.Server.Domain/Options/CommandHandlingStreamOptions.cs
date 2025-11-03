using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistant.Server.Domain.Options
{
	public class CommandHandlingStreamOptions
	{
		[ConfigurationKeyName("REDIS_STREAMS_STT")]
		public string Stream { get; set; } = string.Empty;

		[ConfigurationKeyName("REDIS_STREAMS_STT_GROUP")]
		public string Group { get; set; } = string.Empty;
	}
}

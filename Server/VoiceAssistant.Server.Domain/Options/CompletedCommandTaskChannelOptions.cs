using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistant.Server.Domain.Options
{
	public class CompletedCommandTaskChannelOptions
	{
		[ConfigurationKeyName("REDIS_CHANNELS_COM_COMPLETED")]
		public string Channel { get; set; } = string.Empty;
	}
}

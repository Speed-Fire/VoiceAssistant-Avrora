using Microsoft.Extensions.Configuration;

namespace VoiceAssistant.Server.Domain.Options
{
	public class KeycloakOptions
	{
		[ConfigurationKeyName("")]
		public string AdminId { get; set; } = string.Empty;

		[ConfigurationKeyName("")]
		public string AdminSecret { get; set; } = string.Empty;
	}
}

using Microsoft.Extensions.Configuration;

namespace VoiceAssistant.Server.Domain.Options
{
	public class KeycloakOptions
	{
		[ConfigurationKeyName("AUTH_CLIENT_ADMIN_ID")]
		public string AdminId { get; set; } = string.Empty;

		[ConfigurationKeyName("AUTH_CLIENT_ADMIN_SECRET")]
		public string AdminSecret { get; set; } = string.Empty;
	}
}

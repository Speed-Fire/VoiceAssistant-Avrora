namespace VoiceAssistant.Server.Options
{
	public class KeycloakOptions
	{
		[ConfigurationKeyName("")]
		public string AdminId { get; set; } = string.Empty;

		[ConfigurationKeyName("")]
		public string AdminSecret { get; set; } = string.Empty;
	}
}

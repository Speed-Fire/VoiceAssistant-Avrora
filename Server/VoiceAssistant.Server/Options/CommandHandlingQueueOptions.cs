namespace VoiceAssistant.Server.Options
{
	public class CommandHandlingQueueOptions
	{
		[ConfigurationKeyName("REDIS_QUEUES_COMMAND_HANDLING_PENDING")]
		public string PendingQueue { get; set; } = string.Empty;

		[ConfigurationKeyName("REDIS_QUEUES_COMMAND_HANDLING_PROCESSING")]
		public string ProcessingQueue { get; set; } = string.Empty;

		[ConfigurationKeyName("REDIS_QUEUES_COMMAND_HANDLING_TIMESTAMPS")]
		public string TimestampsHash { get; set; } = string.Empty;
	}
}

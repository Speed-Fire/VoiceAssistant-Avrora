using Microsoft.Extensions.Options;

namespace VoiceAssistant.Server.Options
{
	public class RecognitionQueueOptions
	{
		[ConfigurationKeyName("REDIS_QUEUES_RECOGNITION_PENDING")]
		public string PendingQueue { get; set; } = string.Empty;

		[ConfigurationKeyName("REDIS_QUEUES_RECOGNITION_PROCESSING")]
		public string ProcessingQueue { get; set; } = string.Empty;

		[ConfigurationKeyName("REDIS_SETS_RECOGNITION_TIMESTAMPS")]
		public string TimestampsSet { get; set; } = string.Empty;
	}
}

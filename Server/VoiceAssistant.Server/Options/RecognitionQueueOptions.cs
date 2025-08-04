using Microsoft.Extensions.Options;

namespace VoiceAssistant.Server.Options
{
	public class RecognitionQueueOptions
	{
		[ConfigurationKeyName("REDIS_QUEUES_RECOGNITION_PENDING")]
		public string PendingQueue { get; set; } = string.Empty;

		[ConfigurationKeyName("REDIS_QUEUES_RECOGNITION_PROCESSING")]
		public string ProcessingQueue { get; set; } = string.Empty;

		[ConfigurationKeyName("REDIS_QUEUES_RECOGNITION_TIMESTAMPS")]
		public string TimestampsQueue { get; set; } = string.Empty;
	}
}

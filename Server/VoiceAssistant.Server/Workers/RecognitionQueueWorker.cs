using StackExchange.Redis;
using VoiceAssistant.Server.Options;

namespace VoiceAssistant.Server.Workers
{
	public class RecognitionQueueWorker : ReliableQueueWorker
	{
		public RecognitionQueueWorker(
			ConnectionMultiplexer redis,
			IConfiguration config,
			RecognitionQueueOptions options)
			: base(
				  redis,
				  config,
				  options.PendingQueue,
				  options.ProcessingQueue,
				  options.TimestampsHash)
		{
		}
	}
}

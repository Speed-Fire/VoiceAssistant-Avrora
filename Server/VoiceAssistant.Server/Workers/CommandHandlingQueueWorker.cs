using StackExchange.Redis;
using VoiceAssistant.Server.Options;

namespace VoiceAssistant.Server.Workers
{
	public class CommandHandlingQueueWorker : ReliableQueueWorker
	{
		public CommandHandlingQueueWorker(
			ConnectionMultiplexer redis,
			IConfiguration config,
			CommandHandlingQueueOptions options)
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

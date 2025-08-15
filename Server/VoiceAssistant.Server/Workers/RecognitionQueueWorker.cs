using StackExchange.Redis;
using VoiceAssistant.Server.Extensions;
using VoiceAssistant.Server.Options;

namespace VoiceAssistant.Server.Workers
{
	public class RecognitionQueueWorker : ReliableQueueWorker
	{
		public RecognitionQueueWorker(
			ConnectionMultiplexer redis,
			IConfiguration config,
			[FromKeyedServices(DIConsts.KEY_LUA_MOVE_BACK_TO_PENDING)] LoadedLuaScript moveBackToPendingScript,
			RecognitionQueueOptions options)
			: base(
				  redis,
				  config,
				  moveBackToPendingScript,
				  options.PendingQueue,
				  options.ProcessingQueue,
				  options.TimestampsSet)
		{
		}
	}
}

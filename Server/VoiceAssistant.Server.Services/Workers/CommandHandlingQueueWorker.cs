using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using VoiceAssistant.Server.Domain;
using VoiceAssistant.Server.Domain.Options;
using VoiceAssistant.Server.Workers;

namespace VoiceAssistant.Server.Services.Workers
{
	public class CommandHandlingQueueWorker : ReliableQueueWorker
	{
		public CommandHandlingQueueWorker(
			ConnectionMultiplexer redis,
			IConfiguration config,
			[FromKeyedServices(DIConsts.KEY_LUA_MOVE_BACK_TO_PENDING)] LoadedLuaScript moveBackToPendingScript,
			CommandHandlingQueueOptions options)
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


using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Threading.Tasks;
using VoiceAssistant.Server.Domain.Options;
using VoiceAssistant.Server.Hubs;
using VoiceAssistant.Server.Services.Abstract;

namespace VoiceAssistant.Server.Workers
{
	public class CommandTaskListener : BackgroundService
	{
		private readonly IConnectionMultiplexer _redis;
		private readonly ICommandTaskService _taskService;
		private readonly IHubContext<CommandTaskHub> _taskHub;
		private readonly CompletedCommandTaskChannelOptions _channelOptions;

		private ISubscriber? _subscriber;

		public CommandTaskListener(
			IConnectionMultiplexer redis,
			ICommandTaskService taskService,
			IHubContext<CommandTaskHub> taskHub,
			CompletedCommandTaskChannelOptions channelOptions)
		{
			_redis = redis;
			_taskService = taskService;
			_taskHub = taskHub;
			_channelOptions = channelOptions;
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			var sub = _redis.GetSubscriber();
			_subscriber = sub;
			await sub.SubscribeAsync(RedisChannel.Literal(_channelOptions.Channel),
				OnCommandTaskCompleted);

			await base.StartAsync(cancellationToken);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			if(_subscriber != null)
			{
				await _subscriber.UnsubscribeAsync(RedisChannel.Literal(_channelOptions.Channel),
					OnCommandTaskCompleted);
			}

			await base.StopAsync(cancellationToken);
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			return Task.CompletedTask;
		}

		private async void OnCommandTaskCompleted(RedisChannel channel, RedisValue value)
		{
			var taskId = value.ToString();

			var result = await _taskService.GetCompletedTask(taskId);
			if (!result.IsT0)
				return;

			var signalRConnectionResult = await _taskService.GetTaskSignalRConnection(taskId);

			signalRConnectionResult.Switch(
				async connection =>
				{
					await _taskHub.Clients.Client(connection)
						.SendAsync("OnCommandTaskCompleted", result.AsT0);
				},
				none =>
				{

				});
		}
	}
}

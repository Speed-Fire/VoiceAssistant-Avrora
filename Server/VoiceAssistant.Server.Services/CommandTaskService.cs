using OneOf;
using OneOf.Types;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VoiceAssistant.Server.Domain.Models;
using VoiceAssistant.Server.Domain.Options;
using VoiceAssistant.Server.Services.Abstract;

namespace VoiceAssistant.Server.Services
{
	internal class CommandTaskService : ICommandTaskService
	{
		private readonly ConnectionMultiplexer _redis;
		private readonly CommandHandlingStreamOptions _streamOptions;

		public CommandTaskService(
			ConnectionMultiplexer redis,
			CommandHandlingStreamOptions streamOptions)
		{
			_redis = redis;
			_streamOptions = streamOptions;
		}

		public async Task<OneOf<CompletedCommandTask, None>> GetCompletedTask(string taskId, CancellationToken cancellationToken = default)
		{
			var db = _redis.GetDatabase();
			var json = await db.StringGetAsync($"command-task:{taskId}:result");

			if (string.IsNullOrEmpty(json))
				return new None();

			var completedTask = JsonSerializer
				.Deserialize<CompletedCommandTask>(json!);

			return completedTask!;
		}

		public async Task PushTask(CommandTask task, CancellationToken cancellationToken = default)
		{
			var json = JsonSerializer.Serialize(task);

			var db = _redis.GetDatabase();

			await db.StreamAddAsync(_streamOptions.Stream, [
				new("payload", json)
				]);
		}

		public async Task<OneOf<string, None>> GetTaskSignalRConnection(string taskId, CancellationToken cancellationToken = default)
		{
			var db = _redis.GetDatabase();
			var result = await db.StringGetAsync(taskId);
			if (result == RedisValue.Null)
				return new None();
			else
				return (string)result!;
		}

		public async Task RegisterTask(string taskId, string signalrConnection, CancellationToken cancellationToken = default)
		{
			var db = _redis.GetDatabase();
			await db.StringSetAsync(taskId, signalrConnection, TimeSpan.FromMinutes(2));
		}

		public async Task RegisterTasks(IEnumerable<string> taskIds, string signalrConnection, CancellationToken cancellationToken = default)
		{
			var db = _redis.GetDatabase();

			var tasks = new List<Task>();
			foreach(var id in taskIds)
			{
				var task = db.StringSetAsync(id, signalrConnection, TimeSpan.FromMinutes(2));
				tasks.Add(task);
			}

			await Task.WhenAll(tasks);
		}
	}
}

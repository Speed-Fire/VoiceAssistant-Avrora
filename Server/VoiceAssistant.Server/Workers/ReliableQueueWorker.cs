
using StackExchange.Redis;
using System.Reflection;
using VoiceAssistant.Server.Extensions;

namespace VoiceAssistant.Server.Workers
{
	public class ReliableQueueWorker : BackgroundService
	{
		private readonly IConnectionMultiplexer _redis;
		private readonly LoadedLuaScript _moveBackToPendingScript;

		private readonly int _tempEntryLifetime;

		private readonly string _pendingQueue;
		private readonly string _prosessingQueue;
		private readonly string _timestampsSet;

		public ReliableQueueWorker(
			IConnectionMultiplexer redis,
			IConfiguration config,
			LoadedLuaScript moveBackToPendingScript,
			string pendingQueue,
			string prosessinggQueue,
			string timestampsSet)
		{
			_redis = redis;
			_moveBackToPendingScript = moveBackToPendingScript;
			_tempEntryLifetime = int.Parse(config["Workers:ReliableQueue:TempEntryLifetime"]!);
			_pendingQueue = pendingQueue;
			_prosessingQueue = prosessinggQueue;
			_timestampsSet = timestampsSet;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				var db = _redis.GetDatabase();
				
				var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _tempEntryLifetime;

				var entriesToRem = await db.SortedSetRangeByScoreAsync(_timestampsSet, stop: time);

				if(entriesToRem is not null && entriesToRem.Length > 0)
				{
					await db.ScriptEvaluateAsync(_moveBackToPendingScript.ExecutableScript,
						[_pendingQueue, _prosessingQueue, _timestampsSet],
						entriesToRem);
				}

				await Task.Delay(1000, stoppingToken);
			}
		}
	}
}

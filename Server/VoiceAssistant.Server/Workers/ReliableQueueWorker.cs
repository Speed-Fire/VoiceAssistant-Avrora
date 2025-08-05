
using StackExchange.Redis;

namespace VoiceAssistant.Server.Workers
{
	public class ReliableQueueWorker : BackgroundService
	{
		private readonly ConnectionMultiplexer _redis;

		private readonly int _tempEntryLifetime;

		private readonly string _pendingQueue;
		private readonly string _prosessingQueue;
		private readonly string _timestampsSet;

		public ReliableQueueWorker(
			ConnectionMultiplexer redis,
			IConfiguration config,
			string pendingQueue,
			string prosessinggQueue,
			string timestampsSet)
		{
			_redis = redis;
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

				var list = await db.ListRangeAsync(_prosessingQueue);
				var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

				foreach (var item in list)
				{
					var time = await db.HashGetAsync(_timestampsSet, item);
					if (!time.HasValue)
						continue;

					var timestamp = long.Parse(time!);
					if (now - timestamp <= _tempEntryLifetime)
						continue;

					var trans = db.CreateTransaction();
					trans.AddCondition(Condition.HashExists(_timestampsSet, item));

					_ = trans.ListRemoveAsync(_prosessingQueue, item);
					_ = trans.ListLeftPushAsync(_pendingQueue, item);
					_ = trans.HashDeleteAsync(_timestampsSet, item);

					await trans.ExecuteAsync(CommandFlags.FireAndForget);
				}

				await Task.Delay(1000, stoppingToken);
			}
		}
	}
}

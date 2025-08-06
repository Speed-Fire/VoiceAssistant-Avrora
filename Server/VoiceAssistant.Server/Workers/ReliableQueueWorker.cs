
using StackExchange.Redis;
using System.Reflection;
using VoiceAssistant.Server.Extensions;

namespace VoiceAssistant.Server.Workers
{
	public class ReliableQueueWorker : BackgroundService
	{
		private readonly ConnectionMultiplexer _redis;
			
		private readonly int _tempEntryLifetime;

		private readonly string _pendingQueue;
		private readonly string _prosessingQueue;
		private readonly string _timestampsSet;

		private LoadedLuaScript? _moveBackToPendingScript;

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

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			using (var luaScript = Assembly.GetEntryAssembly()!
			.GetManifestResourceStream("VoiceAssistant.Server.Lua.ReliableQueue.MoveBackToPending.lua")){
				var script = string.Empty;
				
				using(var reader = new StreamReader(luaScript!))
				{
					script = await reader.ReadToEndAsync(cancellationToken);
				}

				if (cancellationToken.IsCancellationRequested)
					return;

				var db = _redis.GetDatabase();
				var server = _redis.GetMasterServer();

				if (server is null)
					throw new InvalidOperationException("Master server not found.");

				var loadedScript = await LuaScript.Prepare(script).LoadAsync(server);
				
				_moveBackToPendingScript = loadedScript;
			}
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

					if (_moveBackToPendingScript is null)
						throw new InvalidOperationException("Moving script is not loaded.");

					await db.ScriptEvaluateAsync(_moveBackToPendingScript, new
					{
						pendingQueue = (RedisKey)_pendingQueue,
						processingQueue = (RedisKey)_prosessingQueue,
						timestampsHash = (RedisKey)_timestampsSet,
						task_id = item
					});
				}

				await Task.Delay(1000, stoppingToken);
			}
		}
	}
}


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
				
				var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _tempEntryLifetime;

				var entriesToRem = await db.SortedSetRangeByScoreAsync(_timestampsSet, stop: time);

				if(entriesToRem is not null && entriesToRem.Length > 0)
				{
					if (_moveBackToPendingScript is null)
						throw new InvalidOperationException("Moving script is not loaded.");

					await db.ScriptEvaluateAsync(_moveBackToPendingScript.ExecutableScript,
						[_pendingQueue, _prosessingQueue, _timestampsSet],
						entriesToRem);
				}

				await Task.Delay(1000, stoppingToken);
			}
		}
	}
}

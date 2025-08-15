using StackExchange.Redis;
using System.Reflection;
using VoiceAssistant.Server.Extensions;

namespace VoiceAssistant.Server.Misc
{
	public class LuaScriptStoragePreparer
	{
		private readonly IConnectionMultiplexer _redis;
		private readonly LuaScriptStorage _storage;

		private bool _prepared = false;

		public LuaScriptStoragePreparer(IConnectionMultiplexer redis, LuaScriptStorage storage)
		{
			_redis = redis;
			_storage = storage;
		}

		public async Task Prepare()
		{
			if (_prepared)
				return;

			var moveBackToPendingScript =
				GetScriptFileContent("VoiceAssistant.Server.Lua.ReliableQueue.MoveBackToPending.lua");
			var pushRecognitionTaskScript =
				GetScriptFileContent("VoiceAssistant.Server.Lua.ReliableQueue.MoveBackToPending.lua");

			await _storage.LoadScriptAsync(_redis, 
				DIConsts.KEY_LUA_MOVE_BACK_TO_PENDING, moveBackToPendingScript);
			await _storage.LoadScriptAsync(_redis,
				DIConsts.KEY_LUA_PUSH_RECOGNITION_TASK, pushRecognitionTaskScript);
		}

		static string GetScriptFileContent(string filename)
		{
			using var luaScript = Assembly.GetEntryAssembly()!
				.GetManifestResourceStream(filename);
			var script = string.Empty;

			using (var reader = new StreamReader(luaScript!))
			{
				script = reader.ReadToEnd();
			}

			return script;
		}
	}
}

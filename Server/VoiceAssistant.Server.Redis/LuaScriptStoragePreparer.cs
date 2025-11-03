using StackExchange.Redis;
using System.Reflection;
using VoiceAssistant.Server.Domain;

namespace VoiceAssistant.Server.Redis
{
	public class LuaScriptStoragePreparer
	{
		private readonly LuaScriptStorage _storage;

		private readonly Dictionary<string, string> _pendingScripts = [];

		private bool _prepared = false;

		public LuaScriptStoragePreparer(LuaScriptStorage storage)
		{
			_storage = storage;
		}

		public void Add(string key, string path)
		{
			if (_prepared)
				return;

			_pendingScripts.Add(key, path);
		}

		public async Task Prepare(IConnectionMultiplexer redis)
		{
			if (_prepared)
				return;

			foreach(var pair in  _pendingScripts)
			{
				var script = GetScriptFileContent(pair.Value);
				await _storage.LoadScriptAsync(redis, pair.Key, script);
			}
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

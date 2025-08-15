using StackExchange.Redis;
using VoiceAssistant.Server.Extensions;

namespace VoiceAssistant.Server.Misc
{
	public class LuaScriptStorage
	{
		private readonly Dictionary<string, LoadedLuaScript> _scripts = [];

		public LoadedLuaScript this[string key] => GetScript(key);

		public LoadedLuaScript GetScript(string name)
		{
			return _scripts[name];
		}

		public async Task LoadScriptAsync(IConnectionMultiplexer redis, string name, string script)
		{
			var server = redis.GetMasterServer();

			if (server is null)
				throw new InvalidOperationException("Master server not found.");

			var loadedScript = await LuaScript.Prepare(script).LoadAsync(server);
			_scripts.Add(name, loadedScript);
		}
	}
}

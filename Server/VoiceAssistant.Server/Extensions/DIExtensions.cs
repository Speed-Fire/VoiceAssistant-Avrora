using StackExchange.Redis;
using VoiceAssistant.Server.Misc;

namespace VoiceAssistant.Server.Extensions
{
	public static class DIExtensions
	{
		public static IServiceCollection AddLuaScripts(this IServiceCollection services)
		{
			services
				.AddSingleton<LuaScriptStorage>()
				.AddSingleton<LuaScriptStoragePreparer>();

			services
				.AddKeyedSingleton<LoadedLuaScript>(DIConsts.KEY_LUA_MOVE_BACK_TO_PENDING, GetScript)
				.AddKeyedSingleton<LoadedLuaScript>(DIConsts.KEY_LUA_PUSH_RECOGNITION_TASK, GetScript);

			return services;
		}

		private static LoadedLuaScript GetScript(IServiceProvider services, object key)
		{
			var storage = services.GetRequiredService<LuaScriptStorage>();
			return storage[(string)key];
		}
	}
}

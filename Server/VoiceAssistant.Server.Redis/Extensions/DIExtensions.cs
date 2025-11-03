using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using VoiceAssistant.Server.Domain;

namespace VoiceAssistant.Server.Redis.Extensions
{
	public static class DIExtensions
	{
		private static LuaScriptStorage _storage = new();
		private static LuaScriptStoragePreparer _preparer = new(_storage);

		public static IServiceCollection AddLuaScriptServices(this IServiceCollection services)
		{
			services
				.AddSingleton<LuaScriptStorage>(_storage)
				.AddSingleton<LuaScriptStoragePreparer>(_preparer);

			return services;
		}

		public static IServiceCollection AddLuaScript(this IServiceCollection services,
			string key, string path)
		{
			_preparer.Add(key, path);

			var provider = services.AddKeyedSingleton(key, GetScript!);

			return services;
		}

		private static LoadedLuaScript GetScript(IServiceProvider services, object key)
		{
			var storage = services.GetRequiredService<LuaScriptStorage>();
			return storage[(string)key];
		}
	}
}

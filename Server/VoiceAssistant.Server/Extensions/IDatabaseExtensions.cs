using StackExchange.Redis;

namespace VoiceAssistant.Server.Extensions
{
	public static class IDatabaseExtensions
	{
		public static Task<RedisResult> ScriptEvaluateAsync(this IDatabase db, LoadedLuaScript script,
			RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flags = CommandFlags.None)
		{
			return db.ScriptEvaluateAsync(script.ExecutableScript, keys, values, flags);
		}
	}
}

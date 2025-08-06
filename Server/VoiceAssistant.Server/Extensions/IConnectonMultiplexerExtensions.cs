using StackExchange.Redis;

namespace VoiceAssistant.Server.Extensions
{
	public static class IConnectonMultiplexerExtensions
	{
		public static IServer? GetMasterServer(this IConnectionMultiplexer redis)
		{
			foreach(var server in redis.GetServers())
			{
				if (!server.IsConnected || server.IsReplica)
					continue;

				return server;
			}

			return null;
		}
	}
}

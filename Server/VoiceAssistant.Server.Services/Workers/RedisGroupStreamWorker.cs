using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistant.Server.Services.Workers
{
	public class RedisGroupStreamWorker : BackgroundService
	{
		private readonly IConnectionMultiplexer _redis;

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			throw new NotImplementedException();
		}
	}
}

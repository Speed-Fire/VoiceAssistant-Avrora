using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistant.Client.Abstractions
{
	public interface IServerCommunicator
	{
		Task<Stream> SendCommandAsync(Stream audio, CancellationToken stoppingToken);
	}
}

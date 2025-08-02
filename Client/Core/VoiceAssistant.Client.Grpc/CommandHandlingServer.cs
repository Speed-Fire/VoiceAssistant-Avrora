using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VoiceAssistant.Client.Abstractions;

namespace VoiceAssistant.Client.Grpc
{
	internal class CommandHandlingServer : ICommandHandler
	{
		private readonly CommandHandler.CommandHandlerClient _client;

		public CommandHandlingServer(GrpcChannel channel)
		{
			_client = new CommandHandler.CommandHandlerClient(channel);
		}

		public async IAsyncEnumerable<string> HandleAsync(
			Stream audio,
			[EnumeratorCancellation] CancellationToken stoppingToken)
		{
			var bytes = await ByteString.FromStreamAsync(audio, stoppingToken);

			var commands = _client.Handle(new() { Audio = bytes }, cancellationToken: stoppingToken);

			var responseStream = commands.ResponseStream;
			while(await responseStream.MoveNext(stoppingToken))
			{
				var command = responseStream.Current;
				yield return command.Command;
			}
		}
	}
}

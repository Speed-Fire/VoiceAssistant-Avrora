using Google.Protobuf;
using Grpc.Core;
using VoiceAssistant.Server;

namespace VoiceAssistant.Server.Services
{
	public class CommandHandlingService : CommandHandler.CommandHandlerBase
	{
		public override async Task Handle(
			CommandRequest request,
			IServerStreamWriter<CommandReply> responseStream,
			ServerCallContext context)
		{
			string text = await ConvertToTextAsync(request.Audio);

			List<string> commands = await RecognizeCommandsAsync(text);

			foreach(var command in commands)
			{
				await responseStream.WriteAsync(new() { Command = command });
			}
		}

		private async Task<List<string>> RecognizeCommandsAsync(string text)
		{
			throw new NotImplementedException();
		}

		private async Task<string> ConvertToTextAsync(ByteString audio)
		{
			throw new NotImplementedException();
		}
	}
}

using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Renci.SshNet;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json.Nodes;
using VoiceAssistant.Server;
using VoiceAssistant.Server.Extensions;
using VoiceAssistant.Server.Services.Abstract;

namespace VoiceAssistant.Server.Services
{
	public class CommandHandlingService : CommandHandler.CommandHandlerBase
	{
		private readonly ICommandHandlingService _commandHandlingService;

		public CommandHandlingService(ICommandHandlingService commandHandlingService)
		{
			_commandHandlingService = commandHandlingService;
		}

		[Authorize]
		public override async Task Handle(
			CommandRequest request,
			IServerStreamWriter<CommandReply> responseStream,
			ServerCallContext context)
		{
			var user = context.GetHttpContext().User.FindFirstValue("sub");
			var audio = new MemoryStream();
			request.Audio.WriteTo(audio);

			await _commandHandlingService.Handle(user, audio, context.CancellationToken);
		}
	}
}

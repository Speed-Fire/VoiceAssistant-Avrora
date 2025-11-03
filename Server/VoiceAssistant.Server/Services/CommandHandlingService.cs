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
using VoiceAssistant.Server.Services.Dtos;

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
		public override async Task<CommandReply> Handle(
			CommandRequest request,
			ServerCallContext context)
		{
			var user = context.GetHttpContext().User.FindFirstValue("sub");
			var audio = new MemoryStream();
			request.Audio.WriteTo(audio);

			var createDto = new CreateCommandTaskDto()
			{
				User = user!,
				Audio = audio,
			};

			var res =
				await _commandHandlingService.Handle(createDto, context.CancellationToken);

			return new() { PendingCommandTaskId = res.TaskId.ToString() };
		}
	}
}

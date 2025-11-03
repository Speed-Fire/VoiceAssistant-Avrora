using Microsoft.Extensions.DependencyInjection;
using Renci.SshNet;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VoiceAssistant.Server.Domain;
using VoiceAssistant.Server.Domain.Models;
using VoiceAssistant.Server.Domain.Options;
using VoiceAssistant.Server.Services.Abstract;
using VoiceAssistant.Server.Services.Dtos;
using VoiceAssistant.Server.Services.Extensions;

namespace VoiceAssistant.Server.Services
{
	internal class CommandHandlingServiceV2 : ICommandHandlingService
	{
		private readonly ConnectionMultiplexer _redis;
		private readonly SftpClient _audioFTP;
		private readonly CommandHandlingStreamOptions _streamOptions;

		public CommandHandlingServiceV2(
			ConnectionMultiplexer redis,
			[FromKeyedServices(DIConsts.KEY_FTP_AUDIO)] SftpClient audioFTP,
			[FromKeyedServices(DIConsts.KEY_LUA_PUSH_RECOGNITION_TASK)] LoadedLuaScript pushRecognitionTaskScript,
			CommandHandlingStreamOptions streamOptions)
		{
			_redis = redis;
			_audioFTP = audioFTP;
			_streamOptions = streamOptions;
		}

		public async Task<PendingCommandTaskDto> Handle(CreateCommandTaskDto createDto, CancellationToken cancellationToken = default)
		{
			var taskId = Guid.NewGuid();

			var audioUrl = await UploadAudioToFTP(taskId, createDto.Audio, cancellationToken);

			var comTask = new CommandTask()
			{
				TaskId = taskId,
				User = createDto.User,
				AudioUrl = audioUrl,
			};

			await EnqueueRecognitionTask(comTask);

			return new() { TaskId = taskId };
		}

		private async Task<string> UploadAudioToFTP(Guid taskid, Stream audio, CancellationToken stoppingToken)
		{
			await _audioFTP.ConnectAsync(stoppingToken);

			var path = taskid + ".mp3";

			_audioFTP.UploadFile(audio, path);
			_audioFTP.Disconnect();

			return path;
		}

		private async Task EnqueueRecognitionTask(CommandTask comTask)
		{
			var json = JsonSerializer.Serialize(comTask);

			var db = _redis.GetDatabase();

			await db.StreamAddAsync(_streamOptions.Stream, [
				new("payload", json)
				]);
		}
	}
}

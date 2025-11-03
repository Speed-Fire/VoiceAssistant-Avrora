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
	internal class CommandHandlingService : ICommandHandlingService
	{
		private readonly ConnectionMultiplexer _redis;
		private readonly SftpClient _audioFTP;
		private readonly LoadedLuaScript _pushRecognitionTaskScript;
		private readonly TasksDataOptions _tasksData;
		private readonly RecognitionQueueOptions _recognitionQueue;

		public CommandHandlingService(
			ConnectionMultiplexer redis,
			[FromKeyedServices(DIConsts.KEY_FTP_AUDIO)] SftpClient audioFTP,
			[FromKeyedServices(DIConsts.KEY_LUA_PUSH_RECOGNITION_TASK)] LoadedLuaScript pushRecognitionTaskScript,
			TasksDataOptions tasksData,
			RecognitionQueueOptions recognitionQueue)
		{
			_redis = redis;
			_audioFTP = audioFTP;
			_pushRecognitionTaskScript = pushRecognitionTaskScript;
			_tasksData = tasksData;
			_recognitionQueue = recognitionQueue;
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

			return new() { TaskId =  taskId };
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

			var keys = new RedisKey[]
			{
				_tasksData.TaskDescriptionMap,
				_tasksData.TaskStatusMap,
				_recognitionQueue.PendingQueue
			};

			var values = new RedisValue[]
			{
				comTask.TaskId.ToString(),
				json
			};

			var result = await db.ScriptEvaluateAsync(_pushRecognitionTaskScript,
				keys, values);
		}
	}
}

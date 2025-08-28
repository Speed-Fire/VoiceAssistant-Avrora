using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Renci.SshNet;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json.Nodes;
using VoiceAssistant.Server;
using VoiceAssistant.Server.Extensions;
using VoiceAssistant.Server.Options;

namespace VoiceAssistant.Server.Services
{
	public class CommandHandlingService : CommandHandler.CommandHandlerBase
	{
		private readonly ConnectionMultiplexer _redis;
		private readonly SftpClient _audioFTP;
		private readonly LoadedLuaScript _pushRecognitionTaskScript;
		private readonly TasksDataOptions _tasksData;
		private readonly RecognitionQueueOptions _recognitionQueue;

		private const string JSON_TASK_FORMAT =
			"""
			{{
				"Id": {0},
				"User": {1},
				"Status": 0,
				"Content": {2}
			}}
			""";

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

		[Authorize]
		public override async Task Handle(
			CommandRequest request,
			IServerStreamWriter<CommandReply> responseStream,
			ServerCallContext context)
		{
			var taskId = Guid.NewGuid().ToString();
			var user = context.GetHttpContext().User.FindFirstValue("sub");
			var audio = new MemoryStream();
			request.Audio.WriteTo(audio);

			var audio_url = await UploadAudioToFTP(taskId, audio, context.CancellationToken);
			await EnqueueRecognitionTask(taskId, user, audio_url);

			var commands = (await GetRecognitionResult(taskId)).Split('|');
			
			foreach(var command in commands)
			{
				await responseStream.WriteAsync(new() { Command = command });
			}
		}

		private async Task<string> UploadAudioToFTP(string taskid, Stream audio, CancellationToken stoppingToken)
		{
			await _audioFTP.ConnectAsync(stoppingToken);

			var path = taskid + ".mp3";

			_audioFTP.UploadFile(audio, path);
			_audioFTP.Disconnect();

			return path;
		}

		private async Task EnqueueRecognitionTask(string taskId, string user, string audio_url)
		{
			var json = string.Format(JSON_TASK_FORMAT,
				taskId, user, audio_url);

			var db = _redis.GetDatabase();

			var keys = new RedisKey[]
			{
				_tasksData.TaskDescriptionMap,
				_tasksData.TaskStatusMap,
				_recognitionQueue.PendingQueue
			};

			var values = new RedisValue[]
			{
				taskId,
				json
			};

			var result = await db.ScriptEvaluateAsync(_pushRecognitionTaskScript,
				keys, values);
		}

		private async Task<string> GetRecognitionResult(string taskId)
		{
			var resultKey = $"{_recognitionQueue.PendingQueue}:{taskId}";
			var db = _redis.GetDatabase();

			for(int i = 0; i < 10; i++)
			{
				var result = await db.StringGetDeleteAsync(resultKey);
				if (result != RedisValue.Null)
				{
					return result!;
				}

				await Task.Delay(1000);
			}

			return "Server timeout exceeded";
		}
	}
}

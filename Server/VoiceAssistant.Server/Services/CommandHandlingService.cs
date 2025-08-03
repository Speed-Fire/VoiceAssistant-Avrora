using Google.Protobuf;
using Grpc.Core;
using Renci.SshNet;
using StackExchange.Redis;
using System.Text.Json.Nodes;
using VoiceAssistant.Server;

namespace VoiceAssistant.Server.Services
{
	public class CommandHandlingService : CommandHandler.CommandHandlerBase
	{
		private readonly ConnectionMultiplexer _redis;
		private readonly SftpClient _audioFTP;
		private readonly string _recognitionQueueName;

		public CommandHandlingService(
			ConnectionMultiplexer redis,
			[FromKeyedServices("AudioFTP")] SftpClient audioFTP,
			IConfiguration config)
		{
			_redis = redis;
			_audioFTP = audioFTP;
			_recognitionQueueName = config["Redis:Queues:Recognition"]!;
		}

		public override async Task Handle(
			CommandRequest request,
			IServerStreamWriter<CommandReply> responseStream,
			ServerCallContext context)
		{
			var taskId = Guid.NewGuid().ToString();
			var audio = new MemoryStream();
			request.Audio.WriteTo(audio);

			var audio_url = await UploadAudioToFTP(taskId, audio, context.CancellationToken);
			await EnqueueRecognitionTask(taskId, audio_url);

			var commands = (await GetRecognitionResult(taskId)).Split('|');
			
			foreach(var command in commands)
			{
				await responseStream.WriteAsync(new() { Command = command });
			}
		}

		private async Task<string> UploadAudioToFTP(string filename, Stream audio, CancellationToken stoppingToken)
		{
			await _audioFTP.ConnectAsync(stoppingToken);

			var path = filename;

			_audioFTP.UploadFile(audio, path);

			return path;
		}

		private async Task EnqueueRecognitionTask(string taskId, string audio_url)
		{
			var json = string.Format("{{\"Id\":\"{0}\":\"{1}\"}}", taskId, audio_url);

			var db = _redis.GetDatabase();
			await db.ListLeftPushAsync(_recognitionQueueName, json);
		}

		private async Task<string> GetRecognitionResult(string taskId)
		{
			var resultKey = $"{_recognitionQueueName}:{taskId}";
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

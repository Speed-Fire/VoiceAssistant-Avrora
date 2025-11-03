using Microsoft.Extensions.Configuration;

namespace VoiceAssistant.Server.Domain.Options
{
	public class TasksDataOptions
	{
		[ConfigurationKeyName("REDIS_HASHES_TASKS_DESCRIPTION")]
		public string TaskDescriptionMap { get; set; } = string.Empty;

		[ConfigurationKeyName("REDIS_HASHES_TASKS_STATUS")]
		public string TaskStatusMap { get; set; } = string.Empty;
	}
}

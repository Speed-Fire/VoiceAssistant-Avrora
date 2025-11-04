using Microsoft.AspNetCore.SignalR;
using VoiceAssistant.Server.Services.Abstract;

namespace VoiceAssistant.Server.Hubs
{
	public class CommandTaskHub : Hub
	{
		private readonly ICommandTaskService _taskService;

		public CommandTaskHub(ICommandTaskService taskService)
		{
			_taskService = taskService;
		}

		public async Task RegisterTask(string taskId)
		{
			await _taskService.RegisterTask(taskId, Context.ConnectionId, Context.ConnectionAborted);
		}

		public async Task RegisterTask(IEnumerable<string> taskIds)
		{
			await _taskService.RegisterTasks(taskIds, Context.ConnectionId, Context.ConnectionAborted);
		}
	}
}

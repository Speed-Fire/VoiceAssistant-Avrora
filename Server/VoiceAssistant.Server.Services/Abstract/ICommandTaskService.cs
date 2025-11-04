using OneOf;
using OneOf.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceAssistant.Server.Domain.Models;

namespace VoiceAssistant.Server.Services.Abstract
{
	public interface ICommandTaskService
	{
		Task RegisterTask(string taskId, string signalrConnection, CancellationToken cancellationToken = default);
		Task RegisterTasks(IEnumerable<string> taskIds, string signalrConnection, CancellationToken cancellationToken = default);
		Task<OneOf<CompletedCommandTask, None>> GetCompletedTask(string taskId, CancellationToken cancellationToken = default);
		Task PushTask(CommandTask task, CancellationToken cancellationToken = default);
		Task<OneOf<string, None>> GetTaskSignalRConnection(string taskId, CancellationToken cancellationToken = default);
	}
}

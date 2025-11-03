using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceAssistant.Server.Services.Dtos;

namespace VoiceAssistant.Server.Services.Abstract
{
	public interface ICommandHandlingService
	{
		Task<PendingCommandTaskDto> Handle(CreateCommandTaskDto createDto, CancellationToken cancellationToken = default);
	}
}

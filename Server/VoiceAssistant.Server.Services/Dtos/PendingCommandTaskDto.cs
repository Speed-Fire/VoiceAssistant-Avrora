using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistant.Server.Services.Dtos
{
	public class PendingCommandTaskDto
	{
		public required Guid TaskId { get; set; }
	}
}

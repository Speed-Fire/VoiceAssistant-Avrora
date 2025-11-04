using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistant.Server.Services.Dtos
{
	public class CreateCommandTaskDto
	{
		public required string User { get; init; }
		public required string SignalRConnection { get; init; }
		public required MemoryStream Audio { get; init; }
	}
}

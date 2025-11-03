using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistant.Server.Domain.Models
{
	public class CommandTask
	{
		public required Guid TaskId { get; init; }
		public required string User { get; init; }
		public int Status { get; init; } = 0;
		public required string AudioUrl { get; init; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistant.Server.Domain.Models
{
	public class CompletedCommandTask
	{
		public required string TaskId { get; init; }
		public required string User { get; init; }
		public required string Content { get; init; }
	}
}

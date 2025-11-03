using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VoiceAssistant.Server.Domain;
using VoiceAssistant.Server.Redis.Extensions;
using VoiceAssistant.Server.Services.Abstract;

namespace VoiceAssistant.Server.Services.Extensions
{
	public static class DIExtensions
	{
		public static IServiceCollection AddServices(this IServiceCollection services)
		{
			services
				.AddTransient<ICommandHandlingService, CommandHandlingService>();

			return services;
		}
	}
}

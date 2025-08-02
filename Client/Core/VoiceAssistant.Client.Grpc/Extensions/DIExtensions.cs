using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceAssistant.Client.Abstractions;

namespace VoiceAssistant.Client.Grpc.Extensions
{
	public static class DIExtensions
	{
		public static IServiceCollection RegisterGrpc(this IServiceCollection services)
		{
			services
				.AddSingleton<GrpcChannel>(GrpcChannel.ForAddress(""))
				.AddTransient<ICommandHandler, CommandHandlingServer>();

			return services;
		}
	}
}

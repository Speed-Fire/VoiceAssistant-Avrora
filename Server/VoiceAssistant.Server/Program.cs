using StackExchange.Redis;
using VoiceAssistant.Server.Services;

namespace VoiceAssistant.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            

            // Add services to the container.
            builder.Services.AddGrpc();

            builder.Services.AddSingleton<ConnectionMultiplexer>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var redis_address = config["Redis_address"];

                return ConnectionMultiplexer.Connect(redis_address!);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<CommandHandlingService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}
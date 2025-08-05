using Renci.SshNet;
using StackExchange.Redis;
using VoiceAssistant.Server.Options;
using VoiceAssistant.Server.Services;
using VoiceAssistant.Server.Workers;

namespace VoiceAssistant.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddGrpc();

            builder.Services
                .Configure<RecognitionQueueOptions>(builder.Configuration)
                .Configure<CommandHandlingQueueOptions>(builder.Configuration);

            builder.Services
                .AddHostedService<RecognitionQueueWorker>()
                .AddHostedService<CommandHandlingQueueWorker>();

            builder.Services
                .AddSingleton<ConnectionMultiplexer>(provider =>
                {
                    var config = provider.GetRequiredService<IConfiguration>();

                    var redis_config = new ConfigurationOptions()
                    {
                        EndPoints = {
                            { config["REDIS_HOST"]!, int.Parse(config["REDIS_PORT"]!) }
                        },
                        Password = config["REDIS_PASSWORD"]!,
                        KeepAlive = 100,
                        ConnectTimeout = 10000
                    };

                    return ConnectionMultiplexer.Connect(redis_config);
                })
                .AddKeyedTransient<SftpClient>("AudioFTP", (provider, key) =>
                {
                    var config = provider.GetRequiredService<IConfiguration>();
                    var host = config["SFTP_HOST"];
                    var port = int.Parse(config["SFTP_PORT"]!);
                    var username = config["SFTP_USERS_AUDIO_NAME"];
                    var password = config["SFTP_USERS_AUDIO_PASSWORD"];

                    return new SftpClient(host!, port, username!, password!);
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<CommandHandlingService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}
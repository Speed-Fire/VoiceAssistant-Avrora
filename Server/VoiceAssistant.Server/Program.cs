using Microsoft.AspNetCore.Authentication.JwtBearer;
using Renci.SshNet;
using StackExchange.Redis;
using System.Reflection;
using System.Threading;
using VoiceAssistant.Server.Extensions;
using VoiceAssistant.Server.Misc;
using VoiceAssistant.Server.Options;
using VoiceAssistant.Server.RestClients;
using VoiceAssistant.Server.Services;
using VoiceAssistant.Server.Workers;

namespace VoiceAssistant.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var config = builder.Configuration;

            // Add services to the container.
            builder.Services.AddGrpc();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = config["AUTH_SERVER_ADDRESS"]!;    
                    options.TokenValidationParameters = new()
                    {
                        ValidateAudience = false,
                        ValidateIssuer = true
                    };
                    
                });
            builder.Services.AddAuthorization();

            builder.Services.AddHttpClient<KeycloakClient>(client =>
            {
				client.BaseAddress = new Uri(config["AUTH_SERVER_ADDRESS"]!);
				client.DefaultRequestHeaders.Add("Accept", "application/json");
			});

            builder.Services
                .Configure<RecognitionQueueOptions>(builder.Configuration)
                .Configure<CommandHandlingQueueOptions>(builder.Configuration)
                .Configure<KeycloakOptions>(builder.Configuration);

            builder.Services
                .AddHostedService<RecognitionQueueWorker>()
                .AddHostedService<CommandHandlingQueueWorker>();

            builder.Services
                .AddLuaScripts();

            builder.Services
                .AddSingleton<KeycloakClient>();

            builder.Services
                .AddSingleton<IConnectionMultiplexer>(provider =>
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
                .AddKeyedTransient<SftpClient>(DIConsts.KEY_FTP_AUDIO, (provider, key) =>
                {
                    var config = provider.GetRequiredService<IConfiguration>();
                    var host = config["SFTP_HOST"];
                    var port = int.Parse(config["SFTP_PORT"]!);
                    var username = config["SFTP_USERS_AUDIO_NAME"];
                    var password = config["SFTP_USERS_AUDIO_PASSWORD"];

                    return new SftpClient(host!, port, username!, password!);
                });

            var app = builder.Build();

            var luaPreparer = app.Services.GetRequiredService<LuaScriptStoragePreparer>();
            await luaPreparer.Prepare();

            // Configure the HTTP request pipeline.
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGrpcService<CommandHandlingService>();
            app.MapGrpcService<RegistrationService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}
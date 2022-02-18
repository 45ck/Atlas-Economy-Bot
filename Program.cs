using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Bot
{
    class Program
    {
        static void Main ( string[] args )
        {
            // One of the more flexible ways to access the configuration data is to use the Microsoft's Configuration model,
            // this way we can avoid hard coding the environment secrets. I opted to use the Json and environment variable providers here.
            IConfiguration config = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "DC_")
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            RunAsync(config).GetAwaiter().GetResult();
        }

        static async Task RunAsync (IConfiguration configuration )
        {
            Configuration.LoadConfig();

            // Dependency injection is a key part of the Interactions framework but it needs to be disposed at the end of the app's lifetime.
            var services = ConfigureServices(configuration);

            var client = services.GetRequiredService<DiscordSocketClient>();
            var commands = services.GetRequiredService<InteractionService>();
            client.Log += LogAsync;
            commands.Log += LogAsync;

            // Slash Commands and Context Commands are can be automatically registered, but this process needs to happen after the client enters the READY state.
            // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands. To determine the method we should
            // register the commands with, we can check whether we are in a DEBUG environment and if we are, we can register the commands to a predetermined test guild.
            client.Ready += async ( ) =>
            {
                if (IsDebug())
                {
                    // loop through each guild id in the config and add it here.
                    foreach (ulong guildId in Configuration.Config.GuildIds)
                    {
                        await commands.RegisterCommandsToGuildAsync(guildId, true);
                    }
                }
                else
                    await commands.RegisterCommandsGloballyAsync(true);
            };

            // Here we can initialize the service that will register and execute our commands
            await services.GetRequiredService<CommandHandler>().InitializeAsync();

            // Bot token can be provided from the Configuration object we set up earlier
            await client.LoginAsync(TokenType.Bot, Configuration.Config.Secerity.Token);
            await client.StartAsync();

            // Keeps the bot open until the bot is closed by either the admin or an error.
            await Task.Delay(Timeout.Infinite);
        }

        static Task LogAsync(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        static ServiceProvider ConfigureServices ( IConfiguration configuration )
        {
            return new ServiceCollection()
                .AddSingleton(configuration)
                .AddSingleton<DiscordSocketClient>(provider => new DiscordSocketClient(new DiscordSocketConfig()
                {
                    GatewayIntents = GatewayIntents.All     // allow intents for messages.
                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }

        static bool IsDebug ( )
        {
            #if DEBUG
                return true;
            #else
                return false;
            #endif
        }
    }
}
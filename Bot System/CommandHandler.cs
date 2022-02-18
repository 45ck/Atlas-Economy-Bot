using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync ( )
        {
            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            // Process the command execution results 
            _commands.SlashCommandExecuted += SlashCommandExecuted;
            _commands.ContextCommandExecuted += ContextCommandExecuted;
            _commands.ComponentCommandExecuted += ComponentCommandExecuted;

        }

        private Task ComponentCommandExecuted (ComponentCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                    default:
                        break;
                }
            }    

            return Task.CompletedTask;
        }

        private Task ContextCommandExecuted (ContextCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private async Task SlashCommandExecuted (SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                if (!Configuration.Config.Errors.DisplayErrorsInChat)
                {
                    await arg2.Interaction.RespondAsync("", ChatUtils.Error(arg2.Guild.Id, "There has been a problem when attempting to run this command, please contact an administrator if this error persists."));
                }
                else
                {
                    // handle arguments through the chatutils.error function which connects the config's error settings to here. 
                    switch (arg3.Error)
                    {
                        case InteractionCommandError.UnmetPrecondition:
                            await arg2.Interaction.RespondAsync("", ChatUtils.Error(arg2.Guild.Id, arg3.ErrorReason));
                            break;
                        case InteractionCommandError.UnknownCommand:
                            await arg2.Interaction.RespondAsync("", ChatUtils.Error(arg2.Guild.Id, "Sorry this is a unknown command."));
                            break;
                        case InteractionCommandError.BadArgs:
                            await arg2.Interaction.RespondAsync("", ChatUtils.Error(arg2.Guild.Id, "There is an invalid number of arguments, please check your parameters/arguments in the command."));
                            break;
                        case InteractionCommandError.Exception:
                            await arg2.Interaction.RespondAsync("", ChatUtils.Error(arg2.Guild.Id, "There was a command exception error when attempting to run the command"));
                            break;
                        case InteractionCommandError.Unsuccessful:
                            await arg2.Interaction.RespondAsync("", ChatUtils.Error(arg2.Guild.Id, "Unfortunately, there was an unknown error, and that forced the command to not be executed"));
                            break;
                        default:
                            break;
                    }
                }

                await arg2.Interaction.RespondAsync("", ChatUtils.Error(arg2.Guild.Id, "Unfortunately, !!!!there was an unknown error, and that forced the command to not be executed"));
            }
        }

        private async Task HandleInteraction (SocketInteraction arg)
        {
            try
            {
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new SocketInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine("HandleInteraction Error: " + ex);

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgment will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if(arg.Type == InteractionType.ApplicationCommand)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }   
        }
    }
}

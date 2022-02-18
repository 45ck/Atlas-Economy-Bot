using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Modules
{
    // Interaction modules must be public and inherit from an IInterationModuleBase
    public class PrivateModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private CommandHandler handler;

        // Constructor injection is also a valid way to access the dependencies
        public PrivateModule(CommandHandler _handler)
        {
            handler = _handler;
        }

        [SlashCommand("private", "Set your account to private.")]
        public async Task SetPrivate ()
        {
            Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);    // get account

            // if their account is already private, let them know and don't update it, as that is not necessary.
            if (account.IsPrivate)
            {
                await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, "You already have your account set to private mode."));    // print account balance 
                return;
            }

            account.IsPrivate = true;

            //add this to logs
            account.Logs.Add(new Economy.Log("Set account to private."));

            Bot.Economy.UpdateAccount(account); // save account data into file

            await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, "Your account has been changed to private mode."));    // print account balance
        }

        [SlashCommand("public", "Set your account to public.")]
        public async Task SetPublic()
        {
            Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);    // get account

            // if their account is already public (isPrivate = false), let them know and don't update it, as that is not necessary.
            if (!account.IsPrivate)
            {
                await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, "You already have your account set to public mode."));    // print account balance 
                return;
            }

            account.IsPrivate = false;

            //add this to logs
            account.Logs.Add(new Economy.Log("Set account to public."));

            Bot.Economy.UpdateAccount(account); // save account data into file

            await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, "Your account has been changed to public mode."));    // print account balance
        }

    }
}

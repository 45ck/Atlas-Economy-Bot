using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Modules
{
    // Interation modules must be public and inherit from an IInterationModuleBase
    public class DailyModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private CommandHandler handler;

        // Constructor injection is also a valid way to access the dependencies
        public DailyModule(CommandHandler _handler)
        {
            handler = _handler;
        }


        [SlashCommand("daily", "Get your daily amount of free currency.")]
        public async Task Daily()
        {
            var settings = GuildSettings.Load(Context.Guild.Id);

            // if there is no daily payout
            if (settings.DailyPayoutAmount == 0)
            {
                await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "This server does not have daily payouts enabled."));    // print account balance
                return;
            }

            Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);    // get current user's account

            // check to see if we can use the daily, since we can only use it once every 24 hours. We make sure the user can only use it once every 24 hours here.
            if ((DateTime.Now - account.TimeSinceLastDaily).TotalHours >= 24)
            {
                account.TimeSinceLastDaily = DateTime.Now;  //reset daily to right now.
            } else
            {
                // calculate the difference between 
                TimeSpan difference = account.TimeSinceLastDaily.AddDays(1) - DateTime.Now;

                // user has not waited the full duration of the day, so we let them know.
                await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, "You need to wait " + difference.Hours + " hours, " 
                    + difference.Minutes + " minutes and " + difference.Seconds + " seconds until you can receive your daily again."));    // print account balance
                return;
            }

            account.Balance = account.Balance + settings.DailyPayoutAmount; // add daily to their account.

            account.Transactions.Add(new Economy.Transaction()    
            {
                Amount = settings.DailyPayoutAmount,
                Information = "Received a daily payout of " + Bot.Economy.CurrencyToFormatedString(settings.DailyPayoutAmount),
                PayeeDiscordAccountId = Context.User.Id,
                PayerDiscordAccountId = 0,
                TimeOfTransaction = DateTime.Now,
            });

            account.Logs.Add(new Economy.Log("Received a daily payout of " + Bot.Economy.CurrencyToFormatedString(settings.DailyPayoutAmount)));

            Bot.Economy.UpdateAccount(account);

            // send message in server
            await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, "You have received your daily payout of " + Bot.Economy.CurrencyToFormatedString(settings.DailyPayoutAmount) + "."));    // print account balance
            return;
        }
    }
}

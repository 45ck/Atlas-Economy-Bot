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
    // Interaction modules must be public and inherit from an IInterationModuleBase
    public class LogsAndTransactionsModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private CommandHandler handler;

        // Constructor injection is also a valid way to access the dependencies
        public LogsAndTransactionsModule(CommandHandler _handler)
        {
            handler = _handler;
        }

        public const string fileDirectory = @"\Output\";

        [SlashCommand("logs", "View your accounts logs")]
        public async Task ViewLogs ()
        {
            Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);    // get account
            account.Logs.Add(new Economy.Log { Details = "Accessed logs", TimeOfLog = DateTime.Now }); 
            Bot.Economy.UpdateAccount(account); // save account data into file

            // create a text file in the logs outpt

            string fileLocation = Directory.GetCurrentDirectory() + fileDirectory + Context.User.Id.ToString() + ".txt";

            // go through each log and write it into the string fileContents which will be pushed into the file we will save after.
            string fileContents = "";
            account.Logs.Reverse(); // make it so the most recent logs are added first.
            int count = 0;  // counter tally for file.
            foreach (Bot.Economy.Log log in account.Logs)
            {
                count++;
                // add a index count and time of log to each log.
                fileContents += "[" + count.ToString() + "] [" + log.TimeOfLog.ToString() + "] - " + log.Details + Environment.NewLine;
            }

            File.WriteAllText(fileLocation, fileContents);  // push the logs we have made into a string as the file itself.

            // send the file to discord chat
            await Context.Interaction.RespondWithFileAsync(File.OpenRead(fileLocation),
            "logs_" + DateTime.Now.ToShortDateString() + ".txt",
            null,
            Bot.ChatUtils.Echo(Context.Guild.Id, "Your logs are attached as a text file above!"));
        }

        [SlashCommand("transactions", "View your accounts transactions with other users.")]
        public async Task Transactions([Summary(description:"view the user's IDs who where involved in the transaction (payee and payer)")] bool viewDetails = false)
        {
            Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);    // get account

            // create a text file in the transactions outpt

            string fileLocation = Directory.GetCurrentDirectory() + fileDirectory + Context.User.Id.ToString() + ".txt";

            // go through each log and write it into the string fileContents which will be pushed into the file we will save after.
            string fileContents = "";
            account.Transactions.Reverse(); // make it so the most recent transactions are added first.
            int count = 0;  // counter tally for file.
            foreach (Bot.Economy.Transaction log in account.Transactions)
            {
                count++;
                // add a counter to be the index of each transaction log.
                if (viewDetails)    // check if the user wants to view the ids of payee and payers 
                    fileContents += "[" + count.ToString() + "] [" + log.TimeOfTransaction.ToString() + "] - " + log.Information +  "    - [Involved user's payer:" + log.PayerDiscordAccountId + ", payee:" + log.PayerDiscordAccountId + "]" + Environment.NewLine;
                else
                    fileContents += "[" + count.ToString() + "] [" + log.TimeOfTransaction.ToString() + "] - " + log.Information + Environment.NewLine;
            }

            File.WriteAllText(fileLocation, fileContents);  // push the logs we have made into a string as the file itself.

            // send the file to discord chat
            await Context.Interaction.RespondWithFileAsync(File.OpenRead(fileLocation),
            "logs_" + DateTime.Now.ToShortDateString() + ".txt",
            null,
            Bot.ChatUtils.Echo(Context.Guild.Id, "Your transactions are attached as a text file above!"));
        }


        [SlashCommand("join-date", "Get an estimated join date of a user.")]
        public async Task JoinDate([Summary(description: "View a user's estimated join date.")] SocketGuildUser user = null)
        {
            Bot.Economy.Account account; 
            // if the command wants to view another user, this will figure it out.
            if (user == null)
                account = Bot.Economy.GetAccountFromId(Context.User.Id);
            else
                account = Bot.Economy.GetAccountFromId(user.Id);

            // get the days since the join date
            TimeSpan elaspedTime = DateTime.Now.Subtract(account.Logs[0].TimeOfLog);

            // show user the estimated join date and the time elapsed since than
            if (user != null) 
                await ReplyAsync("", false, Bot.ChatUtils.Echo(Context.Guild.Id, user.Username + " has an estimated join date of " + account.Logs[0].TimeOfLog.ToString()
                    + Environment.NewLine + "This was " + ((int)Math.Ceiling(elaspedTime.TotalDays)).ToString() + " days ago.")[0]);
            else
                await ReplyAsync("", false, Bot.ChatUtils.Echo(
                    Context.Guild.Id, "You have a estimated join date of " + account.Logs[0].TimeOfLog.ToString()
                    + Environment.NewLine + "This was " + ((int)Math.Ceiling(elaspedTime.TotalDays)).ToString() + " days ago.")[0]);

        }

        [UserCommand("Join Date")]
        public async Task AppGetJoinDate(IUser user)
        {
            Bot.Economy.Account account;
            // if the command wants to view another user, this will figure it out.
            if (user == null)
                account = Bot.Economy.GetAccountFromId(Context.User.Id);
            else
                account = Bot.Economy.GetAccountFromId(user.Id);

            // get the days since the join date
            TimeSpan elaspedTime = DateTime.Now.Subtract(account.Logs[0].TimeOfLog);

            // show user the estimated join date and the time elapsed since than
            if (user != null)
                await Context.Interaction.RespondAsync("", Bot.ChatUtils.Echo(Context.Guild.Id, user.Username + " has an estimated join date of " + account.Logs[0].TimeOfLog.ToString()
                    + Environment.NewLine + "This was " + ((int)Math.Ceiling(elaspedTime.TotalDays)).ToString() + " days ago."));
            else
                await Context.Interaction.RespondAsync("", Bot.ChatUtils.Echo(
                    Context.Guild.Id, "You have a estimated join date of " + account.Logs[0].TimeOfLog.ToString()
                    + Environment.NewLine + "This was " + ((int)Math.Ceiling(elaspedTime.TotalDays)).ToString() + " days ago."));
        }
    }
}

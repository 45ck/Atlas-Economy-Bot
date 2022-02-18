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
    public class EconomyModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private CommandHandler handler;

        // Constructor injection is also a valid way to access the dependencies
        public EconomyModule(CommandHandler _handler)
        {
            handler = _handler;
        }

        [SlashCommand("bal", "See your current account balance.")]
        public async Task Balance ([Summary(description: "get the balance of another user, if their account is publicly viewable.")] IUser user = null)
        {
            if (user == null)
            {
                Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);    // get account
                await Context.Interaction.RespondAsync("", ChatUtils.EchoWithThumbnail(Context.Guild.Id, "Your account balance is currently: " + account.ToString(), // print account balance
                    Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()));  //display the user's personal icon as a way to associate the balance further with the user. If there is no url, use the default one they have been assigned.
            } else
            {
                Bot.Economy.Account account = Bot.Economy.GetAccountFromId(user.Id);    // get account
                // check if their account is private, we do not want to show user's accounts who have set theirs to private status
                if (account.IsPrivate)
                    await Context.Interaction.RespondAsync("", ChatUtils.EchoWithThumbnail(Context.Guild.Id, user.Username + " has set their account to private mode, you can not view private accounts.", 
                        user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()));    //display the requested user's icon as a way to associate the balance further with that user. If there is no url, use the default one they have been assigned.
                else
                    await Context.Interaction.RespondAsync("", ChatUtils.EchoWithThumbnail(Context.Guild.Id, user.Username + " has an account balance of: " + account.ToString(),  // print account balance 
                        user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()));   //display the requested user's icon as a way to associate the balance further with that user. If there is no url, use the default one they have been assigned.
            }
        }

        [UserCommand("Get Balance")]
        public async Task CheckAUsersBalance(IUser user)
        {
            var account = Bot.Economy.GetAccountFromId(user.Id);
            // check to see if user's account is not private, also finds guild through the channel.
            if (account.IsPrivate)
                await Context.Interaction.RespondAsync("", ChatUtils.EchoWithThumbnail((Context.Channel as SocketGuildChannel).Guild.Id, user.Username + " has set their account to private mode, you can not view private accounts.",
                    user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()));    //display the requested user's icon as a way to associate the balance further with that user. If there is no url, use the default one they have been assigned.
            else
                await Context.Interaction.RespondAsync("", ChatUtils.EchoWithThumbnail((Context.Channel as SocketGuildChannel).Guild.Id, user.Username + " has an account balance of: " + account.ToString(),  // print account balance 
                    user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()));   //display the requested user's icon as a way to associate the balance further with that user. If there is no url, use the default one they have been assigned.
        }

        [SlashCommand("pay", "See your current account balance.")]
        public async Task Pay( [Summary(description: "The user who you will be sending a payment to.")] SocketGuildUser user, 
            [Summary(description: "The amount of currency you want to pay the user.")] double currencyAmount,
            [Summary(description: "The message the user will be notified with in their direct messages.")] string message = null)
        {
            Bot.Economy.Account payerAccount = Bot.Economy.GetAccountFromId(Context.User.Id);    // get current user's account
            Bot.Economy.Account payeeAccount = Bot.Economy.GetAccountFromId(user.Id);    // get the account of the person being paid.

            // check if the user has enough currency to pay the user.
            if (payerAccount.Balance < currencyAmount)
            {
                double amountNeededToPay = currencyAmount - payerAccount.Balance; // calculate how much currency the user needs to pay them properly
                await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "You do not have enough currency to pay " + user.Username + ". You need " + Bot.Economy.CurrencyToFormatedString(amountNeededToPay) + " more to pay."));    // print account balance
                return;
            }

            payerAccount.Balance = payerAccount.Balance - currencyAmount; // subtract the payment from the payerAccount.
             // log it to transactions and logs
            payerAccount.Transactions.Add(new Economy.Transaction()    
            {
                Amount = currencyAmount,
                Information = "Paid " + user.Username + " " + Bot.Economy.CurrencyToFormatedString(currencyAmount),
                PayeeDiscordAccountId = payeeAccount.Id,
                PayerDiscordAccountId = payerAccount.Id,
                TimeOfTransaction = DateTime.Now,
            });
            payerAccount.Logs.Add(new Economy.Log()
            {
                Details = "Paid " + user.Username + " " + Bot.Economy.CurrencyToFormatedString(currencyAmount),
                TimeOfLog = DateTime.Now
            });

            payeeAccount.Balance = payeeAccount.Balance + currencyAmount; // add the payment to the payee's account.                                                            
            // log it to transactions and logs
            payeeAccount.Transactions.Add(new Economy.Transaction()
            {
                Amount = currencyAmount,
                Information = "Received " + Bot.Economy.CurrencyToFormatedString(currencyAmount) + " from " + Context.User.Username,
                PayeeDiscordAccountId = payeeAccount.Id,
                PayerDiscordAccountId = payerAccount.Id,
                TimeOfTransaction = DateTime.Now,
            });
            payeeAccount.Logs.Add(new Economy.Log()
            {
                Details = "Received " + Bot.Economy.CurrencyToFormatedString(currencyAmount) + " from " + Context.User.Username,
                TimeOfLog = DateTime.Now,
            });

            // save to file;
            Bot.Economy.UpdateAccount(payeeAccount);
            Bot.Economy.UpdateAccount(payerAccount);

            // send message in server
            await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, "Your payment to " + user.Mention + " has been successfully put through."));    // print account balance

            // dm the payee the payment notification
            var notificationEmbeded = ChatUtils.GenerateTemplateEmbeded(Context.Guild.Id, "");  // get template embeded so we can import all settings in the embeded automatically
            notificationEmbeded.WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl());    // if the user does not have a profile picture 
            notificationEmbeded.WithAuthor(new EmbedAuthorBuilder()
            {
                IconUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(),
                Name = "You have received " + Bot.Economy.CurrencyToFormatedString(currencyAmount) + " from " + Context.User.Username
            });
            notificationEmbeded.WithDescription(message ?? "You can check this transaction by going to the discord server " + Context.Guild.Name + " and executing the command /transactions or by checking your account balance by doing /bal ");
            await user.SendMessageAsync("", false, notificationEmbeded.Build());

            return;
        }

        [SlashCommand("baltop", "View the people with the most currency")]
        public async Task Baltop()
        {
            string directoryToSearch = Directory.GetCurrentDirectory() + Bot.Economy.saveDirectory;     // find the directory we need to search - which is the folder where the account jsons are saved as individual files.

            DirectoryInfo directoryInfo = new DirectoryInfo(directoryToSearch);     

            FileInfo[] files = directoryInfo.GetFiles("*.json"); //get all JSON files from the directory

            List<Bot.Economy.Account> accounts = new List<Economy.Account>();

            // loop through each file in the directory
            foreach (FileInfo file in files)
            {
                // find user id by getting the actual filename by removing the file type, for example .txt or .json
                ulong idFromFile = ulong.Parse(file.Name.Split('.')[0]);

                accounts.Add(Bot.Economy.GetAccountFromId(idFromFile)); // add user to the list of accounts.
            }

            var accountsInOrder = accounts.OrderBy(x => x.Balance).Reverse();     // order accounts in order of highest to lowest.

            // loop through each account to generate the baltop embeded message
            int indexCounter = 0;   // the counter used to count the top 10 users in order,
            var embededTemplate = ChatUtils.GenerateTemplateEmbeded(Context.Guild.Id, "this description will be replaced.", "The top 10 richest users");    // get a embeded template from the chat utlis to have a similar template
            SocketGuildUser richestUser = null;     // the #1 richest user in all of baltop.
            Bot.Economy.Account richestAccount = new Economy.Account();
            string descriptionBuilder = "This is the ranking of the highest account balances across all accounts registered with this bot." + Environment.NewLine;   // the main body of text under the other embeded elements.
            // for every account in the currently saved accounts.
            foreach (var account in accountsInOrder)
            {
                // check to make sure account is not private.
                if (account.IsPrivate)
                    continue;

                indexCounter++;
                if (indexCounter <= 10)     // we only want the top 10 accounts.
                {
                    // the display name of the user, if we can't get their username since they are in the server, or we don't have permission, display a message saying why to the user.
                    // since this is a multi-server economy bot, it is more than likely that people on the baltop will not be in every server the bot is on, thus we can't get their username.
                    string displayName = "";
                    var user = Context.Guild.GetUser(account.Id);
                    if (user != null)
                        displayName = user.Username;
                    else
                        displayName = "This user is not in this discord server, their user name could not be retrieved.";

                    // if we have not found the richest user yet, than we can assume that this is the first loop of accounts, and thus we have found the richest user.
                    if (richestUser == null && user != null)
                    {
                        richestUser = user;
                        richestAccount = account;
                    }

                    // add each user into the list.
                    descriptionBuilder += Environment.NewLine + "[ " + indexCounter.ToString() + " ] - **" + displayName + "**" + Environment.NewLine
                       + "*at " + account.ToString() + ". They first used this bot " + 
                       ((int)Math.Ceiling(DateTime.Now.Subtract(account.Logs[0].TimeOfLog).TotalDays)).ToString() + " days ago.*" + Environment.NewLine;
                }
            }

            // add the main body of text to the embeded. 
            embededTemplate.WithDescription(descriptionBuilder);

            // add the richest user at the top to give them a better effect 
            embededTemplate.WithAuthor(new EmbedAuthorBuilder()
            {
                Name = "The current richest user in this discord server is " + richestUser.Username + " at " + richestAccount.ToString(),
                IconUrl = "https://cdn.discordapp.com/attachments/940734878327124039/941630725277483098/2385865.png"
            });

            // add the richest user to the thumbnail to further their status and reward for being the richest user.
            embededTemplate.WithThumbnailUrl(richestUser.GetAvatarUrl() ?? richestUser.GetDefaultAvatarUrl());

            // build the embeded, since interaction.respondasync only takes embeded arrays we must convert the embeded into a list and than into an array. It only contains 1 array value.
            var listEmbed = new List<Embed>();
            listEmbed.Add(embededTemplate.Build());
            await Context.Interaction.RespondAsync("", listEmbed.ToArray());

        }
    }
}

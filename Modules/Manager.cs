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
    public class ManagerModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private CommandHandler _handler;

        // Constructor injection is also a valid way to access the dependecies
        public ManagerModule(CommandHandler handler)
        {
            _handler = handler;
        }

        [Group("manager", "All commands that can only be executed by manager roles.")]
        [RequireManager]
        public class ManagerGroup : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("bal", "See another persons balance.")]
            public async Task Balance([Summary(description: "The user who's balance you will be viewing.")] SocketUser user)
            {
                Bot.Economy.Account account = Bot.Economy.GetAccountFromId(user.Id);    // get account
                await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, user.Username + " has an account balance of: " + account.ToString()));    // print account balance

                // add this to logs, so we know the user has used a management command to view someone elses potentially private account.
                Bot.Economy.Account selfUser = Bot.Economy.GetAccountFromId(Context.User.Id);
                selfUser.Logs.Add(new Economy.Log("Using management, viewed the balance of " + user.Username + " who had their account set to " + (account.IsPrivate ? "Private." : "Public.")));
                Bot.Economy.UpdateAccount(account);
            }

            [SlashCommand("pay", "Pay anyone with an infinite amount of currency, no currency will be taken from you when using this.")]
            public async Task Pay([Summary(description: "The user who you will be sending a payment to.")] SocketGuildUser user, 
                [Summary(description: "The amount of currency you want to pay the user.")] double currencyAmount,
                [Summary(description: "The message the user will be notified with in their direct messages.")] string message = null)
            {
                // we do not need to subtract any currency from the user as they are using /manager pay which does not take currency from the user.

                Bot.Economy.Account payeeAccount = Bot.Economy.GetAccountFromId(user.Id);    // get the account of the person being paid.
                Bot.Economy.Account payerAccount = Bot.Economy.GetAccountFromId(Context.User.Id);    // get current user's account
                payerAccount.Transactions.Add(new Economy.Transaction()
                {
                    Amount = currencyAmount,
                    Information = "Paid " + user.Username + " " + Bot.Economy.CurrencyToFormatedString(currencyAmount) + " through management.",
                    PayeeDiscordAccountId = payeeAccount.Id,
                    PayerDiscordAccountId = payerAccount.Id,
                    TimeOfTransaction = DateTime.Now,
                });
                payerAccount.Logs.Add(new Economy.Log()
                {
                    Details = "Paid " + user.Username + " " + Bot.Economy.CurrencyToFormatedString(currencyAmount) + " through management.",
                    TimeOfLog = DateTime.Now,
                });
                
                // save to file, usually we would save at the same time, but if someone wants to pay themselves, they wont be able to, since the old version of themselfs (payee) will overide the new (payer).
                Bot.Economy.UpdateAccount(payerAccount);

                payeeAccount.Balance = payeeAccount.Balance + currencyAmount; // add the payment to the payee's account.                                                            
                // log it to transactions and logs
                payeeAccount.Transactions.Add(new Economy.Transaction()
                {
                    Amount = currencyAmount,
                    Information = "Received " + currencyAmount.ToString() + " from " + Context.User.Username,
                    PayeeDiscordAccountId = payeeAccount.Id,
                    PayerDiscordAccountId = payerAccount.Id,
                    TimeOfTransaction = DateTime.Now,
                });
                payeeAccount.Logs.Add(new Economy.Log()
                {
                    Details = "Received " + currencyAmount.ToString() + " from " + Context.User.Username,
                    TimeOfLog = DateTime.Now,
                });

                // save to file;
                Bot.Economy.UpdateAccount(payeeAccount);

                await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, "Your management payment to " + user.Mention + " has been successfully put through."));    // print account balance
                                                                                                                                                                                       // dm the payee the payment notification
                var notificationEmbeded = ChatUtils.GenerateTemplateEmbeded(Context.Guild.Id, "");  // get template embeded so we can import all settings in the embeded automatically
                notificationEmbeded.WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl());    // if the user does not have a profile picture 
                notificationEmbeded.WithAuthor(new EmbedAuthorBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(),
                    Name = "You have received " + Bot.Economy.CurrencyToFormatedString(currencyAmount) + " from " + Context.User.Username + " using management."
                });
                notificationEmbeded.WithDescription(message ?? "You can check this transaction by going to the discord server " + Context.Guild.Name + " and executing the command /transactions or by checking your account balance by doing /bal ");
                await user.SendMessageAsync("", false, notificationEmbeded.Build());

                return;
            }

            [SlashCommand("remove", "Remove an amount of currency from a user's account balance.")]
            public async Task Remove([Summary(description: "The user who you will be taking currency from.")] SocketGuildUser user, [Summary(description: "The amount of currency you will be taking from the user.")] double currencyAmount)
            {
                Bot.Economy.Account payeeAccount = Bot.Economy.GetAccountFromId(user.Id);    // get the account of the person being paid.
                Bot.Economy.Account payerAccount = Bot.Economy.GetAccountFromId(Context.User.Id);    // get current user's account
                payerAccount.Transactions.Add(new Economy.Transaction()
                {
                    Amount = currencyAmount,
                    Information = "Removed " + Bot.Economy.CurrencyToFormatedString(currencyAmount) + " from " + user.Username + " through management.",
                    PayeeDiscordAccountId = payeeAccount.Id,
                    PayerDiscordAccountId = payerAccount.Id,
                    TimeOfTransaction = DateTime.Now,
                });
                payerAccount.Logs.Add(new Economy.Log()
                {
                    Details = "Removed " + Bot.Economy.CurrencyToFormatedString(currencyAmount) + " from " + user.Username + " through management.",
                    TimeOfLog = DateTime.Now,
                });

                Bot.Economy.UpdateAccount(payerAccount);

                payeeAccount.Balance = payeeAccount.Balance - currencyAmount; // remove currency from the payees account balance                                                            
               
                // make sure a user never has a negative balance.
                if (payeeAccount.Balance < 0)
                {
                    payeeAccount.Balance = 0;
                }
                
                // log it to transactions and logs
                payeeAccount.Transactions.Add(new Economy.Transaction()
                {
                    Amount = currencyAmount,
                    Information = "Management removed " + currencyAmount.ToString() + " from " + Context.User.Username,
                    PayeeDiscordAccountId = payeeAccount.Id,
                    PayerDiscordAccountId = payerAccount.Id,
                    TimeOfTransaction = DateTime.Now,
                });
                payeeAccount.Logs.Add(new Economy.Log()
                {
                    Details = "Management removed " + currencyAmount.ToString() + " from " + Context.User.Username,
                    TimeOfLog = DateTime.Now,
                });

                // save to file;
                Bot.Economy.UpdateAccount(payeeAccount);

                await Context.Interaction.RespondAsync("", ChatUtils.Success(Context.Guild.Id, currencyAmount.ToString() + " has been removed from the account " + user.Mention + "."));    // print account balance
                return;
            }



            [SlashCommand("view-logs", "Get the logs of a user's account.")]
            public async Task Logs([Summary(description: "The user who's logs you will be viewing")] SocketGuildUser user)
            {
                Bot.Economy.Account targetUsersAccount = Bot.Economy.GetAccountFromId(user.Id);    // get the account of the person being viewed.
                Bot.Economy.Account currentUsersAccount = Bot.Economy.GetAccountFromId(Context.User.Id);    // get current user's account
                
                // add view logs to their logs
                currentUsersAccount.Logs.Add(new Economy.Log("Using management, viewed the logs of the user " + user.Username));
                Bot.Economy.UpdateAccount(currentUsersAccount);

                // create a text file in the logs output

                string fileLocation = Directory.GetCurrentDirectory() + LogsAndTransactionsModule.fileDirectory + user.Id.ToString() + ".txt";

                // go through each log and write it into the string fileContents which will be pushed into the file we will save after.
                string fileContents = "";
                targetUsersAccount.Logs.Reverse(); // make it so the most recent logs are added first.
                int count = 0;  // counter tally for file.
                foreach (Bot.Economy.Log log in targetUsersAccount.Logs)
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
                Bot.ChatUtils.Echo(Context.Guild.Id, "The logs are attached as a text file above!"));
                return;
            }


            [SlashCommand("add-propaganda", "Add a image to the propaganda list")]
            public async Task AddPropaganda([Summary(description: "The image URL of the propaganda such as: https://www.somewebsite.com/propaganda.png")] string imageUrl)
            {
                // if the URl is not valid than display a message saying so.
                Uri uriResult;
                bool isUrlReal = Uri.TryCreate(imageUrl, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!isUrlReal)
                {
                    await Context.Interaction.RespondAsync("",
                 Bot.ChatUtils.Echo(Context.Guild.Id, imageUrl + " is not a valid image link."));
                    return;
                }

                var settings = GuildSettings.Load(Context.Guild.Id);

                if (settings.PropagandaImagesUrls.Contains(imageUrl))
                {
                    await Context.Interaction.RespondAsync("",
                 Bot.ChatUtils.Echo(Context.Guild.Id, imageUrl + " is already in the propaganda list."));
                    return;
                }

                // add the propaganda to the list, than save it.
                settings.PropagandaImagesUrls.Add(imageUrl);
                GuildSettings.Save(settings);

                await Context.Interaction.RespondAsync("",
                Bot.ChatUtils.Echo(Context.Guild.Id, "Added " + imageUrl + " to the list of propaganda."));
                return;
            }

            [SlashCommand("remove-propaganda", "Remove a image from the propaganda list")]
            public async Task RemovePropaganda([Summary(description: "The image URL of the propaganda such as: https://www.somewebsite.com/propaganda.png")] string imageUrl)
            {
                var settings = GuildSettings.Load(Context.Guild.Id);

                if (!settings.PropagandaImagesUrls.Contains(imageUrl))
                {
                    await Context.Interaction.RespondAsync("",
                 Bot.ChatUtils.Echo(Context.Guild.Id, imageUrl + " could not be found in the propaganda list."));
                    return;
                }

                // add the propaganda to the list, than save it.
                settings.PropagandaImagesUrls.Remove(imageUrl);
                GuildSettings.Save(settings);

                await Context.Interaction.RespondAsync("",
                Bot.ChatUtils.Echo(Context.Guild.Id, "Removed " + imageUrl + " from the list of propaganda."));
                return;
            }


            [SlashCommand("set-daily", "Set the amount of currency given using the /daily command - if you want to disable, set to 0")]
            public async Task SetDaily([Summary(description: "The amount of currency that is issued each time /daily is actioned.")] double amountOfCurrency)
            {
                var settings = GuildSettings.Load(Context.Guild.Id);
                settings.DailyPayoutAmount = amountOfCurrency;
                GuildSettings.Save(settings);

                // if the amount of daily is enabled - if the amount if above 0, /daily is enabled - if the amount is set to 0, /daily is disabled
                if (amountOfCurrency > 0)
                {
                    await Context.Interaction.RespondAsync("",
                    Bot.ChatUtils.Success(Context.Guild.Id, "Set the daily amount to " + Economy.CurrencyToFormatedString(amountOfCurrency) + ". This will be paid out when the /daily command is used."));
                } else
                {
                    await Context.Interaction.RespondAsync("",
                       Bot.ChatUtils.Success(Context.Guild.Id, "/daily is now disabled."));
                }
                
            }
        }

    }
}

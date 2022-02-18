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
    public class RanksModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private CommandHandler handler;

        // Constructor injection is also a valid way to access the dependencies
        public RanksModule(CommandHandler _handler)
        {
            handler = _handler;
        }

        [SlashCommand("rankup", "Purchase the next available rank with currency.")]
        public async Task Rankup()
        {
            try
            {
                var settings = GuildSettings.Load(Context.Guild.Id);

                // if their is no ranks to view.
                if (settings.Ranks.Count <= 0)
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "There are currently no ranks in " + Context.Guild.Name + ", which means you cannot rankup."));
                    return;
                }

                var account = Economy.GetAccountFromId(Context.User.Id);
                var guildUser = Context.User as SocketGuildUser; // get user as a guild user to access guild roles.
                var orderdRanks = settings.Ranks.OrderBy(x => x.Cost);  // order ranks into cost order, so the ranks are in order in the same as !rankup
                int rankCounter = 0; // count the position in the hierarchy of ranks to use for rankup requirement of a minimum rank.
                foreach (RankSystem.Rank rank in orderdRanks)
                {
                    var roleToCheck = Context.Guild.GetRole(rank.RoleId);   // get the role that is in the guild
                    if (guildUser.Roles.Contains(roleToCheck))  // check if the guild user has that guild role
                    {
                        rankCounter++; 
                        // we have not found the role, the user already has the role, so this would indicate that they have already purchased/rankuped this role, so we skip till the next one.
                        continue;
                    }
                    else
                    {
                        // here we check if there is a minimum role required before using rankup
                        // if their is at least 1 minimum role, than we need to check.
                        if (settings.MinimumRole.Count() > 0)
                        {
                            string minimumRolesStringBuilder = "";
                            bool atleastHasOneRole = false;
                            // loop through each minimum role, and check to see that the user has at least 1 of the roles
                            foreach (ulong minimumRoleId in settings.MinimumRole)
                            {
                                // get role in server and check if the user has the role.
                                var minimumRole = Context.Guild.GetRole(minimumRoleId);
                                if (guildUser.Roles.Contains(minimumRole))
                                {
                                    // if they have this role, than we can set this to true.
                                    atleastHasOneRole = true;
                                }

                                // just encase there are no minimum roles, we can build a string of all the minimum roles a user will need.
                                minimumRolesStringBuilder += minimumRole.Mention + Environment.NewLine;     
                            }

                            // if they don't have atleast 1 minimum role, let the user know, and discontinue rankup command (exit)
                            if (!atleastHasOneRole)
                            {
                                await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "In order for you to use the rankup system, you need to already have one of the following roles: " + Environment.NewLine + minimumRolesStringBuilder));
                                return;
                            }
                        }

                        // check to see if the user has enough currency to purchase a rankup.
                        if (account.Balance >= rank.Cost)
                        {
                            account.Balance -= rank.Cost;   // take currency from their balance to purchase rank
                            // add logs and transactions data for tracking later.
                            account.Logs.Add(new Economy.Log()
                            {
                                Details = "Purchased " + Context.Guild.GetRole(rank.RoleId).Name + " for " + rank.ToString(),
                                TimeOfLog = DateTime.Now
                            });
                            account.Transactions.Add(new Economy.Transaction()
                            {
                                Amount = rank.Cost,
                                PayerDiscordAccountId = Context.User.Id,
                                PayeeDiscordAccountId = Context.Client.CurrentUser.Id,
                                Information = "Purchased " + Context.Guild.GetRole(rank.RoleId).Name + " for " + rank.ToString(),
                                TimeOfTransaction = DateTime.Now
                            });
                            Economy.UpdateAccount(account);

                            // we now know that the user can afford the rank, the user doesn't already have the rank, and wants to rankup. 
                            // we can initiate the rankup.

                            await guildUser.AddRoleAsync(roleToCheck); // add role to user.
                            await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, "Congratulations! You have successfully ranked up to " + roleToCheck.Mention + ". This rank was purchased for " + rank.ToString()));
                            return;
                        }
                        else
                        {
                            double currencyNeeded = rank.Cost - account.Balance;    // find out how much currency we need in order to purchase a rankup, this will be useful to the user who probably wants to know.
                            await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "You do not have enough currency to rankup. You need " + currencyNeeded.ToString()));
                            return;
                        }
                    }
                }

                // if we have skipped all ranks, it should be assumed that we have reached the max rank
                await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "You cannot rankup any further, as you have already achieved the maximum rank in the rankup system."));
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("Ranks.cs:line 62"))     // their is an error because the bot's rank is lower in the hierarchy than the request rank.
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.RoleHiearchyError(Context.Guild.Id));
                }
            }
        }

        [SlashCommand("view-ranks", "View all ranks in this discord's rankup system.")]
        public async Task ViewRanks()
        {
            var settings = GuildSettings.Load(Context.Guild.Id);

            // if their is no ranks to view.
            if (settings.Ranks.Count <= 0)
            {
                await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "There are currently no ranks in " + Context.Guild.Name + "."));
                return;
            }

            // loop through each rank in order and display it's cost 
            int indexCounter = 0;  
            var embededTemplate = ChatUtils.GenerateTemplateEmbeded(Context.Guild.Id, "You start off at the 1st rank, and progress to the last rank." + Environment.NewLine);
            var orderdRanks = settings.Ranks.OrderBy(x => x.Cost);  // order ranks into cost order, so the ranks are in order in the same as !rankup
            foreach (RankSystem.Rank rank in orderdRanks)
            {
                // check to see if the rank has a description.
                indexCounter++;
                if (rank.Description != null)
                {
                    embededTemplate.AddField(new EmbedFieldBuilder()
                    {
                        Name = "[ " + indexCounter.ToString() + " ] - __" + Context.Guild.GetRole(rank.RoleId).Name + "__ costs " + rank.ToString() + Environment.NewLine,
                        Value = "*" + rank.Description + "*"
                    });
                } else
                {
                    embededTemplate.AddField(new EmbedFieldBuilder()
                    {
                        Name = "[ " + indexCounter.ToString() + " ] - __" + Context.Guild.GetRole(rank.RoleId).Name + "__ costs " + rank.ToString() + Environment.NewLine,
                        Value = "*" + "There is no description for this rank." + "*"
                    });

                }
            }
            // we have to present the embed as an array of embeds, so we can easily make a list of them, than convert that into an array
            var listEmbed = new List<Embed>();
            listEmbed.Add(embededTemplate.Build());
            await Context.Interaction.RespondAsync("", listEmbed.ToArray());
        }
    }
}

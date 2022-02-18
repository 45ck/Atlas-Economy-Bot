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
    public class SettingsModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private CommandHandler _handler;

        // Constructor injection is also a valid way to access the dependencies
        public SettingsModule( CommandHandler handler )
        {
            _handler = handler;
        }

        // [Group] will create a command group. [SlashCommand]s and [ComponentInteraction]s will be registered with the group prefix
        [Group("settings", "Change the bot's settings and properties.")]
        [RequireAdmin]
        public class GroupExample : InteractionModuleBase<SocketInteractionContext>
        {
            public enum SettingsColor
            {
                Black,
                White,
                Red,
                Lime,
                Blue,
                Yelow,
                Aqua,
                Pink,
                Silver,
                Gray,
                Brown,
                Olive,
                Green,
                Purple,
                Teal,
                Navy,
                Gold
            }

            /// <summary>
            /// Change the color of the bot when sending embeded messages.
            /// </summary>
            /// <param name="color"></param>
            /// <returns></returns>
            [SlashCommand("color", "Change the color theme of the bot.")]
            public async Task ChangeColor( SettingsColor color)
            {
                var settings = GuildSettings.Load(Context.Guild.Id);

                switch (color)
                {
                    case SettingsColor.Black:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(0,0,0);
                        break;
                    case SettingsColor.White:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(255, 255, 255);
                        break;
                    case SettingsColor.Red:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(255, 0, 0);
                        break;
                    case SettingsColor.Lime:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(0, 255, 0);
                        break;
                    case SettingsColor.Blue:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(0, 0, 255);
                        break;
                    case SettingsColor.Yelow:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(255, 255, 0);
                        break;
                    case SettingsColor.Aqua:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(0, 255, 255);
                        break;
                    case SettingsColor.Pink:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(255, 0, 255);
                        break;
                    case SettingsColor.Silver:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(192, 192, 192);
                        break;
                    case SettingsColor.Gray:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(128, 128, 128);
                        break;
                    case SettingsColor.Brown:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(128, 0, 0);
                        break;
                    case SettingsColor.Olive:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(128, 128, 0);
                        break;
                    case SettingsColor.Green:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(0, 128, 0);
                        break;
                    case SettingsColor.Purple:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(128, 0, 128);
                        break;
                    case SettingsColor.Teal:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(0, 128, 128);
                        break;
                    case SettingsColor.Navy:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(0, 0, 128);
                        break;
                    case SettingsColor.Gold:
                        settings.ColorTheme = new GuildSettings.SerialisedColor(255,215,0);
                        break;
                }

                GuildSettings.Save(settings);

                await Context.Interaction.RespondAsync("", ChatUtils.Success(Context.Guild.Id, "The bot color theme has been changed to '" + color.ToString() + "'"));

                // add this to logs, so we know who has changed what setting.
                Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);
                account.Logs.Add(new Economy.Log("Changed setting of color to " + color.ToString()));
                Bot.Economy.UpdateAccount(account);
            }

            [SlashCommand("add-manager", "Add a manager role to the bot's list of accepted manager roles.")]
            public async Task AddManager(SocketRole role)
            {
                if (role.IsManaged)     // make sure that no bot roles are added.
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "The role " + role.Mention + " cannot be added to the accepted managers role list as it is a managed role, most likely owned by a bot."));
                    return;
                }

                var settings = GuildSettings.Load(Context.Guild.Id);

                // if this role has already been put into management roles.
                if (settings.ManagerRoles.Contains(role.Id))
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "The role " + role.Mention + " has already been added to the accepted manager roles list."));
                    return;
                }

                // add the role to the list.
                settings.ManagerRoles.Add(role.Id);

                GuildSettings.Save(settings);

                await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, "The role " + role.Mention + " has been added to the list of management roles."));

                // add this to logs, so we know who has changed what setting.
                Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);
                account.Logs.Add(new Economy.Log("Added a manager role to settings, the role is named '" + role.Name + "'"));
                Bot.Economy.UpdateAccount(account);
            }

            [SlashCommand("remove-manager", "Remove a manager role from the bot's list of accepted manager roles.")]
            public async Task RemoveManager(SocketRole role)
            {
                var settings = GuildSettings.Load(Context.Guild.Id);

                // if this role has already been put into management roles.
                if (!settings.ManagerRoles.Contains(role.Id))
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "The role " + role.Mention + " is not currently in the list of accepted manager roles, thus you cannot remove it."));
                    return;
                }

                // add the role to the list.
                settings.ManagerRoles.Remove(role.Id);

                GuildSettings.Save(settings);

                await Context.Interaction.RespondAsync("", ChatUtils.Success(Context.Guild.Id, "The role " + role.Mention + " has been removed from the list of accepted management roles."));

                // add this to logs, so we know who has changed what setting.
                Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);
                account.Logs.Add(new Economy.Log("Removed a manager role from settings, the role is named '" + role.Name + "'"));
                Bot.Economy.UpdateAccount(account);
            }

            [SlashCommand("view-manager-roles", "View all currently accepted management roles.")]
            public async Task ViewAcceptedManagerRoles()
            {
                var settings = GuildSettings.Load(Context.Guild.Id);

                // if this role has already been put into management roles.
                if (settings.ManagerRoles.Count <= 0)
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "There are currently no accepted management roles in " + Context.Guild.Name + "."));
                    return;
                }

                string roles = "All current management roles in this discord server: " + Environment.NewLine;
                foreach(ulong roleId in settings.ManagerRoles)
                {
                    roles += Context.Guild.GetRole(roleId).Mention + Environment.NewLine;
                }

                await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, roles));
            }


            [SlashCommand("add-manager-user", "Allow an individual user to have manager permissions, despite their role.")]
            public async Task AddManagerUser(SocketUser user)
            {
                var settings = GuildSettings.Load(Context.Guild.Id);

                // if this user is already in the management users
                if (settings.ManagerUsers.Contains(user.Id))
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "The user " + user.Mention + " has already been added to the management user list."));
                    return;
                }

                // add the user to the list.
                settings.ManagerUsers.Add(user.Id);

                GuildSettings.Save(settings);

                await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, "The user " + user.Mention + " has been added to the list of management users. This user will now have permission to fully execute all management slash commands."));

                // add this to logs, so we know who has changed what setting.
                Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);
                account.Logs.Add(new Economy.Log("Added a manager user to settings, the user is named '" + user.Username + "'"));
                Bot.Economy.UpdateAccount(account);
            }

            [SlashCommand("remove-manager-user", "Remove a manager user from the managers list.")]
            public async Task RemoveManagerUser(SocketUser user)
            {
                var settings = GuildSettings.Load(Context.Guild.Id);

                // if this user has already been put into management users list.
                if (!settings.ManagerRoles.Contains(user.Id))
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "The user " + user.Mention + " is not currently in the list of management users, thus you cannot remove them."));
                    return;
                }

                // add the user to the list.
                settings.ManagerUsers.Remove(user.Id);

                GuildSettings.Save(settings);

                await Context.Interaction.RespondAsync("", ChatUtils.Success(Context.Guild.Id, "The user " + user.Mention + " has been removed from the list of management users,"));

                // add this to logs, so we know who has changed what setting.
                Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);
                account.Logs.Add(new Economy.Log("Removed a user from the list of management users, the user is named '" + user.Username + "'"));
                Bot.Economy.UpdateAccount(account);
            }

            [SlashCommand("view-manager-users", "View all current users who have management permissions")]
            public async Task ViewManagementUsers()
            {
                var settings = GuildSettings.Load(Context.Guild.Id);

                // if this role has already been put into management roles.
                if (settings.ManagerUsers.Count <= 0)
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "There are currently no users who individually have management permissions in " + Context.Guild.Name + "."));
                    return;
                }

                string roles = "All current users in this discord server who individually have management permission: " + Environment.NewLine;
                foreach (ulong userId in settings.ManagerUsers)
                {
                    roles += Context.Guild.GetUser(userId).Mention + Environment.NewLine;
                }

                await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, roles));
            }

            [SlashCommand("add-rank", "Add a rank into the rankup system")]
            public async Task AddRank([Summary(description:"The role associated with this rank, this is the role given to the user when they rankup")] SocketRole role, 
                [Summary(description:"The cost in currency that this role costs to purchase using the rankup command")] double cost,
                [Summary(description: "The description of this rank, what does it mean and why do people want it?")] string description = null)
            {
                if (role.IsManaged || role.IsEveryone)     // make sure that no bot roles are added.
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "The role " + role.Mention + " cannot be added to the rankup system as it is a managed role, most likely owned by a bot."));
                    return;
                } 

                var settings = GuildSettings.Load(Context.Guild.Id);

                // if this role is already a rank
                if (settings.Ranks.Where(x => x.RoleId == role.Id).Count() > 0)
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "The role " + role.Mention + " has already been added to the rankup system."));
                    return;
                }

                // a rank cannot have the same price as another rank, due to it being a incremental rankup system.
                if (settings.Ranks.Where(x => x.Cost == cost).Count() > 0)
                {
                    var rankThatIsTheSame = settings.Ranks.Where(x => x.Cost == cost).ToList()[0];
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "In the rankup system, you cannot have multiple ranks with the same cost. This is because ranks are ordered in their cost." +
                        " You need to make sure that all ranks have different prices." + Environment.NewLine +
                        "You tried to add '" + role.Name + "' for a cost of " + Bot.Economy.CurrencyToFormatedString(cost) + 
                        ", which is the same as '" + Context.Guild.GetRole(rankThatIsTheSame.RoleId).Name + "' which has a cost of " + rankThatIsTheSame.ToString()));
                    return;
                }

                // add the role to the rankup system.
                settings.Ranks.Add(new RankSystem.Rank()
                {
                    RoleId = role.Id,
                    Cost = cost,
                    Description = description
                });

                GuildSettings.Save(settings);

                await Context.Interaction.RespondAsync("", ChatUtils.Success(Context.Guild.Id, "The role " + role.Mention + " has been added to the rankup system."));

                // add this to logs, so we know who has changed what setting.
                Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);
                account.Logs.Add(new Economy.Log("Added a rank to the rankup system, the rank is called '" + role.Name + "'"));
                Bot.Economy.UpdateAccount(account);
            }

            [SlashCommand("remove-rank", "Remove a rank into the rankup system")]
            public async Task RemoveRank([Summary(description: "The role associated with the rank you want to remove.")] SocketRole role)
            {
                var settings = GuildSettings.Load(Context.Guild.Id);

                // if this role is not in ranks
                if (settings.Ranks.Where(x => x.RoleId == role.Id).Count() <= 0)
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "The role " + role.Mention + " is not in the rankup system, thus it cannot be removed."));
                    return;
                }

                // remove the role from the rankup system.
                settings.Ranks.Remove(settings.Ranks.Where(x => x.RoleId == role.Id).ToArray()[0]);

                GuildSettings.Save(settings);

                await Context.Interaction.RespondAsync("", ChatUtils.Success(Context.Guild.Id, "The role " + role.Mention + " has been removed from the rankup system."));

                // add this to logs, so we know who has changed what setting.
                Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);
                account.Logs.Add(new Economy.Log("Removed a rank from the rankup system, the rank is called '" + role.Name + "'"));
                Bot.Economy.UpdateAccount(account);
            }

            [SlashCommand("add-minimum-rankup-role", "Each user must have at least 1 of these roles in order to use the rankup system")]
            public async Task AddMinimumRole([Summary(description: "The role that will be added to the list of required roles in the rankup system")] SocketRole role)
            {
                if (role.IsManaged)     // make sure that no bot roles are added.
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "The role " + role.Mention + " cannot be added to the rankup system as it is a managed role, most likely owned by a bot."));
                    return;
                }

                var settings = GuildSettings.Load(Context.Guild.Id);

                // if this role is already a minimum required role
                if (settings.MinimumRole.Where(x => x == role.Id).Count() > 0)
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "The role " + role.Mention + " has already been added as a minimum required role to the rankup system."));
                    return;
                }

                // add the role to the list of minimum required roles.
                settings.MinimumRole.Add(role.Id);

                GuildSettings.Save(settings);

                await Context.Interaction.RespondAsync("", ChatUtils.Success(Context.Guild.Id, "The role " + role.Mention + " has been added to the minimum required roles."));

                // add this to logs, so we know who has changed what setting.
                Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);
                account.Logs.Add(new Economy.Log("Added a minimum required role to the rankup system, the role is called '" + role.Name + "'"));
                Bot.Economy.UpdateAccount(account);
            }

            [SlashCommand("remove-minimum-rankup-role", "Remove a role from the minimum required roles in the rankup system")]
            public async Task RemoveMinimumRole([Summary(description: "The role that you want to remove from the list of minimum required roles.")] SocketRole role)
            {
                var settings = GuildSettings.Load(Context.Guild.Id);

                // if this role is not in ranks
                if (settings.MinimumRole.Where(x => x == role.Id).Count() <= 0)
                {
                    await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "The role " + role.Mention + " is not in the minimum required roles list, thus it cannot be removed."));
                    return;
                }

                // remove the role from the minimum required rankup roles
                settings.MinimumRole.Remove(role.Id);

                GuildSettings.Save(settings);

                await Context.Interaction.RespondAsync("", ChatUtils.Success(Context.Guild.Id, "The role " + role.Mention + " has been removed from the minimum required role list in the rankup system."));

                // add this to logs, so we know who has changed what setting.
                Bot.Economy.Account account = Bot.Economy.GetAccountFromId(Context.User.Id);
                account.Logs.Add(new Economy.Log("Removed a role from the minimum required role list in the rankup system, the role is called '" + role.Name + "'"));
                Bot.Economy.UpdateAccount(account);
            }
        }
    }
}

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    /// <summary>
    /// Check if the user is a manager/admin 
    /// </summary>
    public class RequireManagerAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    var application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
                    // loop through each role that we want to check that the user has, to see if they are considered a manager.
                    foreach(ulong managerRoleId in Bot.GuildSettings.Load(context.Guild.Id).ManagerRoles)
                    {
                        // check if user has the adminRole, if they do than let them use the command, if not eventually after looping through it will return as they can't use it.
                        if ((context.User as SocketGuildUser).Roles.Any(r => r.Id == managerRoleId))
                            return PreconditionResult.FromSuccess();
                    }

                    // now lets check individual users, loop through each user and check if the user is considered a manager
                    foreach(ulong managerUserId in Bot.GuildSettings.Load(context.Guild.Id).ManagerUsers)
                    {
                        if (context.User.Id == managerUserId) 
                            return PreconditionResult.FromSuccess();
                    }
                    return PreconditionResult.FromError(ErrorMessage ?? "This command can only be run by managers.");
                default:
                    return PreconditionResult.FromError($"{nameof(RequireManagerAttribute)} is not supported by this {nameof(TokenType)}.");
            }
        }
    }

}

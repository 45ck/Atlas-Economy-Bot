using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class RequireAdminAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync (IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    var application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
                    if (!(context.User as IGuildUser).GuildPermissions.Administrator)
                        return PreconditionResult.FromError(ErrorMessage ?? "Command can only be run by a administrator.");
                    return PreconditionResult.FromSuccess();
                default:
                    return PreconditionResult.FromError($"{nameof(RequireAdminAttribute)} is not supported by this {nameof(TokenType)}.");
            }
        }
    }
}

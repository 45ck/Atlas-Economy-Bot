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
    public class UtilityModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private CommandHandler _handler;

        // Constructor injection is also a valid way to access the dependencies
        public UtilityModule ( CommandHandler handler )
        {
            _handler = handler;
        }


        [SlashCommand("help", "Get help with this bot,")]
        public async Task Help()
        {
            // get help message from config and than spit it to discord as a embeded message to the user.
            string helpMessage = Bot.Configuration.Config.ConfigurableMessages.HelpMessage;
            await Context.Interaction.RespondAsync("", ChatUtils.Echo(Context.Guild.Id, helpMessage));   
        }


        // cool command I added which sends a reaction to the message when you right click through apps.
        [MessageCommand("Vouch")]
        [RequireOwner]
        public async Task Vouch(IMessage message)
        {
            await Context.Interaction.DeferAsync(); // creates the "bot is thinking...."

            // adds emojis one at a time.
            await message.AddReactionAsync(new Emoji("👍"));
            await message.AddReactionAsync(new Emoji("🇻"));
            await message.AddReactionAsync(new Emoji("🇴"));
            await message.AddReactionAsync(new Emoji("🇺"));
            await message.AddReactionAsync(new Emoji("🇨"));
            await message.AddReactionAsync(new Emoji("🇭"));
            await message.AddReactionAsync(new Emoji("✅"));

            // generates vouch message
            string vouchMessage = " ";
            if (message.Content != null)
                vouchMessage = Environment.NewLine + "'" + message.Content + "'";

            // sends message.
            await Context.Interaction.FollowupAsync("", ChatUtils.Echo(Context.Guild.Id, Context.User.Mention + " vouched " + message.Author.Mention +
                vouchMessage));
        }

        // cool command I added which sends a reaction to the message when you right click through apps.
        [MessageCommand("Imp")]
        [RequireOwner]
        public async Task Imp(IMessage message)
        {
            await Context.Interaction.DeferAsync(); // creates the "bot is thinking...."

            // adds emojis one at a time.
            await message.AddReactionAsync(new Emoji("😱"));
            await message.AddReactionAsync(new Emoji("😎"));
            await message.AddReactionAsync(new Emoji("🇮"));
            await message.AddReactionAsync(new Emoji("🇲"));
            await message.AddReactionAsync(new Emoji("🇵"));
            await message.AddReactionAsync(new Emoji("🤨"));
            await message.AddReactionAsync(new Emoji("😳"));

            await Context.Interaction.FollowupAsync("Done!", ephemeral: true);  // ephemeral messages will only work when bot and server permissions are configured correctly (enable global commands)
        }
    }
}

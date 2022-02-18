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
    public class PropagandaModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private CommandHandler handler;

        // Constructor injection is also a valid way to access the dependencies
        public PropagandaModule(CommandHandler _handler)
        {
            handler = _handler;
        }

        // need to only do random once, so that way the seed is always unique. If instalise every time you use the command it will use a similar seed.
        Random random = new Random();

        [SlashCommand("propaganda", "View a random propaganda image")]
        public async Task Propaganda()
        {
            // load settings from guild
            var settings = GuildSettings.Load(Context.Guild.Id);

            // if their is no propaganda been set, than we can show a message saying that
            if (settings.PropagandaImagesUrls.Count <= 0)
            {
                await Context.Interaction.RespondAsync("", ChatUtils.Error(Context.Guild.Id, "This server does not have any propaganda to show you 🥺"));   
                return;
            }

            // now we know there is at least 1 propaganda image.
            var embededBuilder = ChatUtils.GenerateTemplateEmbeded(Context.Guild.Id, "Here is some propaganda for you: ");      // get a template of the embeded
            embededBuilder.WithImageUrl(settings.PropagandaImagesUrls[random.Next(0, settings.PropagandaImagesUrls.Count)]);    // select a random image from propaganda

            // because the respondasync method only takes in embeded arrays, we need to make an array with only 1 var in it.
            List<Embed> embeds = new List<Embed>();
            embeds.Add(embededBuilder.Build());
            await Context.Interaction.RespondAsync("", embeds.ToArray());    // show image of the propaganda.
            return;
        }
    }
}

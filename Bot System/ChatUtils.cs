using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public static class ChatUtils
    {
        /// <summary>
        /// A simple way to make a 1 line embeded message.
        /// </summary>
        /// <param name="toSay"></param>
        /// <param name="messsageTitle" description="optional title of the embeded"></param>
        /// <returns></returns>
        public static Embed[] Echo(ulong currentGuildId, object toSay, object messsageTitle = null)
        {
            // compile all information above into a embeded object
            Embed[] embededArray = new Embed[1] { GenerateTemplateEmbeded(currentGuildId, toSay, messsageTitle).Build() };
            return embededArray;
        }

        /// <summary>
        /// Generate an error message embeded 
        /// </summary>
        /// <param name="currentGuildId"></param>
        /// <param name="errorMessage"></param>
        /// <param name="errorTitle"></param>
        /// <returns></returns>
        public static Embed[] Error(ulong currentGuildId, object errorMessage, object errorTitle = null)
        {
            // get embeded
            var builder = GenerateTemplateErrorEmbeded(currentGuildId, errorMessage, errorTitle);

            // compile all information above into a embeded object
            Embed[] embededArray = new Embed[1] { builder.Build() };
            return embededArray;
        }


        /// <summary>
        /// Generates a success embeded for the user
        /// </summary>
        /// <param name="currentGuildId"></param>
        /// <param name="succesMessage"></param>
        /// <param name="succesTitle"></param>
        /// <returns></returns>
        public static Embed[] Success(ulong currentGuildId, object succesMessage, object succesTitle = null)
        {
            var builder = new EmbedBuilder();
            builder.Description = succesMessage.ToString();
            // if there is a title, than we can add it.
            if (succesTitle != null)
                builder.Title = succesTitle.ToString();

            // set Success to color of Success in config.
            builder.WithColor(Configuration.Config.Success.SuccessColor.r,
                Configuration.Config.Success.SuccessColor.g,
                Configuration.Config.Success.SuccessColor.b);
            builder.WithCurrentTimestamp(); // include timestamp for users help

            // if succesIconUrl is not null than we will use it for errors.
            if (Bot.Configuration.Config.Success.SuccessIconUrl != null && Bot.Configuration.Config.Success.SuccessIconUrl != String.Empty)
            {
                builder.WithThumbnailUrl(Bot.Configuration.Config.Success.SuccessIconUrl);
            }

            // compile all information above into a embeded object
            Embed[] embededArray = new Embed[1] { builder.Build() };
            return embededArray;
        }


        /// <summary>
        /// Echo but does not require input of a guild. 
        /// </summary>
        /// <param name="toSay"></param>
        /// <param name="messsageTitle" description="optional title of the embeded"></param>
        /// <returns></returns>
        public static Embed[] EchoMinimum(object toSay, object messsageTitle = null)
        {
            var builder = new EmbedBuilder();
            builder.Description = toSay.ToString();
            // if there is a title, than we can add it.
            if (messsageTitle != null)
                builder.Title = messsageTitle.ToString();

            builder.WithCurrentTimestamp(); // include timestamp for users help
            if (Configuration.Config.BotIconUrl != null)    // if the bot has a icon, we can use it as their thumbnail
                builder.ThumbnailUrl = Configuration.Config.BotIconUrl;

            // compile all information above into a embeded object
            Embed[] embededArray = new Embed[1] { builder.Build() };
            return embededArray;
        }


        /// <summary>
        /// Generate a template editable embeded message
        /// </summary>
        /// <param name="toSay"></param>
        /// <param name="messsageTitle" description="optional title of the embeded"></param>
        /// <returns></returns>
        public static EmbedBuilder GenerateTemplateEmbeded(ulong currentGuildId, object toSay, object messsageTitle = null)
        {
            var builder = new EmbedBuilder();
            builder.Description = toSay.ToString();
            // if there is a title, than we can add it.
            if (messsageTitle != null)
                builder.Title = messsageTitle.ToString();

            // load color from individual guild settings
            var settings = GuildSettings.Load(currentGuildId);
            builder.WithColor(settings.ColorTheme.r, settings.ColorTheme.g, settings.ColorTheme.b);
            builder.WithCurrentTimestamp(); // include timestamp for users help
            if (Configuration.Config.BotIconUrl != null)    // if the bot has a icon, we can use it as their thumbnail
                builder.ThumbnailUrl = Configuration.Config.BotIconUrl;

            // return an editable embeded builder.
            return builder;
        }

        public static EmbedBuilder GenerateTemplateErrorEmbeded(ulong currentGuildId, object errorMessage, object errorTitle = null)
        {
            var builder = new EmbedBuilder();
            builder.Description = errorMessage.ToString();
            // if there is a title, than we can add it.
            if (errorTitle != null)
                builder.Title = errorTitle.ToString();

            // set error to color of error in config.
            builder.WithColor(Configuration.Config.Errors.ErrorColor.r,
                Configuration.Config.Errors.ErrorColor.g,
                Configuration.Config.Errors.ErrorColor.b);
            builder.WithCurrentTimestamp(); // include timestamp for users help

            // if errorIconUrl is not null than we will use it for errors.
            if (Bot.Configuration.Config.Errors.ErrorIconUrl != null && Bot.Configuration.Config.Errors.ErrorIconUrl != String.Empty)
            {
                builder.WithThumbnailUrl(Bot.Configuration.Config.Errors.ErrorIconUrl);
            }
            return builder; 
        }


        /// <summary>
        /// The echo function but you can add an author icon
        /// </summary>
        /// <param name="toSay"></param>
        /// <param name="messsageTitle" description="optional title of the embeded"></param>
        /// <returns></returns>
        public static Embed[] EchoWithAuthorIcon(ulong currentGuildId, object toSay, string iconUrl, object messsageTitle = null)
        {
            // compile all information above into a embeded object
            var embeded = GenerateTemplateEmbeded(currentGuildId, toSay, messsageTitle);
            var author = new EmbedAuthorBuilder();
            author.WithIconUrl(iconUrl);    // set icon here, use author to set icon
            embeded.WithAuthor(author);
            Embed[] embededArray = new Embed[1] { embeded.Build() };
            return embededArray;
        }

        /// <summary>
        /// The echo function but you can add an thumbnail
        /// </summary>
        /// <param name="toSay"></param>
        /// <param name="messsageTitle" description="optional title of the embeded"></param>
        /// <returns></returns>
        public static Embed[] EchoWithThumbnail(ulong currentGuildId, object toSay, string iconUrl, object messsageTitle = null)
        {
            // compile all information above into a embeded object
            var embeded = GenerateTemplateEmbeded(currentGuildId, toSay, messsageTitle);
            embeded.WithThumbnailUrl(iconUrl);
            Embed[] embededArray = new Embed[1] { embeded.Build() };
            return embededArray;
        }

        /// <summary>
        /// Generate an error with picture on how to fix role hierarchy error.
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public static Embed[] RoleHiearchyError (ulong guildId)
        {
            var builder = ChatUtils.GenerateTemplateErrorEmbeded(guildId, "e");
            builder.WithImageUrl("https://cdn.discordapp.com/attachments/940734878327124039/941802391030882314/Help_1.png");
            builder.WithTitle("Failed to execute command. Your permissions for this bot are not correctly setup correctly.");
            builder.WithDescription("Please make sure in your server settings that the bot's role is above all roles that it needs to interact with, as shown in the example below. The bot needs to be able to interact with roles that are associated with ranks, management and more. For these commands to work the bot must be above all roles it needs to interact with.");

            // need to make embeded into an array to be compatible with respondasync
            List<Embed> embeds = new List<Embed>();
            embeds.Add(builder.Build());

            return embeds.ToArray();
        }

        /// <summary>
        /// Generate a picture and error message on how to fix server intents.
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public static Embed[] ServerIntentsError(ulong guildId)
        {
            var builder = ChatUtils.GenerateTemplateErrorEmbeded(guildId, "e");
            builder.WithImageUrl("https://cdn.discordapp.com/attachments/940734878327124039/941809492650262568/Help_2.png");
            builder.WithTitle("Failed to execute command. Your bot application settings are not correctly setup.");
            builder.WithDescription("Please make sure to enable Privileged Gateway Intents on your bot application such as in the example below. " + Environment.NewLine 
                + " You can do so by first clicking the link, than selecting the bot application that corresponds with this bot, than go to the 'Bot' setting, and from their enable all 3 intents (PRESENCE INTENT, SERVER MEMBERS INTENT and MESSAGE CONTENT INTENT) ");
            builder.WithUrl("https://discord.com/developers/applications");

            // need to make embeded into an array to be compatible with respondasync
            List<Embed> embeds = new List<Embed>();
            embeds.Add(builder.Build());

            return embeds.ToArray();
        }
    } 
}

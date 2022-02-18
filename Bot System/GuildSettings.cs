using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Discord;

namespace Bot
{
    public static class GuildSettings
    {
        public const string settingsFileDirectory = @"/GuildSettings/";

        /// <summary>
        /// The config template that is used for serialization into YAML
        /// </summary>
        public struct Settings
        {
            public char Prefix { get; set; }    // the prefix of the bot, if they are using text-based commands, such as '!pay' or '!bal'
            public SerialisedColor ColorTheme { get; set; } // the color theme of the bot in this guild.

            public ulong GuildId { get; set; }  // the id of the guild that this settings represents
            public List<ulong> ManagerRoles { get; set; }     // all users who have any of these roles are able to execute /manager commands.
            public List<ulong> ManagerUsers { get; set; }     // all users who are able to execute /manager commands
            public List<RankSystem.Rank> Ranks { get; set; }    // the ranks of each discord that a user can rankup with
            public List<ulong> MinimumRole { get; set; }    // the list of required roles before using /rankup - if there are none it will result into allowing anyone to use /rankup
            public double DailyPayoutAmount { get; set; } // the amount of currency that is paid out when doing /daily - if it is at 0, it will say the command is disabled.
            public List<string> PropagandaImagesUrls { get; set; }      // the images for the propaganda command
        }


        /// <summary>
        ///  Load config.yaml through serialization. If config.yaml does not exist it will create a new one.
        /// </summary>
        public static Settings Load(ulong guildID)
        {
            Settings guildSettings;
            // get file location where all guildsettings are stored.
            string fileLocation =  Directory.GetCurrentDirectory() + settingsFileDirectory + guildID.ToString() + ".yaml";

            // If their is no config than create a new config, if their is a config, skip this and just load it in.
            if (!File.Exists(fileLocation))
            {
                // create settings for guild that doesn't have any yet.
                var newSettings = new Settings()
                {
                    Prefix = '!',
                    ColorTheme = new SerialisedColor(Configuration.Config.EmbededColor.r, Configuration.Config.EmbededColor.g, Configuration.Config.EmbededColor.b),
                    GuildId = guildID,
                    ManagerRoles = new List<ulong>(),
                    ManagerUsers = new List<ulong>(),
                    Ranks = new List<RankSystem.Rank>(),
                    MinimumRole = new List<ulong>(),
                    DailyPayoutAmount = 0,
                    PropagandaImagesUrls = new List<string>()
                };

                return Save(newSettings);
            }
            else
            {
                // create a deserializer instance to convert YAML into C# objects.
                var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

                // get settings from the directory that it is stored in
                guildSettings = deserializer.Deserialize<Settings>(File.ReadAllText(fileLocation));
                return guildSettings;
            }
        }

        /// <summary>
        /// Save settings to file for later
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static Settings Save(Settings settings)
        {
            // get file location where all guildsettings are stored.
            string fileLocation = Directory.GetCurrentDirectory() + settingsFileDirectory + settings.GuildId.ToString() + ".yaml";

            // serialise the config above into yaml
            var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
            var yamlAsTextForFile = serializer.Serialize(settings);

            // put the serialised yaml into a file for us to configure in the file.
            File.WriteAllText(fileLocation, yamlAsTextForFile);

            return settings;
        }

        public struct SerialisedColor
        {
            public int r;
            public int g;
            public int b;

            public SerialisedColor(int R, int G, int B)
            {
                this.r = R;
                this.g = G;
                this.b = B;
            }
        }
    }
}


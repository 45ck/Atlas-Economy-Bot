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
    public static class Configuration
    {
        /// <summary>
        /// The config template that is used for serialization into YAML
        /// </summary>
        public struct ConfigType
        {
            public Securitys Secerity { get; set; }
            public string Name { get; set; }    // the name of the bot
            public char Prefix { get; set; }    // the prefix of the bot
            public SerialisedColor EmbededColor { get; set; }   // the default color theme of the bot

            public List<ulong> GuildIds { get; set; }   // all guilds that this bot will register commands in, this does not matter if global commands are enabled.
            public string BotIconUrl { get; set; }  // the icon of the bot, leave as empty if their is none

            public EconomyType Economy { get; set; }

            public ErrorMessageConfig Errors { get; set; }
            public SuccessMessageConfig Success {   get; set; }

            // always leave messages at the bottom since they are largest, most bulky in the config, so we don't want them in the way of the main configuration.
            public Messages ConfigurableMessages { get; set; }
        }

        public struct Messages
        {
            public string HelpMessage { get; set; }    // the message that is displayed as a help message
        }

        public struct Securitys
        {
            public string Token { get; set; }   // the bot token
            public ulong Id { get; set; }       // the bot id
        }

        public struct EconomyType
        {
            public string CurrencyPrefixs { get; set; }     // prefixs for currency's such as "$" would look like "$money"
            public string CurrencySuffixs { get; set; }     // suffixs for currency such as "$" would look like "money$"
        }

        public struct ErrorMessageConfig
        {
            public string ErrorIconUrl { get; set; }    // if errorIconURL is set to nothing than no error icon will be displayed.
            public bool DisplayErrorsInChat { get; set; } // if displayErrorsInChat is enabled, the error will display in chat for everyone to see, if it is disabled a "failed" message will occur instead.
            public SerialisedColor ErrorColor { get; set; } // the color that is displayed on errors.
        }

        public struct SuccessMessageConfig
        {
            public string SuccessIconUrl { get; set; }    // if is set to nothing than no success icon will be displayed.
            public bool DisplaySuccessInChat { get; set; } // if is enabled, the success will display in chat for everyone to see, if it is disabled a "failed" message will occur instead.
            public SerialisedColor SuccessColor { get; set; } // the color that is displayed on success.
        }

        public static ConfigType Config = new ConfigType();


        /// <summary>
        ///  Load config.yaml through serialization. If config.yaml does not exists it will create a new one.
        /// </summary>
        public static void LoadConfig()
        {
            // If their is no config than create a new config, if their is a config, skip creating a new one and go to loading it
            if (!File.Exists("config.yaml"))
            {
                // create a new config instance - this is the default configuration.
                var newConfig = new ConfigType()
                {
                    Name = "",
                    BotIconUrl = "",
                    Secerity = new Securitys()
                    {
                        Token = "Insert your discord Token here.",
                        Id = 0,
                    },
                    Prefix = '!',
                    EmbededColor = new SerialisedColor(255, 255, 255),
                    GuildIds = new List<ulong>() { 000000000000000000 },
                    ConfigurableMessages = new Messages()
                    {
                        HelpMessage = "**This is where a help message should go**," + Environment.NewLine + " edit this in your *__config.yaml__*",
                    },
                    Errors = new ErrorMessageConfig()
                    {
                        ErrorIconUrl = "https://cdn.discordapp.com/attachments/940734878327124039/940735080534511686/ErrorMessage.png",
                        DisplayErrorsInChat = true,
                        ErrorColor = new SerialisedColor(255, 0, 0)
                    },
                    Success = new SuccessMessageConfig()
                    {
                        SuccessIconUrl = "https://cdn.discordapp.com/attachments/940734878327124039/941658299407929414/SuccessLogo_Small.png",
                        DisplaySuccessInChat = true,
                        SuccessColor = new SerialisedColor(0, 255, 0)
                    },
                    Economy = new EconomyType()
                    {
                        CurrencyPrefixs = "🪙",
                        CurrencySuffixs = " gold"
                    }
                    
                };

                // serialize the config above into yaml
                var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
                var yamlAsTextForFile = serializer.Serialize(newConfig);

                // put the serialized yaml into a file for us to configure in the file.
                File.WriteAllText("config.yaml", yamlAsTextForFile);
            }
            else
            {
                // create a deserialiser instance with camelCase naming conventions.
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                // get config from the config file.
                Config = deserializer.Deserialize<ConfigType>(File.ReadAllText("config.yaml"));

                Console.WriteLine(Config.Secerity.Token);
            }
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

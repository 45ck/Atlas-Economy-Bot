Hi, 

Please compile the bot yourself. You will need nugget package manager installed to download all libraries. In visual studio, in the solution explorer, if you go to the 
project references, you can see all required libraries that you need to install using NUGET.

Than you should be able to compile into "atlas bot/bin/debug". Please
keep the debug compile state as the compile state, as you will be able to view errors this way, and slash commands will be registered as guild-commands.

I have left documented config files in both config.yaml and GuildSettings/readme.yaml

I would highly recommend looking over these configs so you have an idea of how the bot stores data. I would also recommend testing the bot in discord, and seeing the changes 
of the YAML files so you understand how the data is stored.

All guild settings can be customised through /settings or /management commands.

All config settings cannot be changed from slash commands, you have to edit it in the config.yaml.

If you need any further help let me know. 

I hope you enjoy the bot.

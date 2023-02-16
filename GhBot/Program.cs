using GhBot;
using GhBot.Data;

Logger.UseConsoleLogs();

Logger.Debug("Fetching config...");
if (!Data.TryGetConfig("bot.cfg", out BotConfig config))
{
    Data.SaveConfig("bot.cfg", config);
    Logger.Fatal("Config does not exist. One has been created for you.");
}

Logger.Debug("Done!");

await new Bot().Run(config);
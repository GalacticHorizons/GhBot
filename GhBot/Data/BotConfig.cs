namespace GhBot.Data;

public struct BotConfig
{
    public string Token;
    public string Prefix;

    public ulong ServerId;
    public ulong LogsChannelId;

    public string DbIp;
    public string DbName;
    public string DbUsername;
    public string DbPassword;
}
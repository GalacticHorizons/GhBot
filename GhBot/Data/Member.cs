using System;

namespace GhBot.Data;

public class Member
{
    [PrimaryKey]
    public ulong DiscordId;

    public uint Level;

    public uint XP;

    public ulong TotalMessages;

    public DateTime LastLvl;

    public uint Coins;

    public Member()
    {
        DiscordId = 0;
        Level = 0;
        XP = 0;
        TotalMessages = 0;
        LastLvl = DateTime.UnixEpoch;
        Coins = 0;
    }
    
    public Member(ulong discordId) : this()
    {
        DiscordId = discordId;
    }
}
using System;

namespace GhBot.Data;

public class Member
{
    public ulong DiscordId;

    public uint Level;

    public uint LvlMsgs;

    public ulong TotalMessages;

    public DateTime LastLvl;

    public Member()
    {
        DiscordId = 0;
        Level = 0;
        LvlMsgs = 0;
        TotalMessages = 0;
        LastLvl = DateTime.UnixEpoch;
    }
    
    public Member(ulong discordId) : this()
    {
        DiscordId = discordId;
    }
}
using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using GhBot.Data;

namespace GhBot;

public class LevelHandler
{
    private DiscordSocketClient _client;

    public LevelHandler(DiscordSocketClient client)
    {
        client.MessageReceived += OnMessage;

        _client = client;
    }

    private async Task OnMessage(SocketMessage arg)
    {
        if (arg is not SocketUserMessage msg)
            return;
        
        // Ignore bots, they can't level up!
        if (msg.Author.IsBot)
            return;

        Member member = await Data.Data.GetMember(msg.Author.Id);

        if (member == null)
        {
            member = new Member(msg.Author.Id) { Level = 0, TotalMessages = 1, XP = Level.CalculateXpFromMessage(msg.Content), LastLvl = DateTime.Now };
            await Data.Data.CreateMember(member);
            /*await msg.Channel.SendMessageAsync(
                "Hey! You just sent your first message! Type `/level` in chat to see your level!");*/
            return;
        }

        member.TotalMessages++;

        bool hasLevelledUp = false;
        uint coinsGained = 0;
        
        if (Level.MessageMeetsRequirements(msg.Content, member))
        {
            member.XP += Level.CalculateXpFromMessage(msg.Content);

            uint maxXp = Level.CalculateMaxXp(member.Level);
            if (member.XP >= maxXp)
            {
                member.XP -= maxXp;
                member.Level++;
                hasLevelledUp = true;

                coinsGained = Level.CalculateCoinsForLevel(member.Level);
                member.Coins += coinsGained;
            }

            member.LastLvl = DateTime.Now;
        }

        // Update the member *before* sending the message in case the update, or the message goes wrong.
        // Doing this prevents the user from being messaged many times in case something goes wrong.
        await Data.Data.UpdateMember(member);

        if (hasLevelledUp)
            await msg.Channel.SendMessageAsync(
                $"{msg.Author.Mention}, you've just levelled up to level {member.Level}, and earned {coinsGained} coins!");
    }
}
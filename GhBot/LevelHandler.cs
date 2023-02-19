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
        SocketUserMessage msg = (SocketUserMessage) arg;
        // Ignore bots, they can't level up!
        if (msg.Author.IsBot)
            return;

        Member member = await Data.Data.GetMember(msg.Author.Id);

        if (member == null)
        {
            member = new Member(msg.Author.Id) { Level = 0, TotalMessages = 1, LvlMsgs = 1, LastLvl = DateTime.Now };
            await Data.Data.CreateMember(member);
            /*await msg.Channel.SendMessageAsync(
                "Hey! You just sent your first message! Type `/level` in chat to see your level!");*/
            return;
        }

        member.TotalMessages++;

        bool hasLevelledUp = false;
        
        if (msg.Content.Length >= 4 && DateTime.Now - member.LastLvl >= TimeSpan.FromSeconds(5))
        {
            member.LvlMsgs++;

            if (member.LvlMsgs >= (member.Level * 5) + 5)
            {
                member.LvlMsgs = 0;
                member.Level++;
                hasLevelledUp = true;
            }

            member.LastLvl = DateTime.Now;
        }

        // Update the member *before* sending the message in case the update, or the message goes wrong.
        // Doing this prevents the user from being messaged many times in case something goes wrong.
        await Data.Data.UpdateMember(member);

        if (hasLevelledUp)
            await msg.Channel.SendMessageAsync(
                $"{msg.Author.Mention}, you've just levelled up to level {member.Level}, and earned 528930754983257902357234958037 coins!");
    }
}
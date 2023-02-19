using System;
using System.Threading.Tasks;
using Discord.WebSocket;

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
        
        Console.WriteLine((await Data.Data.GetMember(msg.Author.Id)).Level);
    }
}
using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using SteamServerQuery;
using Exception = System.Exception;

namespace GhBot.Commands;

public class GeneralCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("hello", "Hello there!")]
    public async Task Hello()
    {
        string[] responses = new[]
        {
            "Hello there!",
            "General kenobi!",
            "Generic hello response!"
        };
        await RespondAsync(responses[Random.Shared.Next(responses.Length)]);
    }

    [SlashCommand("quote", "Quotes.")]
    public async Task Quote(string message)
    {
        await RespondAsync(message + "\n- **" + Context.User.Username + "**, " + DateTime.Now.Year);
    }

    [SlashCommand("whoami", "Get information about yourself.")]
    public async Task Whoami()
    {
        SocketGuildUser user = (SocketGuildUser) Context.User;

        EmbedBuilder builder = new EmbedBuilder()
            .WithAuthor(user)
            .WithTitle("About you")
            .WithColor(Color.Purple)
            .AddField("Nickname", user.Nickname ?? user.Username, true)
            .AddField("ID", user.Id, true)
            .AddField("Member since", user.JoinedAt?.ToString() ?? "Unknown.", true);

        await RespondAsync(embed: builder.Build());
    }

    [SlashCommand("status", "Get the status of the server.")]
    public async Task ServerStatus()
    {
        const string ip = "1.1.1.1";
        const int port  = 27016;
        
        await DeferAsync();
        ServerInfo info;
        try
        {
            info = await SteamServer.QueryServerAsync(ip, port);
        }
        catch (Exception e)
        {
            await FollowupAsync("Failed to get server. Is it online? (Exception: " + e.Message + ")");
            return;
        }

        PlayerInfo[] pInfo;

        try
        {
            pInfo = await SteamServer.QueryPlayersAsync(ip, port);
        }
        catch (Exception e)
        {
            await FollowupAsync("Failed to get players. Something went wrong. (Exception: " + e.Message + ")");
            return;
        }

        StringBuilder players = new StringBuilder();
        if (info.Players > 0)
        {
            for (int i = 0; i < pInfo.Length; i++)
            {
                players.Append(pInfo[i].Name);
                if (i < pInfo.Length - 1)
                    players.Append(", ");
            }
        }
        else
        {
            players.Append("None");
        }
        
        Embed embed = new EmbedBuilder()
            .WithTitle(info.Name)
            .WithFooter("Galactic Horizons")
            .WithCurrentTimestamp()
            .AddField("Players", info.Players + "/" + info.MaxPlayers, true)
            .AddField("IP", ip + ":" + port, true)
            .AddField("Connect", "steam://connect/" + ip + ":" + port, true)
            .AddField("Online players", players.ToString())
            .Build();

        await FollowupAsync(embed: embed);
    }

    [SlashCommand("notified", "Give yourself the notified role.")]
    public async Task GiveNotified()
    {
        SocketGuildUser user = (SocketGuildUser) Context.User;
        await user.AddRoleAsync(1077386286983303200);
        await RespondAsync("Okay, I've given you the <@&1077386286983303200> role!", ephemeral: true);
    }

    [SlashCommand("unnotified", "Remove the notified role.")]
    public async Task RemoveNotified()
    {
        SocketGuildUser user = (SocketGuildUser) Context.User;
        await user.RemoveRoleAsync(1077386286983303200);
        await RespondAsync("Okay, you no longer have the <@&1077386286983303200> role.", ephemeral: true);
    }
}
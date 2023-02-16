using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace GhBot.Commands;

[Discord.Interactions.Group("admin", "Administrator commands")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public class AdminCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("message", "Send a bot message in the given channel.")]
    public async Task BotMessage(ISocketMessageChannel channel, [Remainder] string message)
    {
        await channel.SendMessageAsync(message);
        await RespondAsync("Done!", ephemeral: true);
    }

    [SlashCommand("embed", "Send a bot embed in the given channel.")]
    public async Task BotEmbed(ISocketMessageChannel channel, string json = "", string title = "", string content = "")
    {
        Embed embed;
        if (!string.IsNullOrEmpty(json))
        {
            if (!EmbedBuilder.TryParse(json, out EmbedBuilder builder))
                await RespondAsync("Failed to parse embed json.", ephemeral: true);
            embed = builder.Build();
        }
        else
        {
            embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(content)
                .Build();
            Console.WriteLine(JsonConvert.SerializeObject(embed));
        }

        await channel.SendMessageAsync(embed: embed);
        await RespondAsync("Done!", ephemeral: true);
    }
}
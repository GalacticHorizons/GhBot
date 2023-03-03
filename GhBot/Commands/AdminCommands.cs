using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using GhBot.Data;
using GhBot.Parsers.Rules;
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

    [SlashCommand("rules", "Set the server rules.")]
    public async Task BotRules(SocketTextChannel channel, Attachment file)
    {
        HttpClient client = new HttpClient();
        string text = await client.GetStringAsync(file.Url);
        client.Dispose();

        //RuleBlock[] blocks = RuleBlock.Parse(text);
        RuleBlock[] blocks = JsonConvert.DeserializeObject<RuleBlock[]>(text);

        Embed[] embeds = new Embed[blocks.Length];

        for (int i = 0; i < blocks.Length; i++)
        {
            RuleBlock block = blocks[i];

            uint colorHex = Convert.ToUInt32(block.Color, 16);
            Color color = new Color(colorHex);

            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle(block.Title)
                .WithColor(color);
            
            if (!string.IsNullOrEmpty(block.Description))
                builder.WithDescription(block.Description);

            if (i == blocks.Length - 1)
                builder.WithFooter($"Galactic Horizons rules. Last Updated: {DateTime.Now:yyyy-MM-dd}");

            foreach (Rule rule in block.Rules)
                builder.AddField(rule.Title, rule.Description);

            embeds[i] = builder.Build();
        }

        await channel.SendMessageAsync(embeds: embeds);
        
        await RespondAsync("Done. The rules have been updated. Please make sure to delete any previous rulesets that may be present!");
    }
    
    [SlashCommand("setlevel", "Set a member's level.")]
    public async Task SetMemberLevel(SocketUser user, uint level, uint xp)
    {
        await DeferAsync(true);
        
        Member member = await Data.Data.GetMember(user.Id);
        member ??= new Member(user.Id);

        member.Level = level;
        member.XP = xp;

        await Data.Data.UpdateMember(member);

        await FollowupAsync($"Done! {user}'s level and XP have been set.", ephemeral: true);
    }
}
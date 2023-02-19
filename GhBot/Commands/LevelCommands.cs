using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using GhBot.Data;

namespace GhBot.Commands;

public class LevelCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("level", "Get your current level.")]
    public async Task ShowLevel()
    {
        await DeferAsync();
        
        Member member = await Data.Data.GetMember(Context.User.Id);

        Embed embed = new EmbedBuilder()
            .WithAuthor(Context.User)
            .WithTitle("Your level")
            .AddField("Level", member.Level, true)
            .AddField("Level messages", member.LvlMsgs, true)
            .AddField("Total messages", member.TotalMessages, true)
            .Build();

        await FollowupAsync(embed: embed);
    }
}
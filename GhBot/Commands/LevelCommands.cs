using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Cairo;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GhBot.Data;
using StbImageSharp;
using StbImageWriteSharp;
using Color = Cairo.Color;
using ColorComponents = StbImageWriteSharp.ColorComponents;
using Format = Cairo.Format;

namespace GhBot.Commands;

public class LevelCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("level", "Get your current level.")]
    public async Task ShowLevel(SocketGuildUser member = null)
    {
        bool memberNull = member == null;
        
        member ??= (SocketGuildUser) Context.User;

        if (member.IsBot)
        {
            await RespondAsync($"{member.Nickname ?? member.Username} is a bot, and so doesn't earn levels!");
            return;
        }
        
        await DeferAsync();

        Member dbMember = await Data.Data.GetMember(member.Id);

        if (dbMember == null)
        {
            await FollowupAsync($"{(memberNull ? "You haven't" : (member.Nickname ?? member.Username) + " hasn't")} sent any messages yet!");
            return;
        }

        using ImageSurface surface = new ImageSurface(Format.Rgb24, 700, 250);
        using Context c = new Context(surface);

        ImageSurface galaxy = new ImageSurface("Content/galaxy.png");
        c.SetSourceSurface(galaxy, -195, -383);
        c.Paint();
        galaxy.Dispose();

        const double circX = 125;
        const double circY = 250 / 2f;
        const double radius = 100;
        const double imgSize = radius * 2 + 5;
        
        c.Arc(circX, circY, radius, 0, MathF.PI * 2);
        c.Clip();

        string url = member.GetDisplayAvatarUrl(size: 256);
        HttpClient client = new HttpClient();
        byte[] imgData = await client.GetByteArrayAsync(url);
        client.Dispose();
        ImageResult result = ImageResult.FromMemory(imgData, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
        // For some reason this data is in BGRA too because apparently cairo can't read images properly..
        // Either that or I am doing something stupidly wrong. Probably that.
        BgraConvert(result.Data);
        ImageSurface user = new ImageSurface(result.Data, Format.Rgb24, result.Width, result.Height, 4 * result.Width);
        double scaleX = imgSize / result.Width;
        double scaleY = imgSize / result.Height;
        c.Save();
        c.Scale(scaleX, scaleY);
        c.SetSourceSurface(user, (int) ((circX - imgSize / 2) / scaleX), (int) ((circY - imgSize / 2) / scaleY));
        c.Paint();
        user.Dispose();
        
        c.Restore();
        c.ResetClip();
        c.Arc(circX, circY, radius, 0, MathF.PI * 2);
        c.SetSourceRGB(0, 0, 0);
        c.LineWidth = 5;
        c.Stroke();

        c.SetSourceRGB(1, 1, 1);
        c.Rectangle(0, 0, 700, 250);
        c.Fill();

        //RoundRect(c, 100, 100, 100, 100, 20);
        RandomPath(c, 100);
        c.SetSourceRGB(0, 0, 0);
        c.Stroke();
        
        c.SelectFontFace("Serif", FontSlant.Normal, FontWeight.Normal);
        
        Console.WriteLine(c.Status);

        ImageWriter writer = new ImageWriter();
        using MemoryStream stream = new MemoryStream();
        byte[] data = surface.Data;
        // For some reason the data we are getting is in BGRA format so we have to convert it.
        BgraConvert(data);

        writer.WritePng(data, surface.Width, surface.Height, ColorComponents.RedGreenBlueAlpha, stream);
        
        await FollowupWithFileAsync(attachment: new FileAttachment(stream, "sdf.png"));
    }

    private static void BgraConvert(byte[] data)
    {
        for (int i = 0; i < data.Length; i += 4)
            (data[i], data[i + 2]) = (data[i + 2], data[i]);
    }

    private static void RoundRect(Context c, double x, double y, double width, double height, double r)
    {
        double halfR = r / 2;
        
        c.NewPath();
        c.Arc(x + halfR, y + halfR, r, Math.PI, -Math.PI / 2);
        c.LineTo(x + r + width, y - r);
        c.Arc(x + r + width, y, r, -Math.PI / 2, 0);
        c.LineTo(x + r * 2 + width, y + r + height);
        c.Arc(x + r + width, y + r + height, r, 0, Math.PI / 2);
        c.LineTo(x, y + r * 2 + width);
        c.Arc(x, y + r + height, r, Math.PI / 2, Math.PI);
        c.LineTo(x, y);
        
        c.ClosePath();
    }

    public static void RandomPath(Context c, int numIterations)
    {
        for (int i = 0; i < numIterations; i++)
        {
            c.LineTo(Random.Shared.Next(0, 700), Random.Shared.Next(0, 250));
        }
    }
}
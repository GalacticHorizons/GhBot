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
        member ??= (SocketGuildUser) Context.User;

        if (member.IsBot)
        {
            await RespondAsync($"{member.Nickname ?? member.Username} is a bot, and so doesn't earn levels!");
            return;
        }
        
        await DeferAsync();

        Member dbMember = await Data.Data.GetMember(member.Id);
        dbMember ??= new Member();
        
        using ImageSurface surface = new ImageSurface(Format.Rgb24, 600, 200);
        using Context c = new Context(surface);
        
        RoundRect(c, 5, 5, surface.Width - 10, surface.Height - 10, 30);
        c.Clip();

        ImageSurface galaxy = new ImageSurface("Content/galaxy.png");
        c.SetSourceSurface(galaxy, -195, -383);
        c.Paint();
        galaxy.Dispose();
        
        RoundRect(c, 20, 20, surface.Width - 40, surface.Height - 40, 30);
        c.SetSourceRGBA(0, 0, 0, 0.5);
        c.Fill();

        const double circX = 100;
        const double circY = 200 / 2f;
        const double radius = 70;
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
        /*c.Arc(circX, circY, radius, 0, MathF.PI * 2);
        c.SetSourceRGB(0, 0, 0);
        c.LineWidth = 5;
        c.Stroke();*/

        const double userPosX = 180;
        const double userPosY = 75;

        c.SelectFontFace("sans-serif", FontSlant.Normal, FontWeight.Normal);
        c.SetSourceRGB(1, 1, 1);
        int fontSize = 40;
        c.SetFontSize(fontSize);
        string text = member.Nickname ?? member.Username;
        TextExtents extents = c.TextExtents(text);

        const int maxWidth = 285;
        // The level circle takes up space that could be used by the text. So if the text's width is larger than its space,
        // keep reducing its size until it fits.
        while (extents.Width >= maxWidth)
        {
            fontSize -= 1;
            c.SetFontSize(fontSize);
            extents = c.TextExtents(text);

            // If, however, the font size goes below a certain level (25 is about as low as can reasonably be gone while
            // still looking decent), then trim the string to fit.
            if (fontSize < 23)
            {
                string checkStr = "" + text[0];
                int i = 1;
                extents = c.TextExtents(checkStr);
                TextExtents extentsOfEllipsis = c.TextExtents("...");
                while (extents.Width < maxWidth - extentsOfEllipsis.Width)
                {
                    checkStr += text[i++];
                    extents = c.TextExtents(checkStr);
                }

                text = checkStr + "...";
                
                break;
            }
        }
        
        c.MoveTo(userPosX, userPosY);
        c.ShowText(text);
        c.SetSourceRGB(0.6, 0.6, 0.6);
        c.MoveTo(userPosX, userPosY + 30);
        c.SelectFontFace("sans-serif", FontSlant.Normal, FontWeight.Bold);
        c.SetFontSize(25);
        c.ShowText("#" + member.Discriminator);

        uint maxLvlMsgs = (dbMember.Level * 5 + 5);
        uint lvlMsgs = dbMember.LvlMsgs;

        const double progressX = userPosX;
        const double progressY = 130;
        const double progressW = 375;
        const double progressH = 30;
        const double progressR = progressH / 2d;
        
        RoundRect(c, progressX, progressY, progressW, progressH, progressR);
        c.SetSourceRGB(0, 0, 0);
        c.FillPreserve();
        c.Clip();

        RoundRect(c, progressX - progressW + (progressW * lvlMsgs / maxLvlMsgs), progressY, progressW, progressH, progressR);
        c.SetSourceRGB(102 / 255.0, 51 / 255.0, 153 / 255.0);
        c.Fill();

        text = lvlMsgs + "/" + maxLvlMsgs + " XP";
        extents = c.TextExtents(text);
        c.MoveTo(progressX + progressW / 2 - (extents.Width / 2 + extents.XBearing), progressY + progressH / 2 - (extents.Height / 2 + extents.YBearing));
        c.SetSourceRGB(1, 1, 1);
        c.ShowText(text);

        c.ResetClip();
        RoundRect(c, progressX, progressY, progressW, progressH, progressR);
        c.SetSourceRGB(0, 0, 0);
        c.LineWidth = 2;
        c.Stroke();

        const double lvlX = 525;
        const double lvlY = 75;
        const double lvlR = 40;
        
        c.Arc(lvlX, lvlY, lvlR, 0, Math.PI * 2);
        c.SetSourceRGB(0.2, 0.2, 0.2);
        c.FillPreserve();
        c.SetSourceRGB(0, 0, 0);
        c.LineWidth = 4;
        c.Stroke();

        c.SetFontSize(50);
        text = dbMember.Level.ToString();
        extents = c.TextExtents(text);
        c.MoveTo(lvlX - (extents.Width / 2 + extents.XBearing), lvlY - (extents.Height / 2 + extents.YBearing));
        c.SetSourceRGB(1, 1, 1);
        c.ShowText(text);

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
        const double halfRadians = Math.PI;
        const double quarterRadians = Math.PI / 2;
        
        c.NewPath();
        c.ArcNegative(r + x, r + y, r, -quarterRadians, halfRadians);
        c.LineTo(x, y + height - r);
        c.ArcNegative(r + x, height - r + y, r, halfRadians, quarterRadians);
        c.LineTo(x + width - r, y + height);
        c.ArcNegative(x + width - r, y + height - r, r, quarterRadians, 0);
        c.LineTo(x + width, y + r);
        c.ArcNegative(x + width - r, y + r, r, 0, -quarterRadians);
        c.LineTo(x + r, y);

        c.ClosePath();
    }
}
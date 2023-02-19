using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GhBot.Data;

namespace GhBot;

public class Bot
{
    public Bot()
    {
        Instance = this;
        Client = new DiscordSocketClient(new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.All
        });
    }
    
    public DiscordSocketClient Client;

    public LevelHandler LevelHandler;

    public BotConfig Config;

    private InteractionService _interactionService;

    public async Task Run(BotConfig config)
    {
        Config = config;
        
        Client.Log += ClientOnLog;
        
        Client.Ready += ClientOnReady;

        await Client.LoginAsync(TokenType.Bot, Config.Token);
        await Client.StartAsync();
        await Client.SetActivityAsync(new Game("you 👀", ActivityType.Watching));

        _interactionService = new InteractionService(Client);
        _interactionService.Log += ClientOnLog;

        Client.InteractionCreated += OnInteract;

        LevelHandler = new LevelHandler(Client);
        
        await Task.Delay(-1);
    }

    private Task ClientOnLog(LogMessage arg)
    {
        Logger.LogType type = arg.Severity switch
        {
            LogSeverity.Critical => Logger.LogType.Fatal,
            LogSeverity.Error => Logger.LogType.Error,
            LogSeverity.Warning => Logger.LogType.Warn,
            LogSeverity.Info => Logger.LogType.Info,
            LogSeverity.Verbose => Logger.LogType.Debug,
            LogSeverity.Debug => Logger.LogType.Debug,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        Logger.Log(type, "<" + arg.Source + "> " + arg.Message + arg.Exception);
        return Task.CompletedTask;
    }

    private async Task ClientOnReady()
    {
        await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), null);
        await _interactionService.RegisterCommandsToGuildAsync(1075435117310398535);
    }

    private async Task OnInteract(SocketInteraction arg)
    {
        SocketInteractionContext ctx = new SocketInteractionContext(Client, arg);
        await _interactionService.ExecuteCommandAsync(ctx, null);
    }

    public static Bot Instance { get; private set; }
}
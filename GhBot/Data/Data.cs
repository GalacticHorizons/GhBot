using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;

namespace GhBot.Data;

public static class Data
{
    public const string DataDir = "Data";

    public static bool TryGetConfig(string configName, out BotConfig config)
    {
        string path = Path.Combine(DataDir, configName);

        config = default;

        if (!File.Exists(path))
            return false;

        config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText(path));
        return true;
    }

    public static void SaveConfig(string configName, BotConfig config)
    {
        Directory.CreateDirectory(DataDir);
        File.WriteAllText(Path.Combine(DataDir, configName), JsonConvert.SerializeObject(config, Formatting.Indented));
    }
}
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace GhBot.Data;

public static class Data
{
    public const string DataDir = "Data";
    
    private static string ConnectionString
    {
        get
        {
            BotConfig config = Bot.Instance.Config;

            return "server=" + config.DbIp + "; uid=" + config.DbUsername + "; pwd=" + config.DbPassword +
                   "; database=" + config.DbName;
        }
    }

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

    public static async Task<Member> GetMember(ulong discordId)
    {
        await using MySqlConnection connection = await ConnectDatabase();
        await using MySqlCommand cmd = new MySqlCommand(SqlQueries.SelectDiscordMemberWithId, connection);
        cmd.Parameters.AddWithValue("@discordId", discordId);
        await using DbDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        Member member;
        if (reader.HasRows)
        {
            await reader.ReadAsync();
            member = AsType<Member>(reader);
        }
        else
            member = null;
        
        return member;
    }

    public static async Task CreateMember(Member member)
    {
        await using MySqlConnection connection = await ConnectDatabase();
        await SqlQueries.Insert(connection, SqlQueries.MembersTableName, member);
    }

    public static async Task UpdateMember(Member member)
    {
        await using MySqlConnection connection = await ConnectDatabase();
        await SqlQueries.Update(connection, SqlQueries.MembersTableName, member);
    }

    public static T AsType<T>(DbDataReader reader) where T : new()
    {
        Type type = typeof(T);

        T t = new T();
        
        int i = 0;
        foreach (FieldInfo info in type.GetFields())
        {
            object value = reader.GetValue(i);
            info.SetValue(t, value);
            i++;
        }

        return t;
    }

    private static async Task<MySqlConnection> ConnectDatabase()
    {
        MySqlConnection connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static async Task CloseDatabase(MySqlConnection connection)
    {
        await connection.CloseAsync();
    }
}
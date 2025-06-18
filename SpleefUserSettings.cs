using Microsoft.Data.Sqlite;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace SpleefResurgence
{
    public class PlayerSettings
    {
        public bool ShowScore { get; set; } = true;
        public bool ShowLavarise { get; set; } = true;
        public bool GetBuffs { get; set; } = true;
        public bool ChatLavarise { get; set; } = false;
        public bool GetMusicBox { get; set; } = true;
        public bool GetPaintSprayer { get; set; } = true;
        public bool BlockSpamDebug { get; set; } = false;
    }


    public class SpleefUserSettings
    {
        private readonly string DbPath = Path.Combine(TShock.SavePath, "SpleefCoin.sqlite");
        private readonly SpleefCoin spleefCoin;

        public SpleefUserSettings(SpleefCoin spleefCoin)
        {
            this.spleefCoin = spleefCoin;
            var sql = @"CREATE TABLE IF NOT EXISTS PlayerSettings (
                        Username TEXT PRIMARY KEY,
                        ShowScore INTEGER DEFAULT 1,
                        GetBuffs INTEGER DEFAULT 1,
                        ShowLavarise INTEGER DEFAULT 1,
                        ChatLavarise INTEGER DEFAULT 0,
                        GetMusicBox INTEGER DEFAULT 1,
                        GetPaint INTEGER DEFAULT 1,
                        BlockSpamDebug INTEGER DEFAULT 0
                        );";

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private void ExecuteMultipleStatements(string sql, Dictionary<string, object> parameters)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var transaction = connection.BeginTransaction();

            foreach (var statement in sql.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = statement.Trim();

                foreach (var kvp in parameters)
                {
                    if (!command.Parameters.Contains(kvp.Key))
                        command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public PlayerSettings GetSettings(string username)
        {
            var sql = "SELECT * FROM PlayerSettings WHERE Username = @username";
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                PlayerSettings settings = new();
                while (reader.Read())
                {
                    settings.ShowScore = reader.GetInt32(1) == 1;
                    settings.GetBuffs = reader.GetInt32(2) == 1;
                    settings.ShowLavarise = reader.GetInt32(3) == 1;
                    settings.ChatLavarise = reader.GetInt32(4) == 1;
                    settings.GetMusicBox = reader.GetInt32(5) == 1;
                    settings.GetPaintSprayer = reader.GetInt32(6) == 1;
                    settings.BlockSpamDebug = reader.GetInt32(7) == 1;
                    return settings;
                }
            }
            return new PlayerSettings();
        }

        public void EditSettingsCommand(CommandArgs args)
        {
            string username = args.Player.Account.Name;
            
            PlayerSettings userSettings = GetSettings(username);


            if (args.Parameters.Count <= 1)
            {
                args.Player.SendInfoMessage("[c/00FFFF:Settings list:]");
                
                if (userSettings.ShowScore)
                    args.Player.SendInfoMessage($"[c/88E788:Show score is enabled!] Change it with /toggle showscore disable");
                else
                    args.Player.SendInfoMessage($"[c/FF474C:Show score is disabled!] Change it with /toggle showscore enable");

                if (userSettings.GetBuffs)
                    args.Player.SendInfoMessage("[c/88E788:Global buffs are enabled!] Change it with /toggle buff disable");
                else
                    args.Player.SendInfoMessage($"[c/FF474C:Global buffs are disabled!] Change it with /toggle buff enable");

                if (userSettings.ShowLavarise)
                    args.Player.SendInfoMessage("[c/88E788:Show lavarise timer is enabled!] Change it with /toggle showlavarise disable");
                else
                    args.Player.SendInfoMessage($"[c/FF474C:Show lavarise timer is disabled!] Change it with /toggle showlavarise enable");

                if (userSettings.ChatLavarise)
                    args.Player.SendInfoMessage("[c/88E788:Chat lavarise timer is enabled!] Change it with /toggle chatlavarise disable - [c/ff0000:this will be a thing in the future but doesn't work rn]");
                else
                    args.Player.SendInfoMessage($"[c/FF474C:Chat lavarise timer is disabled!] Change it with /toggle chatlavarise enable - [c/ff0000:this will be a thing in the future but doesn't work rn]");

                if (userSettings.GetMusicBox)
                    args.Player.SendInfoMessage("[c/88E788:Get music box is enabled!] Change it with /toggle musicbox disable");
                else
                    args.Player.SendInfoMessage($"[c/FF474C:Get music box is disabled!] Change it with /toggle musicbox enable");

                if (userSettings.GetPaintSprayer)
                    args.Player.SendInfoMessage("[c/88E788:Get paint sprayer is enabled!] Change it with /toggle paint disable");
                else
                    args.Player.SendInfoMessage($"[c/FF474C:Get paint sprayer is disabled!] Change it with /toggle paint enable");

                if (userSettings.BlockSpamDebug)
                    args.Player.SendInfoMessage("[c/88E788:Blockspam debug is enabled!] Change it with /toggle debug disable");
                else
                    args.Player.SendInfoMessage($"[c/FF474C:Blockspam debug is disabled!] Change it with /toggle debug enable (this will disable lavarise timer and score)");
                return;
            }

            if (args.Parameters.Count == 2)
            {
                if (args.Parameters[0] != "showscore" && args.Parameters[0] != "buff" && args.Parameters[0] != "showlavarise" && args.Parameters[0] != "chatlavarise" && args.Parameters[0] != "musicbox" && args.Parameters[0] != "paint" && args.Parameters[0] != "debug")
                {
                    args.Player.SendErrorMessage($"{args.Parameters[0]} aint a setting");
                    return;
                }

                int Setting;
                switch (args.Parameters[1])
                {
                    case "true":
                    case "1":
                    case "enable":
                        Setting = 1;
                        break;
                    case "false":
                    case "0":
                    case "disable":
                        Setting = 0;
                        break;
                    default:
                        args.Player.SendErrorMessage("this can only be disabled or enabled");
                        return;
                }
                string sql;
                if (args.Parameters[0] == "showscore")
                {
                    sql = $"UPDATE PlayerSettings SET ShowScore = @setting WHERE Username = @username";
                    if (Setting == 1 && userSettings.BlockSpamDebug)
                        sql += $"; UPDATE PlayerSettings SET BlockSpamDebug = 0 WHERE Username = @username";
                }
                else if (args.Parameters[0] == "buff")
                    sql = $"UPDATE PlayerSettings SET GetBuffs = @setting WHERE Username = @username";
                else if (args.Parameters[0] == "showlavarise")
                {
                    sql = $"UPDATE PlayerSettings SET ShowLavarise = @setting WHERE Username = @username";
                    if (Setting == 1 && userSettings.BlockSpamDebug)
                        sql += $"; UPDATE PlayerSettings SET BlockSpamDebug = 0 WHERE Username = @username";
                }
                else if (args.Parameters[0] == "chatlavarise")
                    sql = $"UPDATE PlayerSettings SET ChatLavarise = @setting WHERE Username = @username";
                else if (args.Parameters[0] == "musicbox")
                    sql = $"UPDATE PlayerSettings SET GetMusicBox = @setting WHERE Username = @username";
                else if (args.Parameters[0] == "paint")
                    sql = $"UPDATE PlayerSettings SET GetPaint = @setting WHERE Username = @username";
                else
                {
                    sql = $"UPDATE PlayerSettings SET BlockSpamDebug = @setting WHERE Username = @username";
                    if (Setting == 1)
                    {
                        if (userSettings.ShowLavarise)
                            sql += $"; UPDATE PlayerSettings SET ShowLavarise = 0 WHERE Username = @username";
                        if (userSettings.ShowScore)
                            sql += $"; UPDATE PlayerSettings SET ShowScore = 0 WHERE Username = @username";
                    }
                }

                var parameters = new Dictionary<string, object>
                {
                    { "@setting", Setting },
                    { "@username", username }
                };
                ExecuteMultipleStatements(sql, parameters);
                args.Player.SendSuccessMessage($"{args.Parameters[0]} {args.Parameters[1]}d!");
            }
            else
            {
                args.Player.SendErrorMessage("what are you doing");
                return;
            }
        }
    }
}

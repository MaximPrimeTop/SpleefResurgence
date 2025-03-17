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
    }


    public class SpleefUserSettings
    {
        private readonly string DbPath = Path.Combine(TShock.SavePath, "SpleefCoin.sqlite");

        public SpleefUserSettings()
        {
            var sql = @"CREATE TABLE IF NOT EXISTS PlayerSettings (
                        Username TEXT PRIMARY KEY,
                        ShowScore INTEGER DEFAULT 1,
                        GetBuffs INTEGER DEFAULT 1,
                        ShowLavarise INTEGER DEFAULT 1
                        );";

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
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
                return;
            }

            if (args.Parameters.Count == 2)
            {
                if (args.Parameters[0] != "showscore" && args.Parameters[0] != "buff" && args.Parameters[0] != "showlavarise")
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
                    sql = $"UPDATE PlayerSettings SET ShowScore = @setting WHERE Username = @username";
                else if (args.Parameters[0] == "buff")
                    sql = $"UPDATE PlayerSettings SET GetBuffs = @setting WHERE Username = @username";
                else
                    sql = $"UPDATE PlayerSettings SET ShowLavarise = @setting WHERE Username = @username";

                using var connection = new SqliteConnection($"Data Source={DbPath}");
                connection.Open();

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@setting", Setting);
                command.Parameters.AddWithValue("@username", username);
                command.ExecuteNonQuery();
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

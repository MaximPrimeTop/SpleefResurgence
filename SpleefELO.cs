using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace SpleefResurgence
{
    public class SpleefELO
    {
        private static string DbPath = Path.Combine(TShock.SavePath, "SpleefCoin.sqlite");

        static SpleefELO()
        {
            var sql = @"CREATE TABLE IF NOT EXISTS PlayerStats (
                        Username TEXT PRIMARY KEY,
                        ELO REAL DEFAULT 0.0
                        );";

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public static float GetElo(string username)
        {
            var sql = "SELECT * FROM PlayerStats WHERE Username = @username";
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    float elo = reader.GetFloat(1);
                    TShock.Log.ConsoleInfo($"{username} has {elo} ELO");
                    return elo;
                }
            }
            return -1;
        }

        public static void GetEloCommand(CommandArgs args)
        {
            string username = args.Parameters[0];
            if (!SpleefCoin.isUserInTable(username))
            {
                args.Player.SendErrorMessage($"{username} does not exist in the table");
                return;
            }
            float elo = GetElo(username);
            args.Player.SendSuccessMessage($"{username} has {elo} ELO");
        }

        public static void SetElo (string username, float elo)
        {
            var sql = $"UPDATE PlayerStats SET ELO = @elo WHERE Username = @username";

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@elo", elo);
            command.Parameters.AddWithValue("@username", username);
            command.ExecuteNonQuery();
        }

        public static void SetEloCommand(CommandArgs args)
        {
            string username = args.Parameters[0];
            if (!SpleefCoin.isUserInTable(username))
            {
                args.Player.SendErrorMessage($"{username} does not exist in the table");
                return;
            }
            float elo = (float)Convert.ToDouble(args.Parameters[1]);
            SetElo(username, elo);
            args.Player.SendSuccessMessage($"Set {username}'s elo to {elo}");
        }

        private static float final (float x, float y, float z)
        {
            return (x * y * z);
        }

        public static float prob (float x, float y)
        {
            return 1 / (1 + (float)Math.Pow(10, (x - y) / 400));
        }

        public static float EloAdd (float p1, float p2, float scale, int W)
        {
            if (W == 1 && (p1 >= p2 || p1 < p2))
                return final(p2, prob(p1, p2), scale);
            else
                return -final(p1, prob(p2, p1), scale);
        }
    }
}

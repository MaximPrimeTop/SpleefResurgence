using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace SpleefResurgence.uh
{
    public class BridgeUserSettings
    {
        private static readonly string DbPath = Path.Combine(TShock.SavePath, "SpleefCoin.sqlite");

        public static void CreateTable()
        {
            var sql = @"CREATE TABLE IF NOT EXISTS PlayerBridgeInfo (
                        Username TEXT PRIMARY KEY,
                        Associated_uid INTEGER DEFAULT NULL,
                        Notify INTEGER DEFAULT 1
                        );";

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public static int? GetUid(string username)
        {
            var sql = "SELECT * FROM PlayerBridgeInfo WHERE Username = @username";
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    int uid = reader.GetInt32(1);
                }
            }
            return null;
        }

        public static bool? GetNotify(string username)
        {
            var sql = "SELECT * FROM PlayerBridgeInfo WHERE Username = @username";
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    bool notify = reader.GetInt32(2) == 1;
                    return notify;
                }
            }
            return null;
        }
    }
}

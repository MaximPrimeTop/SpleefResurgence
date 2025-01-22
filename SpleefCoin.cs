using Microsoft.Xna.Framework;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using NuGet.Protocol.Plugins;

namespace SpleefResurgence
{
    public class SpleefCoin
    {
        private string DbPath = Path.Combine(TShock.SavePath, "SpleefCoin.sqlite");

        public SpleefCoin()
        {
            var sql = @"CREATE TABLE IF NOT EXISTS PlayerCoins (
                        Username TEXT PRIMARY KEY,
                        Coins INTEGER DEFAULT 0
                        );";

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public void DoSqlStuff()
        {
            var sql = "INSERT INTO PlayerCoins (Username) " +
                  "VALUES (@Username)";

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Username", "MaximPrime");

            var rowInserted = command.ExecuteNonQuery();
        }

        public void AddCoins(CommandArgs args)
        {
            string username = args.Parameters[0];
            int amount = Convert.ToInt32(args.Parameters[1]);

            var sql = $"UPDATE PlayerCoins SET Coins = Coins + @amount WHERE Username = @username";

            using var connection = new SqliteConnection($"Data Source={DbPath}"); 
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@username", username);
            command.ExecuteNonQuery();
            args.Player.SendSuccessMessage($"nice +{amount} spleef coin to {username}");
            var plr = TSPlayer.FindByNameOrID(username);
            var players = TSPlayer.FindByNameOrID(username);
            if (players == null || players.Count == 0)
            {
                args.Player.SendInfoMessage($"{username} isn't online atm");
            }
            else
            {
                var player = players[0];
                player.SendMessage($"You just got {amount} Spleef Coins!", Color.Orange);
            }
        }

        public void GetCoins(CommandArgs args)
        {
            string username;
            if (args.Parameters.Count == 1)
                username = args.Parameters[0];
            else
                username = args.Player.Account.Name;
            var sql = "SELECT * FROM PlayerCoins WHERE Username = @username";
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                        var Username = reader.GetString(0);
                        var Coins = reader.GetInt32(1);
                        args.Player.SendInfoMessage($"{Username}: {Coins}");
                }
            }
            else
            {
                args.Player.SendErrorMessage("worgn");
            }
        }

        public void GetLeaderboard (CommandArgs args)
        {
            var sql = "SELECT Username FROM PlayerCoins ORDER BY Coins DESC LIMIT 10";
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public void MigrateUsersToSpleefDatabase()
        {
            string tshockDbPath = Path.Combine(TShock.SavePath, "tshock.sqlite");
            string spleefDbPath = Path.Combine(TShock.SavePath, "SpleefCoin.sqlite");
            string selectQuery = "SELECT Username FROM Users;";

            try
            {
                // Connect to TShock database and fetch usernames
                using (var tshockConnection = new SqliteConnection($"Data Source={tshockDbPath}"))
                {
                    tshockConnection.Open();

                    using (var selectCommand = new SqliteCommand(selectQuery, tshockConnection))
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        using (var spleefConnection = new SqliteConnection($"Data Source={spleefDbPath}"))
                        {
                            spleefConnection.Open();

                            while (reader.Read())
                            {
                                string username = reader["Username"].ToString();

                                // Insert the username into the Spleef Coins database
                                string insertQuery = @"
                                INSERT OR IGNORE INTO PlayerCoins (Username, Coins) VALUES (@Username, 0);
                            ";

                                using (var insertCommand = new SqliteCommand(insertQuery, spleefConnection))
                                {
                                    insertCommand.Parameters.AddWithValue("@Username", username);
                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }

                TShock.Log.ConsoleInfo("Successfully migrated all users to the Spleef Coins database.");
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"Error migrating users: {ex.Message}");
            }
        }

    }
}

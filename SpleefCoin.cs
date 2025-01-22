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

        public void AddCoins(string username, int amount)
        {
            var sql = $"UPDATE PlayerCoins SET Coins = Coins + @amount WHERE Username = @username";

            using var connection = new SqliteConnection($"Data Source={DbPath}"); 
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@username", username);
            command.ExecuteNonQuery();
            TShock.Log.ConsoleInfo($"Gave {username} {amount} Spleef Coins!");
            var players = TSPlayer.FindByNameOrID(username);
            if (players == null || players.Count == 0)
            {
                TShock.Log.ConsoleInfo($"{username} isn't online atm");
            }
            else
            {
                var player = players[0];
                if (amount == 1)
                    player.SendMessage($"You just got {amount} Spleef Coin!", Color.Orange);
                else if (amount == 0)
                    player.SendMessage($"Hmmm you were just given {amount} Spleef Coins, someone's doing a silly", Color.Orange);
                else if (amount < 0)
                    player.SendMessage($"You just lost {-amount} Spleef Coins. :(", Color.Orange);
                else
                    player.SendMessage($"You just got {amount} Spleef Coin!", Color.Orange);
            }
        }

        public void AddCoinsCommand(CommandArgs args)
        {
            string username = args.Parameters[0];
            int amount = Convert.ToInt32(args.Parameters[1]);
            AddCoins(username, amount);
            args.Player.SendSuccessMessage($"Gave {username} {amount} Spleef Coins!");
        }

        public int GetCoins(string username)
        {
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
                    var coins = reader.GetInt32(1);
                    TShock.Log.ConsoleInfo($"{username} has {coins} Spleef Coins.");
                    return coins;                      
                }
            }
            return -1;

        }

        public void GetCoinsCommand(CommandArgs args)
        {
            string username;
            if (args.Parameters.Count == 1)
                username = args.Parameters[0];
            else
                username = args.Player.Account.Name;
            int coins = GetCoins(username);
            if (coins == -1)
                args.Player.SendErrorMessage($"{username} either has {coins} Spleef Coins or this is an error.");
            else
                args.Player.SendInfoMessage($"{username} has {coins} Spleef Coins.");
        }

        public void GetLeaderboard (CommandArgs args)
        {
            args.Player.SendMessage($"Spleef Coin leaderboard:", Color.Orange);
            int i = 1;
            if (args.Parameters.Count == 1 && args.Parameters[0] == "all")
            {
                var sql = "SELECT * FROM PlayerCoins ORDER BY Coins DESC";
                using var connection = new SqliteConnection($"Data Source={DbPath}");
                connection.Open();

                using var command = new SqliteCommand(sql, connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var Username = reader.GetString(0);
                    var Coins = reader.GetInt32(1);
                    args.Player.SendInfoMessage($"{i}) {Username}: {Coins}");
                    i++;
                }
            }
            else
            {
                var sql = "SELECT * FROM PlayerCoins ORDER BY Coins DESC LIMIT 10";
                using var connection = new SqliteConnection($"Data Source={DbPath}");
                connection.Open();

                using var command = new SqliteCommand(sql, connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var Username = reader.GetString(0);
                    var Coins = reader.GetInt32(1);
                    args.Player.SendInfoMessage($"{i}) {Username}: {Coins}");
                    i++;
                }
            }
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

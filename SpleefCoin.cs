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
        private readonly string DbPath = Path.Combine(TShock.SavePath, "SpleefCoin.sqlite");

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

        public bool isUserInTable(string username)
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
                    TShock.Log.ConsoleInfo($"{username} exists in the table and has {coins} Spleef Coins.");
                    return true;
                }
            }
            TShock.Log.ConsoleInfo($"{username} does not exist in the table.");
            return false;
        }

        public void AddCoins(string username, int amount, bool isSilent)
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
                TShock.Log.ConsoleInfo($"{username} isn't online atm");
            else if (!isSilent)
            {
                var player = players[0];
                if (amount == 1)
                    player.SendMessage($"You just got {amount} Spleef Coin!", Color.Orange);
                else if (amount == 0)
                    player.SendMessage($"Hmmm you were just given {amount} Spleef Coins, someone's doing a silly", Color.Orange);
                else if (amount == -1)
                    player.SendMessage($"You just lost {-amount} Spleef Coin. :(", Color.Orange);
                else if (amount < 0)
                    player.SendMessage($"You just lost {-amount} Spleef Coins. :(", Color.Orange);
                else
                    player.SendMessage($"You just got {amount} Spleef Coin!", Color.Orange);
            }
        }

        public void AddCoinsCommand(CommandArgs args)
        {
            string username = args.Parameters[0];
            if (!isUserInTable(username))
            {
                args.Player.SendErrorMessage($"{username} does not exist in the table");
                return;
            }
            int amount = Convert.ToInt32(args.Parameters[1]);
            AddCoins(username, amount, false);
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
            if (args.Parameters.Count >= 1)
            {
                username = args.Parameters[0];
                if (!isUserInTable(username))
                {
                    args.Player.SendErrorMessage($"{username} does not exist in the table");
                    return;
                }
            }
            else
                username = args.Player.Account.Name;
            int coins = GetCoins(username);
            if (coins == -1)
                args.Player.SendErrorMessage($"{username} either has {coins} Spleef Coins or this is an error.");
            else
                args.Player.SendInfoMessage($"{username} has {coins} Spleef Coins.");
        }

        public void GetLeaderboard(CommandArgs args)
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
                int page = 1;
                if (args.Parameters.Count == 1)
                    page = Convert.ToInt32(args.Parameters[0]);

                i = (page - 1) * 10 + 1;
                var sql = $"SELECT * FROM PlayerCoins ORDER BY Coins DESC LIMIT {(page-1)*10}, 10";
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

        public void TransferCoins (string sender, string receiver, int coins)
        {
            AddCoins(receiver, coins, false);
            AddCoins(sender, -coins, true);

            TShock.Log.ConsoleInfo($"{sender} transferred {coins} Spleef Coins to {receiver}!");
        }

        public void TransferCoinsCommand(CommandArgs args)
        {
            string sender = args.Player.Account.Name;
            string receiver = args.Parameters[0];
            int coins = Convert.ToInt32(args.Parameters[1]);

            if (coins <= 0) 
            {
                args.Player.SendErrorMessage($"you can't transfer negative or 0 coins silly");
                return;
            }
            if (GetCoins(sender) < coins)
            {
                args.Player.SendErrorMessage($"You do not have enough Spleef Coins");
                return;
            }
            if (isUserInTable(receiver))
            {
                TransferCoins(sender, receiver, coins);
                args.Player.SendSuccessMessage($"Transfered {coins} Spleef Coins to {receiver}");
                return;
            }
            args.Player.SendErrorMessage($"An account by the name {receiver} doesn't exist");
        }

        public static void MigrateUsersToSpleefDatabase()
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

                    using var selectCommand = new SqliteCommand(selectQuery, tshockConnection);

                    using var reader = selectCommand.ExecuteReader();

                    using var spleefConnection = new SqliteConnection($"Data Source={spleefDbPath}");
                    spleefConnection.Open();

                    while (reader.Read())
                    {
                        string username = reader["Username"].ToString();

                        // Insert the username into the Spleef Coins database
                        string insertQuery = @"
                                INSERT OR IGNORE INTO PlayerCoins (Username, Coins) VALUES (@Username, 0);
                                INSERT OR IGNORE INTO PlayerSettings (Username, ShowScore, GetBuffs, ShowLavarise, Chatlavarise, GetMusicBox, BlockSpamDebug) VALUES (@Username, 1, 1, 1, 0, 1, 0);
                                INSERT OR IGNORE INTO PlayerStats (Username, ELO) VALUES (@Username, 0.0);";
                            ;

                        using var insertCommand = new SqliteCommand(insertQuery, spleefConnection);

                        insertCommand.Parameters.AddWithValue("@Username", username);
                        insertCommand.ExecuteNonQuery();
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

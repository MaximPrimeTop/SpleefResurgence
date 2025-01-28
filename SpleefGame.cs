using IL.Terraria.GameContent.ObjectInteractions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using Terraria.Audio;
using IL.Terraria.GameContent.ItemDropRules;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using GetText.Loaders;

namespace SpleefResurgence
{
    public class SpleefGame
    {
        public static PluginSettings Config => PluginSettings.Config;
        private readonly Spleef pluginInstance;
        //private readonly InventoryEdit inventoryEdit;
        private readonly SpleefCoin spleefCoin;

        public SpleefGame(Spleef plugin, SpleefCoin spleefCoin/*, InventoryEdit inventoryEdit*/)
        {
            this.pluginInstance = plugin;
            //this.inventoryEdit = inventoryEdit;
            this.spleefCoin = spleefCoin;
        }

        private bool isGaming = false;

        private string CommandToStartRound;
        private string CommandToEndRound;
        private string SnowArenaCommand;
        private string NormalArenaCommand;
        private string LandmineArenaCommand;
        private int tpposx;
        private int tpposy;
        private int NumOfPlayers;
        private int RoundCounter = 0;

        private Random rnd = new Random();

        private Dictionary<string, int[]> PlayerInfo = new(); // 0 - score, 1 - tpx, 2 - tpy 3 - alive/dead 1/0

        bool isPlayerOnline(string playername)
        {
            var players = TSPlayer.FindByNameOrID(playername);
            if (players == null || players.Count == 0)
            {
                return false;
            }
            return true;
        }

        private async void Boulders(string GameType)
        {
            await Task.Delay(20000);

            if (GameType == "1" || GameType == "boulder")
            {
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    Vector2 position = plr.TPlayer.position;
                    plr.GiveItem(540, 50);
                }
                TSPlayer.All.SendMessage("[i:540] Boulders have been given out! [i:540]", Color.DeepPink);
            }

            if (GameType == "12" || GameType == "bouncy")
            {
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    Vector2 position = plr.TPlayer.position;
                    plr.GiveItem(5383, 5);
                }
                TSPlayer.All.SendMessage("[i:5383] Boulders have been given out! [i:5383]", Color.DeepPink);
            }
            if (GameType == "13" || GameType == "cactus")
            {
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    Vector2 position = plr.TPlayer.position;
                    plr.GiveItem(4390, 15);
                }
                TSPlayer.All.SendMessage("[i:4390] Boulders have been given out! [i:4390]", Color.DeepPink);
            }
        }

        private void ChooseArena(string GameArena)
        {

            if (GameArena == "random" || GameArena == "r")
            {
                GameArena = Convert.ToString(rnd.Next(3));
            }

            if (GameArena == "snow" || GameArena == "1")
            {
                if (SnowArenaCommand == "emdy")
                    GameArena = "0";
                else
                {
                    TSPlayer.All.SendMessage("[i:593] (Snow arena) [i:593]", Color.White);
                    Commands.HandleCommand(TSPlayer.Server, SnowArenaCommand);
                }
            }
            if (GameArena == "landmine" || GameArena == "2")
            {
                if (LandmineArenaCommand == "emdy")
                    GameArena = "0";
                else
                {
                    TSPlayer.All.SendMessage("[i:937] (Landmine arena) [i:937]", Color.Green);
                    Commands.HandleCommand(TSPlayer.Server, LandmineArenaCommand);
                }
            }
            if (GameArena == "normal" || GameArena == "0")
            {
                TSPlayer.All.SendMessage("[i:776] (Normal arena) [i:776]", Color.Cyan);
                Commands.HandleCommand(TSPlayer.Server, NormalArenaCommand);
            }
        }

        private void StartRound(CommandArgs args, string[] GameType, string GameArena, int GimmickAmount = 1)
        {
            foreach (KeyValuePair<string, int[]> player in PlayerInfo)
            {
                if (!isPlayerOnline(player.Key))
                {
                    args.Player.SendErrorMessage($"yo bro this {player.Key} he doesn't exist or some shit, idk bro either wait fro him to join back or restart the game tbh cus duh this guy is not here");
                    return;
                }
                var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                player.Value[3] = 1;
                plr.Teleport(player.Value[1], player.Value[2]);
                plr.SetBuff(BuffID.Webbed, 100);
            }
            RoundCounter++;
            Commands.HandleCommand(TSPlayer.Server, CommandToStartRound);
            TSPlayer.All.SendMessage($"Round {RoundCounter} started", Color.DarkSeaGreen);
            ServerApi.Hooks.NetGetData.Register(pluginInstance, OnGetData);
            for (int i = 0; i < GimmickAmount; i++)
            {
                ChooseGimmick(GameType[i]);
                if (GameType[i] == "11" || GameType[i] == "gravedigger")
                    GameArena = "0";
            }
            ChooseArena(GameArena);        
        }

        public void TheGaming(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("erm no");
                return;
            }

            else if (args.Parameters.Count == 1)
            {
                if (args.Parameters[0] == "help")
                {
                    args.Player.SendInfoMessage("this shit's so outdated bro i'm so lazy to do this just read the doc please");
                }
                else if (args.Parameters[0] == "stop" && isGaming)
                {
                    SpleefCoin.MigrateUsersToSpleefDatabase();
                    var sortedPlayerInfo = PlayerInfo
                                .OrderByDescending(entry => entry.Value[0])
                                .ToList();
                    foreach (KeyValuePair<string, int[]> player in sortedPlayerInfo)
                    {
                        var players = TSPlayer.FindByNameOrID(player.Key);
                        if (isPlayerOnline(player.Key))
                        {
                            var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                            if (plr.Account.Name == player.Key)
                                spleefCoin.AddCoins(plr.Account.Name, player.Value[0], false);
                            else
                            {
                                spleefCoin.AddCoins(plr.Account.Name, player.Value[0], true);
                                plr.SendMessage($"Sent {player.Value[0]} Spleef Coin to your account {plr.Account.Name}", Color.Purple);
                            }
                        }
                        else
                        {
                            spleefCoin.AddCoins(player.Key, player.Value[0], false);
                        }
                        TSPlayer.All.SendMessage($"{player.Key} : {player.Value[0]}", Color.Coral);
                    }
                    PlayerInfo.Clear();
                    isGaming = false;
                    TSPlayer.All.SendMessage($"Game ended, after {RoundCounter} rounds {sortedPlayerInfo[0].Key} WON!!!!!!!!!", Color.MediumTurquoise);
                    RoundCounter = 0;
                }

                else if (args.Parameters[0] == "score" && isGaming) // /game score
                {
                    var sortedPlayerInfo = PlayerInfo
                                    .OrderByDescending(entry => entry.Value[0])
                                    .ToList();
                    foreach (KeyValuePair<string, int[]> plr in sortedPlayerInfo)
                    {
                        TSPlayer.All.SendMessage($"{plr.Key} : {plr.Value[0]}", Color.Coral);
                    }
                }
            }
            else if (args.Parameters.Count == 2 && args.Parameters[0] == "add" && isGaming) // /game add name
            {
                PlayerInfo.Add(args.Parameters[1], new int[] { 0, tpposx * 16, tpposy * 16, 1 });
                NumOfPlayers++;
                args.Player.SendSuccessMessage($"added {args.Parameters[1]} to the game!");
            }

            else if (args.Parameters.Count == 2 && args.Parameters[0] == "remove" && isGaming) // /game remove name
            {
                string name = args.Parameters[1];
                int[] playerValue;
                if (!PlayerInfo.TryGetValue($"{name}", out playerValue))
                {
                    args.Player.SendErrorMessage("uh nuh uh");
                    return;
                }
                playerValue = PlayerInfo[name];
                int points = playerValue[0];
                spleefCoin.AddCoins(name, points, false);
                PlayerInfo.Remove(name);
                NumOfPlayers--;
                args.Player.SendSuccessMessage($"removed {name} from the game and awarded {points} Spleef Coins!");
            }

            else if (args.Parameters.Count >= 3 && args.Parameters[0] == "start" && isGaming) // /game start [amount] type arena
            {
                int GimmickAmount = Convert.ToInt32(args.Parameters[1]);
                string[] GameTypes = new string[10];
                if (args.Parameters.Count == 3)
                {
                    GameTypes[0] = args.Parameters[1];
                    StartRound(args, GameTypes, args.Parameters[2]);
                }
                else if (args.Parameters.Count >= 3 && args.Parameters.Count == 3 + Convert.ToInt32(args.Parameters[1]))
                {
                    for (int i = 0; i < GimmickAmount; i++)
                        GameTypes[i] = args.Parameters[i + 2];
                    StartRound(args, GameTypes, args.Parameters[GimmickAmount+2], GimmickAmount);
                }
            }

            else if (args.Parameters.Count == 3 && args.Parameters[0] == "edit" && isGaming) // /game edit player +score
            {
                if (!isPlayerOnline(args.Parameters[1]))
                {
                    args.Player.SendErrorMessage("this guy does not exist bro");
                    return;
                }
                var PlayerToEdit = TSPlayer.FindByNameOrID(args.Parameters[1])[0];
                int Value = Convert.ToInt32(args.Parameters[2]);

                if (PlayerInfo.ContainsKey(PlayerToEdit.Name))
                {
                    PlayerInfo[PlayerToEdit.Name][0] += Value;
                    TSPlayer.All.SendMessage($"{PlayerToEdit.Name} score is now {PlayerInfo[PlayerToEdit.Name][0]}!", Color.ForestGreen);
                }

                else
                {
                    args.Player.SendErrorMessage("this guy is not in the current game bro");
                }
            }


            else if (args.Parameters.Count >= 4 && args.Parameters[0] == "template" && !isGaming) // /game template templateName <NumOfPlayers> <players>
            {
                NumOfPlayers = Convert.ToInt32(args.Parameters[2]);
                if (NumOfPlayers + 3 != args.Parameters.Count)
                {
                    args.Player.SendErrorMessage("wrong amount of players");
                    return;
                }
                string name = args.Parameters[1];
                var gameTemplate = PluginSettings.Config.GameTemplates.FirstOrDefault(c => c.Name == name);
                if (gameTemplate != null)
                {
                    CommandToStartRound = gameTemplate.LavariseCommand;
                    CommandToEndRound = gameTemplate.FillCommand;
                    SnowArenaCommand = gameTemplate.SnowArenaCommand;
                    NormalArenaCommand = gameTemplate.NormalArenaCommand;
                    LandmineArenaCommand = gameTemplate.LandmineArenaCommand;
                    tpposx = gameTemplate.tpposx;
                    tpposy = gameTemplate.tpposy;
                }
                else
                {
                    args.Player.SendErrorMessage("dummas there isnt a template named like that");
                    return;
                }
                for (int i = 3; i <= NumOfPlayers + 2; i++)
                {
                    PlayerInfo.Add(args.Parameters[i], new int[] { 0, tpposx * 16, tpposy * 16, 1 });
                }
                isGaming = true;
                TSPlayer.All.SendMessage("Game started, get ready!", Color.SeaShell);
            }
        }

        private int counter = 0;
        private string SecondWinner;

        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.PlayerDeathV2)
            {
                using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    byte playerID = reader.ReadByte();

                    var plr = TSPlayer.FindByNameOrID(Convert.ToString(playerID))[0];

                    foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                    {
                        var plrOG = TSPlayer.FindByNameOrID(player.Key)[0];
                        if (plr == plrOG && player.Value[3] == 1)
                        {
                            PlayerInfo[player.Key][3] = 0;
                            counter++;

                            if (counter == NumOfPlayers - 1)
                            {
                                SecondWinner = player.Key;
                                PlayerInfo[player.Key][0] += 1;
                                Commands.HandleCommand(TSPlayer.Server, CommandToEndRound);
                            }

                            else if (counter == NumOfPlayers)
                            {
                                counter = 0;
                                PlayerInfo[player.Key][0] += 2;
                                TSPlayer.All.SendMessage($"Round ended! {player.Key} won this round and {SecondWinner} got second place!", Color.LimeGreen);
                                var sortedPlayerInfo = PlayerInfo
                                    .OrderByDescending(entry => entry.Value[0])
                                    .ToList();
                                foreach (KeyValuePair<string, int[]> plrr in sortedPlayerInfo)
                                {
                                    TSPlayer.All.SendMessage($"{plrr.Key} : {plrr.Value[0]}", Color.Coral);
                                }
                                ServerApi.Hooks.NetGetData.Deregister(pluginInstance, OnGetData);
                            }

                        }
                    }
                    
                }
            }
        }
        private void ChooseGimmick(string GameType)
        {
            if (GameType == "random" || GameType == "r")
            {
                GameType = Convert.ToString(rnd.Next(16));
            }

            if (GameType == "0" || GameType == "normal")
            {
                TSPlayer.All.SendMessage("[i:776] Normal round! [i:776]", Color.Cyan);
            }

            if (GameType == "1" || GameType == "boulder")
            {
                TSPlayer.All.SendMessage("[i:540] Boulder round! [i:540]", Color.Gray);
                Boulders(GameType);
            }

            if (GameType == "2" || GameType == "cloud")
            {
                TSPlayer.All.SendMessage("[i:53] Cloud in a bottle round! [i:53]", Color.White);
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.GiveItem(53, 1);
                }
            }

            if (GameType == "3" || GameType == "tsunami")
            {
                TSPlayer.All.SendMessage("[i:3201] Tsunami in a bottle round! [i:3201]", Color.DeepSkyBlue);
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.GiveItem(3201, 1);
                }
            }

            if (GameType == "4" || GameType == "blizzard")
            {
                TSPlayer.All.SendMessage("[i:987] Blizzard in a bottle round! [i:987]", Color.White);
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.GiveItem(987, 1);
                }
            }

            if (GameType == "5" || GameType == "portal")
            {
                TSPlayer.All.SendMessage("[i:3384] Portal round! [i:3384]", Color.White);
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.GiveItem(3384, 1);
                }
            }

            if (GameType == "6" || GameType == "bomb fish")
            {
                TSPlayer.All.SendMessage("[i:3196] Bomb fish round! [i:3196]", Color.DarkGray);
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.GiveItem(3196, 50);
                }
            }

            if (GameType == "7" || GameType == "rocket15")
            {
                TSPlayer.All.SendMessage("[i:759] Rocket round! (15 rockets) [i:759]", Color.Orange);
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.GiveItem(759, 1);
                    plr.GiveItem(772, 15);
                }
            }

            if (GameType == "8" || GameType == "ice rod")
            {
                TSPlayer.All.SendMessage("[i:496] Ice rod round! [i:496]", Color.DeepSkyBlue);
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.GiveItem(496, 1);
                }
            }

            if (GameType == "9" || GameType == "soc")
            {
                TSPlayer.All.SendMessage("[i:3097] Shield of Cthulhu round! [i:3097]", Color.Red);
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.GiveItem(3097, 1);
                }
            }

            if (GameType == "10" || GameType == "slime")
            {
                TSPlayer.All.SendMessage("[i:2430] Slime saddle round! [i:2430]", Color.DeepSkyBlue);
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.GiveItem(2430, 1);
                }
            }

            if (GameType == "11" || GameType == "gravedigger")
            {
                TSPlayer.All.SendMessage("[i:4711] Gravedigger round! [i:4711]", Color.Gray);
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.GiveItem(4711, 1);
                }
            }

            if (GameType == "12" || GameType == "bouncy")
            {
                TSPlayer.All.SendMessage("[i:5383] Bouncy boulder round! [i:5383]", Color.LightPink);
                Boulders(GameType);
            }

            if (GameType == "13" || GameType == "cactus")
            {
                TSPlayer.All.SendMessage("[i:4390] Rolling cactus round! [i:4390]", Color.Green);
                Boulders(GameType);
            }

            if (GameType == "14" || GameType == "rocket100")
            {
                TSPlayer.All.SendMessage("[i:759] Rocket round! (100 rockets) [i:759]", Color.Orange);
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.GiveItem(759, 1);
                    plr.GiveItem(772, 100);
                }
            }
            if (GameType == "15" || GameType == "baloon")
            {
                TSPlayer.All.SendMessage("[i:159] Balloon round! [i:159]", Color.Red);
                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.GiveItem(159, 1);
                }
            }
        }
    }
}

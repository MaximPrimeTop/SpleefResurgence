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
using Newtonsoft.Json;
using System.Collections;
using Steamworks;

namespace SpleefResurgence
{
    public class SpleefGame
    {
        public readonly int[] MusicBoxIDs = { 562, 1600, 564, 1601, 1596, 1602, 1603, 1604, 4077, 4079, 1597, 566, 1964, 1610, 568, 569, 570, 1598, 2742, 571, 573, 3237, 1605, 1608, 567, 572, 574, 1599, 1607, 5112, 4979, 1606,
            4985, 4990, 563, 1609, 3371, 3236, 3235, 1963, 1965, 3370, 3044, 3796, 3869, 4078, 4080, 4081, 4082, 4237, 4356, 4357, 4358, 4421, 4606, 4991, 4992, 5006, 5014, 5015, 5016, 5017, 5018, 5019, 5020, 5021, 5022, 5023,
            5024, 5025, 5026, 5027, 5028, 5029, 5030, 5031, 5032, 5033, 5034, 5035, 5036, 5037, 5038, 5039, 5040, 5044, 5362, 565};

        public static PluginSettings Config => PluginSettings.Config;
        private readonly Spleef pluginInstance;
        private readonly InventoryEdit inventoryEdit;
        private readonly SpleefCoin spleefCoin;

        public SpleefGame(Spleef plugin, SpleefCoin spleefCoin, InventoryEdit inventoryEdit)
        {
            this.pluginInstance = plugin;
            this.inventoryEdit = inventoryEdit;
            this.spleefCoin = spleefCoin;
        }

        private bool isGaming = false;
        private bool isRound = false;

        private string CommandToStartRound;
        private string CommandToEndRound;
        private int ExactPlayerCount;
        private int ChillPlayerCount;
        private int RoundCounter = 0;

        private Random rnd = new();
        private class Playering
        {
            public int score;
            public bool isAlive;
            public int place;
            public string accName;
            public bool isIngame;
        }
        private Dictionary<string, Playering> PlayerInfo = new();
        private List<Map> MapsInfo = new();
        List<KeyValuePair<string, Playering>> PlayerInfoList;

        bool isPlayerOnline(string playername)
        {
            var players = TSPlayer.FindByNameOrID(playername);
            if (players == null || players.Count == 0)
            {
                return false;
            }
            return true;
        }

        private void StartRound(CommandArgs args, string[] GameType, Map GameArena, int GimmickAmount = 1)
        {
            if (ExactPlayerCount <= 1)
            {
                args.Player.SendErrorMessage($"Can't start a round with this amount of players! Make sure there are at least 2 players");
                return;
            }
            foreach (KeyValuePair<string, Playering> player in PlayerInfo)
            {
                if (player.Value.isIngame == true)
                {
                    if (!isPlayerOnline(player.Key))
                    {
                        args.Player.SendErrorMessage($"yo bro this {player.Key} he doesn't exist or some shit, idk bro either wait fro him to join back or remove him from the game ngl he probably stinks");
                        return;
                    }
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    inventoryEdit.ClearPlayerEverything(plr);
                    inventoryEdit.AddItem(plr, 0, 1, 776);
                    inventoryEdit.AddItem(plr, 9, 1, 1299);
                    inventoryEdit.AddItem(plr, 40, 1, 776);
                    inventoryEdit.AddArmor(plr, 3, 158);
                }
            }
            counter = 0;
            RoundCounter++;
            Commands.HandleCommand(TSPlayer.Server, CommandToStartRound);
            TSPlayer.All.SendMessage($"Round {RoundCounter} started", Color.DarkSeaGreen);
            for (int i = 0; i < GimmickAmount; i++)
            {
                if (GameType[i] == "random" || GameType[i] == "r")
                    GameType[i] = Convert.ToString(rnd.Next(18));
                if (GameType[i] == "11" || GameType[i] == "gravedigger")
                    GameArena = MapsInfo.FirstOrDefault(c => c.MapName == "gravedigger");
            }
            isRound = true;
            ServerApi.Hooks.NetGetData.Register(pluginInstance, OnGetData);
            ChooseArena(GameArena);
            for (int i = 0; i < GimmickAmount; i++)
                ChooseGimmick(GameType[i]);



            Item MusicBox = new();
            MusicBox.SetDefaults(MusicBoxIDs[rnd.Next(MusicBoxIDs.Length)]);
            GiveEveryoneArmor(MusicBox.netID);
            string SongName = MusicBox.Name.Substring(MusicBox.Name.IndexOf('(') + 1, MusicBox.Name.IndexOf(')') - MusicBox.Name.IndexOf('(') - 1);
            if (MusicBox.Name.Split(' ')[0] == "Otherwordly")
                SongName = "Otherwordly" + SongName;
            TSPlayer.All.SendMessage($"[i:{MusicBox.netID}] Playing {SongName} [i:{MusicBox.netID}]", Color.LightPink);
        }
        //im tired brah
        private void AnnounceScore()
        {
            PlayerInfoList = PlayerInfo
                .OrderByDescending(entry => entry.Value.score)
                .ToList();
            TSPlayer.All.SendMessage($"Game score:", Color.DeepPink);
            foreach (KeyValuePair<string, Playering> plr in PlayerInfoList)
            {
                if (plr.Value.isIngame == true)
                    TSPlayer.All.SendMessage($"{plr.Key} : {plr.Value.score}", Color.Coral);
                else
                    TSPlayer.All.SendMessage($"{plr.Key} : {plr.Value.score}", Color.Gray);
            }
        }
        public void TheGaming(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("erm no");
                return;
            }
            if (args.Parameters[0] == "help")
            {
                args.Player.SendInfoMessage("/game template <templateName> <ExactPlayerCount> <list player names> - creates a new game");
                args.Player.SendMessage("these commands only work if there's a game going on!", Color.OrangeRed);
                args.Player.SendInfoMessage("/game start <gimmickAmount> <list gimmicks> <map> - starts a round");
                args.Player.SendInfoMessage("/game stop - ends the game (alias /game end), however if there's a round active, stops the round.");
                args.Player.SendInfoMessage("/game add <playername> - adds a player to the ongoing game");
                args.Player.SendInfoMessage("/game remove <playername> - removes a player from the game");
                args.Player.SendInfoMessage("/game score - shows the current score");
                args.Player.SendInfoMessage("/game edit <playername> <amount> - changes the player's score by the given amount");
            }
            else if (!isGaming)
            {
                switch (args.Parameters[0])
                {
                    case "template":
                        if (args.Parameters.Count != 2)
                        {
                            args.Player.SendErrorMessage("wrong amount of parameters!");
                            args.Player.SendErrorMessage("/game template <templateName>");
                            return;
                        }
                        string name = args.Parameters[1];
                        var gameTemplate = PluginSettings.Config.GameTemplates.FirstOrDefault(c => c.Name == name);
                        if (gameTemplate != null)
                        {
                            CommandToStartRound = gameTemplate.LavariseCommand;
                            CommandToEndRound = gameTemplate.FillCommand;
                            foreach (var map in gameTemplate.Maps)
                                MapsInfo.Add(map);
                        }
                        else
                        {
                            args.Player.SendErrorMessage("dummas there isnt a template named like that");
                            return;
                        }
 
                        isGaming = true;
                        TSPlayer.All.SendMessage("Game started! Join with /j", Color.SeaShell);
                        ServerApi.Hooks.ServerLeave.Register(pluginInstance, OnPlayerLeave);
                        return;
                    case "start":
                    case "stop":
                    case "end":
                    case "add":
                    case "remove":
                    case "score":
                    case "edit":
                        args.Player.SendErrorMessage("You can't use that command yet! You should first create a game");
                        args.Player.SendInfoMessage("/game template <templateName> <numOfPlayers> <list player names>");
                        return;
                    default:
                        args.Player.SendErrorMessage("That's not a valid command");
                        args.Player.SendInfoMessage("/game template <templateName> <numOfPlayers> <list player names>");
                        return;
                }
            }
            else if (isGaming && !isRound)
            {
                switch (args.Parameters[0])
                {
                    case "start":
                        ChillPlayerCount = ExactPlayerCount;
                        if (args.Parameters.Count < 3)
                        {
                            args.Player.SendErrorMessage("not enough parameters!");
                            args.Player.SendInfoMessage("/game start <gimmickAmount> <list gimmicks> <map>");
                            return;
                        }
                        string mapname = args.Parameters[2];
                        Map map;
                        if (mapname == "r" || mapname == "random")
                            map = MapsInfo[rnd.Next(MapsInfo.Count)];
                        if (int.TryParse(mapname, out int n) && n <= MapsInfo.Count && n >= 0)
                            map = MapsInfo[n];
                        else
                            map = MapsInfo.FirstOrDefault(c => c.MapName == mapname);

                        if (map == null)
                        {
                            List<string> mapNames = Config.GameTemplates
                                .Select((c, index) => $"{index} - {c.Name}")
                                .ToList();
                            string mapList = string.Join(", ", mapNames);
                            args.Player.SendErrorMessage("that is not a valid map!");
                            args.Player.SendInfoMessage($"Here are all valid maps: {mapNames}");
                            return;
                        }

                        int GimmickAmount = Convert.ToInt32(args.Parameters[1]);
                        string[] GameTypes = new string[100];

                        if (args.Parameters.Count == 3)
                        {
                            GameTypes[0] = args.Parameters[1];
                            StartRound(args, GameTypes, map);
                        }
                        else if (args.Parameters.Count >= 3 && args.Parameters.Count == 3 + Convert.ToInt32(args.Parameters[1]))
                        {
                            for (int i = 0; i < GimmickAmount; i++)
                                GameTypes[i] = args.Parameters[i + 2];
                            StartRound(args, GameTypes, map, GimmickAmount);
                        }
                        break;
                    case "stop":
                    case "end":
                        SpleefCoin.MigrateUsersToSpleefDatabase();
                        AnnounceScore();
                        TSPlayer.All.SendMessage($"Game ended, after {RoundCounter} rounds {PlayerInfoList[0].Key} WON!!!!!!!!!", Color.MediumTurquoise);
                        foreach (KeyValuePair<string, Playering> player in PlayerInfoList)
                        {
                            var players = TSPlayer.FindByNameOrID(player.Key);
                            if (isPlayerOnline(player.Key))
                            {
                                var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                                if (player.Value.accName == player.Key)
                                    spleefCoin.AddCoins(player.Value.accName, player.Value.score, false);
                                else
                                {
                                    spleefCoin.AddCoins(player.Value.accName, player.Value.score, true);
                                    plr.SendMessage($"Sent {player.Value.score} Spleef Coin to your account {player.Value.accName}", Color.Purple);
                                }
                            }
                            else
                                spleefCoin.AddCoins(player.Value.accName, player.Value.score, false);
                        }
                        PlayerInfo.Clear();
                        MapsInfo.Clear();
                        isGaming = false;
                        RoundCounter = 0;
                        ServerApi.Hooks.ServerLeave.Deregister(pluginInstance, OnPlayerLeave);
                        break;
                    case "add":
                        if (args.Parameters.Count != 2)
                        {
                            args.Player.SendErrorMessage("wrong amount of parameters!");
                            args.Player.SendInfoMessage("/game add <playername>");
                            return;
                        }
                        string playername = args.Parameters[1];
                        if (!isPlayerOnline(playername))
                        {
                            args.Player.SendErrorMessage("this guy does not exist bro");
                            return;
                        }
                        var playerToAdd = TSPlayer.FindByNameOrID(playername)[0];
                        Playering newPlayering = new()
                        {
                            score = 0,
                            isAlive = false,
                            place = 0,
                            accName = playerToAdd.Account.Name,
                            isIngame = true
                        };
                        playername = playerToAdd.Name;
                        PlayerInfo.Add(playername, newPlayering);
                        ExactPlayerCount++;
                        TSPlayer.All.SendMessage($"{playername} has been added to the game!", Color.Green);
                        break;
                    case "remove":
                        if (args.Parameters.Count != 2)
                        {
                            args.Player.SendErrorMessage("wrong amount of parameters!");
                            args.Player.SendInfoMessage("/game remove <playername>");
                            return;
                        }
                        string name = args.Parameters[1];
                        Playering playerToRemove;
                        if (!PlayerInfo.TryGetValue($"{name}", out playerToRemove))
                        {
                            args.Player.SendErrorMessage("uh nuh uh");
                            return;
                        }
                        playerToRemove = PlayerInfo[name];
                        int points = playerToRemove.score;
                        spleefCoin.AddCoins(playerToRemove.accName, points, false);
                        if (playerToRemove.isIngame)
                            ExactPlayerCount--;
                        PlayerInfo.Remove(name);
                        args.Player.SendSuccessMessage($"removed {name} from the game and awarded {points} Spleef Coins!");
                        TSPlayer.All.SendMessage($"{name} has been completely removed from the game!", Color.Red);
                        break;
                    case "score":
                        AnnounceScore();
                        break;
                    case "edit":
                        if (args.Parameters.Count != 3)
                        {
                            args.Player.SendErrorMessage("wrong amount of parameters!");
                            args.Player.SendInfoMessage("/game edit <playername> <amount>");
                            return;
                        }
                        if (!isPlayerOnline(args.Parameters[1]))
                        {
                            args.Player.SendErrorMessage("this guy does not exist bro");
                            return;
                        }
                        var PlayerToEdit = TSPlayer.FindByNameOrID(args.Parameters[1])[0];
                        int Value = Convert.ToInt32(args.Parameters[2]);

                        if (PlayerInfo.ContainsKey(PlayerToEdit.Name))
                        {
                            PlayerInfo[PlayerToEdit.Name].score += Value;
                            TSPlayer.All.SendMessage($"{PlayerToEdit.Name} score is now {PlayerInfo[PlayerToEdit.Name].score}!", Color.ForestGreen);
                        }

                        else
                        {
                            args.Player.SendErrorMessage("this guy is not in the current game bro");
                        }
                        break;
                    case "template":
                        args.Player.SendErrorMessage("You can't use this command right now! There is already a game going on.");
                        break;
                    default:
                        args.Player.SendErrorMessage("That's not a valid command.");
                        break;
                }
            }
            else if (isGaming && isRound)
            {
                switch (args.Parameters[0])
                {
                    case "stop":
                    case "end":
                        ServerApi.Hooks.NetGetData.Deregister(pluginInstance, OnGetData);
                        Commands.HandleCommand(TSPlayer.Server, CommandToEndRound);
                        TSPlayer.All.SendMessage($"Forcefully ended round!", Color.BlueViolet);
                        isRound = false;
                        break;
                    case "score":
                        AnnounceScore();
                        break;
                    case "start":
                    case "add":
                    case "remove":
                    case "edit":
                        args.Player.SendErrorMessage("You can't use this command right now! There is currently a round going on. Wait until it ends or end it with /game stop");
                        break;
                    case "template":
                        args.Player.SendErrorMessage("You can't use this command right now! There is already a game going on.");
                        break;
                    default:
                        args.Player.SendErrorMessage("That's not a valid command.");
                        break;
                }
            }
        }

        public void JoinGame (CommandArgs args)
        {
            if (!isGaming)
            {
                args.Player.SendErrorMessage("There is no game to join!");
                return;
            }

            string playername = args.Player.Name;

            if (PlayerInfo.ContainsKey(playername))
            {
                Playering playerToAdd = PlayerInfo[playername];
                
                if (playerToAdd.isIngame == true)
                {
                    args.Player.SendErrorMessage("You are already in the game!");
                }
                else if (!isRound)
                {
                    playerToAdd.isIngame = true;
                    ExactPlayerCount++;
                    TSPlayer.All.SendMessage($"{playername} has joined back into the game!", Color.Green);
                }
                else
                {
                    args.Player.SendErrorMessage("You can't join yet! There is currently a round going on. Wait until it ends and join.");
                }
                return;
            }    
            
            Playering newPlayering = new()
            {
                score = 0,
                isAlive = false,
                place = 0,
                accName = args.Player.Account.Name,
                isIngame = true
            };
            PlayerInfo.Add(playername, newPlayering);
            ExactPlayerCount++;
            TSPlayer.All.SendMessage($"{playername} has joined the game!", Color.Green);
        }

        public void LeaveGame (CommandArgs args)
        {
            if (!isGaming)
            {
                args.Player.SendErrorMessage("There is no game to leave!"); //just in case
                return;
            }

            string playername = args.Player.Name;

            Playering playerToRemove;
            if (!PlayerInfo.TryGetValue($"{playername}", out playerToRemove))
            {
                args.Player.SendErrorMessage("You are not in the game!");
                return;
            }

            if (playerToRemove.isIngame == false)
            {
                args.Player.SendErrorMessage("You have already left!");
                return;
            }

            if (isRound)
            {
                args.Player.SendErrorMessage("You can't leave yet! There is currently a round going on. Wait until it ends and leave.");
                return;
            }
            
            playerToRemove = PlayerInfo[playername];
            playerToRemove.isIngame = false;
            ExactPlayerCount--;
            TSPlayer.All.SendMessage($"{playername} has left the game!", Color.Red);
        }

        private int counter = 0;

        private void CheckRound(int counter)
        {
            if (counter == ExactPlayerCount - 1)
                Commands.HandleCommand(TSPlayer.Server, CommandToEndRound);

            else if (counter == ExactPlayerCount)
            {
                PlayerInfoList = PlayerInfo
                   .OrderByDescending(entry => entry.Value.isIngame)
                   .ThenByDescending(entry => entry.Value.place)
                   .ToList();
                if (ChillPlayerCount == 2)
                {
                    PlayerInfoList[0].Value.score += 1;
                    TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round!", Color.LimeGreen);
                }
                else if (ChillPlayerCount >= 3 && ChillPlayerCount <= 6)
                {
                    PlayerInfoList[0].Value.score += 2;
                    PlayerInfoList[1].Value.score += 1;
                    TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round and {PlayerInfoList[1].Key} got second place!", Color.LimeGreen);
                }
                else if (ChillPlayerCount >= 7 && ChillPlayerCount <= 9)
                {
                    PlayerInfoList[0].Value.score += 3;
                    PlayerInfoList[1].Value.score += 2;
                    PlayerInfoList[2].Value.score += 1;
                    TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round {PlayerInfoList[1].Key} got second place and {PlayerInfoList[2].Key} got third place!", Color.LimeGreen);
                }
                else if (ChillPlayerCount >= 10 && ChillPlayerCount <= 13)
                {
                    PlayerInfoList[0].Value.score += 4;
                    PlayerInfoList[1].Value.score += 3;
                    PlayerInfoList[2].Value.score += 2;
                    PlayerInfoList[3].Value.score += 1;
                    TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round {PlayerInfoList[1].Key} got second place {PlayerInfoList[2].Key} got third place and {PlayerInfoList[3].Key} got fourth place!", Color.LimeGreen);
                }
                PlayerInfo = PlayerInfoList.ToDictionary(pair => pair.Key, pair => pair.Value);

                AnnounceScore();
                TSPlayer.All.SendMessage($"You can join the game by typing /j", Color.LightPink);
                TSPlayer.All.SendMessage($"If you want to leave the game type /l", Color.LightPink);
                isRound = false;
                ServerApi.Hooks.NetGetData.Deregister(pluginInstance, OnGetData);
            }
        }

        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.PlayerDeathV2)
            {
                using var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
                byte playerID = reader.ReadByte();

                var plr = TShock.Players[playerID];

                foreach (KeyValuePair<string, Playering> player in PlayerInfo)
                {
                    if (player.Value.isIngame)
                    {
                        var plrOG = TSPlayer.FindByNameOrID(player.Key)[0];
                        if (plr == plrOG && player.Value.isAlive)
                        {
                            PlayerInfo[player.Key].isAlive = false;
                            PlayerInfo[player.Key].place = counter;
                            counter++;
                            inventoryEdit.ClearPlayerEverything(plr);
                            inventoryEdit.AddItem(plr, 0, 1, 776);
                            inventoryEdit.AddItem(plr, 9, 1, 1299);
                            inventoryEdit.AddItem(plr, 40, 1, 776);
                            inventoryEdit.AddArmor(plr, 3, 158);
                            CheckRound(counter);
                        }
                    }
                }
            }
        }

        private void OnPlayerLeave(LeaveEventArgs args)
        {
            int playerID = args.Who;
            var plr = TShock.Players[playerID];
            if (!isRound)
            {
                Commands.HandleCommand(plr, "/l");
                return;
            }
            foreach (KeyValuePair<string, Playering> player in PlayerInfo)
            {
                if (player.Value.isIngame)
                {
                    var plrOG = TSPlayer.FindByNameOrID(player.Key)[0];
                    if (plr == plrOG && player.Value.isAlive)
                    {
                        PlayerInfo[player.Key].isAlive = false;
                        PlayerInfo[player.Key].place = counter;
                        PlayerInfo[player.Key].isIngame = false;
                        ExactPlayerCount--;
                        TSPlayer.All.SendMessage($"{player.Key} has left the game midround! What a stinker...", Color.LightYellow);
                        CheckRound(counter);
                    }
                }
            }
        }


        private void GiveEveryoneItems(int itemID, int stack)
        {

            foreach (KeyValuePair<string, Playering> player in PlayerInfo)
            {
                if (player.Value.isIngame)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    int slot = inventoryEdit.FindNextFreeSlot(plr);
                    inventoryEdit.AddItem(plr, slot, stack, itemID);
                }
            }
        }

        private void GiveEveryoneArmor(int itemID)
        {
            foreach (KeyValuePair<string, Playering> player in PlayerInfo)
            {
                if (player.Value.isIngame == true)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    int slot = inventoryEdit.FindNextFreeAccessorySlot(plr);
                    if (slot == -1)
                    {
                        slot = inventoryEdit.FindNextFreeSlot(plr);
                        inventoryEdit.AddItem(plr, slot, 1, itemID);
                    }
                    else
                        inventoryEdit.AddArmor(plr, slot, itemID);
                }
            }
        }

        private void SetEveryoneBuff(int BuffID, int time)
        {
            foreach (KeyValuePair<string, Playering> player in PlayerInfo)
            {
                if (player.Value.isIngame == true)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.SetBuff(BuffID, time);
                }
            }
        }

        private void TpAndWebEveryone(int x1, int y1)
        {
            foreach (KeyValuePair<string, Playering> player in PlayerInfo)
            {
                if (player.Value.isIngame == true)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    plr.Teleport(x1 * 16, y1 * 16);
                    plr.SetBuff(BuffID.Webbed, 100);
                    player.Value.isAlive = true;
                }
            }
        }

        private void ChooseArena(Map GameArena)
        {
            switch (GameArena.MapName)
            {
                case "normal":
                    TSPlayer.All.SendMessage("[i:776] Normal arena [i:776]", Color.Cyan);
                    break;
                case "snow":
                    TSPlayer.All.SendMessage("[i:593] Gravedigger arena [i:593]", Color.White);
                    break;
                case "landmine":
                    TSPlayer.All.SendMessage("[i:937] Landmine arena [i:937]", Color.Green);
                    break;
                case "geyser":
                    TSPlayer.All.SendMessage("[i:3722] Geyser arena [i:3722]", Color.Orange);
                    break;
                case "rope":
                    TSPlayer.All.SendMessage("[i:965] Rope arena [i:965]", Color.RosyBrown);
                    break;
                case "minecart":
                    TSPlayer.All.SendMessage("[i:2340] Minecart arena [i:2340]", Color.Gray);
                    break;
                case "platform":
                    TSPlayer.All.SendMessage("[i:776] Platform arena [i:776]", Color.Cyan);
                    break;
                case "lavafall":
                    TSPlayer.All.SendMessage("[i:207] Lavafall arena [i:207]", Color.OrangeRed);
                    break;
                case "pigron":
                    TSPlayer.All.SendMessage("[i:4613] Pigron arena [i:4613]", Color.DeepPink);
                    break;
            }
            Commands.HandleCommand(TSPlayer.Server, GameArena.MapCommand);
            TpAndWebEveryone(GameArena.tpposx, GameArena.tpposy);
        }


        private async void Boulders(string GameType)
        {
            await Task.Delay(20000);

            switch (GameType)
            {
                case "boulder":
                    GiveEveryoneItems(540, 50);
                    TSPlayer.All.SendMessage("[i:540] Boulders have been given out! [i:540]", Color.DeepPink);
                    break;
                case "bouncy":
                    GiveEveryoneItems(5383, 5);
                    TSPlayer.All.SendMessage("[i:5383] Boulders have been given out! [i:5383]", Color.DeepPink);
                    break;
                case "cactus":
                    GiveEveryoneItems(4390, 25);
                    TSPlayer.All.SendMessage("[i:4390] Boulders have been given out! [i:4390]", Color.DeepPink);
                    break;
            }
        }

        private void ChooseGimmick(string GameType)
        {
            switch (GameType)
            {
                case "0":
                case "normal":
                    TSPlayer.All.SendMessage("[i:776] Normal round! [i:776]", Color.Cyan);
                    break;
                case "1":
                case "boulder":
                    TSPlayer.All.SendMessage("[i:540] Boulder round! [i:540]", Color.Gray);
                    Boulders("boulder");
                    break;
                case "2":
                case "cloud":
                    TSPlayer.All.SendMessage("[i:53] Cloud in a bottle round! [i:53]", Color.White);
                    GiveEveryoneArmor(53);
                    break;
                case "3":
                case "tsunami":
                    TSPlayer.All.SendMessage("[i:3201] Tsunami in a bottle round! [i:3201]", Color.DeepSkyBlue);
                    GiveEveryoneArmor(3201);
                    break;
                case "4":
                case "blizzard":
                    TSPlayer.All.SendMessage("[i:987] Blizzard in a bottle round! [i:987]", Color.White);
                    GiveEveryoneArmor(987);
                    break;
                case "5":
                case "portal":
                    TSPlayer.All.SendMessage("[i:3384] Portal round! [i:3384]", Color.White);
                    GiveEveryoneItems(3384, 1);
                    break;
                case "6":
                case "bombfish":
                    TSPlayer.All.SendMessage("[i:3196] Bomb fish round! [i:3196]", Color.DarkGray);
                    GiveEveryoneItems(3196, 50);
                    break;
                case "7":
                case "rocket15":
                    TSPlayer.All.SendMessage("[i:759] Rocket round! (15 rockets) [i:759]", Color.Orange);
                    GiveEveryoneItems(759, 1);
                    GiveEveryoneItems(772, 15);
                    break;
                case "8":
                case "icerod":
                    TSPlayer.All.SendMessage("[i:496] Ice rod round! [i:496]", Color.DeepSkyBlue);
                    GiveEveryoneItems(496, 1);
                    break;
                case "9":
                case "soc":
                    TSPlayer.All.SendMessage("[i:3097] Shield of Cthulhu round! [i:3097]", Color.Red);
                    GiveEveryoneArmor(3097);
                    break;
                case "10":
                case "slime":
                    TSPlayer.All.SendMessage("[i:2430] Slime saddle round! [i:2430]", Color.DeepSkyBlue);
                    GiveEveryoneItems(2430, 1);
                    break;
                case "11":
                case "gravedigger":
                    TSPlayer.All.SendMessage("[i:4711] Gravedigger round! [i:4711]", Color.Gray);
                    GiveEveryoneItems(4711, 1);
                    break;
                case "12":
                case "bouncy":
                    TSPlayer.All.SendMessage("[i:5383] Bouncy boulder round! [i:5383]", Color.LightPink);
                    Boulders("bouncy");
                    break;
                case "13":
                case "cactus":
                    TSPlayer.All.SendMessage("[i:4390] Rolling cactus round! [i:4390]", Color.Green);
                    Boulders("cactus");
                    break;
                case "14":
                case "rocket100":
                    TSPlayer.All.SendMessage("[i:759] Rocket round! (100 rockets) [i:759]", Color.Orange);
                    GiveEveryoneItems(759, 1);
                    GiveEveryoneItems(772, 100);
                    break;
                case "15":
                case "balloon":
                    TSPlayer.All.SendMessage("[i:159] Balloon round! [i:159]", Color.Red);
                    GiveEveryoneArmor(159);
                    break;
                case "16":
                case "insignia":
                    TSPlayer.All.SendMessage("[i:4989] Soaring insignia round [i:4989]", Color.Pink);
                    GiveEveryoneArmor(4989);
                    break;
                case "17":
                case "longrange":
                    TSPlayer.All.SendMessage("[i:407][i:1923][i:2215] Long range round [i:2215][i:1923][i:407]", Color.RosyBrown);
                    SetEveryoneBuff(BuffID.Builder, 60000);
                    GiveEveryoneArmor(407);
                    GiveEveryoneArmor(1923);
                    GiveEveryoneArmor(2215);
                    break;
                case "18":
                case "flipper":
                    TSPlayer.All.SendMessage("[i:187] Flipper round [i:187]", Color.Blue);
                    GiveEveryoneArmor(187);
                    break;
                default:
                    TSPlayer.All.SendMessage("Invalid gimmick, putting normal", Color.DarkRed);
                    goto case "0";
            }
        }
    }
}

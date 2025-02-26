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

        private class Map
        {
            public string MapCommand;
            public int tpposx;
            public int tpposy;
        }

        private class MobMap : Map
        {
            public int MobID;
            public int Mobposx;
            public int Mobposy;
        }

        private Map ConvertMap(SpleefResurgence.Map Map)
        {
            return new Map
            {
                MapCommand = Map.MapCommand,
                tpposx = Map.tpposx,
                tpposy = Map.tpposy
            };
        }

        private MobMap ConvertMobMap(SpleefResurgence.MobMap Map)
        {
            return new MobMap
            {
                MapCommand = Map.MapCommand,
                tpposx = Map.tpposx,
                tpposy = Map.tpposy,
                MobID = Map.MobID,
                Mobposx = Map.Mobposx,
                Mobposy = Map.tpposy
            };
        }

        private bool isGaming = false;
        private bool isRound = false;

        private string CommandToStartRound;
        private string CommandToEndRound;
        private Map SnowMap;
        private Map NormalMap;
        private Map LandmineMap;
        private Map GeyserMap;
        private Map RopeMap;
        private Map MinecartMap;
        private Map PlatformMap;
        private Map LavafallMap;
        private MobMap PigronMap;
        private int NumOfPlayers;
        private int RoundCounter = 0;

        private Random rnd = new Random();
        private class Playering
        {
            public int score;
            public bool isAlive;
            public int place;
            public string accName;
            public bool isIngame;
        }
        private Dictionary<string, Playering> PlayerInfo = new();
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

        private void StartRound(CommandArgs args, string[] GameType, string GameArena, int GimmickAmount = 1)
        {
            if (NumOfPlayers <= 1)
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
                    GameArena = "snow";
            }
            isRound = true;
            ServerApi.Hooks.NetGetData.Register(pluginInstance, OnGetData);
            ChooseArena(GameArena);
            for (int i = 0; i < GimmickAmount; i++)
                ChooseGimmick(GameType[i]);
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
                args.Player.SendInfoMessage("/game template <templateName> <numOfPlayers> <list player names> - creates a new game");
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
                        if (args.Parameters.Count < 4)
                        {
                            args.Player.SendErrorMessage("not enough parameters!");
                            args.Player.SendErrorMessage("/game template <templateName> <numOfPlayers> <list player names>");
                            return;
                        }
                        NumOfPlayers = Convert.ToInt32(args.Parameters[2]);
                        if (NumOfPlayers + 3 != args.Parameters.Count)
                        {
                            args.Player.SendErrorMessage("wrong amount of players");
                            args.Player.SendErrorMessage("/game template <templateName> <numOfPlayers> <list player names>");
                            return;
                        }
                        string name = args.Parameters[1];
                        var gameTemplate = PluginSettings.Config.GameTemplates.FirstOrDefault(c => c.Name == name);
                        if (gameTemplate != null)
                        {
                            CommandToStartRound = gameTemplate.LavariseCommand;
                            CommandToEndRound = gameTemplate.FillCommand;
                            SnowMap = ConvertMap(gameTemplate.SnowMap);
                            NormalMap = ConvertMap(gameTemplate.NormalMap);
                            LandmineMap = ConvertMap(gameTemplate.LandmineMap);
                            GeyserMap = ConvertMap(gameTemplate.GeyserMap);
                            RopeMap = ConvertMap(gameTemplate.RopeMap);
                            MinecartMap = ConvertMap(gameTemplate.MinecartMap);
                            PlatformMap = ConvertMap(gameTemplate.PlatformMap);
                            LavafallMap = ConvertMap(gameTemplate.LavafallMap);
                            PigronMap = ConvertMobMap(gameTemplate.PigronMap);
                            //gonna remake this at some point
                        }
                        else
                        {
                            args.Player.SendErrorMessage("dummas there isnt a template named like that");
                            return;
                        }
                        for (int i = 3; i <= NumOfPlayers + 2; i++)
                        {
                            string playername = args.Parameters[i];
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
                        }
                        isGaming = true;
                        TSPlayer.All.SendMessage("Game started, get ready!", Color.SeaShell);
                        break;
                    case "start":
                    case "stop":
                    case "end":
                    case "add":
                    case "remove":
                    case "score":
                    case "edit":
                        args.Player.SendErrorMessage("You can't use that command yet! You should first create a game");
                        args.Player.SendInfoMessage("/game template <templateName> <numOfPlayers> <list player names>");
                        break;
                    default:
                        args.Player.SendErrorMessage("That's not a valid command");
                        args.Player.SendInfoMessage("/game template <templateName> <numOfPlayers> <list player names>");
                        break;
                }
            }
            else if (isGaming && !isRound)
            {
                switch (args.Parameters[0])
                {
                    case "start":
                        if (args.Parameters.Count < 3)
                        {
                            args.Player.SendErrorMessage("not enough parameters!");
                            args.Player.SendInfoMessage("/game start <gimmickAmount> <list gimmicks> <map>");
                            return;
                        }
                        int GimmickAmount = Convert.ToInt32(args.Parameters[1]);
                        string[] GameTypes = new string[100];
                        if (args.Parameters.Count == 3)
                        {
                            GameTypes[0] = args.Parameters[1];
                            StartRound(args, GameTypes, args.Parameters[2]);
                        }
                        else if (args.Parameters.Count >= 3 && args.Parameters.Count == 3 + Convert.ToInt32(args.Parameters[1]))
                        {
                            for (int i = 0; i < GimmickAmount; i++)
                                GameTypes[i] = args.Parameters[i + 2];
                            StartRound(args, GameTypes, args.Parameters[GimmickAmount + 2], GimmickAmount);
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
                        isGaming = false;
                        RoundCounter = 0;
                        break;
                    case "add":
                        if (args.Parameters.Count != 2)
                        {
                            args.Player.SendErrorMessage("wrong amount parameters!");
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
                        NumOfPlayers++;
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
                            NumOfPlayers--;
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
                    NumOfPlayers++;
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
            NumOfPlayers++;
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
            NumOfPlayers--;
            TSPlayer.All.SendMessage($"{playername} has left the game!", Color.Red);
        }

        private int counter = 0;
        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.PlayerDeathV2)
            {
                using var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
                byte playerID = reader.ReadByte();

                var plr = TSPlayer.FindByNameOrID(Convert.ToString(playerID))[0];

                foreach (KeyValuePair<string, Playering> player in PlayerInfo)
                {
                    var plrOG = TSPlayer.FindByNameOrID(player.Key)[0];
                    if (plr == plrOG && player.Value.isAlive == true && player.Value.isIngame == true)
                    {
                        PlayerInfo[player.Key].isAlive = false;
                        PlayerInfo[player.Key].place = counter;
                        counter++;
                        //pretty pelase work
                        //yay it wokr!11!1!!
                        if (counter == NumOfPlayers - 1)
                            Commands.HandleCommand(TSPlayer.Server, CommandToEndRound);

                        else if (counter == NumOfPlayers)
                        {
                            PlayerInfoList = PlayerInfo
                               .OrderByDescending(entry => entry.Value.place)
                               .ToList();
                            if (NumOfPlayers == 2)
                            {
                                PlayerInfoList[0].Value.score += 1;
                                TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round!", Color.LimeGreen);
                            }
                            else if (NumOfPlayers >= 3 && NumOfPlayers <= 6)
                            {
                                PlayerInfoList[0].Value.score += 2;
                                PlayerInfoList[1].Value.score += 1;
                                TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round and {PlayerInfoList[1].Key} got second place!", Color.LimeGreen);
                            }
                            else if (NumOfPlayers >= 7 && NumOfPlayers <= 10)
                            {
                                PlayerInfoList[0].Value.score += 3;
                                PlayerInfoList[1].Value.score += 2;
                                PlayerInfoList[2].Value.score += 1;
                                TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round {PlayerInfoList[1].Key} got second place and {PlayerInfoList[2].Key} got third place!", Color.LimeGreen);
                            }
                            else if (NumOfPlayers >= 11 && NumOfPlayers <= 14)
                            {
                                PlayerInfoList[0].Value.score += 4;
                                PlayerInfoList[1].Value.score += 3;
                                PlayerInfoList[2].Value.score += 2;
                                PlayerInfoList[3].Value.score += 1;
                                TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round {PlayerInfoList[1].Key} got second place {PlayerInfoList[2].Key} got third place and {PlayerInfoList[3].Key} got fourth place!", Color.LimeGreen);
                            }
                            PlayerInfo = PlayerInfoList.ToDictionary(pair => pair.Key, pair => pair.Value);

                            AnnounceScore();
                            isRound = false;
                            ServerApi.Hooks.NetGetData.Deregister(pluginInstance, OnGetData);
                        }
                    }
                }
            }
        }

        private void GiveEveryoneItems(int itemID, int stack)
        {

            foreach (KeyValuePair<string, Playering> player in PlayerInfo)
            {
                if (player.Value.isIngame == true)
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

        private void SpawnMob(int npcID, int x, int y)
        {
            int npcIndex = NPC.NewNPC(null, x, y, npcID);
            if (npcIndex >= 0)
                NetMessage.SendData(23, -1, -1, null, npcIndex);
        }

        private void ChooseArena(string GameArena)
        {
            if (GameArena == "random" || GameArena == "r")
                GameArena = Convert.ToString(rnd.Next(8));

            switch (GameArena)
            {
                case "0":
                case "normal":
                    TSPlayer.All.SendMessage("[i:776] (Normal arena) [i:776]", Color.Cyan);
                    Commands.HandleCommand(TSPlayer.Server, NormalMap.MapCommand);
                    TpAndWebEveryone(NormalMap.tpposx, NormalMap.tpposy);
                    break;
                case "snow":
                    if (SnowMap.MapCommand == "emdy")
                        goto case "0";
                    TSPlayer.All.SendMessage("[i:593] (Snow arena) [i:593]", Color.White);
                    Commands.HandleCommand(TSPlayer.Server, SnowMap.MapCommand);
                    TpAndWebEveryone(SnowMap.tpposx, SnowMap.tpposy);
                    break;
                case "1":
                case "landmine":
                    if (LandmineMap.MapCommand == "emdy")
                        goto case "0";
                    TSPlayer.All.SendMessage("[i:937] (Landmine arena) [i:937]", Color.Green);
                    Commands.HandleCommand(TSPlayer.Server, LandmineMap.MapCommand);
                    TpAndWebEveryone(LandmineMap.tpposx, LandmineMap.tpposy);
                    break;
                case "2":
                case "geyser":
                    if (GeyserMap.MapCommand == "emdy")
                        goto case "0";
                    TSPlayer.All.SendMessage("[i:3722] (Geyser arena) [i:3722]", Color.Orange);
                    Commands.HandleCommand(TSPlayer.Server, GeyserMap.MapCommand);
                    TpAndWebEveryone(GeyserMap.tpposx, GeyserMap.tpposy);
                    break;
                case "3":
                case "rope":
                    if (RopeMap.MapCommand == "emdy")
                        goto case "0";
                    TSPlayer.All.SendMessage("[i:965] (Rope arena) [i:965]", Color.RosyBrown);
                    Commands.HandleCommand(TSPlayer.Server, RopeMap.MapCommand);
                    TpAndWebEveryone(RopeMap.tpposx, RopeMap.tpposy);
                    break;
                case "4":
                case "minecart":
                    if (MinecartMap.MapCommand == "emdy")
                        goto case "0";
                    TSPlayer.All.SendMessage("[i:2340] (Minecart arena) [i:2340]", Color.Gray);
                    Commands.HandleCommand(TSPlayer.Server, MinecartMap.MapCommand);
                    TpAndWebEveryone(MinecartMap.tpposx, MinecartMap.tpposy);
                    break;
                case "5":
                case "platform":
                    if (PlatformMap.MapCommand == "emdy")
                        goto case "0";
                    TSPlayer.All.SendMessage("[i:776] (Platform arena) [i:776]", Color.Cyan);
                    Commands.HandleCommand(TSPlayer.Server, PlatformMap.MapCommand);
                    TpAndWebEveryone(PlatformMap.tpposx, PlatformMap.tpposy);
                    break;
                case "6":
                case "lavafall":
                    if (LavafallMap.MapCommand == "emdy")
                        goto case "0";
                    TSPlayer.All.SendMessage("[i:207] (Lavafall arena) [i:207]", Color.OrangeRed);
                    Commands.HandleCommand(TSPlayer.Server, LavafallMap.MapCommand);
                    TpAndWebEveryone(LavafallMap.tpposx, LavafallMap.tpposy);
                    break;
                case "7":
                case "pigron":
                    if (PigronMap.MapCommand == "emdy")
                        goto case "0";
                    TSPlayer.All.SendMessage("[i:4613] (Pigron arena) [i:4613]", Color.DeepPink);
                    Commands.HandleCommand(TSPlayer.Server, PigronMap.MapCommand);
                    TpAndWebEveryone(PigronMap.tpposx, PigronMap.tpposy);
                    SpawnMob(PigronMap.MobID, PigronMap.Mobposx * 16, PigronMap.Mobposy * 16);
                    break;
                default:
                    TSPlayer.All.SendMessage("Invalid arena, putting normal", Color.DarkRed);
                    goto case "0";

            }
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

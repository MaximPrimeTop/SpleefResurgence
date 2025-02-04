﻿using IL.Terraria.GameContent.ObjectInteractions;
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

        private bool isGaming = false;

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

        private Dictionary<string, int[]> PlayerInfo = new(); // 0 - score, 1 - alive/dead 1/0 2 - place in the round
        List<KeyValuePair<string, int[]>> PlayerInfoList;

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
            foreach (KeyValuePair<string, int[]> player in PlayerInfo)
            {
                if (!isPlayerOnline(player.Key))
                {
                    args.Player.SendErrorMessage($"yo bro this {player.Key} he doesn't exist or some shit, idk bro either wait fro him to join back or restart the game tbh cus duh this guy is not here");
                    return;
                }
                var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                inventoryEdit.ClearPlayerEverything(plr);
                inventoryEdit.AddItem(plr, 0, 1, 776);
                inventoryEdit.AddItem(plr, 40, 1, 776);
                inventoryEdit.AddArmor(plr, 3, 158);

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
            ServerApi.Hooks.NetGetData.Register(pluginInstance, OnGetData);
            ChooseArena(GameArena);
            for (int i = 0; i < GimmickAmount; i++)
                ChooseGimmick(GameType[i]);
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
                    PlayerInfoList = PlayerInfo
                               .OrderByDescending(entry => entry.Value[0])
                               .ToList();
                    foreach (KeyValuePair<string, int[]> player in PlayerInfoList)
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
                    TSPlayer.All.SendMessage($"Game ended, after {RoundCounter} rounds {PlayerInfoList[0].Key} WON!!!!!!!!!", Color.MediumTurquoise);
                    RoundCounter = 0;
                }

                else if (args.Parameters[0] == "score" && isGaming) // /game score
                {
                    PlayerInfoList = PlayerInfo
                                    .OrderByDescending(entry => entry.Value[0])
                                    .ToList();
                    foreach (KeyValuePair<string, int[]> plr in PlayerInfoList)
                        TSPlayer.All.SendMessage($"{plr.Key} : {plr.Value[0]}", Color.Coral);
                }
                else
                {
                    args.Player.SendErrorMessage("erm no");
                    return;
                }
            }
            else if (args.Parameters.Count == 2 && args.Parameters[0] == "add" && isGaming) // /game add name
            {
                PlayerInfo.Add(args.Parameters[1], new int[] { 0, 0, 0});
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
                    StartRound(args, GameTypes, args.Parameters[GimmickAmount + 2], GimmickAmount);
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
                    SnowMap = ConvertMap(gameTemplate.SnowMap);
                    NormalMap = ConvertMap(gameTemplate.NormalMap);
                    LandmineMap = ConvertMap(gameTemplate.LandmineMap);
                    GeyserMap = ConvertMap(gameTemplate.GeyserMap);
                    RopeMap = ConvertMap(gameTemplate.RopeMap);
                    MinecartMap = ConvertMap(gameTemplate.MinecartMap);
                    PlatformMap = ConvertMap(gameTemplate.PlatformMap);
                    LavafallMap = ConvertMap(gameTemplate.LavafallMap);
                    PigronMap = ConvertMobMap(gameTemplate.PigronMap);

                }
                else
                {
                    args.Player.SendErrorMessage("dummas there isnt a template named like that");
                    return;
                }
                for (int i = 3; i <= NumOfPlayers + 2; i++)
                {
                    PlayerInfo.Add(args.Parameters[i], new int[] { 0, 0, 0 });
                }
                isGaming = true;
                TSPlayer.All.SendMessage("Game started, get ready!", Color.SeaShell);
            }
            else
            {
                args.Player.SendErrorMessage("erm no");
                return;
            }
        }

        private int counter = 0;
        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.PlayerDeathV2)
            {
                using var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
                byte playerID = reader.ReadByte();

                var plr = TSPlayer.FindByNameOrID(Convert.ToString(playerID))[0];

                foreach (KeyValuePair<string, int[]> player in PlayerInfo)
                {
                    var plrOG = TSPlayer.FindByNameOrID(player.Key)[0];
                    if (plr == plrOG && player.Value[1] == 1)
                    {
                        PlayerInfo[player.Key][1] = 0;
                        PlayerInfo[player.Key][2] = counter;
                        counter++;
                        //pretty pelase work

                        if (counter == NumOfPlayers - 1)
                            Commands.HandleCommand(TSPlayer.Server, CommandToEndRound);

                        else if (counter == NumOfPlayers)
                        {
                            PlayerInfoList = PlayerInfo
                               .OrderByDescending(entry => entry.Value[2])
                               .ToList();
                            if (NumOfPlayers == 2)
                            {
                                PlayerInfoList[0].Value[0] += 1;
                                TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round!", Color.LimeGreen);
                            }
                            else if (NumOfPlayers >= 3 && NumOfPlayers <= 6)
                            {
                                PlayerInfoList[0].Value[0] += 2;
                                PlayerInfoList[1].Value[0] += 1;
                                TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round and {PlayerInfoList[1].Key} got second place!", Color.LimeGreen);
                            }
                            else if (NumOfPlayers >= 7 && NumOfPlayers <= 10)
                            {
                                        
                                PlayerInfoList[1].Value[0] += 2;
                                PlayerInfoList[2].Value[0] += 1;
                                TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round {PlayerInfoList[1].Key} got second place and {PlayerInfoList[2].Key} got third place!", Color.LimeGreen);
                            }
                            else if (NumOfPlayers >= 11 && NumOfPlayers <= 14)
                            {
                                PlayerInfoList[0].Value[0] += 4;
                                PlayerInfoList[1].Value[0] += 3;
                                PlayerInfoList[2].Value[0] += 2;
                                PlayerInfoList[3].Value[0] += 1;
                                TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round {PlayerInfoList[1].Key} got second place {PlayerInfoList[2].Key} got third place and {PlayerInfoList[3].Key} got fourth place!", Color.LimeGreen);
                            }
                            PlayerInfo = PlayerInfoList.ToDictionary(pair => pair.Key, pair => pair.Value);
                            
                            PlayerInfoList = PlayerInfo
                               .OrderByDescending(entry => entry.Value[0])
                               .ToList();
                            foreach (KeyValuePair<string, int[]> plrr in PlayerInfoList)
                                TSPlayer.All.SendMessage($"{plrr.Key} : {plrr.Value[0]}", Color.Coral);
                            ServerApi.Hooks.NetGetData.Deregister(pluginInstance, OnGetData);
                        }
                    }
                }
            }
        }

        private void GiveEveryoneItems(int itemID, int stack)
        {
            foreach (KeyValuePair<string, int[]> player in PlayerInfo)
            {
                var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                int slot = inventoryEdit.FindNextFreeSlot(plr);
                inventoryEdit.AddItem(plr, slot, stack, itemID);
            }
        }

        private void GiveEveryoneArmor(int itemID)
        {
            foreach (KeyValuePair<string, int[]> player in PlayerInfo)
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

        private void SetEveryoneBuff(int BuffID, int time)
        {
            foreach (KeyValuePair<string, int[]> player in PlayerInfo)
            {
                var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                plr.SetBuff(BuffID, time);
            }
        }

        private void TpAndWebEveryone(int x1, int y1)
        {
            foreach (KeyValuePair<string, int[]> player in PlayerInfo)
            {
                var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                plr.Teleport(x1 * 16, y1 * 16);
                plr.SetBuff(BuffID.Webbed, 100);
                player.Value[1] = 1;
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

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
using System.Diagnostics;
using Terraria.Audio;
using IL.Terraria.GameContent.ItemDropRules;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using GetText.Loaders;
using Newtonsoft.Json;
using System.Collections;
using Steamworks;
using Microsoft.Xna.Framework.Input;
using Org.BouncyCastle.Asn1.X509;

namespace SpleefResurgence
{
    public class SpleefGame
    {
        public readonly int[] MusicBoxIDs = { 562, 1600, 564, 1601, 1596, 1602, 1603, 1604, 4077, 4079, 1597, 566, 1964, 1610, 568, 569, 570, 1598, 2742, 571, 573, 3237, 1605, 1608, 567, 572, 574,
            1599, 1607, 5112, 4979, 1606, 4985, 4990, 563, 1609, 3371, 3236, 3235, 1963, 1965, 3370, 3044, 3796, 3869, 4078, 4080, 4081, 4082, 4237, 4356, 4357, 4358, 4421, 4606, 4991, 4992, 5006,
            5014, 5015, 5016, 5017, 5018, 5019, 5020, 5021, 5022, 5023,5024, 5025, 5026, 5027, 5028, 5029, 5030, 5031, 5032, 5033, 5034, 5035, 5036, 5037, 5038, 5039, 5040, 5044, 5362, 565 };

        public readonly int[] OneBreakTiles = { 0, 1, 6, 7, 8, 9, 22, 39, 40, 45, 46, 47, 56, 57, 59, 118, 119, 120, 121, 140, 145, 146, 147, 148, 150, 151, 152, 153, 154, 155, 156, 160, 161, 163,
            164, 166, 167, 168, 169, 170, 175, 176, 177, 189, 196, 197, 200, 204, 206, 230, 262, 263, 264, 265, 266, 267, 268, 273, 274, 284, 325, 326, 327, 346, 347, 348, 367, 368, 371, 396, 397,
            398, 399, 400, 401, 402, 403, 407, 408, 409, 415, 416, 417, 418, 472, 473, 478, 507, 508, 563, 659, 666, 667, 669, 670, 671, 672, 673, 674, 675, 676, 687, 688, 689, 690, 691 };

        public readonly int[] OneBreakTilesPlatforms = { 30, 38, 54, 157, 158, 159, 188, 190, 191, 193, 195, 202, 311, 321, 322, 350, 357, 369, 370, 474, 479, 498, 500, 501, 502, 503, 562, 635 };

        public readonly int[] TwoBreakTiles = { 25, 107, 117, 203, 221 };

        public readonly int[] TwoBreakTilesPlatforms = { 41, 43, 44 };

        public readonly int[] UnplaceableTiles = { 63, 64, 65, 66, 67, 68, 162, 192, 668 };

        public List<Gimmick> Gimmicks;

        public void PutGimmicks()
        {
            Gimmicks = new List<Gimmick>
            {
            new("normal", "[i:776] [c/00FFFF:Normal round] [i:776]",
                () => { }),

            new("boulder", "[i:540] [c/898989:Boulder round] [i:540]",
                () => { Boulders("boulder"); }),

            new("bouncy", "[i:5383] [c/ff3c9b:Bouncy boulder round] [i:5383]",
                () => { Boulders("bouncy"); }),

            new("cactus", "[i:4390] [c/008000:Rolling cactus round] [i:4390]",
                () => { Boulders("cactus"); }),

            new("cloud", "[i:53] [c/FFFFFF:Cloud in a bottle round] [i:53]",
                () => { GiveEveryoneArmor(53); }),

            new("tsunami", "[i:3201] [c/00bfff:Tsunami round] [i:3201]",
                () => { GiveEveryoneArmor(3201); }),

            new("blizzard", "[i:987] [c/FFFFFF:Blizzard in a bottle round] [i:987]",
                () => { GiveEveryoneArmor(987); }),

            new("sandstorm", "[i:857] [c/FFFF00:Sandstorm in a bottle round] [i:857]",
                () => { GiveEveryoneArmor(857); }),

            new("fart", "[i:1724] [c/00FF00:Fart in a jar round] [i:1724]",
                () => { GiveEveryoneArmor(1724); }),

            new("portal", "[i:3384] [c/FFFFFF:Portal round] [i:3384]",
                () => { GiveEveryoneItems(3384, 1); }),

            new("bombfish", "[i:3196] [c/A9A9A9:Bomb fish round] [i:3196]",
                () => { GiveEveryoneItems(3196, 50); }),

            new("rocket15", "[i:759] [c/FFA500:Rocket round (15 rockets)] [i:759]",
                () => { GiveEveryoneItems(759, 1); GiveEveryoneItems(772, 15, 54); }),

            new("rocket100", "[i:759] [c/FFA500:Rocket round (100 rockets)] [i:759]",
                () => { GiveEveryoneItems(759, 1); GiveEveryoneItems(772, 100, 54); }),

            new("icerod", "[i:496] [c/00bfff:Ice rod round] [i:496]",
                () => { GiveEveryoneItems(496, 1); }),

            new("soc", "[i:3097] [c/FF0000:Shield of Cthulhu round] [i:3097]",
                () => { GiveEveryoneArmor(3097); }),

            new("balloon", "[i:159] [c/FF0000:Balloon round] [i:159]",
                () => { GiveEveryoneArmor(159); }),

            new("insignia", "[i:4989] [c/FFC0CB:Soaring insignia round] [i:4989]",
                () => { GiveEveryoneArmor(4989); }),

            new("longrange", "[i:407][i:1923][i:2215] [c/bc8f8f:Long range round] [i:2215][i:1923][i:407]",
                () => { GiveEveryoneArmor(407); GiveEveryoneArmor(1923); GiveEveryoneArmor(2215); }),

            new("flipper", "[i:187] [c/0000FF:Flipper round] [i:187]",
                () => { GiveEveryoneArmor(187); }),

            new("claws", "[i:953] [c/A9A9A9:Climbing claws round] [i:953]",
                () => { GiveEveryoneArmor(953); }),

            new("stool", "[i:4341] [c/bc8f8f:Step stool round] [i:4341]",
                () => { GiveEveryoneArmor(4341); }),

            new("hermes", "[i:54] [c/00FF00:Hermes boots round] [i:54]",
                () => { GiveEveryoneArmor(54); }),

            new("shinystone", "[i:3337] [c/BC8F8F:Shiny Stone round] [i:3337]",
                () => { GiveEveryoneArmor(3337); }),

            new("builder", "[i:2325] [c/bc8f8f:Builder round] [i:2325]",
                () => { SetEveryoneBuff(BuffID.Builder, 60000); }),

            new("mining", "[i:2322] [c/008080:Mining round] [i:2322]",
                () => { SetEveryoneBuff(BuffID.Mining, 60000); }),

            new("panic", "[i:1290] [c/FF0000:Panic round] [i:1290]",
                () => { SetEveryoneBuff(BuffID.Panic, 60000); }),

            new("slime", "[i:2430] [c/00bfff:Slime saddle round] [i:2430]",
                () => { GiveEveryoneMiscEquips(2430, 3); }),

            new("pogo", "[i:4791] [c/FFC0CB:Pogo stick round] [i:4791]",
                () => { GiveEveryoneMiscEquips(4791, 3); }),

            new("golf", "[i:4264] [c/FFFFFF:Golf cart round] [i:4264]",
                () => { GiveEveryoneMiscEquips(4264, 3); }),

            new("fleshknuckles", "[i:3016] [c/FF0000:Flesh knuckles round] [i:3016]",
                () => { GiveEveryoneArmor(3016); }),

            new("feralbite", "[i:1274] [c/000000:Feral bite round AHAHAHAHAHAH I HATE YOU AHHAHAHAHAHAHAHAHAHAHAHAHAHHA!!!!!!!!!!!!!!!!SUFFFERRRRRRRR] [i:1274]",
                () => { SetEveryoneBuff(BuffID.Rabies, 60000); }),
            };
        }

        public static PluginSettings Config => PluginSettings.Config;
        private readonly Spleef pluginInstance;
        private readonly InventoryEdit inventoryEdit;
        private readonly SpleefCoin spleefCoin;
        private readonly SpleefUserSettings spleefSettings;

        public SpleefGame(Spleef plugin, SpleefCoin spleefCoin, InventoryEdit inventoryEdit, SpleefUserSettings spleefSettings)
        {
            this.pluginInstance = plugin;
            this.inventoryEdit = inventoryEdit;
            this.spleefCoin = spleefCoin;
            this.spleefSettings = spleefSettings;
            PutGimmicks();
        }

        public class Gimmick
        {
            public string Name { get; }
            public string Statusmsg { get; }
            public Action Action { get; }

            public Gimmick(string name, string statusmsg, Action action)
            {
                Name = name;
                Statusmsg = statusmsg;
                Action = action;
            }
        }


        private bool isGaming = false;
        private bool isRound = false;
        private bool isJoinable = false;
        private bool isBettable = false;

        private string CommandToStartRound;
        private string CommandToEndRound;
        private int PlayerCount;
        private int RoundCounter;
        private Item MusicBox = new();

        private Random rnd = new();
        private class Playering
        {
            public int score;
            public bool isAlive;
            public int place;
            public string accName;
            public bool isIngame;

            public int GetPlace (int PlayerCount)
            {
                return PlayerCount - place;
            }
        }
        private Dictionary<string, Playering> PlayerInfo = new();
        private List<Map> MapsInfo = new();
        List<KeyValuePair<string, Playering>> PlayerInfoList;

        public bool isPlayerOnline(string playername)
        {
            var players = TSPlayer.FindByNameOrID(playername);
            if (players == null || players.Count == 0)
            {
                return false;
            }
            return true;
        }

        public void CountPlayers()
        {
            PlayerCount = 0;
            foreach (Playering plr in PlayerInfo.Values)
            {
                if (plr.isIngame == true)
                    PlayerCount++;
            }
        }

        private void StartRound(CommandArgs args, string[] GameType, Map GameArena, int GimmickAmount)
        {
            CountPlayers();

            if (PlayerCount <= 1)
            {
                args.Player.SendErrorMessage($"Can't start a round with this amount of players! Make sure there are at least 2 players");
                return;
            }

            ClearEveryoneInventory();
            GiveEveryoneItems(ItemID.CobaltPickaxe, 1, 0);
            GiveEveryoneItems(ItemID.Binoculars, 1, 9);
            GiveEveryoneItems(ItemID.CobaltPickaxe, 1, 40);
            GiveEveryoneArmor(ItemID.Fedora, 18);
            GiveEveryoneArmor(ItemID.SolarDye, 19);
            SetEveryoneBuff(BuffID.Honey, 1000000);
            SetEveryoneBuff(BuffID.Shine, 1000000);
            SetEveryoneBuff(BuffID.NightOwl, 1000000);
            SetEveryoneBuff(BuffID.HeartLamp, 1000000);
            SetEveryoneBuff(BuffID.Campfire, 1000000);

            counter = 0;
            RoundCounter++;
            Commands.HandleCommand(TSPlayer.Server, CommandToStartRound);
            statusRound = $"[c/8FBC8F:Round {RoundCounter}]\n";
            TSPlayer.All.SendMessage($"Round {RoundCounter} started", Color.DarkSeaGreen);
            for (int i = 0; i < GimmickAmount; i++)
                if (GameType[i] == "random" || GameType[i] == "r")
                    GameType[i] = Convert.ToString(rnd.Next(Gimmicks.Count - 2));
            isRound = true;
            ServerApi.Hooks.NetGetData.Register(pluginInstance, OnGetData);
            ChooseArena(GameArena);
            for (int i = 0; i < GimmickAmount; i++)
                ChooseGimmick(GameType[i]);


            GiveEveryoneArmor(0, 18);
            GiveEveryoneArmor(0, 19);
            MusicBox.SetDefaults(MusicBoxIDs[rnd.Next(MusicBoxIDs.Length)]);
            GiveEveryoneArmor(MusicBox.netID);
            string SongName = MusicBox.Name.Substring(MusicBox.Name.IndexOf('(') + 1, MusicBox.Name.IndexOf(')') - MusicBox.Name.IndexOf('(') - 1);
            if (MusicBox.Name.Split(' ')[0] == "Otherworldly")
                SongName = "Otherworldly " + SongName;
            statusRound += $"[i:{MusicBox.netID}] [c/FFB6C1:Playing {SongName}] [i:{MusicBox.netID}]";
            TSPlayer.All.SendMessage($"[i:{MusicBox.netID}] Playing {SongName} [i:{MusicBox.netID}]", Color.LightPink);
            UpdateScore();
        }
        //im tired brah
        private void AnnounceScore()
        {
            TSPlayer.All.SendMessage(statusScore, Color.Coral);
        }

        public void UpdateScore()
        {
            PlayerInfoList = PlayerInfo
                .OrderByDescending(entry => entry.Value.score)
                .ToList();
            statusScore = "[c/FF1493:Game score:]\n";
            foreach (KeyValuePair<string, Playering> plr in PlayerInfoList)
            {
                if (plr.Value.isIngame == true)
                {
                    if (plr.Value.isAlive)
                        statusScore += $"[c/FF8559:{plr.Key} : {plr.Value.score}] [i:58]\n";
                    else
                    {
                        if (plr.Value.GetPlace(PlayerCount) == 1)
                            statusScore += $"[c/FF8559:{plr.Key} : {plr.Value.score}] [i:4601]\n";
                        else if (plr.Value.GetPlace(PlayerCount) == 2)
                            statusScore += $"[c/FF8559:{plr.Key} : {plr.Value.score}] [i:4600]\n";
                        else if (plr.Value.GetPlace(PlayerCount) == 3)
                            statusScore += $"[c/FF8559:{plr.Key} : {plr.Value.score}] [i:4599]\n";
                        else
                            statusScore += $"[c/FF8559:{plr.Key} : {plr.Value.score}] [i:321]\n";
                    }
                }
                else
                    statusScore += $"[c/898989:{plr.Key} : {plr.Value.score}]\n";
            }
        }

        private void ListStuff(CommandArgs args)
        {
            if (args.Parameters.Count < 2 || args.Parameters.Count > 3)
            {
                args.Player.SendErrorMessage("wrong amount of parameters!");
                args.Player.SendInfoMessage("/game list (gimmick, template, map <templatename>)");
                return;
            }
            switch (args.Parameters[1])
            {
                case "gimmick":
                    List<string> gimmickList = Gimmicks.Select((g, index) => $"{index} - {g.Name}").ToList();
                    string gimmickNames = string.Join(", ", gimmickList);
                    args.Player.SendMessage("List of all gimmicks:", Color.OrangeRed);
                    args.Player.SendInfoMessage(gimmickNames);
                    break;
                case "template":
                    List<string> templateList = Config.GameTemplates.Select((t) => t.Name).ToList();
                    string templateNames = string.Join(", ", templateList);
                    args.Player.SendMessage("List of all templates:", Color.OrangeRed);
                    args.Player.SendInfoMessage(templateNames);
                    break;
                case "map":
                    if (args.Parameters.Count != 3)
                    {
                        args.Player.SendErrorMessage("wrong amount of parameters!");
                        args.Player.SendErrorMessage("/game list map <templatename>");
                        args.Player.SendInfoMessage("Check all the templates with /game list template");
                        return;
                    }
                    var template = PluginSettings.Config.GameTemplates.FirstOrDefault(c => c.Name == args.Parameters[2]);
                    if (template == null)
                    {
                        args.Player.SendErrorMessage("That's not a valid template!");
                        args.Player.SendInfoMessage("Check all the templates with /game list template");
                        return;
                    }
                    List<string> mapList = template.Maps.Select((m, index) => $"{index} - {m.MapName}").ToList();
                    string mapNames = string.Join(", ", mapList);
                    args.Player.SendMessage($"List of all maps for {args.Parameters[2]}:", Color.OrangeRed);
                    args.Player.SendInfoMessage(mapNames);
                    break;
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
                args.Player.SendInfoMessage("/game list (gimmick, template, map <templatename>)");
                args.Player.SendInfoMessage("/game template <templateName> [isJoinable] [isBettable] - creates a new game. By default it's joinable and people can't place bets(bets aren't implemented yet).");
                args.Player.SendMessage("these commands only work if there's a game going on!", Color.OrangeRed);
                args.Player.SendInfoMessage("/game start <gimmickAmount> <list gimmicks> <map> or /game start <gimmick> <map> or  - starts a round");
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
                        if (args.Parameters.Count < 2 || args.Parameters.Count > 4)
                        {
                            args.Player.SendErrorMessage("wrong amount of parameters!");
                            args.Player.SendErrorMessage("/game template <templateName> [isJoinable] [isBettable]");
                            return;
                        }
                        string name = args.Parameters[1];
                        var gameTemplate = PluginSettings.Config.GameTemplates.FirstOrDefault(c => c.Name == name);
                        if (gameTemplate == null)
                        {
                            args.Player.SendErrorMessage("dummas there isnt a template named like that");
                            return;
                        }
                        
                        CommandToStartRound = gameTemplate.LavariseCommand;
                        CommandToEndRound = gameTemplate.FillCommand;
                        foreach (var map in gameTemplate.Maps)
                            MapsInfo.Add(map);
                        if (args.Parameters.Count > 2)
                        {
                            switch (args.Parameters[2])
                            {
                                case "true":
                                case "1":
                                case "enable":
                                    isJoinable = true;
                                    break;
                                case "false":
                                case "0":
                                case "disable":
                                    isJoinable = false;
                                    break;
                                default:
                                    args.Player.SendErrorMessage($"zzzzzzzzz yeah idk what you meant by {args.Parameters[2]}");
                                    return;
                            }
                            if (args.Parameters.Count > 3)
                            {
                                switch (args.Parameters[3])
                                {
                                    case "true":
                                    case "1":
                                    case "enable":
                                        isBettable = true;
                                        break;
                                    case "false":
                                    case "0":
                                    case "disable":
                                        isBettable = false;
                                        break;
                                    default:
                                        args.Player.SendErrorMessage($"zzzzzzzzz yeah idk what you meant by {args.Parameters[2]}");
                                        return;
                                }
                            }
                            else
                                isBettable = false;
                        }
                        else
                        {
                            isJoinable = true;
                            isBettable = false;
                        }

                        isGaming = true;
                        if (isJoinable)
                            TSPlayer.All.SendMessage("Game started! Join with /j", Color.SeaShell);
                        else
                            TSPlayer.All.SendMessage("Game started!", Color.SeaShell);
                        if (isBettable)
                            TSPlayer.All.SendMessage("Start placing your bets! Bet with /bet username amount", Color.SeaShell);

                        ServerApi.Hooks.ServerLeave.Register(pluginInstance, OnPlayerLeave);
                        ServerApi.Hooks.GameUpdate.Register(pluginInstance, OnWorldUpdate);
                        UpdateScore();
                        return;
                    case "list":
                        ListStuff(args);
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
                    case "list":
                        ListStuff(args);
                        break;
                    case "start":
                        if (args.Parameters.Count < 3)
                        {
                            args.Player.SendErrorMessage("not enough parameters!");
                            args.Player.SendInfoMessage("/game start <gimmickAmount> <list gimmicks> <map>");
                            return;
                        }
                        int GimmickAmount = 1;
                        string mapname;
                        string[] GameTypes = new string[100];
                        if (args.Parameters.Count > 3)
                        {
                            GimmickAmount = Convert.ToInt32(args.Parameters[1]);
                            mapname = args.Parameters[GimmickAmount + 2];
                            for (int i = 0; i < GimmickAmount; i++)
                                GameTypes[i] = args.Parameters[i + 2];
                            // /game start <gimmickAmount> <list gimmicks> <map>
                        }
                        else
                        {
                            GameTypes[0] = args.Parameters[1];
                            mapname = args.Parameters[2];
                            // /game start <gimmick> <map>
                        }

                        Map map;
                        if (mapname == "r" || mapname == "random")
                            map = MapsInfo[rnd.Next(MapsInfo.Count)];
                        else if (int.TryParse(mapname, out int n) && n <= MapsInfo.Count && n >= 0)
                            map = MapsInfo[n];
                        else
                            map = MapsInfo.FirstOrDefault(c => c.MapName == mapname);

                        if (map == null)
                        {
                            List<string> mapList = MapsInfo.Select((c, index) => $"{index} - {c.MapName}").ToList();
                            string mapNames = string.Join(", ", mapList);
                            args.Player.SendErrorMessage("that is not a valid map!");
                            args.Player.SendInfoMessage($"Here are all valid maps: {mapNames}");
                            return;
                        }
                        StartRound(args, GameTypes, map, GimmickAmount);
                        if (isBettable)
                        {
                            TSPlayer.All.SendMessage("All bets are closed now!", Color.SeaShell);
                            isBettable = false;
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
                        UpdateScore();
                        ServerApi.Hooks.ServerLeave.Deregister(pluginInstance, OnPlayerLeave);
                        ServerApi.Hooks.GameUpdate.Deregister(pluginInstance, OnWorldUpdate);
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
                        
                        if (PlayerInfo.ContainsKey(playername))
                        {
                            Playering playerToAddBack = PlayerInfo[playername];

                            if (playerToAddBack.isIngame == true)
                            {
                                args.Player.SendErrorMessage("They are already in the game!");
                                return;
                            }

                            playerToAddBack.isIngame = true;
                            playerToAddBack.place = -100;
                            TSPlayer.All.SendMessage($"{playername} has been addded back into the game!", Color.Green);
                            UpdateScore();
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
                        TSPlayer.All.SendMessage($"{playername} has been added to the game!", Color.Green);
                        UpdateScore();
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
                            PlayerCount--;
                        PlayerInfo.Remove(name);
                        args.Player.SendSuccessMessage($"removed {name} from the game and awarded {points} Spleef Coins!");
                        TSPlayer.All.SendMessage($"{name} has been completely removed from the game!", Color.Red);
                        UpdateScore();
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
                    case "list":
                        ListStuff(args);
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
        /*
        private class Bet
        {
            string Gambler;
            int Amount;
            string Dissapointment;
            public Bet(string name, int amount, string name2)
            {
                Gambler = name;
                Amount = amount;
                Dissapointment = name2;
            }
        }

        List<Bet> Bets = new();

        public void Betting (CommandArgs args) // /bet username amount
        {
            string gamblerName = args.Player.Account.Name;
            int amount = Convert.ToInt32(args.Parameters[1]);
            string fighterName = args.Parameters[0];
            if (isBettable && isRoundBettable)
            {
                Bets.Add(new Bet(gamblerName, amount, fighterName));
            }
        }

        public void Challenge (CommandArgs args) // /challenge username amount
        {

        }
        */
        public void JoinGame (CommandArgs args)
        {
            if (!isGaming)
            {
                args.Player.SendErrorMessage("There is no game to join!");
                return;
            }

            if (!isJoinable)
            {
                args.Player.SendErrorMessage("You can't join this game!");
                return;
            }

            string playername = args.Player.Name;

            if (PlayerInfo.ContainsKey(playername))
            {
                Playering playerToAdd = PlayerInfo[playername];
                
                if (playerToAdd.isIngame == true)
                {
                    args.Player.SendErrorMessage("You are already in the game!");
                    return;
                }

                playerToAdd.isIngame = true;
                playerToAdd.place = -100;
                TSPlayer.All.SendMessage($"{playername} has joined back into the game!", Color.Green);
                UpdateScore();
                return;
            }    
            
            Playering newPlayering = new()
            {
                score = 0,
                isAlive = false,
                place = -100,
                accName = args.Player.Account.Name,
                isIngame = true
            };
            PlayerInfo.Add(playername, newPlayering);
            TSPlayer.All.SendMessage($"{playername} has joined the game!", Color.Green);
            UpdateScore();
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

            if (isRound && PlayerInfo[playername].isAlive)
            {
                PlayerInfo[playername].isAlive = false;
                PlayerInfo[playername].place = counter;
                PlayerInfo[playername].isIngame = false;
                counter++;
                TSPlayer.All.SendMessage($"{playername} has left the game midround! What a stinker...", Color.Red);
                CheckRound(counter);
                UpdateScore();
                return;
            }
            
            playerToRemove = PlayerInfo[playername];
            playerToRemove.isIngame = false;
            TSPlayer.All.SendMessage($"{playername} has left the game!", Color.Red);
            UpdateScore();
        }

        public void CheckScore(CommandArgs args)
        {
            if (!isGaming)
            {
                args.Player.SendErrorMessage("There is no game active!"); //just in case
                return;
            }
            UpdateScore();
            args.Player.SendInfoMessage(statusScore);
        }

        private int counter = 0;

        private Stopwatch suicideTimer = new();

        private void CheckRound(int counter)
        {
            if (counter == PlayerCount - 1)
            {
                suicideTimer.Restart();
                Commands.HandleCommand(TSPlayer.Server, CommandToEndRound);
            }

            else if (counter == PlayerCount)
            {
                bool isSuicide = false;

                suicideTimer.Stop();
                if (suicideTimer.Elapsed.TotalSeconds < 3)
                    isSuicide = true;
                PlayerInfoList = PlayerInfo
                   .OrderByDescending(entry => entry.Value.isIngame)
                   .ThenByDescending(entry => entry.Value.place)
                   .ToList();
                if (!isSuicide)
                {
                    if (PlayerCount == 2)
                    {
                        PlayerInfoList[0].Value.score += 1;
                        TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round!", Color.LimeGreen);
                    }
                    else if (PlayerCount >= 3 && PlayerCount <= 6)
                    {
                        PlayerInfoList[0].Value.score += 2;
                        PlayerInfoList[1].Value.score += 1;
                        TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round and {PlayerInfoList[1].Key} got second place!", Color.LimeGreen);
                    }
                    else if (PlayerCount >= 7 && PlayerCount <= 9)
                    {
                        PlayerInfoList[0].Value.score += 3;
                        PlayerInfoList[1].Value.score += 2;
                        PlayerInfoList[2].Value.score += 1;
                        TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round {PlayerInfoList[1].Key} got second place and {PlayerInfoList[2].Key} got third place!", Color.LimeGreen);
                    }
                    else if (PlayerCount >= 10 && PlayerCount <= 13)
                    {
                        PlayerInfoList[0].Value.score += 4;
                        PlayerInfoList[1].Value.score += 3;
                        PlayerInfoList[2].Value.score += 2;
                        PlayerInfoList[3].Value.score += 1;
                        TSPlayer.All.SendMessage($"Round ended! {PlayerInfoList[0].Key} won this round {PlayerInfoList[1].Key} got second place {PlayerInfoList[2].Key} got third place and {PlayerInfoList[3].Key} got fourth place!", Color.LimeGreen);
                    }
                }
                else
                {
                    if (PlayerCount == 2)
                    {
                        TSPlayer.All.SendMessage($"Round ended! It's a suicide so no one gets points!", Color.DarkRed);
                    }
                    else if (PlayerCount >= 3 && PlayerCount <= 6)
                    {
                        PlayerInfoList[0].Value.score += 1;
                        PlayerInfoList[1].Value.score += 1;
                        TSPlayer.All.SendMessage($"Round ended! It's a suicide so {PlayerInfoList[0].Key} and {PlayerInfoList[1].Key} both get 1 point!", Color.DarkRed);
                    }
                    else if (PlayerCount >= 7 && PlayerCount <= 9)
                    {
                        PlayerInfoList[0].Value.score += 2;
                        PlayerInfoList[1].Value.score += 2;
                        PlayerInfoList[2].Value.score += 1;
                        TSPlayer.All.SendMessage($"Round ended! It's a suicide so {PlayerInfoList[0].Key} and {PlayerInfoList[1].Key} get 2 points and {PlayerInfoList[2].Key} got third place!", Color.LimeGreen);
                    }
                    else if (PlayerCount >= 10 && PlayerCount <= 13)
                    {
                        PlayerInfoList[0].Value.score += 3;
                        PlayerInfoList[1].Value.score += 3;
                        PlayerInfoList[2].Value.score += 2;
                        PlayerInfoList[3].Value.score += 1;
                        TSPlayer.All.SendMessage($"Round ended! It's a suicide so {PlayerInfoList[0].Key} and {PlayerInfoList[1].Key} get 3 points {PlayerInfoList[2].Key} got third place and {PlayerInfoList[3].Key} got fourth place!", Color.LimeGreen);
                    }
                }
                PlayerInfo = PlayerInfoList.ToDictionary(pair => pair.Key, pair => pair.Value);

                TSPlayer.All.SendMessage($"You can join the game by typing /j", Color.LightPink);
                TSPlayer.All.SendMessage($"If you want to leave the game type /l", Color.LightPink);
                isRound = false;
                ServerApi.Hooks.NetGetData.Deregister(pluginInstance, OnGetData);
                UpdateScore();
            }
        }

        private string statusScore;
        private string statusRound;
        private const string thethingy = "\n\n\n\n\n\n\n\n\n\n\n\n";

        private void OnWorldUpdate(EventArgs args)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Active && player.Account.Name != null)
                {
                    PlayerSettings settings = spleefSettings.GetSettings(player.Account.Name);
                    if (settings.ShowScore)
                    {
                         if (settings.ShowLavarise)
                            player.SendData(PacketTypes.Status, thethingy + Spleef.statusLavariseTime + "\n\n" + statusScore + "\n" + statusRound, number2: 1);
                         else
                            player.SendData(PacketTypes.Status, thethingy + statusScore + "\n" + statusRound, number2: 1);
                    }
                    else if (settings.ShowLavarise)
                        player.SendData(PacketTypes.Status, thethingy + Spleef.statusLavariseTime, number2: 1);
                }
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
                            inventoryEdit.AddArmor(plr, 4, MusicBox.netID);
                            CheckRound(counter);
                        }
                    }
                }
                UpdateScore();
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
                    if (plr == plrOG)
                    {
                        if (player.Value.isAlive)
                        {
                            PlayerInfo[player.Key].isAlive = false;
                            PlayerInfo[player.Key].place = counter;
                            PlayerInfo[player.Key].isIngame = false;
                            counter++;
                            TSPlayer.All.SendMessage($"{player.Key} has left the game midround! What a stinker...", Color.LightYellow);
                            CheckRound(counter);
                        }
                        else
                        {
                            PlayerInfo[player.Key].isIngame = false;
                            TSPlayer.All.SendMessage($"{player.Key} has left the game midround! But he died so it's alright.", Color.LightYellow);
                        }
                    }   
                }
            }
            UpdateScore();
        }


        private void GiveEveryoneItems(int itemID, int stack, int slot = -1)
        {
            foreach (KeyValuePair<string, Playering> player in PlayerInfo)
            {
                if (player.Value.isIngame)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    if (slot == -1)
                        slot = inventoryEdit.FindNextFreeSlot(plr);
                    inventoryEdit.AddItem(plr, slot, stack, itemID);
                }
            }
        }

        private void GiveEveryoneMiscEquips(int itemID, int slot)
        {
            foreach (KeyValuePair<string, Playering> player in PlayerInfo)
            {
                if (player.Value.isIngame)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    inventoryEdit.AddMiscEquip(plr, slot, itemID);
                }
            }
        }

        private void ClearEveryoneInventory()
        {
            foreach (KeyValuePair<string, Playering> player in PlayerInfo)
            {
                if (player.Value.isIngame)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    inventoryEdit.ClearPlayerEverything(plr);
                }
            }
        }

        private void GiveEveryoneArmor(int itemID, int slot = -1)
        {
            Commands.HandleCommand(TSPlayer.Server, "/gamemode master");
            foreach (KeyValuePair<string, Playering> player in PlayerInfo)
            {
                if (player.Value.isIngame == true)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                    if (slot == -1)
                        slot = inventoryEdit.FindNextFreeAccessorySlot(plr);
                    if (slot == -1)
                    {
                        slot = inventoryEdit.FindNextFreeSlot(plr);
                        inventoryEdit.AddItem(plr, slot, 1, itemID);
                    }
                    else
                        inventoryEdit.AddArmor(plr, slot, itemID);
                }
            }
            Commands.HandleCommand(TSPlayer.Server, "/gamemode normal");
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
                    statusRound += "[i:776] [c/00FFFF:Normal arena] [i:776]\n";
                    TSPlayer.All.SendMessage("[i:776] Normal arena [i:776]", Color.Cyan);
                    break;
                case "gravedigger":
                    statusRound += "[i:4711] [c/FFFFFF:Gravedigger arena] [i:4711]\n";
                    TSPlayer.All.SendMessage("[i:4711] Gravedigger arena [i:4711]", Color.White);
                    GiveEveryoneItems(4711, 1);
                    break;
                case "landmine":
                    statusRound += "[i:937] [c/00FF00:Landmine arena] [i:937]\n";
                    TSPlayer.All.SendMessage("[i:937] Landmine arena [i:937]", Color.Green);
                    break;
                case "geyser":
                    statusRound += "[i:3722] [c/FFA500:Geyser arena] [i:3722]\n";
                    TSPlayer.All.SendMessage("[i:3722] Geyser arena [i:3722]", Color.Orange);
                    break;
                case "rope":
                    statusRound += "[i:965] [c/bc8f8f:Rope arena] [i:965]\n";
                    TSPlayer.All.SendMessage("[i:965] Rope arena [i:965]", Color.RosyBrown);
                    break;
                case "minecart":
                    statusRound += "[i:2340] [c/808080:Minecart arena] [i:2340]\n";
                    TSPlayer.All.SendMessage("[i:2340] Minecart arena [i:2340]", Color.Gray);
                    break;
                case "platform":
                    statusRound += "[i:776] [c/00FFFF:Platform arena] [i:776]\n";
                    TSPlayer.All.SendMessage("[i:776] Platform arena [i:776]", Color.Cyan);
                    break;
                case "lavafall":
                    statusRound += "[i:207] [c/FF4500:Lavafall arena] [i:207]\n";
                    TSPlayer.All.SendMessage("[i:207] Lavafall arena [i:207]", Color.OrangeRed);
                    break;
                case "pigron":
                    statusRound += "[i:4613] [c/ff1493:Pigron arena] [i:4613]\n";
                    TSPlayer.All.SendMessage("[i:4613] Pigron arena [i:4613]", Color.DeepPink);
                    break;
                case "sand":
                    statusRound += "[i:169] [c/FFFF00:Sand arena] [i:169]\n";
                    TSPlayer.All.SendMessage("[i:169] Sand arena [i:169]", Color.Yellow);
                    break;
                case "meteor":
                    statusRound += "[i:116] [c/bc8f8f:Meteor arena] [i:116]\n";
                    TSPlayer.All.SendMessage("[i:116] Meteor arena [i:116]", Color.RosyBrown);
                    break;
                case "honey":
                    statusRound += "[i:1125] [c/FFFF00:Honey arena] [i:1125]\n";
                    TSPlayer.All.SendMessage("[i:1125] Honey arena [i:1125]", Color.Yellow);
                    break;
                default:
                    statusRound += $"[i:776] [c/00FFFF:{GameArena.MapName} arena] [i:776]\n";
                    TSPlayer.All.SendMessage($"[i:776] {GameArena.MapName} arena [i:776]", Color.Cyan);
                    break;
            }
            Commands.HandleCommand(TSPlayer.Server, GameArena.MapCommand);
            TpAndWebEveryone(GameArena.tpposx, GameArena.tpposy);
            if (GameArena.Items.Any())
            {
                foreach (var item in GameArena.Items)
                {
                    if (item.Type == "inventory")
                        GiveEveryoneItems(item.ID, item.Stack, item.Slot);
                    else if (item.Type == "armor")
                        GiveEveryoneArmor(item.ID, item.Slot);
                    else if (item.Type == "misc")
                        GiveEveryoneMiscEquips(item.ID, item.Slot);
                }
            }
            if (GameArena.Buffs.Any())
            {
                foreach (var buff in GameArena.Buffs)
                    SetEveryoneBuff(buff.ID, buff.TimeInSeconds * 60);
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
            Gimmick gimmick;
            if (int.TryParse(GameType, out int n))
                gimmick = Gimmicks[n];
            
            else if ((gimmick = Gimmicks.FirstOrDefault(g => g.Name == GameType)) == null)
            {
                TSPlayer.All.SendMessage("Invalid gimmick, putting normal", Color.DarkRed);
                gimmick = Gimmicks[0];
            }

            statusRound += $"{gimmick.Statusmsg}\n";
            TSPlayer.All.SendInfoMessage(gimmick.Statusmsg);
            gimmick.Action();
        }
    }
}


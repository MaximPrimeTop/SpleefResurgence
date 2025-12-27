using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using TShockAPI.Hooks;

namespace SpleefResurgence
{
    public class SpleefGame
    {
        public readonly int[] MusicBoxIDs = { 562, 1600, 564, 1601, 1596, 1602, 1603, 1604, 4077, 4079, 1597, 566, 1964, 1610, 568, 569, 570, 1598, 2742, 571, 573, 3237, 1605, 1608, 567, 572, 574,
            1599, 1607, 5112, 4979, 1606, 4985, 4990, 563, 1609, 3371, 3236, 3235, 1963, 1965, 3370, 3044, 3796, 3869, 4078, 4080, 4081, 4082, 4237, 4356, 4357, 4358, 4421, 4606, 4991, 4992, 5006,
            5014, 5015, 5016, 5017, 5018, 5019, 5020, 5021, 5022, 5023,5024, 5025, 5026, 5027, 5028, 5029, 5030, 5031, 5032, 5033, 5034, 5035, 5036, 5037, 5038, 5039, 5040, 5362, 565 };

        public readonly static ushort[] OneBreakTiles = { 0, 1, 6, 7, 8, 9, 22, 39, 40, 45, 46, 47, 56, 57, 59, 118, 119, 120, 121, 140, 145, 146, 147, 148, 150, 151, 152, 153, 154, 155, 156, 160, 163,
            164, 166, 167, 168, 169, 175, 176, 177, 189, 196, 197, 200, 204, 206, 230, 262, 263, 264, 265, 266, 267, 268, 273, 274, 284, 325, 326, 327, 346, 347, 348, 367, 368, 371, 396, 397,
            398, 399, 400, 401, 402, 403, 407, 408, 409, 415, 416, 417, 418, 472, 473, 478, 507, 508, 563, 659, 666, 667, 669, 670, 671, 672, 673, 674, 675, 676, 687, 688, 689, 690, 691 };

        public readonly int[] OneBreakTilesPlatforms = { 30, 38, 54, 157, 158, 159, 161, 188, 190, 191, 193, 195, 202, 311, 321, 322, 350, 357, 369, 370, 474, 479, 498, 500, 501, 502, 503, 562, 635 };

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

            new("bouncybomb", "[i:3115] [c/FFC0CB:Bouncy bomb round] [i:3115]",
                () => { GiveEveryoneItems(3115, 50); }),

            new("lavabomb", "[i:4825] [c/FFA500:Lava bomb round] [i:4825]",
                () => { Boulders("lavabomb"); }),

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

            new("invisibility", "[i:297] [c/00FFFF:Invisility round] [i:297]",
                () => { SetEveryoneBuff(BuffID.Invisibility, 60000); }),

            new("featherfall", "[i:320] [c/00FFFF:Featherfall round] [i:320]",
                () => { SetEveryoneBuff(BuffID.Featherfall, 60000); }),

            new("slime", "[i:2430] [c/00bfff:Slime saddle round] [i:2430]",
                () => { GiveEveryoneMiscEquips(2430, 3); }),

            new("pogo", "[i:4791] [c/FFC0CB:Pogo stick round] [i:4791]",
                () => { GiveEveryoneMiscEquips(4791, 3); }),

            new("golf", "[i:4264] [c/FFFFFF:Golf cart round] [i:4264]",
                () => { GiveEveryoneMiscEquips(4264, 3); }),

            new("unicorn", "[i:3260] [c/800080:Unicorn round] [i:3260]",
                () => { GiveEveryoneMiscEquips(3260, 3); }),

            new("scutlix", "[i:2771] [c/0000FF:Scutlix round] [i:2771]",
                () => { GiveEveryoneMiscEquips(2771, 3); }),

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
        private readonly SpleefELO spleefELO;
        private BlockSpam blockSpam;

        public SpleefGame(Spleef plugin, SpleefCoin spleefCoin, InventoryEdit inventoryEdit, SpleefUserSettings spleefSettings, SpleefELO spleefELO)
        {
            this.pluginInstance = plugin;
            this.inventoryEdit = inventoryEdit;
            this.spleefCoin = spleefCoin;
            this.spleefSettings = spleefSettings;
            this.spleefELO = spleefELO;
            PutGimmicks();
        }

        public void SetBlockSpam(BlockSpam blockSpam)
        {
            this.blockSpam = blockSpam;
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
        public bool isRound = false;
        private bool isJoinable = false;
        private bool isBettable = false;
        private bool isChallenge = false;

        private string CommandToStartRound;
        private string CommandToEndRound;
        private string CommandToRandomizeDirt;
        private string OtherLavaRiseCommand = "null";
        private string ParameterLavaRise = "null";
        private string ParameterRandomizeDirt = "null";
        private string MoreParameters = "";
        private string GameTemplateName;
        private int PlayerCount;
        private int RoundCounter;
        private Item MusicBox = new();
        private int PaintItemID;

        private Random rnd = new();

        private class Playering
        {
            public string Name;
            public int score = 0;
            public bool isAlive = false;
            public int place = -100;
            public string accName;
            public bool isIngame = true;
            public bool isReady = false;
            public float BetPayout;
            public float ELO;

            public Playering(string name,string accName, int score = 0, float betPayout = 1.0f)
            {
                Name = name;
                this.score = score;
                this.accName = accName;
                BetPayout = betPayout;
                var spleefCoin = new SpleefCoin();
                var spleefELO = new SpleefELO(spleefCoin);
                ELO = spleefELO.GetElo(accName);
            }

            public int GetPlace (int PlayerCount)
            {
                return PlayerCount - place;
            }
        }

        private List<Map> MapsInfo = new();
        private static List<Playering> PlayerInfo = new();

        public static bool isPlayerOnline(string playername, out TSPlayer player)
        {
            var players = TSPlayer.FindByNameOrID(playername);
            if (players == null || players.Count == 0)
            {
                player = null;
                return false;
            }
            player = players[0];
            return true;
        }

        public bool isPlayerIngame(string playername)
        {
            //PlayerInfoList = PlayerInfo.ToList();
            if (!isPlayerOnline(playername, out TSPlayer player))
                return false;
            if (PlayerInfo.Exists(p => p.accName == player.Account.Name))
                return true;
            return false;
        }

        public void CountPlayers()
        {
            PlayerCount = 0;
            foreach (Playering plr in PlayerInfo)
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

            counter = 0;
            RoundCounter++;
            statusRound = $"[c/8FBC8F:Round {RoundCounter}]\n";
            TSPlayer.All.SendMessage($"Round {RoundCounter} started", Color.DarkSeaGreen);
            for (int i = 0; i < GimmickAmount; i++)
                if (GameType[i] == "random" || GameType[i] == "r")
                    GameType[i] = Convert.ToString(rnd.Next(Gimmicks.Count - 2));
            if (isBettable && !isBetsLocked)
            {
                isBetsLocked = true;
                TSPlayer.All.SendMessage("All bets are closed now!", Color.SeaShell);
            }
            isRound = true;
            ServerApi.Hooks.NetGetData.Register(pluginInstance, OnGetData);

            TpAndWebEveryone(GameArena.ArenaSpawns);
            List<Playering> AlivePlayers = PlayerInfo.FindAll(p => p.isAlive);

            #region default items and buffs
            ClearEveryoneInventory(AlivePlayers);
            GiveEveryoneItems(ItemID.CobaltPickaxe, 1, 0, AlivePlayers);
            GiveEveryoneItems(ItemID.Binoculars, 1, 9, AlivePlayers);
            GiveEveryoneItems(ItemID.CobaltPickaxe, 1, 40, AlivePlayers);
            SetEveryoneBuff(BuffID.Honey, 1000000, AlivePlayers);
            SetEveryoneBuff(BuffID.Shine, 1000000, AlivePlayers);
            SetEveryoneBuff(BuffID.NightOwl, 1000000, AlivePlayers);
            SetEveryoneBuff(BuffID.HeartLamp, 1000000, AlivePlayers);
            SetEveryoneBuff(BuffID.Campfire, 1000000, AlivePlayers);
            #endregion

            #region give paint thing
            List<Playering> PaintPlayers = AlivePlayers.FindAll(p => spleefSettings.GetSettings(p.accName).GetPaintSprayer);
            GiveEveryoneArmor(ItemID.PaintSprayer, Players: PaintPlayers);
            GiveEveryoneItems(PaintItemID, 9999, 20, PaintPlayers);
            #endregion

            ChooseArena(GameArena);
            for (int i = 0; i < GimmickAmount; i++)
                ChooseGimmick(GameType[i]);

            if (ParameterLavaRise != "null")
                Commands.HandleCommand(TSPlayer.Server, ParameterLavaRise);
            else if (OtherLavaRiseCommand != "null")
                Commands.HandleCommand(TSPlayer.Server, GameArena.OtherLavariseCommand);
            else
                Commands.HandleCommand(TSPlayer.Server, CommandToStartRound);

            #region give music box
            MusicBox.SetDefaults(MusicBoxIDs[rnd.Next(MusicBoxIDs.Length)]);
            GiveEveryoneArmor(MusicBox.netID, Players: AlivePlayers.FindAll(p => spleefSettings.GetSettings(p.accName).GetMusicBox));
            string SongName = MusicBox.Name.Substring(MusicBox.Name.IndexOf('(') + 1, MusicBox.Name.IndexOf(')') - MusicBox.Name.IndexOf('(') - 1);
            if (MusicBox.Name.Split(' ')[0] == "Otherworldly")
                SongName = "Otherworldly " + SongName;
            TSPlayer.All.SendMessage($"[i:{MusicBox.netID}] Playing {SongName} [i:{MusicBox.netID}]", Color.LightPink);
            #endregion
            UpdateScore();
        }
        //im tired brah
        private void AnnounceScore()
        {
            TSPlayer.All.SendMessage(statusScore, Color.Coral);
        }

        public void UpdateScore()
        {
            statusScore = "[c/FF1493:Game score:]\n";
            PlayerInfo = PlayerInfo.OrderByDescending(p => p.score).ToList();
            foreach (var plr in PlayerInfo)
            {
                if (plr.isIngame == true)
                {
                    if (plr.isAlive)
                        statusScore += $"[c/FF8559:{plr.Name} : {plr.score}] [i:58]";
                    else
                    {
                        if (plr.GetPlace(PlayerCount) == 1)
                            statusScore += $"[c/FF8559:{plr.Name} : {plr.score}] [i:4601]";
                        else if (plr.GetPlace(PlayerCount) == 2)
                            statusScore += $"[c/FF8559:{plr.Name} : {plr.score}] [i:4600]";
                        else if (plr.GetPlace(PlayerCount) == 3)
                            statusScore += $"[c/FF8559:{plr.Name} : {plr.score}] [i:4599]";
                        else
                            statusScore += $"[c/FF8559:{plr.Name} : {plr.score}] [i:321]";
                    }
                }
                else
                    statusScore += $"[c/898989:{plr.Name} : {plr.score}]";
                if (isBettable)
                    statusScore += $"\nBet Payout - {plr.BetPayout:N3}";
                statusScore += "\n";
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

        public void LoadStuff(GameTemplate gameTemplate)
        {
            MapsInfo.Clear();
            CommandToStartRound = gameTemplate.LavariseCommand;
            CommandToEndRound = gameTemplate.FillCommand;
            CommandToRandomizeDirt = gameTemplate.RandomizeDirtCommand;
            foreach (var map in gameTemplate.Maps)
                MapsInfo.Add(map);
        }

        private void GiveAllScores()
        {
            foreach (Playering player in PlayerInfo)
            {
                var players = TSPlayer.FindByNameOrID(player.Name);
                if (isPlayerOnline(player.Name, out TSPlayer plr))
                {
                    if (player.accName == player.Name)
                        spleefCoin.AddCoins(player.accName, player.score, false);
                    else
                    {
                        spleefCoin.AddCoins(player.accName, player.score, true);
                        plr.SendMessage($"Sent {player.score} Spleef Coin to your account {player.accName}", Color.Purple);
                    }
                }
                else
                    spleefCoin.AddCoins(player.accName, player.score, false);
            }
        }

        public void TheGaming(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("erm no");
                return;
            }
            switch (args.Parameters[0])
            {
                case "help":
                    args.Player.SendInfoMessage("/game list (gimmick, template, map <templatename>)");
                    args.Player.SendInfoMessage("/game template <templateName> [isJoinable] [isBettable] - creates a new game. By default it's joinable and people can't place bets(bets aren't implemented yet).");
                    args.Player.SendMessage("these commands only work if there's a game going on!", Color.OrangeRed);
                    args.Player.SendInfoMessage("/game start <gimmickAmount> <list gimmicks> <map> or /game start <gimmick> <map> or  - starts a round");
                    args.Player.SendInfoMessage("/game stop - ends the game (alias /game end), however if there's a round active, stops the round.");
                    args.Player.SendInfoMessage("/game add <playername> - adds a player to the ongoing game");
                    args.Player.SendInfoMessage("/game remove <playername> - removes a player from the game");
                    args.Player.SendInfoMessage("/game score - shows the current score");
                    args.Player.SendInfoMessage("/game edit score/payout <playername> <amount> - changes the player's score by the given amount");
                    args.Player.SendInfoMessage("/game bet open/close/off/pay - opens/closes bets, turns them off completely or pays out the bets immediately");
                    args.Player.SendInfoMessage("/game reload - reloads stuff duh, useful for bet payouts in 1v1s");
                    break;
                case "list":
                    ListStuff(args);
                    break;
                case "template":
                    if (isGaming)
                    {
                        args.Player.SendErrorMessage("You can't use this command right now! There is already a game going on.");
                        return;
                    }

                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("not enough parameters!");
                        args.Player.SendErrorMessage("/game template <templateName> [isJoinable] [isBettable]");
                        return;
                    }

                    GameTemplateName = args.Parameters[1];
                    var gameTemplate = PluginSettings.Config.GameTemplates.FirstOrDefault(c => c.Name == GameTemplateName);
                    if (gameTemplate == null)
                    {
                        args.Player.SendErrorMessage("dummas there isnt a template named like that");
                        return;
                    }

                    LoadStuff(gameTemplate);

                    isJoinable = true;
                    isBettable = false;
                    for (int i = 2; i < args.Parameters.Count; i++)
                    {
                        if (args.Parameters[i] == "-join" && (args.Parameters[i + 1] == "false" || args.Parameters[i + 1] == "0" || args.Parameters[i + 1] == "disable"))
                            isJoinable = false;
                        else if (args.Parameters[i] == "-bet" && (args.Parameters[i + 1] == "true" || args.Parameters[i + 1] == "1" || args.Parameters[i + 1] == "enable"))
                            isBettable = true;
                    }

                    isGaming = true;
                    if (isJoinable)
                        TSPlayer.All.SendMessage("Game started! Join with /j", Color.SeaShell);
                    else
                        TSPlayer.All.SendMessage("Game started!", Color.SeaShell);
                    if (isBettable)
                    {
                        isBetsLocked = false;
                        TSPlayer.All.SendMessage("Start placing your bets! Bet with /bet add username amount", Color.SeaShell);
                    }
                    RoundCounter = 0;
                    ServerApi.Hooks.ServerLeave.Register(pluginInstance, OnPlayerLeave);
                    ServerApi.Hooks.GameUpdate.Register(pluginInstance, OnWorldUpdate);
                    GeneralHooks.ReloadEvent += OnServerReload;
                    UpdateScore();
                    break;
                case "start":
                    if (!isGaming)
                    {
                        args.Player.SendErrorMessage("You can't use that command yet! You should first create a game");
                        args.Player.SendInfoMessage("/game template <templateName>");
                        return;
                    }

                    if (isRound)
                    {
                        args.Player.SendErrorMessage("You can't use this command right now! There is already a round going on.");
                        break;
                    }

                    if (args.Parameters.Count < 3)
                    {
                        args.Player.SendErrorMessage("not enough parameters!");
                        args.Player.SendInfoMessage("/game start <gimmickAmount> <list gimmicks> <map>");
                        return;
                    }
                    int GimmickAmount = 1;
                    string mapname;
                    byte paintID = (byte)rnd.Next(31);
                    PaintItemID = Spleef.PaintIDtoItemID(paintID);
                    MoreParameters = $" -paint {paintID}";
                    for (int i = 1; i < args.Parameters.Count; i++)
                    {
                        if (args.Parameters[i] == "-rise")
                            ParameterLavaRise = args.Parameters[i + 1];
                        else if (args.Parameters[i] == "-random")
                            ParameterRandomizeDirt = args.Parameters[i + 1];
                        else if (args.Parameters[i] == "-tile")
                            MoreParameters += $" {args.Parameters[i]} {args.Parameters[i + 1]}";
                        else if (args.Parameters[i] == "-paint")
                        {
                            PaintItemID = Spleef.PaintIDtoItemID(Convert.ToByte(args.Parameters[i + 1]));
                            MoreParameters += $" {args.Parameters[i]} {args.Parameters[i + 1]}";
                        }
                    }
                    string[] GameTypes = new string[100];
                    if (args.Parameters.Count > 3)
                    {
                        GimmickAmount = Convert.ToInt32(args.Parameters[1]);
                        mapname = args.Parameters[GimmickAmount + 2];
                        for (int i = 0; i < GimmickAmount; i++)
                            GameTypes[i] = args.Parameters[i + 2];
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
                    OtherLavaRiseCommand = map.OtherLavariseCommand;
                    StartRound(args, GameTypes, map, GimmickAmount);
                    break;
                case "edit":
                    if (!isGaming)
                    {
                        args.Player.SendErrorMessage("You can't use that command yet! You should first create a game");
                        args.Player.SendInfoMessage("/game template <templateName>");
                        return;
                    }
                    if (args.Parameters.Count != 4)
                    {
                        args.Player.SendErrorMessage("wrong amount of parameters!");
                        args.Player.SendInfoMessage("/game edit score <playername> <amount>");
                        args.Player.SendInfoMessage("/game edit payout <playername> <amount>");
                        return;
                    }

                    if (args.Parameters[1] != "score" && args.Parameters[1] != "s" && args.Parameters[1] != "payout" && args.Parameters[1] != "p")
                    {
                        args.Player.SendErrorMessage("worg");
                        args.Player.SendInfoMessage("/game edit score <playername> <amount>");
                        args.Player.SendInfoMessage("/game edit payout <playername> <amount>");
                        return;
                    }

                    string EditName = args.Parameters[2];
                    float Value = (float)Convert.ToDouble(args.Parameters[3]);

                    if (PlayerInfo.Exists(p => p.Name == EditName))
                    {
                        var plr = PlayerInfo.Find(p => p.Name == EditName);
                        if (args.Parameters[1] == "score" || args.Parameters[1] == "s")
                        {
                            plr.score += (int)Value;
                            TSPlayer.All.SendMessage($"{EditName} score is now {plr.score}!", Color.ForestGreen);
                        }
                        else if (args.Parameters[1] == "payout" || args.Parameters[1] == "p")
                        {
                            plr.BetPayout = Value;
                            TSPlayer.All.SendInfoMessage($"{EditName} payout is now {plr.BetPayout}! Better check your bets!", Color.ForestGreen);
                        }
                        UpdateScore();
                        return;
                    }

                    if (!isPlayerOnline(args.Parameters[2], out TSPlayer PlayerToEdit))
                    {
                        args.Player.SendErrorMessage("this guy does not exist bro");
                        return;
                    }

                    if (PlayerInfo.Exists(p => p.Name == PlayerToEdit.Name))
                    {
                        var plr = PlayerInfo.Find(p => p.Name == PlayerToEdit.Name);
                        if (args.Parameters[1] == "score" || args.Parameters[1] == "s")
                        {
                            plr.score += (int)Value;
                            TSPlayer.All.SendMessage($"{PlayerToEdit.Name} score is now {plr.score}!", Color.ForestGreen);
                        }
                        else if (args.Parameters[1] == "payout" || args.Parameters[1] == "p")
                        {
                            plr.BetPayout = Value;
                            TSPlayer.All.SendInfoMessage($"{PlayerToEdit.Name} payout is now {plr.BetPayout}! Better check your bets!", Color.ForestGreen);
                        }
                    }
                    else
                        args.Player.SendErrorMessage("this guy is not in the current game bro");
                    UpdateScore();
                    break;
                case "add":
                    if (!isGaming)
                    {
                        args.Player.SendErrorMessage("You can't use that command yet! You should first create a game");
                        args.Player.SendInfoMessage("/game template <templateName>");
                        return;
                    }

                    if (args.Parameters.Count != 2)
                    {
                        args.Player.SendErrorMessage("wrong amount of parameters!");
                        args.Player.SendInfoMessage("/game add <playername>");
                        return;
                    }
                    string playername = args.Parameters[1];

                    if (!isPlayerOnline(playername, out TSPlayer playerToAdd))
                    {
                        args.Player.SendErrorMessage("this guy does not exist bro");
                        return;
                    }

                    playername = playerToAdd.Name;

                    if (PlayerInfo.Exists(p => p.Name == playername))
                    {
                        Playering playerToAddBack = PlayerInfo.Find(p => p.Name == playername);

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

                    Playering newPlayering = new(playerToAdd.Name, playerToAdd.Account.Name);

                    playername = playerToAdd.Name;
                    PlayerInfo.Add(newPlayering);
                    TSPlayer.All.SendMessage($"{playername} has been added to the game!", Color.Green);
                    UpdateScore();
                    break;
                case "remove":
                    if (!isGaming)
                    {
                        args.Player.SendErrorMessage("You can't use that command yet! You should first create a game");
                        args.Player.SendInfoMessage("/game template <templateName> <numOfPlayers> <list player names>");
                        return;
                    }

                    if (args.Parameters.Count != 2)
                    {
                        args.Player.SendErrorMessage("wrong amount of parameters!");
                        args.Player.SendInfoMessage("/game remove <playername>");
                        return;
                    }
                    string name = args.Parameters[1];
                    if (!PlayerInfo.Exists(p => p.Name == name))
                    {
                        args.Player.SendErrorMessage("uh nuh uh");
                        return;
                    }
                    Playering playerToRemove = PlayerInfo.Find(p => p.Name == name);
                    int points = playerToRemove.score;
                    spleefCoin.AddCoins(playerToRemove.accName, points, false);
                    if (playerToRemove.isAlive)
                        counter++;
                    PlayerInfo.Remove(playerToRemove);
                    args.Player.SendSuccessMessage($"removed {name} from the game and awarded {points} Spleef Coins!");
                    TSPlayer.All.SendMessage($"{name} has been completely removed from the game!", Color.Red);
                    UpdateScore();
                    break;
                case "score":
                    if (!isGaming)
                    {
                        args.Player.SendErrorMessage("You can't use that command yet! You should first create a game");
                        args.Player.SendInfoMessage("/game template <templateName> <numOfPlayers> <list player names>");
                        return;
                    }
                    AnnounceScore();
                    break;
                case "stop":
                case "end":
                    if (!isGaming)
                    {
                        args.Player.SendErrorMessage("You can't use that command yet! You should first create a game");
                        args.Player.SendInfoMessage("/game template <templateName>");
                        return;
                    }

                    if (isRound)
                    {
                        ServerApi.Hooks.NetGetData.Deregister(pluginInstance, OnGetData);
                        StopLavarise();
                        Commands.HandleCommand(TSPlayer.Server, CommandToEndRound);
                        TSPlayer.All.SendMessage($"Forcefully ended round!", Color.BlueViolet);
                        isRound = false;
                        return;
                    }

                    SpleefCoin.MigrateUsersToSpleefDatabase();
                    AnnounceScore();
                    TSPlayer.All.SendMessage($"Game ended, after {RoundCounter} rounds {PlayerInfo[0].Name} WON!!!!!!!!!", Color.MediumTurquoise);
                    GiveAllScores();
                    if (isBettable)
                        PayAllBets();
                    PlayerInfo.Clear();
                    MapsInfo.Clear();
                    statusScore = "";
                    isGaming = false;
                    RoundCounter = 0;
                    blockSpam.FullTimerAnnounce();
                    GeneralHooks.ReloadEvent -= OnServerReload;
                    ServerApi.Hooks.ServerLeave.Deregister(pluginInstance, OnPlayerLeave);
                    break;
                case "bet":
                    if (!isGaming)
                    {
                        args.Player.SendErrorMessage("You can't use that command yet! You should first create a game");
                        args.Player.SendInfoMessage("/game template <templateName>");
                        return;
                    }

                    if (args.Parameters[1] == "open")
                    {
                        isBettable = true;
                        isBetsLocked = false;
                        TSPlayer.All.SendMessage("The bets have been opened! Start placing your bets with /bet!", Color.SeaShell);
                        return;
                    }

                    if (args.Parameters[1] == "close")
                    {
                        isBetsLocked = true;
                        TSPlayer.All.SendMessage("The bets are closed now!", Color.SeaShell);
                        return;
                    }

                    if (args.Parameters[1] == "off")
                    {
                        isBettable = false;
                        isBetsLocked = true;
                        Bets.Clear();
                        TSPlayer.All.SendMessage("All bets have been cleared! You can't place any more bets as well :C", Color.SeaShell);
                        return;
                    }

                    if (args.Parameters[1] == "pay")
                    {
                        PayAllBets();
                        return;
                    }
                    break;
                case "reload":
                    if (!isGaming)
                    {
                        args.Player.SendErrorMessage("You can't use that command yet! You should first create a game");
                        args.Player.SendInfoMessage("/game template <templateName>");
                        return;
                    }

                    CountPlayers();
                    if (isBettable && PlayerCount == 2)
                    {
                        UpdatePayout();
                        args.Player.SendSuccessMessage("Payouts updated!");
                    }
                    UpdateScore();
                    break;
                default:
                    args.Player.SendErrorMessage("That ain't a command buckaroo");
                    args.Player.SendInfoMessage("/game help");
                    break;
            }
        }

        private class Bet
        {
            public string Gambler;
            public int Amount;
            public string Dissapointment;
            public string DissapointmentDisplayName;
            public Bet(string name, int amount, string name2, string displayName)
            {
                Gambler = name;
                Amount = amount;
                Dissapointment = name2;
                DissapointmentDisplayName = displayName;
            }

            public float GetBetPayout()
            {
                if (PlayerInfo.Exists(p => p.accName == Dissapointment))
                    return PlayerInfo.Find(p => p.accName == Dissapointment).BetPayout;
                return -1;
            }
        }

        private List<Bet> Bets = new();
        private bool isBetsLocked = true;

        private int GetAllBets(string name)
        {
            int sum = 0;
            foreach (Bet bet in Bets.FindAll(b => b.Gambler == name))
                sum += bet.Amount;
            return sum;
        }

        public void UpdatePayout() //only works for 2 players rn
        {
            Playering player1 = PlayerInfo[0], player2 = PlayerInfo[1];

            player1.ELO = spleefELO.GetElo(player1.accName);
            player2.ELO = spleefELO.GetElo(player2.accName);

            player1.BetPayout = spleefELO.prob(player1.ELO, player2.ELO) / spleefELO.prob(player2.ELO, player1.ELO);
            player2.BetPayout = spleefELO.prob(player2.ELO, player1.ELO) / spleefELO.prob(player1.ELO, player2.ELO);
        }

        public void PayAllBets()
        {
            foreach (Bet bet in Bets)
            {
                if (bet.Dissapointment == PlayerInfo[0].accName)
                {
                    int payout = (int)Math.Round(bet.Amount * PlayerInfo[0].BetPayout);
                    spleefCoin.AddCoins(bet.Gambler, payout, false);
                    TSPlayer.All.SendMessage($"{bet.Gambler} bet on {bet.DissapointmentDisplayName} {bet.Amount} Spleef Coin(s) and won {payout}!", Color.Chocolate);
                }
                else
                {
                    spleefCoin.AddCoins(bet.Gambler, -bet.Amount, false);
                    TSPlayer.All.SendMessage($"{bet.Gambler} bet on {bet.DissapointmentDisplayName} {bet.Amount} Spleef Coin(s) and lost their bet, what a loser.", Color.Aquamarine);
                }
            }
            Bets.Clear();
        }

        public void Betting (CommandArgs args) // /bet add/remove username amount
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("brotha do /bet help");
                return;
            }

            if (args.Parameters[0] == "help")
            {
                args.Player.SendInfoMessage($"Whenever a game starts you can place bets on who you think will win");
                args.Player.SendInfoMessage($"/bet list - lists all the bets");
                args.Player.SendInfoMessage($"/bet check - lists all your bets");
                args.Player.SendInfoMessage($"/bet add username amount - adds a bet on the player if they're ingame");
                args.Player.SendInfoMessage($"/bet remove username amount - removes an amount from the bet only unless it becomes negative");
                return;
            }

            if (!isBettable)
            {
                args.Player.SendErrorMessage("You can't bet right now!");
                return;
            }

            if (args.Parameters[0] == "list")
            {
                if (Bets.Count == 0)
                {
                    args.Player.SendInfoMessage("There are no bets!");
                    return;
                }
                foreach(Bet bet in Bets)
                    args.Player.SendInfoMessage($"{bet.Gambler} - {bet.Dissapointment} : {bet.Amount} (+{(int)Math.Round(bet.Amount * bet.GetBetPayout())})");
                return;
            }

            string gamblerName = args.Player.Account.Name;

            if (PlayerInfo.Exists(p => p.accName == gamblerName))
            {
                args.Player.SendErrorMessage("Hey! You're in the game, you can't bet!");
                return;
            }

            if (args.Parameters[0] == "check")
            {
                if (Bets.Count(b => b.Gambler == gamblerName) == 0)
                {
                    args.Player.SendInfoMessage("You don't have any bets!");
                    return;
                }
                foreach (Bet bet in Bets.FindAll(b => b.Gambler == gamblerName))
                    args.Player.SendInfoMessage($"{bet.Dissapointment} : {bet.Amount} (+{(int)Math.Round(bet.Amount * bet.GetBetPayout())})");
                return;
            }

            if (isBetsLocked)
            {
                args.Player.SendErrorMessage("The bets are locked now!");
                return;
            }

            if (args.Parameters[0] != "add" && args.Parameters[0] != "remove")
            {
                args.Player.SendErrorMessage("uh wrong do /bet add/remove username amount");
                return;
            }

            string fighterName = args.Parameters[1];

            if (!isPlayerIngame(fighterName))
            {
                args.Player.SendErrorMessage("This player is not in the game!");
                return;
            }

            TSPlayer fighterPlayer = TSPlayer.FindByNameOrID(fighterName)[0];
            fighterName = fighterPlayer.Name;
            string fighterFullName = fighterPlayer.Account.Name;
            int amount = Convert.ToInt32(args.Parameters[2]);

            if (Bets.Exists(b => b.Gambler == gamblerName && b.Dissapointment == fighterFullName))
            {
                Bet bet = Bets.Find(b => b.Gambler == gamblerName && b.Dissapointment == fighterFullName);

                if (args.Parameters[0] == "remove")
                    amount = -amount;

                if (bet.Amount + amount == 0)
                {
                    args.Player.SendSuccessMessage("Removed your entire bet!");
                    Bets.Remove(bet);
                    return;
                }

                if (bet.Amount + amount < 0)
                {
                    args.Player.SendErrorMessage("You can't make your bet negative!");
                    return;
                }

                if (spleefCoin.GetCoins(bet.Gambler) < GetAllBets(bet.Gambler) + amount)
                {
                    args.Player.SendErrorMessage("You can't bet more than you have!");
                    return;
                }

                bet.Amount += amount;
                args.Player.SendSuccessMessage($"Edited your bet on {fighterName}! In total making it {bet.Amount}! (+{(int)Math.Round(bet.Amount * bet.GetBetPayout())})");
                return;
            }

            if (amount <= 0)
            {
                args.Player.SendErrorMessage("Hello, what is that bet");
                return;
            }

            if (spleefCoin.GetCoins(gamblerName) < GetAllBets(gamblerName) + amount)
            {
                args.Player.SendErrorMessage("You can't bet more than you have!");
                return;
            }

            Bets.Add(new Bet(gamblerName, amount, fighterFullName, fighterName));
            var bett = Bets.Find(b => b.Gambler == gamblerName && b.Dissapointment == fighterFullName);
            args.Player.SendSuccessMessage($"Made a bet on {fighterName} - {amount}! (+{(int)Math.Round(bett.Amount * bett.GetBetPayout())})");
        }

        public void Challenge (CommandArgs args) // /challenge username amount
        {

        }

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

            if (PlayerInfo.Exists(p => p.Name == playername))
            {
                Playering playerToAdd = PlayerInfo.Find(p => p.Name == playername);
                
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

            Playering newPlayering = new(args.Player.Name, args.Player.Account.Name);

            PlayerInfo.Add(newPlayering);
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
            
            if (!PlayerInfo.Exists(p => p.Name == playername))
            {
                args.Player.SendErrorMessage("You are not in the game!");
                return;
            }
            Playering playerToRemove = PlayerInfo.Find(p => p.Name == playername);
            if (playerToRemove.isIngame == false)
            {
                args.Player.SendErrorMessage("You have already left!");
                return;
            }

            if (isRound && playerToRemove.isAlive)
            {
                playerToRemove.isAlive = false;
                playerToRemove.place = counter;
                playerToRemove.isIngame = false;
                counter++;
                TSPlayer.All.SendMessage($"{playername} has left the game midround! What a stinker...", Color.Red);
                CheckRound(counter);
                UpdateScore();
                return;
            }
            
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

        private void StopLavarise()
        {
            if (ParameterLavaRise != "null")
            {
                Commands.HandleCommand(TSPlayer.Server, $"{ParameterLavaRise} stop");
                ParameterLavaRise = "null";
            }
            else if (OtherLavaRiseCommand != "null")
            {
                Commands.HandleCommand(TSPlayer.Server, $"{OtherLavaRiseCommand} stop");
                OtherLavaRiseCommand = "null";
            }
            else
                Commands.HandleCommand(TSPlayer.Server, $"{CommandToStartRound} stop");
            Commands.HandleCommand(TSPlayer.Server, CommandToEndRound);
        }

        private void CheckRound(int counter)
        {
            if (counter == PlayerCount - 1)
            {
                suicideTimer.Restart();
                StopLavarise();
            }

            else if (counter == PlayerCount)
            {
                bool isSuicide = false;

                suicideTimer.Stop();
                if (suicideTimer.Elapsed.TotalSeconds < 3)
                    isSuicide = true;
                PlayerInfo = PlayerInfo.
                    OrderByDescending(entry => entry.isIngame).
                    ThenByDescending(entry => entry.place).
                    ToList();
                if (!isSuicide)
                {
                    if (PlayerCount == 2)
                    {
                        PlayerInfo[0].score += 1;
                        TSPlayer.All.SendMessage($"Round ended! {PlayerInfo[0].Name} won this round!", Color.LimeGreen);
                    }
                    else if (PlayerCount >= 3 && PlayerCount <= 6)
                    {
                        PlayerInfo[0].score += 2;
                        PlayerInfo[1].score += 1;
                        TSPlayer.All.SendMessage($"Round ended! {PlayerInfo[0].Name} won this round and {PlayerInfo[1].Name} got second place!", Color.LimeGreen);
                    }
                    else if (PlayerCount >= 7 && PlayerCount <= 9)
                    {
                        PlayerInfo[0].score += 3;
                        PlayerInfo[1].score += 2;
                        PlayerInfo[2].score += 1;
                        TSPlayer.All.SendMessage($"Round ended! {PlayerInfo[0].Name} won this round {PlayerInfo[1].Name} got second place and {PlayerInfo[2].Name} got third place!", Color.LimeGreen);
                    }
                    else if (PlayerCount >= 10 && PlayerCount <= 13)
                    {
                        PlayerInfo[0].score += 4;
                        PlayerInfo[1].score += 3;
                        PlayerInfo[2].score += 2;
                        PlayerInfo[3].score += 1;
                        TSPlayer.All.SendMessage($"Round ended! {PlayerInfo[0].Name} won this round {PlayerInfo[1].Name} got second place {PlayerInfo[2].Name} got third place and {PlayerInfo[3].Name} got fourth place!", Color.LimeGreen);
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
                        PlayerInfo[0].score += 1;
                        PlayerInfo[1].score += 1;
                        TSPlayer.All.SendMessage($"Round ended! It's a suicide so {PlayerInfo[0].Name} and {PlayerInfo[1].Name} both get 1 point!", Color.DarkRed);
                    }
                    else if (PlayerCount >= 7 && PlayerCount <= 9)
                    {
                        PlayerInfo[0].score += 2;
                        PlayerInfo[1].score += 2;
                        PlayerInfo[2].score += 1;
                        TSPlayer.All.SendMessage($"Round ended! It's a suicide so {PlayerInfo[0].Name} and {PlayerInfo[1].Name} get 2 points and {PlayerInfo[2].Name} got third place!", Color.LimeGreen);
                    }
                    else if (PlayerCount >= 10 && PlayerCount <= 13)
                    {
                        PlayerInfo[0].score += 3;
                        PlayerInfo[1].score += 3;
                        PlayerInfo[2].score += 2;
                        PlayerInfo[3].score += 1;
                        TSPlayer.All.SendMessage($"Round ended! It's a suicide so {PlayerInfo[0].Name} and {PlayerInfo[1].Name} get 3 points {PlayerInfo[2].Name} got third place and {PlayerInfo[3].Name} got fourth place!", Color.LimeGreen);
                    }
                }

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

        private void SendScore()
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Active && player.IsLoggedIn && player.Account.Name != null)
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

        private void OnWorldUpdate(EventArgs args)
        {
            SendScore();
        }
        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.PlayerDeathV2)
            {
                using var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
                byte playerID = reader.ReadByte();

                var plr = TShock.Players[playerID];

                foreach (Playering player in PlayerInfo)
                {
                    if (player.isIngame)
                    {
                        var plrOG = TSPlayer.FindByNameOrID(player.Name)[0];
                        if (plr == plrOG && player.isAlive)
                        {
                            player.isAlive = false;
                            player.place = counter;
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
            foreach (Playering player in PlayerInfo)
            {
                if (player.isIngame)
                {
                    var plrOG = TSPlayer.FindByNameOrID(player.Name)[0];
                    if (plr == plrOG)
                    {
                        if (player.isAlive)
                        {
                            player.isAlive = false;
                            player.place = counter;
                            player.isIngame = false;
                            counter++;
                            TSPlayer.All.SendMessage($"{player.Name} has left the game midround! What a stinker...", Color.LightYellow);
                            CheckRound(counter);
                        }
                        else
                        {
                            player.isIngame = false;
                            TSPlayer.All.SendMessage($"{player.Name} has left the game midround! But he died so it's alright.", Color.LightYellow);
                        }
                    }   
                }
            }
            UpdateScore();
        }

        private void GiveEveryoneItems(int itemID, int stack, int slot = -1, List<Playering> Players = null)
        {
            if (Players == null)
                Players = PlayerInfo.FindAll(p => p.isAlive);
            int playerslot;
            foreach (Playering player in Players)
            {
                playerslot = slot;
                var plr = TSPlayer.FindByNameOrID(player.Name)[0];
                if (playerslot == -1)
                    playerslot = inventoryEdit.FindNextFreeSlot(plr);
                inventoryEdit.AddItem(plr, playerslot, stack, itemID);
            }
        }

        private void GiveEveryoneMiscEquips(int itemID, int slot, List<Playering> Players = null)
        {
            if (Players == null)
                Players = PlayerInfo.FindAll(p => p.isAlive);
            foreach (Playering player in Players)
            {
                var plr = TSPlayer.FindByNameOrID(player.Name)[0];
                inventoryEdit.AddMiscEquip(plr, slot, itemID);
            }
        }

        private void ClearEveryoneInventory(List<Playering> Players = null)
        {
            if (Players == null)
                Players = PlayerInfo.FindAll(p => p.isAlive);
            foreach (Playering player in Players)
            {
                var plr = TSPlayer.FindByNameOrID(player.Name)[0];
                inventoryEdit.ClearPlayerEverything(plr);
            }
        }

        private void GiveEveryoneArmor(int itemID, int slot = -1, List<Playering> Players = null)
        {
            if (Players == null)
                Players = PlayerInfo.FindAll(p => p.isAlive);
            int playerslot;
            foreach (Playering player in Players)
            {
                if (player.isIngame)
                {
                    var plr = TSPlayer.FindByNameOrID(player.Name)[0];
                    playerslot = slot;
                    if (playerslot == -1)
                        playerslot = inventoryEdit.FindNextFreeAccessorySlot(plr);
                    if (playerslot == -1)
                    {
                        playerslot = inventoryEdit.FindNextFreeSlot(plr);
                        inventoryEdit.AddItem(plr, playerslot, 1, itemID);
                    }
                    else
                        inventoryEdit.AddArmor(plr, playerslot, itemID);
                }
            }
        }

        private void SetEveryoneBuff(int BuffID, int time, List<Playering> Players = null)
        {
            if (Players == null)
                Players = PlayerInfo.FindAll(p => p.isAlive);
            foreach (Playering player in Players)
            {
                var plr = TSPlayer.FindByNameOrID(player.Name)[0];
                plr.SetBuff(BuffID, time);
            }
        }

        private void TpAndWebEveryone(List<ArenaSpawn> ArenaSpawns, List<Playering> Players = null)
        {
            if (Players == null)
                Players = PlayerInfo.FindAll(p => p.isIngame);
            if (ArenaSpawns.Count == 0)
            {
                foreach (var player in Players)
                {
                    player.isAlive = true;
                }
                return;
            }

            var ShuffledList = Players.OrderBy(p => rnd.Next());
            int counter = 0;
            foreach (Playering player in ShuffledList)
            {
                var plr = TSPlayer.FindByNameOrID(player.Name)[0];
                plr.Teleport(ArenaSpawns[counter % ArenaSpawns.Count].X * 16, ArenaSpawns[counter % ArenaSpawns.Count].Y * 16);
                plr.SetBuff(BuffID.Webbed, 200);
                player.isAlive = true;
                counter++;
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
            if (ParameterRandomizeDirt != "null")
            {
                Commands.HandleCommand(TSPlayer.Server, ParameterRandomizeDirt + MoreParameters);
                var cmd = CustomCommandHandler.CommandsTrack.Find(c => c.Name == ParameterRandomizeDirt[1..]);
            }
            if (GameArena.OtherRandomizeDirtCommand != "null")
                Commands.HandleCommand(TSPlayer.Server, GameArena.OtherRandomizeDirtCommand + MoreParameters);
            else if (CommandToRandomizeDirt != "null")
                Commands.HandleCommand(TSPlayer.Server, CommandToRandomizeDirt + MoreParameters);
            foreach (var item in GameArena.Items)
            {
                if (item.Type == "inventory")
                {
                    if (item.Time > 0)
                        Boulders("yeah", item.ID, item.Time, item.Stack);
                    else
                        GiveEveryoneItems(item.ID, item.Stack, item.Slot);
                }
                else if (item.Type == "armor")
                    GiveEveryoneArmor(item.ID, item.Slot);
                else if (item.Type == "misc")
                    GiveEveryoneMiscEquips(item.ID, item.Slot);
            }
            foreach (var buff in GameArena.Buffs)
                SetEveryoneBuff(buff.ID, buff.TimeInSeconds * 60);
        }

        private async void Boulders(string GameType, int itemID = -1, int timeInSeconds = 20, int itemAmount = 1)
        {
            await Task.Delay(timeInSeconds * 1000);

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
                case "lavabomb":
                    GiveEveryoneItems(4825, 2);
                    TSPlayer.All.SendMessage("[i:4825] Lava bombs have been given out! Oh boy! [i:4825]", Color.DeepPink);
                    break;
                default:
                    GiveEveryoneItems(itemID, itemAmount);
                    TSPlayer.All.SendMessage($"[i:{itemID}] {itemID} have been given out! ^_^ [i:{itemID}]", Color.DeepPink);
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

        private void OnServerReload(ReloadEventArgs args)
        {
            try
            {
                var gameTemplate = PluginSettings.Config.GameTemplates.FirstOrDefault(c => c.Name == GameTemplateName);
                LoadStuff(gameTemplate);
                args.Player.SendSuccessMessage("[SpleefResurgence] Game template reloaded!");
            }
            catch (Exception ex)
            {
                args.Player.SendErrorMessage("There was an issue loading the game template!");
                TShock.Log.ConsoleError(ex.ToString());
            }
        }
    }
}


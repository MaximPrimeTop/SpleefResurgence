using System;
using System.Net;
using TShockAPI;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using SpleefResurgence.Utils;
using IL.SteelSeries.GameSense.DeviceZone;
using On.SteelSeries.GameSense.DeviceZone;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace SpleefResurgence.Game
{
    public class Player
    {
        public string Name;
        public int Score = 0;
        public string AccountName;
        public float Elo;
        public int Place = 0;
        public bool isIngame = true;
        public bool isAlive = false;

        public Player(string name, string accountName, float elo = 1000f)
        {
            Name = name;
            AccountName = accountName;
            Elo = elo;
        }
    }

    public enum RiseType
    {
        Lava,
        Shimmer,
        Honey,
        Water
    }

    public class Map
    {
        public string Name { get; set; }
        public string SchematicName { get; set; }
        public string OwnerName { get; set; }
        public RiseType RiseType { get; set; }
        public List<ArenaSpawn> Spawns { get; set; }
        public List<Gimmick> AdditionalGimmicks { get; set; }

        public Map(string name, string schematicName, string ownerName, RiseType riseType, List<ArenaSpawn> spawns, List<Gimmick> additionalGimmicks)
        {
            Name = name;
            SchematicName = schematicName;
            OwnerName = ownerName;
            RiseType = riseType;
            Spawns = spawns;
            AdditionalGimmicks = additionalGimmicks;
        }
    }

    public class Arena
    {
        public string Name { get; set; }
        // this is the top left corner of the arena
        public int TilePositionX { get; set; }
        public int TilePositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public string DefaultCustomLavariseCommand { get; set; } = "null";

        public List<string> Maps;
        [JsonIgnore]
        public byte CurrentPaint;
        [JsonIgnore]
        public Map CurrentMap;

        public Arena(string name, int tilePositionX, int tilePositionY, int width, int height, List<string> maps)
        {
            Name = name;
            TilePositionX = tilePositionX;
            TilePositionY = tilePositionY;
            Width = width;
            Height = height;
            Maps = maps;
        }

        public void TeleportPlayers(List<TSPlayer> players)
        {
            List<ArenaSpawn> spawns = CurrentMap.Spawns;
            if (players == null || players.Count == 0 || spawns.Count == 0)
                return;

            var ShuffledList = players.OrderBy(p => Spleef.rnd.Next());
            int counter = 0;
            foreach (var player in ShuffledList)
            {
                // will change this
                int spawnX = (spawns[counter % spawns.Count].X) * 16;
                int spawnY = (spawns[counter % spawns.Count].Y) * 16;

                player.Teleport(spawnX, spawnY);
                player.SetBuff(BuffID.Webbed, 200);
            }
        }
        /*
        public bool[,] GetAirTiles()
        {
            bool[,] airTiles = new bool[Width, Height];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    int tileX = i + TilePositionX;
                    int tileY = j + TilePositionY;
                    if (Main.tile[tileX, tileY].active())
                        airTiles[i, j] = true;
                }
            }
            return airTiles;
        }
        */
        public void PasteMap()
        {
            if (CurrentMap == null || CurrentMap.SchematicName == null || CurrentMap.SchematicName == "")
                return;

            //safer to do this instead of /sc paste because you can't undo it otherwise, i'll prob make a schematic system at some point so whatv
            Commands.HandleCommand(TSPlayer.Server, $"/sc load {CurrentMap.SchematicName}");
            Commands.HandleCommand(TSPlayer.Server, $"//p1 {TilePositionX} {TilePositionY}");
            Commands.HandleCommand(TSPlayer.Server, $"//paste");
            /*
            if (!CurrentMap.EmptyTilesSet)
            {
                CurrentMap.EmptyTiles = GetAirTiles();
                CurrentMap.EmptyTilesSet = true;
            }
            */
        }

        public void StartRise()
        {
            
        }



        /*
        public void SetRegions()
        {
            Point p1 = new(-1, -1), p2 = new(-1, -1);
            int counter = 0;
            for (int i = TilePositionY; i < TilePositionY + Height - 1; i++)
            {
                for (int j = TilePositionX; j < TilePositionX + Width - 1; j++)
                    if (Main.tile[i, j].type == TileID.Titanium)
                    {
                        if (p1 == new Point(-1, -1))
                            p1 = new(j, i);
                        else if (p2 == new Point(-1, -1))
                        {
                            p2 = new(j, i);
                        }
                        else
                        {
                            TShock.Regions.AddRegion(p1.X, p2.Y, p2.X - p1.X + 1, p2.Y - p1.Y + 1, $"{Name}{counter}", "server", Main.worldID.ToString());
                            counter++;
                            p1 = new Point(j, i);
                            p2 = new Point(-1, -1);
                        }
                    }
                if (p1 != new Point(-1, -1) && p2 != new Point(-1, -1))
                {
                    TShock.Regions.AddRegion(p1.X, p2.Y, p2.X - p1.X + 1, p2.Y - p1.Y + 1, $"{Name}{counter}", "server", Main.worldID.ToString());
                    counter++;
                }
                p1 = new Point(-1, -1);
                p2 = new Point(-1, -1);
            }
        }
        */
    }

    public class Game
    {
        public Arena Arena;
        public List<Player> Players = new();
        private List<Player> RoundPlayers = new();
        public List<string> Hosters;
        public int PlaceCounter = 0;
        public int RoundCounter = 1;
        public bool isJoinable;
        public bool isBettable;
        public bool isRound;
        private int CurrentMusicBoxID;
        private Stopwatch SuicideTimer = new();

        public Game(Arena arena, string hosterName, bool isJoinable = true, bool isBettable = false)
        {
            isRound = false;
            Arena = arena;
            Players = new();
            this.isJoinable = isJoinable;
            this.isBettable = isBettable;
            RoundCounter = 1;
            Hosters = new() { hosterName };
        }

        public bool isPlayerHoster(string accountname)
        {
            return Hosters.Contains(accountname);
        }

        public readonly int[] MusicBoxIDs = { 562, 1600, 564, 1601, 1596, 1602, 1603, 1604, 4077, 4079, 1597, 566, 1964, 1610, 568, 569, 570, 1598, 2742, 571, 573, 3237, 1605, 1608, 567, 572, 574,
            1599, 1607, 5112, 4979, 1606, 4985, 4990, 563, 1609, 3371, 3236, 3235, 1963, 1965, 3370, 3044, 3796, 3869, 4078, 4080, 4081, 4082, 4237, 4356, 4357, 4358, 4421, 4606, 4991, 4992, 5006,
            5014, 5015, 5016, 5017, 5018, 5019, 5020, 5021, 5022, 5023,5024, 5025, 5026, 5027, 5028, 5029, 5030, 5031, 5032, 5033, 5034, 5035, 5036, 5037, 5038, 5039, 5040, 5362, 565 };

        Dictionary<(int min, int max), int[]> rewardTable = new()
        {
            [(2, 2)] = new[] { 1, 0 },
            [(3, 6)] = new[] { 2, 1 },
            [(7, 9)] = new[] { 3, 2, 1 },
            [(10, 13)] = new[] { 4, 3, 2, 1 }
        };

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

        public bool isPlayerIngame(string name)
        {
            if (isPlayerOnline(name, out var player))
                return Players.Exists(p => p.AccountName == player.Account.Name && p.isIngame);
            return Players.Exists(p => p.Name == name && p.isIngame);
        }

        public void AddPlayer(string name, string accountName)
        {
            if (Players.Exists(p => p.AccountName == accountName))
            {
                var playerToAddBack = Players.Find(p => p.AccountName == accountName);
                playerToAddBack.Name = name;
                playerToAddBack.isIngame = true;
                TShock.Utils.Broadcast($"{name} has joined back into the game!", Color.Green);
                return;
            }

            Players.Add(new Player(name, accountName));
            TShock.Utils.Broadcast($"{name} has joined the game!", Color.Green);
            return;
        }

        public void StartRound(List<Gimmick> gimmicks, string mapName)
        {
            RoundPlayers = Players.FindAll(p => p.isIngame);
            PlaceCounter = RoundPlayers.Count;

            RoundPlayers.ForEach(player => player.isAlive = true);

            List<TSPlayer> players = RoundPlayers.Select(player => TShock.Players.FirstOrDefault(p => p.Name == player.Name)).ToList();

            Arena.CurrentMap = GameConfig.MapJson.LoadMap(mapName, Arena.Name);
            Arena.PasteMap();
            Arena.TeleportPlayers(players);

            players.ForEach(player =>
            {
                InventoryEdit.ClearPlayerEverything(player);
                InventoryEdit.AddItem(player, 0, 1, ItemID.CobaltPickaxe);
                InventoryEdit.AddItem(player, 9, 1, ItemID.Binoculars);
                InventoryEdit.AddItem(player, 40, 1, ItemID.CobaltPickaxe);
                player.SetBuff(BuffID.Honey);
                player.SetBuff(BuffID.Shine);
                player.SetBuff(BuffID.NightOwl);
            });

            gimmicks.ForEach(gimmick => gimmick.ApplyGimmick(players));
            if (Arena.CurrentMap.AdditionalGimmicks != null && Arena.CurrentMap.AdditionalGimmicks.Count > 0)
                Arena.CurrentMap.AdditionalGimmicks.ForEach(gimmick => gimmick.ApplyGimmick(players));

            
            CurrentMusicBoxID = Spleef.MusicBoxIDs[Spleef.rnd.Next(Spleef.MusicBoxIDs.Length)];
            Item MusicBox = new Item();
            MusicBox.SetDefaults(CurrentMusicBoxID);
            string SongName = MusicBox.Name.Substring(MusicBox.Name.IndexOf('(') + 1, MusicBox.Name.IndexOf(')') - MusicBox.Name.IndexOf('(') - 1);
            if (MusicBox.Name.Split(' ')[0] == "Otherworldly")
                SongName = "Otherworldly " + SongName;   
            Arena.CurrentPaint = (byte)Spleef.rnd.Next(31);
            var PaintItemID = Spleef.PaintIDtoItemID(Arena.CurrentPaint);
            players.ForEach(player =>
            {
                if (SpleefUserSettings.GetSettings(player.Account.Name).GetPaintSprayer)
                {
                    InventoryEdit.AddArmor(player, ItemID.PaintSprayer);
                    InventoryEdit.AddItem(player, 20, 9999, ItemID.PaintSprayer);
                }
                if (SpleefUserSettings.GetSettings(player.Account.Name).GetMusicBox)
                {
                    InventoryEdit.AddArmor(player, MusicBox.netID);
                    player.SendMessage($"[i:{MusicBox.netID}] Playing {SongName} [i:{MusicBox.netID}]", Color.LightPink);
                }
            });
            ServerApi.Hooks.NetGetData.Register(Spleef.Instance, OnGetData);
            isRound = true;
            TShock.Utils.Broadcast($"Round {RoundCounter} started", Color.DarkSeaGreen);
            RoundCounter++;
        }

        public void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID != PacketTypes.PlayerDeathV2)
                return;

            using var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));

            byte playerID = reader.ReadByte();

            var plr = TShock.Players[playerID];

            if (plr == null || !plr.Active || !isPlayerIngame(plr.Account.Name))
                return;

            var gamePlayer = Players.Find(p => p.AccountName == plr.Account.Name);
            
            if (gamePlayer == null || !gamePlayer.isAlive)
                return;

            gamePlayer.isAlive = false;
            gamePlayer.Place = PlaceCounter;
            plr.SendMessage($"You got {PlaceCounter}{(gamePlayer.Place == 1 ? "st" : gamePlayer.Place == 2 ? "nd" : gamePlayer.Place == 3 ? "rd" : "th")} place!", Color.OrangeRed);
            PlaceCounter--;

            InventoryEdit.ClearPlayerEverything(plr);
            InventoryEdit.AddItem(plr, 0, 1, ItemID.CobaltPickaxe);
            InventoryEdit.AddItem(plr, 9, 1, ItemID.Binoculars);
            InventoryEdit.AddItem(plr, 40, 1, ItemID.CobaltPickaxe);
            InventoryEdit.AddArmor(plr, ItemID.LuckyHorseshoe);
            if (SpleefUserSettings.GetSettings(plr.Account.Name).GetMusicBox)
                InventoryEdit.AddArmor(plr, CurrentMusicBoxID);

            if (PlaceCounter == 1)
                SuicideTimer.Restart();

            if (PlaceCounter <= 0)
            {
                SuicideTimer.Stop();
                StopRound();
            }
        }

        public void StopRound()
        {
            isRound = false;
            RoundPlayers = RoundPlayers.OrderBy(p => p.Place).ToList();
            bool isSuicide = SuicideTimer.Elapsed.TotalSeconds <= 3;
            int[] rewards = rewardTable.FirstOrDefault(r => RoundPlayers.Count >= r.Key.min && RoundPlayers.Count <= r.Key.max).Value.ToArray();
            
            if (isSuicide)
                rewards[0] = rewards[1];

            for (int i = 0; i < rewards.Length; i++)
                RoundPlayers[i].Score += rewards[i];

            var placements = string.Join(" ", RoundPlayers.Take(rewards.Length).Select((p, i) => $"{p.Name} got place {i + 1} ({rewards[i]} pts)"));

            if (!isSuicide)
                TShock.Utils.Broadcast($"Round {RoundCounter - 1} ended! {placements}", Color.LimeGreen);
            else
                TShock.Utils.Broadcast($"Round {RoundCounter - 1} ended! It's a suicide so {placements}", Color.DarkRed);
            ServerApi.Hooks.NetGetData.Deregister(Spleef.Instance, OnGetData);
        }

        public void ForceStopRound()
        {
            ServerApi.Hooks.NetGetData.Deregister(Spleef.Instance, OnGetData);
            isRound = false;
        }

        public void EndGame()
        {
            SpleefCoin.MigrateUsersToSpleefDatabase();
            Players.ForEach(p => SpleefCoin.AddCoins(p.AccountName, p.Score, false));
        }

        public void ShowScore(TSPlayer player)
        {
            Players = Players.OrderByDescending(p => p.Score).ToList();
            string score = "[c/FF1493:Game score:]\n";
            foreach (var p in Players)
            {
                if (p.isIngame)
                {
                    score += $"[c/FF8559:{p.Name} : {p.Score}] ";
                    switch (p.Place)
                    {
                        case 1: 
                            score += "[i:4601]";
                            break;
                        case 2: 
                            score += "[i:4600]";
                            break;
                        case 3: 
                            score += "[i:4599]";
                            break;
                        default: 
                            score += "[i:321]"; 
                            break;
                    }
                }
                else
                    score += $"[c/898989:{p.Name} : {p.Score}]";
                score += "\n";
            }
            player.SendInfoMessage(score);
        }
    }
}

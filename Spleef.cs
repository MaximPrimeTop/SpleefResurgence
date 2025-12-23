using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using System.Timers;
using Terraria.ID;
using Steamworks;
using System.Reflection;
using Terraria.DataStructures;
using SpleefResurgence.Game;
using SpleefResurgence.CustomCommands;

namespace SpleefResurgence
{
    [ApiVersion(2, 1)]
    public class Spleef : TerrariaPlugin
    {
        public static Spleef Instance { get; private set; }
        public static PluginSettings Config => PluginSettings.Config;
        private readonly CustomCommandHandler commandHandler;
        private readonly TileTracker tileTracker;
        private readonly SpleefGame spleefGame;
        private readonly BlockSpam blockSpam;


        public static Random rnd = new();

        public static readonly int[] MusicBoxIDs = { 562, 1600, 564, 1601, 1596, 1602, 1603, 1604, 4077, 4079, 1597, 566, 1964, 1610, 568, 569, 570, 1598, 2742, 571, 573, 3237, 1605, 1608, 567, 572, 574,
            1599, 1607, 5112, 4979, 1606, 4985, 4990, 563, 1609, 3371, 3236, 3235, 1963, 1965, 3370, 3044, 3796, 3869, 4078, 4080, 4081, 4082, 4237, 4356, 4357, 4358, 4421, 4606, 4991, 4992, 5006,
            5014, 5015, 5016, 5017, 5018, 5019, 5020, 5021, 5022, 5023,5024, 5025, 5026, 5027, 5028, 5029, 5030, 5031, 5032, 5033, 5034, 5035, 5036, 5037, 5038, 5039, 5040, 5362, 565 };

        public override string Author => "MaximPrime";
        public override string Name => "Spleef Resurgence Plugin";
        public override string Description => "ok i think it works yipee.";
        public override System.Version Version => new(3, 0);

        public Spleef(Main game) : base(game)
        {
            Instance = this;
            commandHandler = new CustomCommandHandler(this);
            tileTracker = new TileTracker(this);
            spleefGame = new SpleefGame(this);
            blockSpam = new BlockSpam(this, spleefGame);
            spleefGame.SetBlockSpam(blockSpam);
        }

        public static List<CustomCommand> CustomCommands = new();
        public static List<ExecutingCommand> ActiveCommands = new(); 

        private void RegisterCustomCommands()
        {
            if (!Directory.Exists(CustomCommand.CommandsPath))
                Directory.CreateDirectory(CustomCommand.CommandsPath);

            string[] files = Directory.GetFiles(CustomCommand.CommandsPath, "*.json");
            foreach (string file in files)
            {
                CustomCommand command = CustomCommand.FromJson(file);
                if (CustomCommands.Any(c => c.Name.Equals(command.Name)))
                {
                    TShock.Log.Warn($"Duplicate custom command name found: {command.Name}. Skipping registration.");
                    continue;
                }
                CustomCommands.Add(command);
                Commands.ChatCommands.Add(new Command(command.Permission, command.CommandLogic, command.Name));
                TShock.Log.Info($"Registered custom command: {command.Name}");
            }
        }

        public override void Initialize()
        {
            GameConfig.SetupConfig();
            PluginSettings.Load();

            Commands.ChatCommands.Add(new Command("spleef.customcommand", CCCommands.AddCustomCommand, "addcommand", "addc"));
            Commands.ChatCommands.Add(new Command("spleef.customcommand", CCCommands.DeleteCustomCommand, "delcommand", "delc"));
            Commands.ChatCommands.Add(new Command("spleef.customcommand", CCCommands.ListCustomCommand, "listcommand", "listc"));
            Commands.ChatCommands.Add(new Command("spleef.customcommand", CCCommands.ListActiveCommand, "listactive", "lista"));
            ServerApi.Hooks.GameUpdate.Register(this, OnWorldUpdateCommand);

            Commands.ChatCommands.Add(new Command("spleef.game.hoster", GameCommands.GameCommand, "game"));
            Commands.ChatCommands.Add(new Command("spleef.game.user", GameCommands.JoinGame, "join", "j"));
            /*
            Commands.ChatCommands.Add(new Command("spleef.game.hoster", spleefGame.TheGaming, "game"));
            Commands.ChatCommands.Add(new Command("spleef.game.user", spleefGame.JoinGame, "join", "j"));
            Commands.ChatCommands.Add(new Command("spleef.game.user", spleefGame.LeaveGame, "leave", "l"));
            Commands.ChatCommands.Add(new Command("spleef.game.user", spleefGame.CheckScore, "score"));
            Commands.ChatCommands.Add(new Command("spleef.game.user", spleefGame.Betting, "bet"));
            */

            Commands.ChatCommands.Add(new Command("spleef.tiletrack", tileTracker.ToggleTileTracking, "tilepos"));
            Commands.ChatCommands.Add(new Command("spleef.coolsay", Coolsay, "coolsay"));
            Commands.ChatCommands.Add(new Command("spleef.track", blockSpam.ToggleTrackingCommand, "track"));

            Commands.ChatCommands.Add(new Command("spleef.coin.admin", SpleefCoin.AddCoinsCommand, "addcoin"));
            Commands.ChatCommands.Add(new Command("spleef.coin.user", SpleefCoin.GetCoinsCommand, "coin", "c"));
            Commands.ChatCommands.Add(new Command("spleef.coin.user", SpleefCoin.GetLeaderboard, "leaderboard", "lb"));
            Commands.ChatCommands.Add(new Command("spleef.coin.user", SpleefCoin.TransferCoinsCommand, "transfer"));

            Commands.ChatCommands.Add(new Command("spleef.elo.admin", SpleefELO.SetEloCommand, "eloset"));
            Commands.ChatCommands.Add(new Command("spleef.elo.user", SpleefELO.GetEloCommand, "elo"));
            //Commands.ChatCommands.Add(new Command("spleef.elo.user", spleefELO.GetEloLeaderboard, "elo"));

            Commands.ChatCommands.Add(new Command("spleef.coin.user", PenguinPoints, "pp"));
            Commands.ChatCommands.Add(new Command("die", Die, "die"));
            Commands.ChatCommands.Add(new Command("spleef.impersonate", Impersonate, "impersonate", "imp"));

            Commands.ChatCommands.Add(new Command("spleef.inventory", InventoryEdit.InventoryReset, "inventoryreset", "invreset"));
            Commands.ChatCommands.Add(new Command("spleef.inventory", InventoryEdit.InventoryEditCommand, "inventoryedit", "invedit"));
            Commands.ChatCommands.Add(new Command("spleef.inventory", InventoryEdit.ArmorEdit, "armoredit"));
            Commands.ChatCommands.Add(new Command("spleef.inventory", InventoryEdit.MiscEquipsEdit, "miscedit"));
            Commands.ChatCommands.Add(new Command("spleef.inventory", InventoryEdit.SetInventoryCommand, "inventoryset", "invset"));

            Commands.ChatCommands.Add(new Command("spleef.settings", SpleefUserSettings.EditSettingsCommand, "settings", "toggle"));

            RegisterCustomCommands();
            GeneralHooks.ReloadEvent += OnServerReload;
            ServerApi.Hooks.GamePostInitialize.Register(this, OnWorldLoad);
            ServerApi.Hooks.GameUpdate.Register(this, OnWorldUpdateRain);
            ServerApi.Hooks.GameUpdate.Register(this, OnWorldUpdateBuff);
            SpleefCoin.MigrateUsersToSpleefDatabase();
        }

        public static int PaintIDtoItemID (byte id)
        {
            if (id > 30)
                return 0;
            if (id >= 28)
                return id + 1938;
            if (id > 0)
                return id + 1072;
            return 0;
        }

        public static string statusLavariseTime;

        private void Die(CommandArgs args)
        {
            args.Player.KillPlayer();
        }

        private void PenguinPoints(CommandArgs args)
        {
            TSPlayer.All.SendMessage($"{args.Player.Name} just tried using /pp!!! Laugh at this user", Color.OrangeRed);
        }

        private void Impersonate(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("huhhh wajth????11");
                return;
            }
            if (!SpleefGame.isPlayerOnline(args.Parameters[0], out TSPlayer player))
            {
                args.Player.SendErrorMessage("this guy ain't online");
                return;
            }
            string message = string.Join(' ', args.Parameters.ToArray(), 1, args.Parameters.Count - 1);
            TSPlayer.All.SendMessage($"{player.Group.Prefix}{player.Name}: {message}", player.Group.Color);
        }

        private void Coolsay(CommandArgs args)
        {
            string message = string.Join(" ", args.Parameters);
            TShock.Utils.Broadcast(message, 255, 255, 255);
        }

        private void OnWorldUpdateCommand(EventArgs args)
        {
            foreach (ExecutingCommand cmd in ActiveCommands.ToArray())
            {
                cmd.Update();
            }
        }

        private void OnWorldUpdateRain(EventArgs args)
        {
            if (Main.raining)
            {
                Main.raining = false;
                Main.maxRaining = 0f;
                Main.cloudAlpha = 0f;
                TSPlayer.All.SendMessage("Rain has been stopped automatically.", Color.Aqua);
            }
        }

        private void OnWorldUpdateBuff(EventArgs args)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Active && player.IsLoggedIn && player.Account.Name != null && SpleefUserSettings.GetSettings(player.Account.Name).GetBuffs)
                {
                    player.SetBuff(BuffID.Honey, 1000000000);
                    player.SetBuff(BuffID.HeartLamp, 1000000000);
                    player.SetBuff(BuffID.Campfire, 1000000000);
                    player.SetBuff(BuffID.Shine, 1000000000);
                    player.SetBuff(BuffID.NightOwl, 1000000000);
                }
            }
        }

        private void OnWorldLoad(EventArgs args)
        {
            Commands.HandleCommand(TSPlayer.Server, "/time noon");
            Commands.HandleCommand(TSPlayer.Server, "/freezetime");
            Commands.HandleCommand(TSPlayer.Server, "/gamemode normal");
        }

        private void OnServerReload(ReloadEventArgs args)
        {
            var playerReloading = args.Player;

            try
            {
                CustomCommandHandler.DeRegisterCommands();
                PluginSettings.Load();
                CustomCommandHandler.RegisterCommands();
                SpleefCoin.MigrateUsersToSpleefDatabase();
                playerReloading.SendSuccessMessage("[SpleefResurgence] Config reloaded!");
            }
            catch (Exception ex)
            {
                playerReloading.SendErrorMessage("There was an issue loading the config!");
                TShock.Log.ConsoleError(ex.ToString());
            }
        }     
    }
}

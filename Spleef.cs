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

namespace SpleefResurgence
{
    [ApiVersion(2, 1)]
    public class Spleef : TerrariaPlugin
    {
        public static PluginSettings Config => PluginSettings.Config;
        private SpleefCoin spleefCoin;
        private readonly CustomCommandHandler commandHandler;
        private readonly TileTracker tileTracker;
        private readonly InventoryEdit inventoryEdit;
        private readonly SpleefUserSettings spleefSettings;
        private readonly SpleefGame spleefGame;
        private readonly BlockSpam blockSpam;
        private readonly SpleefELO spleefELO;

        public static Random rnd = new();

        public override string Author => "MaximPrime";
        public override string Name => "Spleef Resurgence Plugin";
        public override string Description => "ok i think it works yipee.";
        public override System.Version Version => new(2, 1, 2);

        public Spleef(Main game) : base(game)
        {
            spleefCoin = new SpleefCoin();
            commandHandler = new CustomCommandHandler(this);
            tileTracker = new TileTracker(this);
            inventoryEdit = new InventoryEdit();
            spleefSettings = new SpleefUserSettings(spleefCoin);
            spleefELO = new SpleefELO(spleefCoin);
            spleefGame = new SpleefGame(this, spleefCoin, inventoryEdit, spleefSettings, spleefELO);
            blockSpam = new BlockSpam(this, spleefSettings, spleefGame);
            spleefGame.SetBlockSpam(blockSpam);
        }
        public override void Initialize()
        {
            PluginSettings.Load();
            CustomCommandHandler.RegisterCommands();
            Commands.ChatCommands.Add(new Command("spleef.customcommand", commandHandler.AddCustomCommand, "addcommand", "addc"));
            Commands.ChatCommands.Add(new Command("spleef.customcommand", commandHandler.RemoveCustomCommand, "delcommand", "delc"));
            Commands.ChatCommands.Add(new Command("spleef.customcommand", commandHandler.ListCustomCommand, "listcommand", "listc"));
            Commands.ChatCommands.Add(new Command("spleef.customcommand", commandHandler.ListActiveCommand, "listactive", "lista"));

            Commands.ChatCommands.Add(new Command("spleef.game.hoster", spleefGame.TheGaming, "game"));
            Commands.ChatCommands.Add(new Command("spleef.game.user", spleefGame.JoinGame, "join", "j"));
            Commands.ChatCommands.Add(new Command("spleef.game.user", spleefGame.LeaveGame, "leave", "l"));
            Commands.ChatCommands.Add(new Command("spleef.game.user", spleefGame.CheckScore, "score"));
            Commands.ChatCommands.Add(new Command("spleef.game.user", spleefGame.Betting, "bet"));

            Commands.ChatCommands.Add(new Command("spleef.tiletrack", tileTracker.ToggleTileTracking, "tilepos"));
            Commands.ChatCommands.Add(new Command("spleef.coolsay", Coolsay, "coolsay"));
            Commands.ChatCommands.Add(new Command("spleef.track", blockSpam.ToggleTrackingCommand, "track"));

            Commands.ChatCommands.Add(new Command("spleef.coin.admin", spleefCoin.AddCoinsCommand, "addcoin"));
            Commands.ChatCommands.Add(new Command("spleef.coin.user", spleefCoin.GetCoinsCommand, "coin", "c"));
            Commands.ChatCommands.Add(new Command("spleef.coin.user", spleefCoin.GetLeaderboard, "leaderboard", "lb"));
            Commands.ChatCommands.Add(new Command("spleef.coin.user", spleefCoin.TransferCoinsCommand, "transfer"));

            Commands.ChatCommands.Add(new Command("spleef.elo.admin", spleefELO.SetEloCommand, "eloset"));
            Commands.ChatCommands.Add(new Command("spleef.elo.user", spleefELO.GetEloCommand, "elo"));
            //Commands.ChatCommands.Add(new Command("spleef.elo.user", spleefELO.GetEloLeaderboard, "elo"));

            Commands.ChatCommands.Add(new Command("spleef.coin.user", PenguinPoints, "pp"));
            Commands.ChatCommands.Add(new Command("die", Die, "die"));
            Commands.ChatCommands.Add(new Command("spleef.impersonate", Impersonate, "impersonate", "imp"));

            Commands.ChatCommands.Add(new Command("spleef.inventory", inventoryEdit.InventoryReset, "inventoryreset", "invreset"));
            Commands.ChatCommands.Add(new Command("spleef.inventory", inventoryEdit.InventoryEditCommand, "inventoryedit", "invedit"));
            Commands.ChatCommands.Add(new Command("spleef.inventory", inventoryEdit.ArmorEdit, "armoredit"));
            Commands.ChatCommands.Add(new Command("spleef.inventory", inventoryEdit.MiscEquipsEdit, "miscedit"));
            Commands.ChatCommands.Add(new Command("spleef.inventory", inventoryEdit.SetInventoryCommand, "inventoryset", "invset"));

            Commands.ChatCommands.Add(new Command("spleef.settings", spleefSettings.EditSettingsCommand, "settings", "toggle"));

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
                if (player != null && player.Active && player.IsLoggedIn && player.IsLoggedIn && player.Account.Name != null && spleefSettings.GetSettings(player.Account.Name).GetBuffs)
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

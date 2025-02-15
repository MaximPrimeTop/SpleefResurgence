using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using System.Threading.Tasks;
using Terraria.ID;
using System.Diagnostics;

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
        private readonly SpleefGame spleefGame;

        public override string Author => "MaximPrime";
        public override string Name => "Spleef Resurgence Plugin";
        public override string Description => "ok i think it works yipee.";
        public override Version Version => new(1, 5, 4);

        public Spleef(Main game) : base(game)
        {
            spleefCoin = new SpleefCoin();
            commandHandler = new CustomCommandHandler();
            tileTracker = new TileTracker(this);
            inventoryEdit = new InventoryEdit();
            spleefGame = new SpleefGame(this, spleefCoin, inventoryEdit);
        }
        public override void Initialize()
        {
            PluginSettings.Load();
            CustomCommandHandler.RegisterCommands();
            Commands.ChatCommands.Add(new Command("spleef.customcommand", commandHandler.AddCustomCommand, "addcommand", "addc"));
            Commands.ChatCommands.Add(new Command("spleef.customcommand", commandHandler.RemoveCustomCommand, "delcommand", "delc"));
            Commands.ChatCommands.Add(new Command("spleef.customcommand", commandHandler.ListCustomCommand, "listcommand", "listc"));
            
            Commands.ChatCommands.Add(new Command("spleef.game", spleefGame.TheGaming, "game"));

            Commands.ChatCommands.Add(new Command("spleef.tiletrack", tileTracker.ToggleTileTracking, "tilepos"));
            Commands.ChatCommands.Add(new Command("spleef.coolsay", Coolsay, "coolsay"));

            Commands.ChatCommands.Add(new Command("spleef.coin.admin", spleefCoin.AddCoinsCommand, "addcoin"));
            Commands.ChatCommands.Add(new Command("spleef.coin.user", spleefCoin.GetCoinsCommand, "coin", "c"));
            Commands.ChatCommands.Add(new Command("spleef.coin.user", spleefCoin.GetLeaderboard, "leaderboard", "lb"));
            Commands.ChatCommands.Add(new Command("spleef.coin.user", spleefCoin.TransferCoinsCommand, "transfer"));

            Commands.ChatCommands.Add(new Command("spleef.coin.user", PenguinPoints, "pp"));
            Commands.ChatCommands.Add(new Command("die", Die, "die"));

            Commands.ChatCommands.Add(new Command("spleef.inventory", InventoryReset, "inventoryreset", "invreset"));
            Commands.ChatCommands.Add(new Command("spleef.inventory", InventoryEditCommand, "inventoryedit", "invedit"));
            Commands.ChatCommands.Add(new Command("spleef.inventory", ArmorEdit, "armoredit"));
            Commands.ChatCommands.Add(new Command("spleef.inventory", MiscEquipsEdit, "miscedit"));

            GeneralHooks.ReloadEvent += OnServerReload;
            ServerApi.Hooks.GamePostInitialize.Register(this, OnWorldLoad);
            ServerApi.Hooks.GameUpdate.Register(this, OnWorldUpdate);

            SpleefCoin.MigrateUsersToSpleefDatabase();
        }

        private void InventoryReset(CommandArgs args)
        {
            string player = args.Parameters[0];
            var players = TSPlayer.FindByNameOrID(player);
            if (players == null || players.Count == 0)
            {
                args.Player.SendErrorMessage("worg");
                return;
            }
            var plr = players[0];
            inventoryEdit.ClearPlayerEverything(plr);
        }

        private void InventoryEditCommand(CommandArgs args)
        {
            string player = args.Parameters[0];
            var players = TSPlayer.FindByNameOrID(player);
            if (players == null || players.Count == 0)
            {
                args.Player.SendErrorMessage("worg");
                return;
            }
            var plr = players[0];
            int slot = Convert.ToInt32(args.Parameters[1]);
            int stack = Convert.ToInt32(args.Parameters[2]);
            int itemID = Convert.ToInt32(args.Parameters[3]);
            inventoryEdit.AddItem(plr, slot, stack, itemID);
        }

        private void ArmorEdit(CommandArgs args)
        {
            string player = args.Parameters[0];
            var players = TSPlayer.FindByNameOrID(player);
            if (players == null || players.Count == 0)
            {
                args.Player.SendErrorMessage("worg");
                return;
            }
            var plr = players[0];
            int slot = Convert.ToInt32(args.Parameters[1]);
            int itemID = Convert.ToInt32(args.Parameters[2]);
            inventoryEdit.AddArmor(plr, slot, itemID);
        }

        private void MiscEquipsEdit(CommandArgs args)
        {
            string player = args.Parameters[0];
            var players = TSPlayer.FindByNameOrID(player);
            if (players == null || players.Count == 0)
            {
                args.Player.SendErrorMessage("worg");
                return;
            }
            var plr = players[0];
            int slot = Convert.ToInt32(args.Parameters[1]);
            int itemID = Convert.ToInt32(args.Parameters[2]);
            inventoryEdit.AddMiscEquip(plr, slot, itemID);
        }

        private void Die(CommandArgs args)
        {
            args.Player.KillPlayer();
        }

        private void PenguinPoints(CommandArgs args)
        {
            TSPlayer.All.SendMessage($"{args.Player.Name} just tried using /pp!!! Laugh at this user", Color.OrangeRed);
        }

        private void Coolsay(CommandArgs args)
        {
            string message = string.Join(" ", args.Parameters);
            TShock.Utils.Broadcast(message, 255, 255, 255);
        }

        private void OnWorldUpdate(EventArgs args)
        {
            if (Main.raining)
            {
                Main.raining = false;
                Main.maxRaining = 0f;
                Main.cloudAlpha = 0f;
                TSPlayer.All.SendMessage("Rain has been stopped automatically.", Color.Aqua);
            }
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Active)
                {
                    player.SetBuff(BuffID.Honey, 600);
                    player.SetBuff(BuffID.Shine, 600);
                    player.SetBuff(BuffID.NightOwl, 600);
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
                playerReloading.SendSuccessMessage("[SpleefPlugin] Config reloaded!");
            }
            catch (Exception ex)
            {
                playerReloading.SendErrorMessage("There was an issue loading the config!");
                TShock.Log.ConsoleError(ex.ToString());
            }
        }     
    }
}

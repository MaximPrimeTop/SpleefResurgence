﻿using Microsoft.Xna.Framework;
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
        //private readonly InventoryEdit inventoryEdit;
        private readonly SpleefGame spleefGame;

        public override string Author => "MaximPrime";
        public override string Name => "Spleef Resurgence Plugin";
        public override string Description => "ok i think it works yipee.";
        public override Version Version => new(1, 4, 2);

        public Spleef(Main game) : base(game)
        {
            spleefCoin = new SpleefCoin();
            commandHandler = new CustomCommandHandler();
            tileTracker = new TileTracker(this);
            //inventoryEdit = new InventoryEdit();
            spleefGame = new SpleefGame(this, spleefCoin/*, inventoryEdit*/);
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
            Commands.ChatCommands.Add(new Command("spleef.coin.user", spleefCoin.GetCoinsCommand, "coin"));
            Commands.ChatCommands.Add(new Command("spleef.coin.user", spleefCoin.GetLeaderboard, "leaderboard", "lb"));

            GeneralHooks.ReloadEvent += OnServerReload;
            ServerApi.Hooks.GamePostInitialize.Register(this, OnWorldLoad);
            ServerApi.Hooks.GameUpdate.Register(this, OnWorldUpdate);

            spleefCoin.MigrateUsersToSpleefDatabase();
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
                }
            }
        }

        private void OnWorldLoad(EventArgs args)
        {
            Commands.HandleCommand(TSPlayer.Server, "/time noon");
            Commands.HandleCommand(TSPlayer.Server, "/freezetime");
        }

        private void OnServerReload(ReloadEventArgs args)
        {
            var playerReloading = args.Player;

            try
            {
                CustomCommandHandler.DeRegisterCommands();
                PluginSettings.Load();
                CustomCommandHandler.RegisterCommands();
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

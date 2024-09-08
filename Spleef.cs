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
        private readonly CustomCommandHandler commandHandler;
        private readonly TileTracker tileTracker;

        public override string Author => "MaximPrime";
        public override string Name => "Spleef Resurgence Plugin";
        public override string Description => "please work finally like what the fuck.";
        public override Version Version => new(1, 2);

        public Spleef(Main game) : base(game)
        {
            commandHandler = new CustomCommandHandler();
            tileTracker = new TileTracker(this);
        }
        public override void Initialize()
        {
            PluginSettings.Load();
            CustomCommandHandler.RegisterCommands();
            Commands.ChatCommands.Add(new Command("spleef.customcommand", commandHandler.AddCustomCommand, "addcommand"));
            Commands.ChatCommands.Add(new Command("spleef.customcommand", commandHandler.RemoveCustomCommand, "delcommand"));
            Commands.ChatCommands.Add(new Command("spleef.customcommand", commandHandler.ListCustomCommand, "listcommand"));
            
            Commands.ChatCommands.Add(new Command("spleef.tiletrack", tileTracker.ToggleTileTracking, "tilepos"));

            GeneralHooks.ReloadEvent += OnServerReload;
            ServerApi.Hooks.GamePostInitialize.Register(this, OnWorldLoad);
            ServerApi.Hooks.GameUpdate.Register(this, OnWorldUpdate);
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
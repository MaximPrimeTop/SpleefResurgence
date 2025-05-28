using Terraria;
using System;
using TShockAPI;
using TShockAPI.Hooks;
using TerrariaApi.Server;
using System.Diagnostics;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Asn1.BC;
using NuGet.Protocol.Plugins;
using System.Collections.Generic;

namespace SpleefResurgence
{
    public class BlockSpam
    {
        private readonly Spleef pluginInstance;
        private readonly SpleefUserSettings spleefSettings;
        private readonly SpleefGame spleefGame;

        public BlockSpam(Spleef plugin, SpleefUserSettings spleefSettings, SpleefGame spleefGame)
        {
            this.pluginInstance = plugin;
            this.spleefSettings = spleefSettings;
            this.spleefGame = spleefGame;
            ServerApi.Hooks.NetGetData.Register(pluginInstance, OnTileEdit);
            ServerApi.Hooks.GameUpdate.Register(pluginInstance, CombinedUpdate);
            ServerApi.Hooks.ServerJoin.Register(pluginInstance, OnPlayerJoin);
            ServerApi.Hooks.ServerLeave.Register(pluginInstance, OnPlayerLeave);
        }

        private void CombinedUpdate(EventArgs args)
        {
            OnWorldUpdate();
            TrackerUpdate();
        }

        class Tracker
        {
            public string name;
            public bool isTracking = false;
            public Stopwatch Timer = new();
            public Stopwatch Timering = new();
            public Stopwatch FullTimer = new();

            public int state = 0; //0 - not tracking, 1 - tracking but no spam yet, 2 - started spamming, 3 - checking if spamming, 4 - checking if spamming 2, 5 - blockspamming more than 20 seconds ok this might be wrong but idc

            public Tracker (string name, int state = 1, bool isTracking = true)
            {
                this.name = name;
                this.isTracking = isTracking;
                Timer = new();
                Timering = new();
                FullTimer = new();
                this.state = state;
            }

            public void StopTracking()
            {
                isTracking = false;
                state = 0;
                Timer.Reset();
                Timering.Reset();
                FullTimer.Reset();
            }

            public void ContinueTracking()
            {
                isTracking = true;
                state = 1;
            }
        }

        private readonly Dictionary<string, Tracker> Trackers = new();

        private void OnPlayerJoin(JoinEventArgs args)
        {
            var player = TShock.Players[args.Who];
            ToggleTracking(player, true, true);
        }

        private void OnPlayerLeave(LeaveEventArgs args)
        {
            Trackers.Remove(TShock.Players[args.Who]. Name);
        }

        public void FullTimerAnnounce()
        {
            foreach (var tracker in Trackers)
            {
                string name = tracker.Value.name;
                bool isTracking = tracker.Value.isTracking;
                if (isTracking)
                    TSPlayer.All.SendInfoMessage($"{name} - {tracker.Value.FullTimer.Elapsed.TotalSeconds:N3}\n");
                else
                    TSPlayer.All.SendInfoMessage($"{name} - N/A\n");
                tracker.Value.FullTimer.Reset();
            }
        }

        private string UpdateTracking()
        {
            string res = "\n\n\n\n\n\n\n\n\n\n\n\n";
            foreach (var tracker in Trackers)
            {
                string name = tracker.Value.name;
                bool isTracking = tracker.Value.isTracking;
                if (isTracking)
                    res += ($"{name} - {tracker.Value.Timer.Elapsed.TotalSeconds:N3} - {tracker.Value.Timering.Elapsed.TotalSeconds:N3} - {tracker.Value.FullTimer.Elapsed.TotalSeconds:N3}\n");
                else
                    res += ($"{name} - N/A\n");
            }
            return res;
        }

        private void OnWorldUpdate()
        {
            foreach (TSPlayer player in TShock.Players)
                if (player != null && player.Active && player.IsLoggedIn && player.Name != null && spleefSettings.GetSettings(player.Name).BlockSpamDebug)
                    player.SendData(PacketTypes.Status, UpdateTracking(), number2: 1);
        }
        
        public void ToggleTrackingCommand(CommandArgs args)
        {
            var player = TSPlayer.FindByNameOrID(args.Parameters[0])[0];

            if (!Trackers.ContainsKey(player.Name))
                ToggleTracking(player, true, true);

            else if (args.Parameters.Count == 1)
                ToggleTracking(player, true, false);
            else
            {
                if (args.Parameters[1] == "start")
                    ToggleTracking(player, true, false);
                else if (args.Parameters[1] == "stop")
                    ToggleTracking(player, false, false);
                else
                    args.Player.SendErrorMessage("start or stop idio");
            }
        }

        public void ToggleTracking(TSPlayer player, bool track, bool isNew)
        {
            if (track)
            {
                if (isNew)
                    Trackers[player.Name] = new(player.Name);
                else
                    Trackers[player.Name].ContinueTracking();
            }
            else
                Trackers[player.Name].StopTracking();
        }

        private void OnTileEdit(GetDataEventArgs args)
        {
            if (args.Handled || args.MsgID != PacketTypes.Tile)
                return;

            var player = TShock.Players[args.Msg.whoAmI];

            if (player == null || !player.Active || !Trackers.ContainsKey(player.Name) || !Trackers[player.Name].isTracking)
                return;

            using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
            {
                var action = reader.ReadByte();
                if (action == 1)
                {
                    Trackers[player.Name].Timering.Restart();
                    Trackers[player.Name].state = 1;
                    if (!Trackers[player.Name].Timer.IsRunning)
                        Trackers[player.Name].Timer.Start();
                    if (!Trackers[player.Name].FullTimer.IsRunning && spleefGame.isRound)
                        Trackers[player.Name].FullTimer.Start();
                    //TSPlayer.All.SendInfoMessage($"{player.Name} placed a block");
                }
            }
        }

        private void TrackerUpdate ()
        {
            
            foreach (var tracker in Trackers)
            {
                var tr = tracker.Value;
                if (tr.state == 1 && tr.Timering.ElapsedMilliseconds > 500)
                {
                    //TSPlayer.All.SendInfoMessage($"looking if {tr.GetName()} stopped block spamming.");
                    tr.Timer.Stop();
                    tr.FullTimer.Stop();
                    tr.state = 4;
                }
                /*
                if (tr.state == 2 && tr.Timering.ElapsedMilliseconds > 1000)
                {
                    //TSPlayer.All.SendInfoMessage($"looking if {tr.GetName()} stopped block spamming..");
                    tr.state = 3;
                }

                if (tr.state == 3 && tr.Timering.ElapsedMilliseconds > 1500)
                {
                    //TSPlayer.All.SendInfoMessage($"looking if {tr.GetName()} stopped block spamming...");
                    tr.state = 4;
                }
                */
                if (tr.state == 4 && tr.Timering.ElapsedMilliseconds > 2000)
                {
                    //TSPlayer.All.SendSuccessMessage($"{tr.GetName()} stopped block spamming!");
                    tr.Timer.Reset();
                    tr.Timering.Reset();
                    tr.state = 1;
                }

                if (tr.state == 1 && tr.Timer.ElapsedMilliseconds > 20000)
                {
                    TSPlayer.All.SendErrorMessage($"{tr.name} has been block spamming for 20 seconds, mods execute this guy");
                    tr.Timer.Reset();
                    tr.Timering.Reset();
                    tr.state = 5;
                }
            }
        }
    }
}

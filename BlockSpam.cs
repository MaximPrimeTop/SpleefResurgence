using TShockAPI;
using TerrariaApi.Server;
using System.Diagnostics;

namespace SpleefResurgence;

enum State
{
    NotTracking,
    Tracking,
    Resetting
}

public class BlockSpam
{
    private readonly Spleef pluginInstance;
    private readonly SpleefUserSettings spleefSettings;
    private readonly SpleefGame spleefGame;

    private readonly Dictionary<string, Tracker> Trackers = new();

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
        public Stopwatch BlockSpamTimer = new();
        public Stopwatch TimeSinceTilePlaced = new();
        public Stopwatch TotalBlockSpamTimer = new();

        public State spamState = 0;

        public Tracker (string name, State spamState = State.Tracking, bool isTracking = true)
        {
            this.name = name;
            BlockSpamTimer = new();
            TimeSinceTilePlaced = new();
            TotalBlockSpamTimer = new();
            this.spamState = spamState;
        }

        public void StopTracking()
        {
            spamState = State.NotTracking;
            BlockSpamTimer.Reset();
            TimeSinceTilePlaced.Reset();
            TotalBlockSpamTimer.Reset();
        }
    }

    private void OnPlayerJoin(JoinEventArgs args)
    {
        var player = TShock.Players[args.Who];
        ToggleTracking(player, true, true);
    }

    private void OnPlayerLeave(LeaveEventArgs args)
    {
        Trackers.Remove(TShock.Players[args.Who].Name);
    }

    public void FullTimerAnnounce()
    {
        foreach (var tracker in Trackers)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Active && player.IsLoggedIn && player.Account.Name != null && spleefSettings.GetSettings(player.Account.Name).BlockSpamDebug)
                {
                    string name = tracker.Value.name;
                    if (tracker.Value.spamState != State.NotTracking) //is tracking
                        player.SendInfoMessage($"{name} - {tracker.Value.TotalBlockSpamTimer.Elapsed.TotalSeconds:N3}\n");
                    else
                        player.SendInfoMessage($"{name} - N/A\n");
                }
            }
            tracker.Value.TotalBlockSpamTimer.Reset();
        }
    }

    private string UpdateTracking()
    {
        string res = "\n\n\n\n\n\n\n\n\n\n\n\n";
        foreach (var tracker in Trackers)
        {
            string name = tracker.Value.name;
            if (tracker.Value.spamState != State.NotTracking)
            {
                res += ($"{name} - {tracker.Value.BlockSpamTimer.Elapsed.TotalSeconds:N3} - {tracker.Value.TimeSinceTilePlaced.Elapsed.TotalSeconds:N3} - {tracker.Value.TotalBlockSpamTimer.Elapsed.TotalSeconds:N3}\n");
            }
            else
            {
                res += ($"{name} - N/A\n");
            }
        }
        return res;
    }

    private void OnWorldUpdate()
    {
        foreach (TSPlayer player in TShock.Players)
            if (player != null && player.Active && player.IsLoggedIn && player.Account.Name != null && spleefSettings.GetSettings(player.Account.Name).BlockSpamDebug)
                player.SendData(PacketTypes.Status, UpdateTracking(), number2: 1);
    }
    
    public void ToggleTrackingCommand(CommandArgs args)
    {
        var player = TSPlayer.FindByNameOrID(args.Parameters[0])[0];

        if (!Trackers.ContainsKey(player.Name))
        {
            ToggleTracking(player, true, true);
        }
        else if (args.Parameters.Count == 1)
        {
            ToggleTracking(player, true, false);
        }
        else
        {
            if (args.Parameters[1] == "start")
            {
                ToggleTracking(player, true, false);
            }
            else if (args.Parameters[1] == "stop")
            {
                ToggleTracking(player, false, false);
            }
            else
            {
                args.Player.SendErrorMessage("start or stop idio");
            }
        }
    }

    public void ToggleTracking(TSPlayer player, bool track, bool isNew)
    {
        if (track)
        {
            if (isNew)
            {
                Trackers[player.Name] = new(player.Name);
            }
            else
            {
                Trackers[player.Name].spamState = State.Tracking;
            }
        }
        else
        {
            Trackers[player.Name].StopTracking();
        }
    }

    private void OnTileEdit(GetDataEventArgs args)
    {
        if (args.Handled || args.MsgID != PacketTypes.Tile)
            return;

        var player = TShock.Players[args.Msg.whoAmI];

        if (player == null || !player.Active || !Trackers.ContainsKey(player.Name) || Trackers[player.Name].spamState == State.NotTracking)
            return;

        using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
        {
            var action = reader.ReadByte();
            if (action == 1)
            {
                Trackers[player.Name].TimeSinceTilePlaced.Restart();
                Trackers[player.Name].spamState = State.Tracking;
                if (!Trackers[player.Name].BlockSpamTimer.IsRunning)
                    Trackers[player.Name].BlockSpamTimer.Start();

                if (!Trackers[player.Name].TotalBlockSpamTimer.IsRunning && spleefGame.isRound)
                    Trackers[player.Name].TotalBlockSpamTimer.Start();
            }
        }
    }

    private void TrackerUpdate()
    {
        foreach (var tracker in Trackers)
        {
            var tr = tracker.Value;
            if (tr.spamState == State.Tracking && tr.TimeSinceTilePlaced.ElapsedMilliseconds > 500)
            {
                tr.BlockSpamTimer.Stop();
                tr.TotalBlockSpamTimer.Stop();
                tr.spamState = State.Resetting;
            }

            if (tr.spamState == State.Resetting && tr.TimeSinceTilePlaced.ElapsedMilliseconds > 2000)
            {
                tr.BlockSpamTimer.Reset();
                tr.TimeSinceTilePlaced.Reset();
                tr.spamState = State.Tracking;
            }

            if (tr.spamState == State.Tracking && tr.BlockSpamTimer.ElapsedMilliseconds > 20000)
            {
                TSPlayer.All.SendErrorMessage($"{tr.name} has been block spamming for 20 seconds, mods execute this guy");
                tr.BlockSpamTimer.Reset();
                tr.TimeSinceTilePlaced.Reset();
            }
        }
    }
}

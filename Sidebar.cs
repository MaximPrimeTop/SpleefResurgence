using System.Timers;
using TerrariaApi.Server;
using TShockAPI;
using Timer = System.Timers.Timer;

namespace SpleefResurgence
{
    public class Sidebar
    {
        private Dictionary<int, string> prevSidebars = new();

        public abstract class SidebarPart
        {
            public string message { get; set; }
            public int priority { get; set; }

            public abstract bool condition(TSPlayer player);

            public void Kill()
            {
                RemovePart(this);
            }
        }

        private class Pretext : SidebarPart
        {
            public override bool condition(TSPlayer player) => true;

            public Pretext(string message)
            {
                this.message = message;
                priority = 1;
            }
        }

        public class TimedAnnouncement : SidebarPart
        {
            private Timer endTimer = new();

            public override bool condition(TSPlayer player) => endTimer.Enabled;

            public TimedAnnouncement(string message, int seconds)
            {
                this.message = message;
                Spleef.ResetTimer(ref endTimer, TimeKill, seconds * 1000, false);
                priority = 0;
            }

            private void TimeKill(object sender, ElapsedEventArgs args)
            {
                Kill();
            }
        }

        public class LavariseSidebar : SidebarPart
        {
            public override bool condition(TSPlayer player) => SpleefUserSettings.GetSettings(player.Name, false).ShowLavarise;

            public LavariseSidebar()
            {
                priority = 2;
                message = "";
            }
        }

        public class BlockSpamDebugSidebar : SidebarPart
        {
            public override bool condition(TSPlayer player) => SpleefUserSettings.GetSettings(player.Name, false).BlockSpamDebug;

            public BlockSpamDebugSidebar()
            {
                priority = 2;
                message = "";
                AddPart(this);
            }
        }

        public class ScoreSidebar : SidebarPart
        {
            public override bool condition(TSPlayer player) => SpleefUserSettings.GetSettings(player.Name, false).ShowScore;

            public ScoreSidebar()
            {
                priority = 3;
                message = "[c/FF1493:Game score:]\n";
                AddPart(this);
            }
        }

        public class RoundSidebar : SidebarPart
        {
            public override bool condition(TSPlayer player) => SpleefUserSettings.GetSettings(player.Name, false).ShowScore;

            public RoundSidebar()
            {
                priority = 3; // same priority as the score so make sure to initialize it after the score sidebar
                message = "";
                AddPart(this);
            }
        }

        private static List<SidebarPart> ActiveParts = new();

        public void Initialize()
        {
            AddPart(new Pretext("\n\n\n\n\n\n\n\n\n\n\n\n hello guys this is me testing shit\n"));
            ServerApi.Hooks.GameUpdate.Register(Spleef.Instance, SendSidebarToAll);
            Commands.ChatCommands.Add(new Command("spleef.sidebar", Sidebar.AnnounceSidebar, "announce"));
        }

        public static void AddPart(SidebarPart part)
        {
            ActiveParts.Add(part);
            ActiveParts = ActiveParts.OrderBy(p => p.priority).ToList();
        }

        public static void RemovePart(SidebarPart part)
        {
            ActiveParts.Remove(part);
        }

        private void SendSidebar(TSPlayer player, bool forced = false)
        {
            // in the future some way to detect if the person's on mobile, prob like another toggle in the user settings
            string message = "";
            foreach (var part in ActiveParts)
                if (part.condition(player))
                    message += part.message;

            if (!forced && prevSidebars.ContainsKey(player.Index) && message == prevSidebars[player.Index])
                return;

            prevSidebars[player.Index] = message;
            player.SendData(PacketTypes.Status, message, number2: 3);
        }

        private int forcedTickCounter = 0;

        private void SendSidebarToAll(EventArgs args)
        {
            forcedTickCounter++;
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Active)
                {
                    if (forcedTickCounter == 600)
                        SendSidebar(player, true);
                    else
                        SendSidebar(player);
                }
            }
            if (forcedTickCounter == 600)
                forcedTickCounter = 0;
        }

        public static void AnnounceSidebar(CommandArgs args)
        {
            int time = int.Parse(args.Parameters[0]);
            string message = args.Parameters[1];

            AddPart(new TimedAnnouncement(message, time));
        }
    }
}

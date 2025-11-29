using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TShockAPI;
using Timer = System.Timers.Timer;
namespace SpleefResurgence.CustomCommands
{
    public class ExecutingCommand
    {
        public CustomCommand Command;
        public TSPlayer Player;
        public Queue<string> CommandQueue = new Queue<string>();
        public bool hasParent => Parent != null;
        public ExecutingCommand Parent;
        public Timer Timer = new();
        public bool isStopped = false;

        public ExecutingCommand(TSPlayer player, CustomCommand command, ExecutingCommand parent = null)
        {
            Player = player;
            Command = command;
            CommandQueue = new(command.CommandList);
            Parent = parent;
        }

        public void PauseExecution()
        {
            Timer.Stop();
            isStopped = true;
        }

        public void ContinueExecution()
        {
            if (isStopped)
            {
                isStopped = false;
                DoCommands();
            }
        }

        public static void ResetTimer(ref Timer timer, ElapsedEventHandler handler, double interval, bool autoReset = true)
        {
            timer.Stop();
            timer.Dispose();
            timer = new Timer(interval)
            {
                AutoReset = autoReset,
                Enabled = true
            };
            timer.Elapsed += handler;
        }

        public void DoCommands()
        {
            if (CommandQueue.Count == 0)
            {
                StopExecution();
                return;
            }
            string command = CommandQueue.Dequeue();
            double waittime = 0;
            if (command[0..3] == "wait" && !double.TryParse(command.Split(' ')[1], out waittime))
                Player.SendErrorMessage("Invalid time. Check the command's config. Using 0s");

            if (waittime == 0)
            {
                ExecuteCommand(command);
                return;
            }
            ResetTimer(ref Timer, (sender, e) => DoCommands(), waittime * 1000, false);
        }

        public void StopExecution()
        {
            Timer.Stop();
            Timer.Dispose();
            Player.SendSuccessMessage($"Finished command: {Command.Name}");
            if (hasParent)
            {
                Parent.ContinueExecution();
                Player.SendInfoMessage($"Continuing: {Command.Name}");
            }
            Spleef.ActiveCommands.Remove(this);
        }

        public bool isCustomCommand(string name)
        {
            return Spleef.CustomCommands.Exists(c => c.Name == name);
        }

        private void ExecuteCommand(string command)
        {
            List<string> cmds = command.Split(' ').ToList();
            Player.SendInfoMessage($"Executing command: {command}");
            if (cmds.Count == 0)
                return;
            if (command[0] == '/' || command[0] == '.')
            {
                if (isCustomCommand(command))
                {
                    CustomCommand customcommand = Spleef.CustomCommands.Find(c => c.Name == cmds[0].Substring(1));
                    customcommand.ExecuteCommands(this);
                    PauseExecution();
                }
                else
                {
                    Commands.HandleCommand(Player, command);
                    DoCommands();
                }
                return;
            }
            switch (cmds[0])
            {
                case "paint":
                    break;
                case "paintmore":
                    break;
                case "replaceblock":
                    break;
                case "replacemore":
                    break;
                case "replacerandom":
                    break;
                case "rise":
                    return;
            }
        }
    }
}

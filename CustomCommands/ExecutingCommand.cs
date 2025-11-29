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
        public bool hasParent = false;
        public ExecutingCommand Parent;
        public Timer Timer = new();
        public bool isStopped = false;

        public ExecutingCommand(TSPlayer player, List<string> commandList)
        {
            Player = player;
            CommandQueue = new(commandList);
        }

        public ExecutingCommand(TSPlayer player, List<string> commandList, ExecutingCommand parent)
        {
            Player = player;
            CommandQueue = new(commandList);
            hasParent = true;
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
            {
                Player.SendErrorMessage("Invalid time. Check the command's config. Using 0s.");
            }
            else
                ExecuteCommand(command);

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
        }

        private void ExecuteCommand(string command)
        {
            List<string> cmds = command.Split(' ').ToList();
            Player.SendInfoMessage($"Executing command: {command}");
            if (cmds.Count == 0)
                return;
            if (command[0] == '/' || command[0] == '.')
                Commands.HandleCommand(Player, command);
            switch (cmds[0])
            {
                case "paint":
                    return;
            }
        }
    }
}

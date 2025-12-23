using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TerrariaApi.Server;
using TShockAPI;
using Terraria.ID;
namespace SpleefResurgence.CustomCommands
{
    public class ExecutingCommand
    {
        public CustomCommand Command;
        public TSPlayer Player;
        public Queue<string> CommandQueue = new Queue<string>();
        public bool hasParent => Parent != null;
        public ExecutingCommand Parent;
        public bool isPaused = false;
        public int TickDelay = 0;

        private bool skipTimeWait = false;

        public ExecutingCommand(TSPlayer player, CustomCommand command, ExecutingCommand parent = null)
        {
            Player = player;
            Command = command;
            CommandQueue = new(command.CommandList);
            Parent = parent;
        }

        public void Update()
        {
            if (isPaused)
                return;

            if (TickDelay > 0)
            {
                TickDelay--;
                if (TickDelay == 0)
                    DoCommands();
            }
        }

        public void StartExecution()
        {
            Player.SendInfoMessage($"Starting command: {Command.Name}");
            if (!TShock.Players.Any(p => p?.Active == true))
            {
                skipTimeWait = true;
                Player.SendWarningMessage("yo you think you're slick, but sorry time based commands only work when there are players online, so all the commands are gonna get run immediately :3");
                return;
            }
            Spleef.ActiveCommands.Add(this);
            DoCommands();
        }

        public void PauseExecution() => isPaused = true;

        public void ContinueExecution() => isPaused = false;

        public void StopExecution()
        {
            Player.SendSuccessMessage($"Finished command: {Command.Name}");
            if (hasParent)
            {
                Parent.ContinueExecution();
                Player.SendInfoMessage($"Continuing: {Command.Name}");
            }
            Spleef.ActiveCommands.Remove(this);
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
            if (!skipTimeWait && command.StartsWith("wait "))
            {
                if (double.TryParse(command.Substring(5), out waittime))
                    Player.SendInfoMessage($"Waiting {waittime} seconds before executing next command.");
                else
                    Player.SendErrorMessage("Invalid time. Check the command's config. Using 0s");
            }

            if (waittime > 0)
            {
                TickDelay = (int)(waittime * 60);
                return;
            }
            ExecuteCommand(command);
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
                    if (cmds.Count < 6)
                    {
                        Player.SendErrorMessage("Invalid syntax for the replace command, check your config yo. Usage: replaceblock x1 y1 x2 y2 paintType");
                        break;
                    }
                    WorldEdit.PaintArea(int.Parse(cmds[1]), int.Parse(cmds[2]), int.Parse(cmds[3]), int.Parse(cmds[4]), byte.Parse(cmds[5]));
                    break;
                case "replaceblock":
                    if (cmds.Count < 7)
                    {
                        Player.SendErrorMessage("Invalid syntax for the replace command, check your config yo. Usage: replaceblock x1 y1 x2 y2 blockType1 blockType2");
                        break;
                    }
                    WorldEdit.ReplaceArea(int.Parse(cmds[1]), int.Parse(cmds[2]), int.Parse(cmds[3]), int.Parse(cmds[4]), ushort.Parse(cmds[5]), ushort.Parse(cmds[6]));
                    break;
                case "replacerandom":
                    if (cmds.Count < 7)
                    {
                        Player.SendErrorMessage("Invalid syntax for the random replace command, check your config yo. Usage: replacerandom x1 y1 x2 y2 blockType");
                        break;
                    }
                    //WorldEdit.ReplaceRandom(int.Parse(cmds[1]), int.Parse(cmds[2]), int.Parse(cmds[3]), int.Parse(cmds[4]), ushort.Parse(cmds[5]));
                    break;
                case "rise":
                    if (cmds.Count < 6)
                    {
                        Player.SendErrorMessage("Invalid syntax for the rise command, check your config yo. Usage: rise x1 y1 x2 y2 liquidType");
                        break;
                    }

                    byte type = 1; //lava
                    if (cmds.Count > 6)
                    {
                        switch (cmds[5])
                        {
                            case "r":
                                Random rnd = new Random();
                                type = (byte)rnd.Next(LiquidID.Count);
                                break;
                            case "water":
                            case "0":
                                type = 0;
                                break;
                            case "lava":
                            case "1":
                                type = 1;
                                break;
                            case "honey":
                            case "2":
                                type = 2;
                                break;
                            case "shimmer":
                            case "3":
                                type = 3;
                                break;
                        }
                    }
                    WorldEdit.Rise(int.Parse(cmds[1]), int.Parse(cmds[2]), int.Parse(cmds[3]), int.Parse(cmds[4]), type);
                    return;
            }
        }
    }
}

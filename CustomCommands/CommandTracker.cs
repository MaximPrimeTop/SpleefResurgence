using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TerrariaApi.Server;
using TShockAPI;
using Terraria.ID;
using System.Diagnostics;
using TShockAPI.Hooks;
namespace SpleefResurgence.CustomCommands
{
    public class CommandTracker
    {

        private static bool isTracking = false;

        public static bool isCustomCommand(string name)
        {
            return Spleef.CustomCommands.Exists(c => c.Name == name);
        }

        public static CustomCommand GetCustomCommand(string name)
        {
            return Spleef.CustomCommands.Find(c => c.Name == name);
        }

        public class ExecutingCommand
        {
            public string Name; 
            public Queue<string> CommandQueue = new();
            public Stopwatch WaitStopwatch = new();
            public double WaitTimeSeconds = 0;
            public bool isPaused = false;
            public bool hasParent = false;
            public ExecutingCommand Parent;
            public TSPlayer Player;
            public byte CurrentPaintID;

            public ExecutingCommand(string name, List<string> commandList, TSPlayer player)
            {
                Name = name;
                CommandQueue = new Queue<string>(commandList);
                Player = player;
                WaitStopwatch.Start();
            }

            public ExecutingCommand(string name, List<string> commandList, ExecutingCommand parentCommand)
            {
                Name = name;
                CommandQueue = new Queue<string>(commandList);
                hasParent = true;
                Parent = parentCommand;
                Player = parentCommand.Player;
                WaitStopwatch.Start();
            }

            public void Pause()
            {
                isPaused = true;
                WaitStopwatch.Stop();
            }

            public void Continue()
            {
                isPaused = false;
                WaitStopwatch.Start();
            }

            public void Execute()
            {
                WaitTimeSeconds = 0;
                WaitStopwatch.Reset();
                string cmdLine = CommandQueue.Dequeue();
                if (cmdLine[0] == '/' || cmdLine[0] == '.')
                {
                    string name;
                    int index = cmdLine.IndexOf(' ');
                    if (index == -1)
                        name = cmdLine[1..];
                    else
                        name = cmdLine[1..index];
                    if (isCustomCommand(name))
                    {
                        Pause();
                        List<string> parameters = cmdLine.Split(' ').ToList();
                        parameters.RemoveAt(0);
                        List<string> commandList = ConvertCmdListWithParametersOrSmth(GetCustomCommand(name).CommandList, parameters, Player);
                        ExecutingCommand newParentCommand = new(name, commandList, this);
                        Spleef.ActiveCommands.Add(newParentCommand);
                        newParentCommand.Execute();
                    }
                    else
                        Commands.HandleCommand(TSPlayer.Server, cmdLine);
                    return;
                }
                List<string> cmds = cmdLine.Split(' ').ToList();
                switch (cmds[0])
                {
                    case "wait":
                        WaitTimeSeconds = double.Parse(cmds[1]);
                        WaitStopwatch.Start();
                        return;
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
                        if (cmds.Count < 5)
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

            public void Stop()
            {
                CommandQueue.Clear();
                WaitTimeSeconds = 0;
            }
        }

        public static List<string> ConvertCmdListWithParametersOrSmth(List<string> commandList, List<string> Parameters, TSPlayer player)
        {
            List<string> cmdList = new();
            foreach (string cmd in commandList)
            {
                List<string> cmds = cmd.Split(' ').ToList();
                for (int i = 0; i < cmds.Count; i++)
                {
                    string c = cmds[i];
                    if (c == "[PLAYERNAME]")
                    {
                        cmds[i] = player.Name;
                    }
                    else if (c[0] == 'x')
                    {
                        int paramIndex;
                        int value;
                        int secondNum;
                        if (c.Contains('+'))
                        {
                            int plusIndex = c.IndexOf('+');
                            paramIndex = int.Parse(c[1..plusIndex]);
                            secondNum = int.Parse(c[plusIndex..]);
                        }
                        else if (c.Contains('-'))
                        {
                            int minusIndex = c.IndexOf('-');
                            paramIndex = int.Parse(c[1..minusIndex]);
                            secondNum = int.Parse(c[minusIndex..]);
                        }
                        else
                        {
                            paramIndex = int.Parse(c[1..]);
                            cmds[i] = Parameters[paramIndex];
                            continue;
                        }
                        if (Parameters.Count <= paramIndex)
                            value = 0;
                        else
                            value = int.Parse(Parameters[paramIndex]) + secondNum;
                        cmds[i] = value.ToString();
                    }
                }
                cmdList.Add(string.Join(' ', cmds));
            }
            return cmdList;
        }

        public static void ExecuteNewCommand(string name, List<string> commandList, TSPlayer player, List<string> parameters)
        {
            if (!isTracking)
            {
                ServerApi.Hooks.GameUpdate.Register(Spleef.Instance, CommandUpdate);
                isTracking = true;
            }
            List<string> newCommandList = ConvertCmdListWithParametersOrSmth(commandList, parameters, player);
            ExecutingCommand newCommand = new(name, newCommandList, player);
            Spleef.ActiveCommands.Add(newCommand);
            newCommand.Execute();
        }

        private static void CommandUpdate(EventArgs args)
        {
            for (int i = 0; i < Spleef.ActiveCommands.Count; i++)
            {
                var command = Spleef.ActiveCommands[i];

                if (command.isPaused)
                    continue;

                if (command.WaitStopwatch.Elapsed.TotalSeconds < command.WaitTimeSeconds)
                    continue;

                if (command.CommandQueue.Count == 0)
                {
                    command.Player.SendSuccessMessage($"Finished command: {command.Name}");

                    Spleef.ActiveCommands.Remove(command);
                    if (command.hasParent)                
                        command.Parent.Continue();
                    i--;
                    continue;
                }

                command.Execute();
            }
            if (Spleef.ActiveCommands.Count == 0)
            {
                ServerApi.Hooks.GameUpdate.Deregister(Spleef.Instance, CommandUpdate);
                isTracking = false;
            }
        }
    }
}

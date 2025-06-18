using Microsoft.Xna.Framework;
using Newtonsoft.Json.Schema;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;

namespace SpleefResurgence
{
    public class CustomCommandHandler
    {
        public static PluginSettings Config => PluginSettings.Config;
        private readonly Spleef pluginInstance;
        private static Random rnd = Spleef.rnd;

        private readonly static ushort[] OneBreakTiles = SpleefGame.OneBreakTiles;

        public CustomCommandHandler(Spleef plugin)
        {
            this.pluginInstance = plugin;
            ServerApi.Hooks.GameUpdate.Register(pluginInstance, OnWorldUpdate);
        }

        public void AddCustomCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Usage: /addcommand <name> [permission]");
                return;
            }

            string name = args.Parameters[0];
            string permission = "spleef.addcommand." + name;
            if (args.Parameters.Count > 1)
            {
                permission = args.Parameters[1];
            }
            List<string> commandlist = new();

            DynamicCommand newCommand = new()
            {
                Name = name,
                Permission = permission,
                CommandList = commandlist
            };

            Config.AllCommands.Add(newCommand);
            PluginSettings.Save();
            args.Player.SendSuccessMessage("new sexy command /{0}", name);
            Commands.ChatCommands.Add(new Command(permission, (args) =>
            {
                CommandLogicAltAlt(args, name, permission, commandlist);
            }, name));
        }

        public void RemoveCustomCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Usage: /delcommand <name>");
                return;
            }
            string name = args.Parameters[0];

            var commandToRemove = Config.AllCommands.FirstOrDefault(c => c.Name == name);
            if (commandToRemove != null)
            {
                Config.AllCommands.Remove(commandToRemove);
                Commands.ChatCommands.RemoveAll(c => c.Name == commandToRemove.Name);
                PluginSettings.Save();
                args.Player.SendSuccessMessage($"WOOOOOOO Command /{name} has been successfully deleted.");
            }
            else
            {
                args.Player.SendErrorMessage("dummas there isnt a command named like that");
            }

        }

        public void ListCustomCommand(CommandArgs args)
        {
            if (Config.AllCommands.Count == 0)
            {
                args.Player.SendErrorMessage("There are no custom commands available.");
                return;
            }

            List<string> commandNames = Config.AllCommands.Select(c => c.Name).ToList();
            string commandList = string.Join(", ", commandNames);

            args.Player.SendMessage($"Custom Commands: {commandList}", Color.Orange);
        }

        public void ListActiveCommand(CommandArgs args)
        {
            if (CommandsTrack.Count == 0)
            {
                args.Player.SendErrorMessage("There are no custom commands active.");
                return;
            }

            string commandList = string.Join(", ", CommandsTrack.Select(c => c.Name).ToList());

            args.Player.SendMessage($"Active custom Commands: {commandList}", Color.Gold);
        }

        public static void RegisterCommands()
        {
            foreach (var cmd in Config.AllCommands)
            {
                Commands.ChatCommands.Add(new Command(cmd.Permission, (args) =>
                {
                    CommandLogicAltAlt(args, cmd.Name, cmd.Permission, cmd.CommandList);
                }, cmd.Name));
            }
        }

        public static void DeRegisterCommands()
        {
            foreach (var cmd in Config.AllCommands)
            {
                Commands.ChatCommands.RemoveAll(c => c.Name == cmd.Name);
            }
        }

        private static CancellationTokenSource lavaRiseCts;
        private static bool isLavaRisePaused = false;
        private static async Task LavaRiseTimer(int timeInSeconds, CancellationToken token = default, string text = "[i:207] Time until lava rises: ")
        {
            Stopwatch Risetimer = new();
            Risetimer.Start();
            while (Risetimer.Elapsed.TotalSeconds < timeInSeconds)
            {
                if (token.IsCancellationRequested)
                {
                    Spleef.statusLavariseTime = "Lava rise stopped!";
                    return;
                }

                if (isLavaRisePaused)
                {
                    await Task.Delay(100);
                    continue;
                }

                Spleef.statusLavariseTime = text + $"{(timeInSeconds - Risetimer.Elapsed.TotalSeconds):N1}";
                await Task.Delay(10);
            }
        }

        private static bool isCustomCommand (string name)
        {
            return Config.AllCommands.Exists(c => c.Name == name);
        }

        public class CustomCommand
        {
            public string Name;
            public Queue<string> CmdList;
            public Stopwatch Stopwatch = new();
            public double WaitTimeSeconds = 0;
            public CommandArgs Args;
            public bool isPaused = false;
            public string Parent;
            public int OneBreakTile = -1;
            public byte Paint = 69;
            public int? ParameterOneBreakTile;
            public byte? ParameterPaint;

            public CustomCommand(string name, List<string> commands, CommandArgs args, string parent, int? tile, byte? paint)
            {
                Name = name;
                CmdList = new Queue<string>(commands);
                Stopwatch.Start();
                Args = args;
                Parent = parent;
                ParameterOneBreakTile = tile;
                ParameterPaint = paint;
            }

            public void PauseCommand()
            {
                Stopwatch.Stop();
                isPaused = true;
            }
            public void ContinueCommand()
            {
                Stopwatch.Start();
                isPaused = false;
            }
        }

        public readonly static List<CustomCommand> CommandsTrack = new();

        public static void CommandLogicAltAlt(CommandArgs args, string name, string permission, List<string> CmdList)
        {
            TSPlayer player = args.Player;
            bool isForced = false;
            string parent = "null";
            int? tile = null;
            byte? paint = null;
            switch (args.Parameters.ElementAtOrDefault(0))
            {
                case "permission":
                    if (!player.Group.HasPermission("spleef.customcommand"))
                    {
                        player.SendErrorMessage($"hey stinker you do not have the permission to do this");
                        return;
                    }
                    player.SendInfoMessage(permission);
                    break;
                case "help":
                    if (!player.Group.HasPermission("spleef.customcommand"))
                    {
                        player.SendErrorMessage($"hey stinker you do not have the permission to do this");
                        break;
                    }
                    player.SendInfoMessage($"To add a command: /{name} add command");
                    player.SendInfoMessage($"To delete a command: /{name} del id [id2]");
                    player.SendInfoMessage($"To view the list of all commands: /{name} list");
                    player.SendInfoMessage($"To execute all the commands: /{name}");
                    player.SendMessage($"You can also make the server wait before executing a command: /{name} add time waittime", Color.Orange);
                    player.SendMessage($"And also an easier way to setup a lavarise: /{name} add rise x1 y1 x2 y2 waittime", Color.Orange);
                    break;
                case "add":
                    if (!player.Group.HasPermission("spleef.customcommand"))
                    {
                        player.SendErrorMessage($"hey stinker you do not have the permission to do this");
                        return;
                    }

                    if (args.Parameters.Count < 2)
                    {
                        player.SendErrorMessage($"Invalid syntax! /{name} add command");
                        return;
                    }
                    else
                    {
                        string NewCmd = string.Join(" ", args.Parameters.Skip(1)); ;
                        player.SendSuccessMessage($"Added: {CmdList.Count + 1}. {NewCmd}");
                        CmdList.Add(NewCmd);
                        PluginSettings.Save();
                    }
                    break;
                case "list":
                    if (!player.Group.HasPermission("spleef.customcommand"))
                    {
                        player.SendErrorMessage($"hey stinker you do not have the permission to do this");
                        return;
                    }

                    if (CmdList.Count == 0)
                    {
                        player.SendInfoMessage($"{name} is emdy :(");
                    }
                    else
                    {
                        int j = 1;
                        player.SendMessage($"List of commands for \"{name}\"", Color.DarkGreen);
                        foreach (string command in CmdList)
                        {
                            player.SendInfoMessage($"{j}. " + command);
                            j++;
                        }
                    }
                    break;
                case "del":
                    if (!player.Group.HasPermission("spleef.customcommand"))
                    {
                        player.SendErrorMessage($"hey stinker you do not have the permission to do this");
                        return;
                    }

                    if (args.Parameters.Count < 2 || args.Parameters.Count > 3)
                    {
                        player.SendErrorMessage($"Invalid syntax! /{name} del <id> [id2]");
                        return;
                    }

                    int cmdi1 = Convert.ToInt32(args.Parameters[1]) - 1;
                    if (cmdi1 < 0 || cmdi1 > CmdList.Count - 1)
                    {
                        player.SendErrorMessage($"Invalid syntax! /{name} del <id> [id2]");
                        return;
                    }
                    if (args.Parameters.Count == 3)
                    {
                        int cmdi2 = Convert.ToInt32(args.Parameters[2]) - 1;
                        if (cmdi2 < cmdi1 || cmdi2 > CmdList.Count - 1)
                        {
                            player.SendErrorMessage($"Invalid syntax! /{name} del <id> [id2]");
                            return;
                        }
                        for (int j = cmdi1; j <= cmdi2; j++)
                        {
                            player.SendSuccessMessage($"Removed: {j + 1}. {CmdList[cmdi1]}");
                            CmdList.RemoveAt(cmdi1);
                        }
                    }
                    else
                    {
                        player.SendSuccessMessage($"Removed: {cmdi1 + 1}. {CmdList[cmdi1]}");
                        CmdList.RemoveAt(cmdi1);
                    }
                    PluginSettings.Save();
                    break;
                case "stop":
                    if (!player.Group.HasPermission("spleef.customcommand"))
                    {
                        player.SendErrorMessage($"hey stinker you do not have the permission to do this");
                        return;
                    }
                    if (lavaRiseCts != null && !lavaRiseCts.IsCancellationRequested)
                    {
                        lavaRiseCts.Cancel();
                        lavaRiseCts = null;
                        player.SendInfoMessage("Lava rise timer has been stopped.");
                    }
                    foreach (var command in CommandsTrack)
                    {
                        if (command.Name == name)
                        {
                            CommandsTrack.Remove(command);
                            player.SendSuccessMessage($"stopped command {command.Name}");

                            if (CommandsTrack.Exists(c => c.Parent == command.Name))
                            {
                                var ChildCommand = CommandsTrack.Find(c => c.Parent == command.Name);
                                Commands.HandleCommand(TSPlayer.Server, $"/{ChildCommand.Name} stop");
                            }

                            if (command.Parent != "null" && CommandsTrack.Exists(c => c.Name == command.Parent))
                            {
                                var ParentCommand = CommandsTrack.Find(c => c.Name == command.Parent);
                                ParentCommand.ContinueCommand();
                            }
                            return;
                        }
                    }
                    player.SendErrorMessage($"{name} ain't active");
                    break;
                default:
                    for(int i = 0; i < args.Parameters.Count; i++)
                    {
                        string param = args.Parameters[i];
                        if (param == "-f")
                            isForced = true;
                        if (param == "-parent")
                        {
                            parent = args.Parameters[i + 1];
                        }
                        else if (param == "-block" || param == "-tile")
                        {
                            if (int.TryParse(args.Parameters[i + 1], out int temptile))
                                tile = temptile;
                            else
                                tile = -1; //means random
                        }
                        else if (param == "-paint")
                        {
                            if (byte.TryParse(args.Parameters[i + 1], out byte temppaint))
                                paint = temppaint;
                            else
                                paint = 69; //means random
                        }
                    }
                    if (!isForced)
                    {
                        foreach (var command in CommandsTrack)
                        {
                            if (command.Name == name)
                            {
                                args.Player.SendErrorMessage($"this {name} do be runnin");
                                return;
                            }
                        }
                    }
                    CustomCommand ccmd = new(name, CmdList, args, parent, tile, paint);
                    CommandsTrack.Add(ccmd);
                    break;
            }
        }

        public void OnWorldUpdate(EventArgs args)
        {
            for (int i = 0; i < CommandsTrack.Count; i++)
            {
                var command = CommandsTrack[i];

                if (command.isPaused)
                    continue;

                if (command.CmdList.Count == 0)
                {
                    command.Args.Player.SendSuccessMessage($"Finished command: {command.Name}");  

                    CommandsTrack.Remove(command);
                    if (command.Parent != "null")
                    {
                        var ParentCommand = CommandsTrack.Find(c => c.Name == command.Parent);
                        ParentCommand.ContinueCommand();
                    }
                    i--;
                    continue;
                }

                if (command.Stopwatch.Elapsed.TotalSeconds < command.WaitTimeSeconds)
                    continue;

                DoCommand(command);
            }
        }

        public void DoCommand(CustomCommand command)
        {
            command.WaitTimeSeconds = 0;
            command.Stopwatch.Reset();
            string cmd = command.CmdList.Dequeue();
            List<string> cmds = cmd.Split(' ').ToList();
            for (int j = 0; j < cmds.Count; j++)
            {
                string S = cmds[j];
                if (S == "[PLAYERNAME]")
                    cmds[j] = command.Args.Player.Name;
                else if (S[0] == 'x')
                {
                    if (command.Args.Parameters.Count < S[1] - '0')
                    {
                        cmds.RemoveAt(j);
                        j--;
                    }
                    else
                    {
                        if (S.Length >= 4)
                        {
                            if (S[2] == '+')
                            {
                                int value = int.Parse(command.Args.Parameters[S[1] - '1']) + int.Parse(S[3..]);
                                cmds[j] = value.ToString();
                            }
                            else if (S[2] == '-')
                            {
                                int value = int.Parse(command.Args.Parameters[S[1] - '1']) - int.Parse(S[3..]);
                                cmds[j] = value.ToString();
                            }
                        }
                        else
                            cmds[j] = command.Args.Parameters[S[1] - '1'];
                    }
                }
            }
            cmd = string.Join(' ', cmds);
            if (cmd[0] == '/' || cmd[0] == '.')
            {
                string name;
                int index = cmd.IndexOf(' ');
                if (index == -1)
                    name = cmd[1..];
                else
                    name = cmd[1..index];
                if (isCustomCommand(name))
                {
                    command.PauseCommand();
                    Commands.HandleCommand(TSPlayer.Server, $"{cmd} -parent {command.Name}");
                }
                else
                    Commands.HandleCommand(TSPlayer.Server, cmd);
                return;
            }
            else if (cmds[0] == "wait")
            {
                command.WaitTimeSeconds = Convert.ToDouble(cmds[1]);
                command.Stopwatch.Start();
            }
            else if (cmds[0] == "timer")
            {
                lavaRiseCts?.Cancel();
                lavaRiseCts = new CancellationTokenSource();
                isLavaRisePaused = false;
                _ = LavaRiseTimer(Convert.ToInt32(cmds[1]), lavaRiseCts.Token);
            }
            else if (cmds[0] == "paint")
            {
                int paintX1 = Convert.ToInt32(cmds[1]);
                int paintY1 = Convert.ToInt32(cmds[2]);
                int paintX2 = Convert.ToInt32(cmds[3]);
                int paintY2 = Convert.ToInt32(cmds[4]);

                if (command.ParameterPaint == null)
                {
                    if (cmds.Count <= 5 || cmds[5] == "r")
                        command.Paint = (byte)rnd.Next(31);
                    else
                        command.Paint = Convert.ToByte(cmds[5]);
                }
                else if (command.ParameterPaint == 69)
                    command.Paint = (byte)rnd.Next(31);
                else
                    command.Paint = (byte)command.ParameterPaint;


                int paintLeft = Math.Min(paintX1, paintX2), paintRight = Math.Max(paintX1, paintX2);
                int paintTop = Math.Min(paintY1, paintY2), paintBottom = Math.Max(paintY1, paintY2);

                for (int j = paintLeft; j <= paintRight; j++)
                {
                    for (int k = paintTop; k <= paintBottom; k++)
                    {
                        WorldGen.paintTile(j, k, command.Paint, true);
                        NetMessage.SendTileSquare(-1, j, k, 1);
                    }
                }
            }
            else if (cmds[0] == "paintmore")
            {
                int paintX1 = Convert.ToInt32(cmds[1]);
                int paintY1 = Convert.ToInt32(cmds[2]);
                int paintX2 = Convert.ToInt32(cmds[3]);
                int paintY2 = Convert.ToInt32(cmds[4]);

                int paintLeft = Math.Min(paintX1, paintX2), paintRight = Math.Max(paintX1, paintX2);
                int paintTop = Math.Min(paintY1, paintY2), paintBottom = Math.Max(paintY1, paintY2);

                if (command.Paint == 69)
                    command.Paint = (byte)rnd.Next(31);

                for (int j = paintLeft; j <= paintRight; j++)
                {
                    for (int k = paintTop; k <= paintBottom; k++)
                    {
                        WorldGen.paintTile(j, k, command.Paint, true);
                        NetMessage.SendTileSquare(-1, j, k, 1);
                    }
                }
            }
            else if (cmds[0] == "rise")
            {
                int x1 = Convert.ToInt32(cmds[1]);
                int y1 = Convert.ToInt32(cmds[2]);
                int x2 = Convert.ToInt32(cmds[3]);
                int y2 = Convert.ToInt32(cmds[4]);
                byte type = 1;

                if (cmds.Count >= 6)
                {
                    if (cmds[5] == "r")
                        type = (byte)rnd.Next(LiquidID.Count);
                    else if (cmds[5] == "water" || cmds[5] == "0")
                        type = 0;
                    else if (cmds[5] == "lava" || cmds[5] == "1")
                        type = 1;
                    else if (cmds[5] == "honey" || cmds[5] == "2")
                        type = 2;
                    else if (cmds[5] == "shimmer" || cmds[5] == "3")
                        type = 3;
                }

                int left = Math.Min(x1, x2), right = Math.Max(x1, x2);
                int top = Math.Min(y1, y2), bottom = Math.Max(y1, y2);

                for (int j = left; j <= right; j++)
                {
                    for (int k = top; k <= bottom; k++)
                    {
                        Main.tile[j, k].ClearTile();
                        NetMessage.SendTileSquare(-1, j, k, 1);
                        WorldGen.PlaceLiquid(j, k, type, 255);
                    }
                }
                Liquid.UpdateLiquid();
            }
            else if (cmds[0] == "replaceblock")
            {
                int x1 = Convert.ToInt32(cmds[1]);
                int y1 = Convert.ToInt32(cmds[2]);
                int x2 = Convert.ToInt32(cmds[3]);
                int y2 = Convert.ToInt32(cmds[4]);

                if (command.ParameterOneBreakTile == null)
                {
                    if (cmds.Count <= 5 || cmds[5] == "r")
                        command.OneBreakTile = SpleefGame.OneBreakTiles[Spleef.rnd.Next(SpleefGame.OneBreakTiles.Length)];
                    else
                        command.OneBreakTile = Convert.ToInt32(cmds[5]);
                }
                else if (command.ParameterOneBreakTile == -1)
                    command.OneBreakTile = SpleefGame.OneBreakTiles[Spleef.rnd.Next(SpleefGame.OneBreakTiles.Length)];
                else
                    command.OneBreakTile = (int)command.ParameterOneBreakTile;

                int left = Math.Min(x1, x2), right = Math.Max(x1, x2);
                int top = Math.Min(y1, y2), bottom = Math.Max(y1, y2);

                for (int j = left; j <= right; j++)
                {
                    for (int k = top; k <= bottom; k++)
                    {
                        if (Main.tile[j, k].active() && Main.tile[j, k].type == 0)
                        {
                            WorldGen.PlaceTile(j, k, command.OneBreakTile, true, true);
                            NetMessage.SendTileSquare(-1, j, k, 1);
                        }
                    }
                }
            }
            else if (cmds[0] == "replacemore")
            {
                int x1 = Convert.ToInt32(cmds[1]);
                int y1 = Convert.ToInt32(cmds[2]);
                int x2 = Convert.ToInt32(cmds[3]);
                int y2 = Convert.ToInt32(cmds[4]);

                int left = Math.Min(x1, x2), right = Math.Max(x1, x2);
                int top = Math.Min(y1, y2), bottom = Math.Max(y1, y2);

                if (command.OneBreakTile == -1)
                    command.OneBreakTile = SpleefGame.OneBreakTiles[Spleef.rnd.Next(SpleefGame.OneBreakTiles.Length)];

                for (int j = left; j <= right; j++)
                {
                    for (int k = top; k <= bottom; k++)
                    {
                        if (Main.tile[j, k].active() && Main.tile[j, k].type == 0)
                        {
                            WorldGen.PlaceTile(j, k, command.OneBreakTile, true, true);
                            NetMessage.SendTileSquare(-1, j, k, 1);
                        }
                    }
                }
            }
            else if (cmds[0] == "fullrandom")
            {
                int x1 = Convert.ToInt32(cmds[1]);
                int y1 = Convert.ToInt32(cmds[2]);
                int x2 = Convert.ToInt32(cmds[3]);
                int y2 = Convert.ToInt32(cmds[4]);

                int left = Math.Min(x1, x2), right = Math.Max(x1, x2);
                int top = Math.Min(y1, y2), bottom = Math.Max(y1, y2);

                for (int j = left; j <= right; j++)
                {
                    for (int k = top; k <= bottom; k++)
                    {
                        if (Main.tile[j, k].active() && Main.tile[j, k].type == 0)
                        {
                            WorldGen.PlaceTile(j, k, OneBreakTiles[Spleef.rnd.Next(OneBreakTiles.Length)], true, true);
                            NetMessage.SendTileSquare(-1, j, k, 1);
                        }
                    }
                }
            }
        }
    }
}

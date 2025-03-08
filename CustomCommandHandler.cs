using Microsoft.Xna.Framework;
using TShockAPI;

namespace SpleefResurgence
{
    public class CustomCommandHandler
    {
        public static PluginSettings Config => PluginSettings.Config;

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
                CommandLogic(args.Player, name, permission, commandlist, args);
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

            var commandToRemove = PluginSettings.Config.AllCommands.FirstOrDefault(c => c.Name == name);
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

        public static void RegisterCommands()
        {
            foreach (var cmd in Config.AllCommands)
            {
                Commands.ChatCommands.Add(new Command(cmd.Permission, (args) =>
                {
                    CommandLogic(args.Player, cmd.Name, cmd.Permission, cmd.CommandList, args);
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
        public static bool isCommanding = false;


        private static async void LavaRiseTimer(int timeInSeconds)
        {
            for (int i = timeInSeconds; i > 0; i--)
            {
                Spleef.statusLavariseTime = $"Time until lava rises: {i}";
                await Task.Delay(1000);
            }
        }

        public static async void CommandLogic(TSPlayer player, string name, string permission, List<string> CmdList, CommandArgs args)
        {
            var count = args.Parameters.Count;

            switch (args.Parameters.ElementAtOrDefault(0))
            {
                case "permission":
                    {
                        if (!player.Group.HasPermission("spleef.customcommand"))
                        {
                            player.SendErrorMessage($"hey stinker you do not have the permission to do this");
                            break;
                        }
                        player.SendInfoMessage(permission);
                    }
                    break;

                case "help":
                    {
                        if (!player.Group.HasPermission("spleef.customcommand"))
                        {
                            player.SendErrorMessage($"hey stinker you do not have the permission to do this");
                            break;
                        }
                        player.SendInfoMessage("To add a command: /{0} add command", name);
                        player.SendInfoMessage("To delete a command: /{0} del numbah [bigeahnumbah]", name);
                        player.SendInfoMessage("To view the list of all commands: /{0} list", name);
                        player.SendInfoMessage("To execute all the commands: /{0}", name);
                        player.SendMessage("You can also make the server wait before executing a command: /" + name + " add time numbre", Color.Orange);
                        player.SendMessage("And also an easier way to setup a lavarise: /" + name + " add lavarise x1 y1 x2 y2 waittime", Color.Orange);
                    }
                    break;
                case "add":
                    {
                        if (!player.Group.HasPermission("spleef.customcommand"))
                        {
                            player.SendErrorMessage($"hey stinker you do not have the permission to do this");
                            break;
                        }

                        if (count < 2)
                        {
                            player.SendErrorMessage("Invalid syntax! /{0} add command", name);
                        }
                        else
                        {
                            string NewCmd = string.Join(" ", args.Parameters.Skip(1)); ;
                            player.SendSuccessMessage("Added: {1}. {0}", NewCmd, CmdList.Count + 1);
                            CmdList.Add(NewCmd);
                            PluginSettings.Save();
                        }
                    }
                    break;
                case "list":
                    {
                        if (!player.Group.HasPermission("spleef.customcommand"))
                        {
                            player.SendErrorMessage($"hey stinker you do not have the permission to do this");
                            break;
                        }

                        if (CmdList.Count == 0)
                        {
                            player.SendInfoMessage("{0} is emdy :(", name);
                        }
                        else
                        {
                            int i = 1;
                            player.SendMessage("List of commands for \"" + name + "\"", Color.DarkGreen);
                            foreach (string command in CmdList)
                            {
                                player.SendInfoMessage($"{i}. " + command);
                                i++;
                            }
                        }
                    }
                    break;
                case "del":
                    {
                        if (!player.Group.HasPermission("spleef.customcommand"))
                        {
                            player.SendErrorMessage($"hey stinker you do not have the permission to do this");
                            break;
                        }
                            

                        if (count < 2 || count > 3)
                        {
                            player.SendErrorMessage("Invalid syntax! /{0} del <numbah> [bigger numbah]", name);
                            break;
                        }

                        int cmdi1 = Convert.ToInt32(args.Parameters[1]) - 1;
                        if (cmdi1 < 0 || cmdi1 > CmdList.Count - 1)
                        {
                            player.SendErrorMessage("Invalid syntax! /{0} del <numbah> [bigger numbah]", name);
                            break;
                        }
                        if (count == 3)
                        {
                            int cmdi2 = Convert.ToInt32(args.Parameters[2]) - 1;
                            if (cmdi2 < cmdi1 || cmdi2 > CmdList.Count - 1)
                            {
                                player.SendErrorMessage("Invalid syntax! /{0} del <numbah> [bigger numbah]", name);
                                break;
                            }
                            for (int i = cmdi1; i <= cmdi2; i++)
                            {
                                player.SendSuccessMessage("Removed: {0}. {1}", i + 1, CmdList[cmdi1]);
                                CmdList.RemoveAt(cmdi1);
                            }
                        }
                        else
                        {
                            player.SendSuccessMessage("Removed: {0}. {1}", cmdi1 + 1, CmdList[cmdi1]);
                            CmdList.RemoveAt(cmdi1);
                        }
                        PluginSettings.Save();
                    }
                    break;
                case "stop":
                    if (!player.Group.HasPermission("spleef.customcommand"))
                    {
                        player.SendErrorMessage($"hey stinker you do not have the permission to do this");
                        break;
                    }
                    isCommanding = false;
                    break;
                default:
                    {
                        isCommanding = true;
                        for (int i = 0; i < CmdList.Count && isCommanding; i++)
                        {
                            string command = CmdList[i];
                            {
                                string[] stuff = command.Split(' ');
                                if (stuff[0] == "wait")
                                {
                                    await Task.Delay(Convert.ToInt32(command[5..]) * 1000);
                                }
                                else if (stuff[0] == "lavarise")
                                {
                                    int x1 = Convert.ToInt32(stuff[1]);
                                    int y1 = Convert.ToInt32(stuff[2]);
                                    int x2 = Convert.ToInt32(stuff[3]);
                                    int y2 = Convert.ToInt32(stuff[4]);
                                    int waittime = Convert.ToInt32(stuff[5]);

                                    Commands.HandleCommand(TSPlayer.Server, $"//p1 {x1} {y1}");
                                    Commands.HandleCommand(TSPlayer.Server, $"//p2 {x2} {y2}");
                                    Commands.HandleCommand(TSPlayer.Server, "//cut");
                                    Commands.HandleCommand(TSPlayer.Server, "//fill lava");
                                    LavaRiseTimer(waittime);
                                    await Task.Delay(waittime * 1000);
                                }
                                else
                                {
                                    var targetPlayer = player;
                                    command = command.Replace("[PLAYERNAME]", $"\"{targetPlayer.Name}\"");

                                    if (args.Parameters.Count == 1)
                                    {
                                        string playerName = "";
                                        for (int j = 0; j < args.Parameters.Count; j++)
                                        {
                                            playerName = playerName + args.Parameters[j];
                                        }
                                        var players = TSPlayer.FindByNameOrID(playerName);
                                        if (players == null || players.Count == 0)
                                        {
                                            player.SendErrorMessage($"{args.Parameters[0]} is not a valid player or subcommand");
                                            return;
                                        }
                                        targetPlayer = players[0];
                                        command = command.Replace("[PLAYERNAME1]", $"\"{targetPlayer.Name}\"");
                                    }
                                    else if (args.Parameters.Count == 2)
                                    {
                                        var players = TSPlayer.FindByNameOrID(args.Parameters[0]);
                                        if (players == null || players.Count == 0)
                                        {
                                            player.SendErrorMessage($"{args.Parameters[0]} is not a valid player or subcommand");
                                            return;
                                        }
                                        targetPlayer = players[0];

                                        command = command.Replace("[PLAYERNAME1]", $"\"{targetPlayer.Name}\"");

                                        players = TSPlayer.FindByNameOrID(args.Parameters[1]);
                                        if (players == null || players.Count == 0)
                                        {
                                            player.SendErrorMessage($"{args.Parameters[0]} is not a valid player or subcommand");
                                            return;
                                        }
                                        targetPlayer = players[0];

                                        command = command.Replace("[PLAYERNAME2]", $"\"{targetPlayer.Name}\"");
                                    }
                                    Commands.HandleCommand(TSPlayer.Server, command);
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
}

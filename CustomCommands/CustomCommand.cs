using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using TShockAPI;
using System.Timers;
using Microsoft.Xna.Framework;

namespace SpleefResurgence.CustomCommands
{
    public class CustomCommand
    {
        [JsonIgnore]
        public static string CommandsPath = Path.Combine(TShock.SavePath, "Spleef", "CustomCommands");
        private static string EditPermission = "spleef.customcommand";

        public string Name { get; set; }
        public string Permission { get; set; }
        public List<string> CommandList { get; set; }

        private string HelpText;

        private string SetHelpText()
        {
            return
            $"To add a command: /{Name} add command\n" +
            $"To delete a command: /{Name} del id [id2]\n" +
            $"To view the list of all commands: /{Name} list" +
            $"To execute all the commands: /{Name}\n" +
            $"You can also make the server wait before executing a command: /{Name} add time waittime\n" +
            $"And also an easier way to setup a lavarise: /{Name} add rise x1 y1 x2 y2 waittime";
        }

        public CustomCommand(string name, string permission, List<string> commandList)
        {
            Name = name;
            Permission = permission;
            CommandList = commandList;
            HelpText = SetHelpText();
        }

        public static CustomCommand FromJson(string filepath)
        {
            string json = File.ReadAllText(filepath);
            CustomCommand command = JsonConvert.DeserializeObject<CustomCommand>(json);
            return new(command.Name, command.Permission, command.CommandList);
        }

        public void ToJson()
        {
            string filepath = Path.Combine(CommandsPath, $"{Name}.json");
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filepath, json);
        }

        public void DeleteJson()
        {
            string filepath = Path.Combine(CommandsPath, $"{Name}.json");
            File.Delete(filepath);
        }

        public void CommandLogic(CommandArgs args)
        {
            TSPlayer player = args.Player;
            switch (args.Parameters.ElementAtOrDefault(0))
            {
                case "help":
                    if (!player.HasPermission(EditPermission))
                        return;
                    player.SendInfoMessage(HelpText);
                    return;
                case "permission":
                    if (!player.HasPermission(EditPermission))
                        return;
                    player.SendInfoMessage($"Permission for /{Name}: {Permission}");
                    return;
                case "add":
                    if (args.Parameters.Count < 2)
                    {
                        player.SendErrorMessage($"Invalid syntax! /{Name} add command");
                        return;
                    }
                    Add(string.Join(' ', args.Parameters.Skip(1)));       
                    return;
                case "delete":
                case "del":
                    if (!player.HasPermission(EditPermission))
                        return;

                    if (args.Parameters.Count < 2 || args.Parameters.Count > 3)
                    {
                        player.SendErrorMessage($"Invalid syntax! /{Name} del <id> [id2]");
                        return;
                    }

                    int id;
                    if (int.TryParse(args.Parameters[1], out id) && id > 0)
                    {
                        args.Player.SendErrorMessage($"Invalid id. /{Name} del <id> [count]");
                        return;
                    }

                    if (args.Parameters.Count > 2)
                    {
                        int count;
                        if (!int.TryParse(args.Parameters[2], out count))
                        {
                            args.Player.SendErrorMessage($"Invalid count. /{Name} del <id> [count]");
                            return;
                        }
                        Delete(id - 1, count);
                        player.SendSuccessMessage($"Deleted from {id} to {id + count - 1}");
                    }
                    else
                    {
                        Delete(int.Parse(args.Parameters[1]));
                        player.SendSuccessMessage($"Deleted {id}");
                    }
                    return;
                case "stop":
                    if (!player.HasPermission(EditPermission))
                        return;

                    if (Spleef.ActiveCommands.Count(c => c.Command.Name == Name) == 0)
                    {
                        player.SendErrorMessage("There is no active command!");
                        return;
                    }
                    if (Spleef.ActiveCommands.Count(c => c.Command.Name == Name) == 1)
                    {
                        Stop();
                        player.SendSuccessMessage($"Stopped {Name}!");
                    }
                    else
                    {
                        Stop();
                        player.SendSuccessMessage($"Stopped all the {Name} commands!");
                    }
                    return;
                case "list":
                    if (!player.HasPermission(EditPermission))
                        return;
                    List(args.Player);
                    return;
                default:
                    ExecuteCommands(player);
                    return;
            }
        }

        public void Add(string command)
        {
            CommandList.Add(command);
            ToJson();
        }

        public void Delete(int id, int count = 1)
        {
            for (int i = 0; i < count && id < CommandList.Count; i++)
                CommandList.RemoveAt(id);
            ToJson();
        }

        public void List(TSPlayer player)
        {
            player.SendMessage($"List of commands for \"{Name}\"", Color.DarkGreen);
            string Commands = string.Join("\n", Spleef.CustomCommands.Select((command, i) => $"{i+1}.{command}").ToList());
            player.SendInfoMessage(Commands);
        }

        public void Stop()
        {
            List<ExecutingCommand> allCommands = Spleef.ActiveCommands.FindAll(c => c.Command.Name == Name).ToList();
            foreach (var command in allCommands)
            {
                command.StopExecution();
                Spleef.ActiveCommands.Remove(command);
            }
        }

        public void ExecuteCommands(TSPlayer player)
        {
            ExecutingCommand command = new(player, this);
            Spleef.ActiveCommands.Add(command);
            command.DoCommands();
        }

        public void ExecuteCommands(ExecutingCommand parent)
        {
            ExecutingCommand command = new(parent.Player, this, parent);
            Spleef.ActiveCommands.Add(command);
            command.DoCommands();
        }
    }
}

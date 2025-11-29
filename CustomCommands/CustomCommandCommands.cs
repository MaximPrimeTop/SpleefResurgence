using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace SpleefResurgence.CustomCommands
{
    public class CCCommands
    {
        public static void AddCustomCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Usage: /addcommand <name> [permission]");
                return;
            }
            string name = args.Parameters[0];
            if (Spleef.CustomCommands.Exists(c => c.Name == name))
            {
                args.Player.SendErrorMessage("This command already exists!");
                return;
            }
            string permission = "spleef.addcommand." + name;
            if (args.Parameters.Count > 1)
                 permission = args.Parameters[1];
            List<string> commandList = new();
            CustomCommand command = new(name, permission, commandList);
            command.ToJson();
            Spleef.CustomCommands.Add(command);
            Commands.ChatCommands.Add(new Command(command.Permission, command.CommandLogic, command.Name));
            TShock.Log.Info($"Registered custom command: {command.Name}");
            args.Player.SendSuccessMessage($"new sexy command /{name}");
        }

        public static void DeleteCustomCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Usage: /delcommand <name>");
                return;
            }
            string name = args.Parameters[0];
            if (Spleef.CustomCommands.Exists(c => c.Name == name))
            {
                CustomCommand command = Spleef.CustomCommands.Find(c => c.Name == name);
                Spleef.CustomCommands.Remove(command);
                command.DeleteJson();
                Commands.ChatCommands.RemoveAll(c => c.Name == command.Name);
                args.Player.SendSuccessMessage($"WOOOOOOO Command /{name} has been successfully deleted.");
            }
            else
                args.Player.SendErrorMessage("dummas there isnt a command named like that");
        }

        public static void ListCustomCommand(CommandArgs args)
        {
            if (Spleef.CustomCommands.Count == 0)
            {
                args.Player.SendErrorMessage("There are no custom commands available.");
                return;
            }

            List<string> commandNames = Spleef.CustomCommands.Select(c => c.Name).ToList();
            string commandList = string.Join(", ", commandNames);

            args.Player.SendMessage($"Custom Commands: {commandList}", Color.Orange);
        }

        public static void ListActiveCommand(CommandArgs args)
        {
            if (Spleef.CustomCommands.Count == 0)
            {
                args.Player.SendErrorMessage("There are no custom commands available.");
                return;
            }

            List<string> commandNames = Spleef.ActiveCommands.Select(c => c.Command.Name).ToList();
            string commandList = string.Join(", ", commandNames);

            args.Player.SendMessage($"Custom Commands: {commandList}", Color.Orange);
        }
    }
}

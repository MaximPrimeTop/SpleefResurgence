using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace SpleefResurgence.Game.Editor
{
    public class GimmickEditor
    {
        enum GimmickEditStep
        {
            None,
            Type,
            Values
        }

        enum GimmickType
        {
            None,
            Item,
            Accessory,
            Buff,
            Mount,
            Mob
        }

        public static void GimmickCommand(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Usage: /gimmick help");
                return;
            }
            switch (args.Parameters[0])
            {
                case "create":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("Usage: /gimmick create <type> <arguments>");
                        return;
                    }
                    CreateGimmick(args);
                    break;
                case "edit":
                    EditGimmick(args);
                    break;
                case "delete":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("Usage: /gimmick delete <type>");
                        return;
                    }
                    DeleteGimmick(args);
                    break;
                case "list":
                    ListGimmicks(args);
                    break;
                case "info":
                    GimmickInfo(args);
                    break;
                case "help":
                    args.Player.SendInfoMessage("Gimmick Editor Commands:");
                    args.Player.SendInfoMessage("/gimmick create <type> <arguments> - Create a new gimmick of the specified type.");
                    args.Player.SendInfoMessage("/gimmick edit <name> - Edit an existing gimmick by its name.");
                    args.Player.SendInfoMessage("/gimmick delete <name> - Delete a gimmick by its name.");
                    args.Player.SendInfoMessage("/gimmick list - List all available gimmicks.");
                    args.Player.SendInfoMessage("/gimmick info <name> - Get information about a specific gimmick by it's ame.");
                    break;
                default:
                    args.Player.SendErrorMessage("Unknown subcommand. Usage: /gimmick <create|edit|delete|list|info>");
                    break;
            }
        }

        public static void CreateGimmick(CommandArgs args)
        {

        }

        public static void EditGimmick(CommandArgs args)
        {

        }

        public static void DeleteGimmick(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Usage: /gimmick delete <name>");
                return;
            }
            string name = args.Parameters[1];
            if (GameConfig.GimmickJson.RemoveGimmick(name))
            {
                args.Player.SendSuccessMessage($"Gimmick '{name}' deleted successfully.");
            }
            else
            {
                args.Player.SendErrorMessage($"Gimmick '{name}' not found.");
            }
        }

        public static void ListGimmicks(CommandArgs args)
        {
            var gimmicks = GameConfig.GimmickJson.ListGimmickNames();
            if (gimmicks.Count == 0)
            {
                args.Player.SendInfoMessage("No gimmicks available.");
                return;
            }
            args.Player.SendInfoMessage("Available Gimmicks:");
            foreach (var gimmick in gimmicks)
            {
                args.Player.SendInfoMessage($"- {gimmick}");
            }
        }

        public static void GimmickInfo(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Usage: /gimmick info <name>");
                return;
            }
            string name = args.Parameters[1];
            var gimmick = GameConfig.GimmickJson.GetGimmick(name);
            if (gimmick == null)
            {
                args.Player.SendErrorMessage($"Gimmick '{name}' not found.");
                return;
            }
            args.Player.SendInfoMessage($"Gimmick Info for '{name}':");
            args.Player.SendInfoMessage($"Type: {gimmick.GetType().Name}");
            args.Player.SendInfoMessage($"Wait Time: {gimmick.WaitTime} ms");
        }
    }
}

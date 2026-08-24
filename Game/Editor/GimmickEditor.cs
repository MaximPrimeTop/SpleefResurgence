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

        public void Initialize()
        {
            Commands.ChatCommands.Add(new Command("spleef.edit.gimmick", GimmickCommand, "gimmick"));
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
                    args.Player.SendInfoMessage("/gimmick create <type> <name> <arguments> - Create a new gimmick of the specified type.");
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
            if (args.Parameters.Count < 3)
            {
                args.Player.SendErrorMessage("Usage: /gimmick create <name> <type> <arguments>");
                return;
            }

            string name = args.Parameters[2];

            if (GameConfig.GimmickJson.GetGimmick(name) != null)
            {
                args.Player.SendErrorMessage($"A gimmick with the name '{name}' already exists.");
                return;
            }

            string type = args.Parameters[1].ToLowerInvariant();

            switch (type) //def gotta shorten this shit somehow
            {
                case "1":
                case "item":
                    if (args.Parameters.Count < 5)
                    {
                        args.Player.SendErrorMessage("Usage: /gimmick create item <name> <itemID> <waitTime> [stack] [slot]");
                        return;
                    }
                    if (!int.TryParse(args.Parameters[3], out int itemID))
                    {
                        args.Player.SendErrorMessage("invalid item ID yo, i dunno look it up on the wiki or smth until i figure out how to set it up with item names");
                        return;
                    }
                    if (itemID < 1 || itemID > Terraria.ID.ItemID.Count)
                    {
                        args.Player.SendErrorMessage($"Invalid item ID. It must be between 1 and {Terraria.ID.ItemID.Count}.");
                        return;
                    }
                    if (!int.TryParse(args.Parameters[4], out int waitTime))
                    {
                        args.Player.SendErrorMessage("Invalid waitTime. It must be an integer.");
                        return;
                    }
                    if (waitTime < 0)
                    {
                        args.Player.SendErrorMessage("nope can't have wait time as negative soz");
                        return;
                    }
                    int stack = 1;
                    if (args.Parameters.Count >= 6 && !int.TryParse(args.Parameters[5], out stack))
                    {
                        args.Player.SendErrorMessage("Invalid stack. It must be an integer.");
                        return;
                    }
                    if (stack < 1 || stack > 9999)
                    {                         
                        args.Player.SendErrorMessage("Stack must be between 1 and 9999.");
                        return;
                    }
                    int slot = -1;
                    if (args.Parameters.Count >= 7 && !int.TryParse(args.Parameters[6], out slot))
                    {
                        args.Player.SendErrorMessage("Invalid slot. It must be an integer.");
                        return;
                    }
                    if ((slot < 1 || slot > 58) && slot != -1)
                    {
                        args.Player.SendErrorMessage("Slot must be between 1 and 58 or be -1.");
                        return;
                    }
                    
                    var gimmickItem = new GimmickItem(itemID, waitTime, stack);
                    GameConfig.GimmickJson.SaveGimmick(name, gimmickItem);
                    args.Player.SendSuccessMessage($"Gimmick '{name}' of type 'item' created successfully.");
                    break;
                case "2":
                case "accessory":
                    if (args.Parameters.Count < 5)
                    {
                        args.Player.SendErrorMessage("Usage: /gimmick create accessory <name> <itemID> <waitTime> [slot]");
                        return;
                    }
                    if (!int.TryParse(args.Parameters[3], out int accItemID))
                    {
                        args.Player.SendErrorMessage("Invalid item ID. It must be an integer.");
                        return;
                    }
                    if (accItemID < 1 || accItemID > Terraria.ID.ItemID.Count)
                    {
                        args.Player.SendErrorMessage($"Invalid item ID. It must be between 1 and {Terraria.ID.ItemID.Count}.");
                        return;
                    }
                    if (!int.TryParse(args.Parameters[4], out int accWaitTime))
                    {
                        args.Player.SendErrorMessage("Invalid waitTime. It must be an integer.");
                        return;
                    }
                    if (accWaitTime < 0)
                    {
                        args.Player.SendErrorMessage("no negative wait time.");
                        return;
                    }
                    int accSlot = -1;
                    if (args.Parameters.Count >= 6 && !int.TryParse(args.Parameters[5], out accSlot))
                    {
                        args.Player.SendErrorMessage("Invalid slot. It must be an integer.");
                        return;
                    }
                    if ((accSlot < 1 || accSlot > 5) && accSlot != -1)
                    {
                        args.Player.SendErrorMessage("Slot must be between 1 and 5 or be -1.");
                        return;
                    }
                    accSlot += 2;

                    var gimmickAccessory = new GimmickAccessory(accItemID, accWaitTime, accSlot);
                    GameConfig.GimmickJson.SaveGimmick(name, gimmickAccessory);
                    args.Player.SendSuccessMessage($"Gimmick '{name}' of type 'accessory' created successfully.");
                    break;
                case "3":
                case "buff":
                    if (args.Parameters.Count < 6)
                    {
                        args.Player.SendErrorMessage("Usage: /gimmick create buff <name> <buffID> <buffDuration> <waitTime>");
                        return;
                    }
                    if (!int.TryParse(args.Parameters[3], out int buffID))
                    {
                        args.Player.SendErrorMessage("Invalid buff ID. It must be an integer.");
                        return;
                    }
                    if (buffID < 1 || buffID > Terraria.ID.BuffID.Count)
                    {
                        args.Player.SendErrorMessage($"Invalid buff ID. It must be between 1 and {Terraria.ID.BuffID.Count}.");
                        return;
                    }
                    if (!int.TryParse(args.Parameters[4], out int buffDuration))
                    {
                        args.Player.SendErrorMessage("Invalid buff duration. It must be an integer.");
                        return;
                    }
                    if (buffDuration < 0)
                    {
                        args.Player.SendErrorMessage("sure really funny, you can't have buff durations as negative");
                        return;
                    }
                    if (!int.TryParse(args.Parameters[5], out int buffWaitTime))
                    {
                        args.Player.SendErrorMessage("Invalid waitTime. It must be an integer.");
                        return;
                    }
                    if (buffWaitTime < 0)
                    {                         
                        args.Player.SendErrorMessage("stop making waittime negative yo");
                        return;
                    }

                    var gimmickBuff = new GimmickBuff(buffID, buffDuration, buffWaitTime);
                    GameConfig.GimmickJson.SaveGimmick(name, gimmickBuff);
                    args.Player.SendSuccessMessage($"Gimmick '{name}' of type 'buff' created successfully.");
                    break;
                case "4":
                case "mount":
                    if (args.Parameters.Count < 5)
                    {
                        args.Player.SendErrorMessage("Usage: /gimmick create mount <name> <itemID> <waitTime>");
                        return;
                    }
                    if (!int.TryParse(args.Parameters[3], out int mountItemID))
                    {
                        args.Player.SendErrorMessage("Invalid item ID. It must be an integer.");
                        return;
                    }
                    if (mountItemID < 1 || mountItemID > Terraria.ID.ItemID.Count)
                    {
                        args.Player.SendErrorMessage($"Invalid item ID. It must be between 1 and {Terraria.ID.ItemID.Count}.");
                        return;
                    }
                    if (!int.TryParse(args.Parameters[4], out int mountWaitTime))
                    {
                        args.Player.SendErrorMessage("Invalid waitTime. It must be an integer.");
                        return;
                    }
                    if (mountWaitTime < 0)
                    {
                        args.Player.SendErrorMessage("negative wait time... no more...");
                        return;
                    }

                    var gimmickMount = new GimmickMount(mountItemID, mountWaitTime);
                    GameConfig.GimmickJson.SaveGimmick(name, gimmickMount);
                    args.Player.SendSuccessMessage($"Gimmick '{name}' of type 'mount' created successfully.");
                    break;
                case "5":
                case "mob":
                    //pain
                    break;
                default:
                    args.Player.SendErrorMessage($"Unknown gimmick type '{type}'.");
                    break;
            }
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
            args.Player.SendInfoMessage($"Type: {gimmick.GetType().Name.Skip(7)}");
            args.Player.SendInfoMessage($"Details: {gimmick.GetInfo()}");
            args.Player.SendInfoMessage($"Wait Time: {gimmick.WaitTime} s");
        }
    }
}

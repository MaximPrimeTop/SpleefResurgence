using TerrariaApi.Server;
using TShockAPI;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
namespace SpleefResurgence.Game.Editor
{
    public class ArenaEditor
    {
        enum EditMode
        {
            None,
            ArenaCorner1,
            ArenaCorner2,
            MapEdit,
            MapEditSpawnAdd,
            MapEditSpawnEdit,
            MapEditSpawnMoving,
            GimmickEdit
        }

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

        class PlayerEditor
        {
            public EditMode Mode = EditMode.None;
            public int MapSpawnPointID = -1;
            public Action<TSPlayer, int, int> TileEditCallback = null;
            public Action<TSPlayer, string> ChatCallback = null;
            public GimmickEditStep GimmickEditStep = GimmickEditStep.None;
            public GimmickType CurrentGimmickType = GimmickType.None;
        }

        private Dictionary<int, PlayerEditor> PlayerEditors = new();

        public void Initialize()
        {
            Commands.ChatCommands.Add(new Command("spleef.edit.arena", ArenaEdit, "arena"));
            Commands.ChatCommands.Add(new Command("spleef.edit.map", MapEdit, "map"));
            GetDataHandlers.TileEdit.Register(OnTileEdit);
            ServerApi.Hooks.ServerChat.Register(Spleef.Instance, OnServerChat);
        }

        private void DeletePlayerBuff(TSPlayer player, int buffID)
        {
            for (int i = 0; i < player.TPlayer.buffType.Length; i++)
            {
                if (player.TPlayer.buffType[i] == buffID)
                {
                    player.TPlayer.DelBuff(i);
                    NetMessage.SendData(MessageID.PlayerBuffs, number: player.Index);
                    return;
                }
            }
        }

        private void RecalculateArenaCorners(int x1, int y1, int x2, int y2, out int cornerX1, out int cornerY1, out int cornerX2, out int cornerY2)
        {
            cornerX1 = Math.Min(x1, x2);
            cornerY1 = Math.Min(y1, y2);
            cornerX2 = Math.Max(x1, x2);
            cornerY2 = Math.Max(y1, y2);
        }

        private void StartArenaEditing(TSPlayer player, Arena arenaToEdit)
        {
            PlayerEditors[player.Index].Mode = EditMode.ArenaCorner1;
            player.SendInfoMessage($"Editing arena {arenaToEdit.Name}.\nSet corner 1 by breaking/placing a tile.\nType 'cancel' to cancel, 'skip' to skip to the next step, or 'lavarise <command>' to set the default lavarise command.");
            if (arenaToEdit.TilePositionX != 0 && arenaToEdit.TilePositionY != 0)
                player.SendInfoMessage($"Current corner 1 (top left) is at {arenaToEdit.TilePositionX},{arenaToEdit.TilePositionY}. You can skip to the next step if you want to keep it.");
            PlayerEditors[player.Index].TileEditCallback = (plr, x, y) =>
            {
                var editor = PlayerEditors[plr.Index];
                switch (editor.Mode)
                {
                    case EditMode.ArenaCorner1:
                        arenaToEdit.TilePositionX = x;
                        arenaToEdit.TilePositionY = y;
                        PlayerEditors[player.Index].Mode = EditMode.ArenaCorner2;
                        plr.SendSuccessMessage($"Set corner 1 of arena {arenaToEdit.Name} to {x},{y}. Now set corner 2.");
                        if (arenaToEdit.Width != 0 && arenaToEdit.Height != 0)
                            plr.SendInfoMessage($"Current corner 2 (bottom right) is at {arenaToEdit.TilePositionX + arenaToEdit.Width},{arenaToEdit.TilePositionY + arenaToEdit.Height}. You can skip to the next step if you want to keep it.");
                        break;
                    case EditMode.ArenaCorner2:
                        int cornerX1, cornerY1, cornerX2, cornerY2;
                        RecalculateArenaCorners(arenaToEdit.TilePositionX, arenaToEdit.TilePositionY, x, y, out cornerX1, out cornerY1, out cornerX2, out cornerY2);
                        arenaToEdit.TilePositionX = cornerX1;
                        arenaToEdit.TilePositionY = cornerY1;
                        arenaToEdit.Width = cornerX2 - cornerX1;
                        arenaToEdit.Height = cornerY2 - cornerY1;
                        GameConfig.ArenaJson.SaveArena(arenaToEdit);
                        PlayerEditors[player.Index].Mode = EditMode.None;
                        plr.SendSuccessMessage($"Set corner 2 of arena {arenaToEdit.Name} to {x},{y}. Arena saved.");
                        plr.SendInfoMessage($"The arena corners are set up from left top to right bottom, if you did them the other way they'll be recalculated automatically\nYou can check with /arena info {arenaToEdit.Name}");
                        editor.ChatCallback = null;
                        break;
                }
            };

            PlayerEditors[player.Index].ChatCallback = (plr, text) =>
            {
                var editor = PlayerEditors[plr.Index];
                if (text == "cancel")
                {
                    editor.Mode = EditMode.None;
                    plr.SendInfoMessage($"Cancelled editing arena {arenaToEdit.Name}");
                    editor.ChatCallback = null;
                    editor.TileEditCallback = null;
                    return;
                }
                if (text == "skip")
                {
                    if (editor.Mode == EditMode.ArenaCorner1)
                    {
                        editor.Mode = EditMode.ArenaCorner2;
                        plr.SendInfoMessage($"Skipped setting corner 1 of arena {arenaToEdit.Name}. Now set corner 2.");
                    }
                    else if (editor.Mode == EditMode.ArenaCorner2)
                    {
                        GameConfig.ArenaJson.SaveArena(arenaToEdit);
                        editor.Mode = EditMode.None;
                        plr.SendSuccessMessage($"Skipped setting corner 2 of arena {arenaToEdit.Name}. Arena saved.");
                        editor.ChatCallback = null;
                        editor.TileEditCallback = null;
                    }
                }
                if (text.StartsWith("lavarise "))
                {
                    arenaToEdit.DefaultCustomLavariseCommand = text.Substring(9);
                    GameConfig.ArenaJson.SaveArena(arenaToEdit);
                    plr.SendSuccessMessage($"Set default lavarise command of arena {arenaToEdit.Name} to {arenaToEdit.DefaultCustomLavariseCommand}");
                }

                plr.SendInfoMessage($"Editing arena {arenaToEdit.Name}.\nCurrent state: {editor.Mode}.\n Type 'cancel' to cancel, 'skip' to skip, or 'lavarise <command>' to set the default lavarise command.");
            };
        }

        public void ArenaEdit(CommandArgs args)
        {
            var player = args.Player;
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Usage: /arena help");
                return;
            }
            if (!PlayerEditors.ContainsKey(player.Index))
                PlayerEditors.Add(player.Index, new PlayerEditor());
            if (args.Parameters[0] == "help")
            {
                player.SendInfoMessage("help mesgeae");
                return;
            }
            else if (args.Parameters[0] == "new")
            {
                if (args.Parameters.Count < 2)
                {
                    args.Player.SendErrorMessage("Usage: /arena new <arenaName>");
                    return;
                }
                string arenaName = args.Parameters[1];
                if (GameConfig.ArenaJson.ListArenaNames().Contains(arenaName))
                {
                    args.Player.SendErrorMessage($"Arena {arenaName} already exists.");
                    return;
                }

                Arena arena = new(arenaName, 0, 0, 0, 0, new());
                GameConfig.ArenaJson.SaveArena(arena);
                args.Player.SendSuccessMessage($"Created a new arena {arenaName}");
                StartArenaEditing(player, arena);
                return;
            }
            else if (args.Parameters[0] == "delete")
            {
                if (args.Parameters.Count < 2)
                {
                    args.Player.SendErrorMessage("Usage: /arena delete <arenaName>");
                    return;
                }
                string arenaToDelete = args.Parameters[1];
                if (GameConfig.ArenaJson.DeleteArena(arenaToDelete))
                    args.Player.SendSuccessMessage($"Deleted arena {arenaToDelete}");
                else
                    args.Player.SendErrorMessage($"Arena {arenaToDelete} does not exist.");
            }
            else if (args.Parameters[0] == "info")
            {
                if (args.Parameters.Count < 2)
                {
                    args.Player.SendErrorMessage("Usage: /arena info <arenaName>");
                    return;
                }
                string arenaName = args.Parameters[1];
                if (!GameConfig.ArenaJson.ListArenaNames().Contains(arenaName))
                {
                    args.Player.SendErrorMessage($"Arena {arenaName} does not exist.");
                    return;
                }
                Arena arena = GameConfig.ArenaJson.LoadArena(arenaName);
                args.Player.SendInfoMessage($"Arena {arena.Name}:");
                args.Player.SendInfoMessage($"- Pos: {arena.TilePositionX}, {arena.TilePositionY}");
                args.Player.SendInfoMessage($"- Size: {arena.Width}x{arena.Height}");
                args.Player.SendInfoMessage($"- Default lavarise command: {arena.DefaultCustomLavariseCommand}");
                args.Player.SendInfoMessage($"- Maps: {string.Join(", ", arena.MapNames)}");
            }
            else if (args.Parameters[0] == "edit")
            {
                if (args.Parameters.Count < 2)
                {
                    args.Player.SendErrorMessage("Usage: /arena edit <arenaName>");
                    return;
                }

                if (!GameConfig.ArenaJson.ListArenaNames().Contains(args.Parameters[1]))
                {
                    args.Player.SendErrorMessage($"Arena {args.Parameters[1]} does not exist.");
                    return;
                }
                Arena arenaToEdit = GameConfig.ArenaJson.LoadArena(args.Parameters[1]);
                StartArenaEditing(player, arenaToEdit);
            }

            else if (args.Parameters[0] == "addmap")
            {
                if (args.Parameters.Count < 3)
                {
                    args.Player.SendErrorMessage("Usage: /arena addmap <arenaName> <mapName>");
                    return;
                }
                if (!GameConfig.ArenaJson.ListArenaNames().Contains(args.Parameters[1]))
                {
                    args.Player.SendErrorMessage($"Arena {args.Parameters[1]} does not exist.");
                    return;
                }
                if (!GameConfig.MapJson.ListMapNames(args.Parameters[1]).Contains(args.Parameters[2]))
                {
                    args.Player.SendErrorMessage($"Map {args.Parameters[2]} does not exist for arena {args.Parameters[1]}.");
                    return;
                }
                Arena arena = GameConfig.ArenaJson.LoadArena(args.Parameters[1]);
                arena.MapNames.Add(args.Parameters[2]);
                GameConfig.ArenaJson.SaveArena(arena);
                args.Player.SendSuccessMessage($"Added map {args.Parameters[2]} to arena {args.Parameters[1]}");
            }
            else
            {
                args.Player.SendErrorMessage("idfk what you said");
            }
        }

        private void StartMapEditing(TSPlayer player, Map mapToEdit, string arenaName)
        {
            PlayerEditors[player.Index].Mode = EditMode.MapEdit;
            player.SendInfoMessage($"Editing map {mapToEdit.Name}");
            player.SendInfoMessage($"Type 'cancel' to cancel, 'spawnadd' to set a spawn at where you're standing or 'spawnedit' to edit an existing one");
            PlayerEditors[player.Index].ChatCallback = (plr, text) =>
            {
                var editor = PlayerEditors[plr.Index];
                if (editor.Mode == EditMode.MapEdit)
                {
                    if (text == "spawnadd")
                    {
                        int x = plr.TileX;
                        int y = plr.TileY;
                        plr.Teleport(x * 16, y * 16);
                        plr.SetBuff(BuffID.Webbed, 100000);
                        player.SendInfoMessage($"Teleported to the tile spawn point {x}, {y}, type 'confirm' or 'undo'");
                        editor.Mode = EditMode.MapEditSpawnAdd;
                        return;
                    }
                    if (text == "spawnedit")
                    {
                        if (mapToEdit.Spawns.Count == 0)
                        {
                            plr.SendErrorMessage($"Map {mapToEdit.Name} has no spawn points to edit.");
                            return;
                        }
                        plr.SendInfoMessage($"Map {mapToEdit.Name} has the following spawn points:");
                        for (int i = 0; i < mapToEdit.Spawns.Count; i++)
                        {
                            plr.SendMessage($"{i}: {mapToEdit.Spawns[i].X}, {mapToEdit.Spawns[i].Y}", Color.Aqua);
                        }
                        plr.SendInfoMessage("Type 'spawnedit <index>' to edit a spawn point.");
                        return;
                    }
                    if (text.StartsWith("spawnedit "))
                    {
                        if (mapToEdit.Spawns.Count == 0)
                        {
                            plr.SendErrorMessage($"Map {mapToEdit.Name} has no spawn points to edit.");
                            return;
                        }
                        if (!int.TryParse(text.Substring(10), out int index))
                        {
                            plr.SendErrorMessage($"Invalid index {text.Substring(10)}.");
                            return;
                        }
                        if (index < 0 || index >= mapToEdit.Spawns.Count)
                        {
                            plr.SendErrorMessage($"Index {index} is out of range.");
                            return;
                        }
                        int x = mapToEdit.Spawns[index].X;
                        int y = mapToEdit.Spawns[index].Y;
                        plr.Teleport(x * 16, y * 16);
                        plr.SetBuff(BuffID.Webbed, 100000);
                        player.SendInfoMessage($"Teleported to the current tile spawn point {x},{y}, type 'edit' or 'undo'");
                        editor.MapSpawnPointID = index;
                        editor.Mode = EditMode.MapEditSpawnEdit;
                        return;
                    }
                    if (text == "gimmickadd")
                    {
                        editor.Mode = EditMode.GimmickEdit;
                        GimmickAddStep(text, editor, player, mapToEdit, arenaName);
                        return;
                    }          
                }
                else if (editor.Mode == EditMode.MapEditSpawnAdd)
                {
                    if (text == "confirm")
                    {
                        if (editor.MapSpawnPointID != -1)
                        {
                            int index = editor.MapSpawnPointID;
                            mapToEdit.Spawns[index] = new(plr.TileX, plr.TileY);
                            plr.SendSuccessMessage($"Edited spawn point {index} to {plr.TileX},{plr.TileY} in map {mapToEdit.Name}");
                            editor.MapSpawnPointID = -1;
                        }
                        else
                        {
                            int x = plr.TileX;
                            int y = plr.TileY;
                            mapToEdit.Spawns.Add(new(x, y));
                            plr.SendSuccessMessage($"Added spawn point at {x},{y} to map {mapToEdit.Name}");
                        }
                        DeletePlayerBuff(plr, BuffID.Webbed);
                        GameConfig.MapJson.SaveMap(mapToEdit, arenaName);
                        editor.Mode = EditMode.MapEdit;
                    }
                }
                else if (editor.Mode == EditMode.MapEditSpawnEdit)
                {
                    if (text == "edit")
                    {
                        DeletePlayerBuff(plr, BuffID.Webbed);
                        editor.Mode = EditMode.MapEditSpawnMoving;
                        plr.SendInfoMessage($"Editing spawn point, type 'set' to set a new spawn point or 'undo' to cancel");
                        return;
                    }
                    if (text == "undo")
                    {
                        DeletePlayerBuff(plr, BuffID.Webbed);
                        plr.SendInfoMessage($"Cancelled editing spawn point");
                        editor.Mode = EditMode.MapEdit;
                        editor.MapSpawnPointID = -1;
                        return;
                    }
                }
                else if (editor.Mode == EditMode.MapEditSpawnMoving)
                {
                    if (text == "set")
                    {
                        int x = plr.TileX;
                        int y = plr.TileY;
                        plr.Teleport(x * 16, y * 16);
                        plr.SetBuff(BuffID.Webbed, 100000);
                        player.SendInfoMessage($"Teleported to the tile spawn point {x},{y}, type 'confirm' or 'undo'");
                        editor.Mode = EditMode.MapEditSpawnAdd;
                        return;
                    }
                }
                else if (editor.Mode == EditMode.GimmickEdit)
                {
                    GimmickAddStep(text, editor, player, mapToEdit, arenaName);
                }
            };
        }

        private void GimmickAddStep(string text, PlayerEditor editor, TSPlayer player, Map mapToEdit, string arenaName)
        {
            if (editor.GimmickEditStep == GimmickEditStep.None)
            {
                editor.GimmickEditStep = GimmickEditStep.Type;
                player.SendInfoMessage("Set the type of gimmick you want to add (1 - item, 2 - accessory, 3 - buff, 4 - mount, 5 - mob");
                return;
            }
            if (editor.GimmickEditStep == GimmickEditStep.Type)
            {
                editor.GimmickEditStep = GimmickEditStep.Values;
                switch (text)
                {
                    case "1":
                    case "item":
                        editor.CurrentGimmickType = GimmickType.Item;
                        player.SendInfoMessage("Set the item ID and wait time (in seconds) for the gimmick, separated by a space");
                        break;
                    case "2":
                    case "accessory":
                        editor.CurrentGimmickType = GimmickType.Accessory;
                        player.SendInfoMessage("Set the item ID, wait time (in seconds), and slot for the gimmick, separated by spaces");
                        player.SendInfoMessage("Set slot to -1 to let the game choose the next free slot of a player");
                        break;
                    case "3":
                    case "buff":
                        editor.CurrentGimmickType = GimmickType.Buff;
                        player.SendInfoMessage("Set the buff ID, buff duration (in seconds), and wait time (in seconds) for the gimmick, separated by spaces");
                        break;
                    case "4":
                    case "mount":
                        editor.CurrentGimmickType = GimmickType.Mount;
                        player.SendInfoMessage("Set the item ID and wait time (in seconds) for the gimmick, separated by a space");
                        break;
                    case "5":
                    case "mob":
                        editor.CurrentGimmickType = GimmickType.Mob;
                        player.SendInfoMessage("Set the mob ID, mob amount, mob spawn tile X, mob spawn tile Y, and wait time (in seconds) for the gimmick, separated by spaces");
                        break;
                    default:
                        editor.GimmickEditStep = GimmickEditStep.Type;
                        player.SendErrorMessage("Invalid gimmick type, please choose from 1 - item, 2 - accessory, 3 - buff, 4 - mount, 5 - mob");
                        break;
                }
            }
            if (editor.GimmickEditStep == GimmickEditStep.Values)
            {
                switch (editor.CurrentGimmickType)
                {
                    case GimmickType.Item:
                        var itemValues = text.Split(' ');
                        if (itemValues.Length != 2)
                        {
                            player.SendErrorMessage("Invalid number of values for the item gimmick, please provide item ID and wait time (in seconds)");
                            return;
                        }
                        if (!int.TryParse(itemValues[0], out int itemID) || !int.TryParse(itemValues[1], out int waitTime))
                        {
                            player.SendErrorMessage("Invalid values for the item gimmick, please provide valid integers for item ID and wait time (in seconds)");
                            return;
                        }
                        var itemGimmick = new GimmickItem(itemID, waitTime);
                        mapToEdit.AdditionalGimmicks.Add(itemGimmick);
                        GameConfig.MapJson.SaveMap(mapToEdit, arenaName);
                        player.SendSuccessMessage($"Added an item gimmick with item ID {itemID} and wait time {waitTime} seconds");
                        break;
                    case GimmickType.Accessory:
                        var accessoryValues = text.Split(' ');
                        if (accessoryValues.Length != 3)
                        {
                            player.SendErrorMessage("Invalid number of values for the accessory gimmick, please provide item ID, wait time (in seconds), and slot");
                            return;
                        }
                        if (!int.TryParse(accessoryValues[0], out int accessoryItemID) || !int.TryParse(accessoryValues[1], out int accessoryWaitTime) || !int.TryParse(accessoryValues[2], out int slot))
                        {
                            player.SendErrorMessage("Invalid values for the accessory gimmick, please provide valid integers for item ID, wait time (in seconds), and slot");
                            return;
                        }
                        var accessoryGimmick = new GimmickAccessory(accessoryItemID, accessoryWaitTime, slot);
                        mapToEdit.AdditionalGimmicks.Add(accessoryGimmick);
                        GameConfig.MapJson.SaveMap(mapToEdit, arenaName);
                        player.SendSuccessMessage($"Added an accessory gimmick with item ID {accessoryItemID}, wait time {accessoryWaitTime} seconds, and slot {slot}");
                        break;
                    case GimmickType.Buff:
                        var buffValues = text.Split(' ');
                        if (buffValues.Length != 3)
                        {
                            player.SendErrorMessage("Invalid number of values for the buff gimmick, please provide buff ID, buff duration (in seconds), and wait time (in seconds)");
                            return;
                        }
                        if (!int.TryParse(buffValues[0], out int buffID) || !int.TryParse(buffValues[1], out int buffDuration) || !int.TryParse(buffValues[2], out int buffWaitTime))
                        {
                            player.SendErrorMessage("Invalid values for the buff gimmick, please provide valid integers for buff ID, buff duration (in seconds), and wait time (in seconds)");
                            return;
                        }
                        var buffGimmick = new GimmickBuff(buffID, buffDuration, buffWaitTime);
                        mapToEdit.AdditionalGimmicks.Add(buffGimmick);
                        GameConfig.MapJson.SaveMap(mapToEdit, arenaName);
                        player.SendSuccessMessage($"Added a buff gimmick with buff ID {buffID}, buff duration {buffDuration} seconds, and wait time {buffWaitTime} seconds");
                        break;
                    case GimmickType.Mount:
                        var mountValues = text.Split(' ');
                        if (mountValues.Length != 2)
                        {
                            player.SendErrorMessage("Invalid number of values for the mount gimmick, please provide item ID and wait time (in seconds)");
                            return;
                        }
                        if (!int.TryParse(mountValues[0], out int mountItemID) || !int.TryParse(mountValues[1], out int mountWaitTime))
                        {
                            player.SendErrorMessage("Invalid values for the mount gimmick, please provide valid integers for item ID and wait time (in seconds)");
                            return;
                        }
                        var mountGimmick = new GimmickMount(mountItemID, mountWaitTime);
                        mapToEdit.AdditionalGimmicks.Add(mountGimmick);
                        GameConfig.MapJson.SaveMap(mapToEdit, arenaName);
                        player.SendSuccessMessage($"Added a mount gimmick with item ID {mountItemID} and wait time {mountWaitTime} seconds");
                        break;
                    case GimmickType.Mob:
                        var mobValues = text.Split(' ');
                        if (mobValues.Length != 5)
                        {
                            player.SendErrorMessage("Invalid number of values for the mob gimmick, please provide mob ID, mob amount, mob spawn tile X, mob spawn tile Y, and wait time (in seconds)");
                            return;
                        }
                        if (!int.TryParse(mobValues[0], out int mobID) || !int.TryParse(mobValues[1], out int mobAmount) || !int.TryParse(mobValues[2], out int mobSpawnX) || !int.TryParse(mobValues[3], out int mobSpawnY) || !int.TryParse(mobValues[4], out int mobWaitTime))
                        {
                            player.SendErrorMessage("Invalid values for the mob gimmick, please provide valid integers for mob ID, mob amount, mob spawn tile X, mob spawn tile Y, and wait time (in seconds)");
                            return;
                        }
                        var mobGimmick = new GimmickMob(mobID, mobAmount, mobWaitTime, mobSpawnX, mobSpawnY);
                        mapToEdit.AdditionalGimmicks.Add(mobGimmick);
                        GameConfig.MapJson.SaveMap(mapToEdit, arenaName);
                        player.SendSuccessMessage($"Added a mob gimmick with mob ID {mobID}, amount {mobAmount}, spawn point ({mobSpawnX},{mobSpawnY}), and wait time {mobWaitTime} seconds");
                        break;

                }
                editor.GimmickEditStep = GimmickEditStep.None;
                editor.CurrentGimmickType = GimmickType.None;
                editor.Mode = EditMode.MapEdit;
            }
        }

        public void MapEdit(CommandArgs args) //haha mapedit from penguin games
        {
            var player = args.Player;
            if (!PlayerEditors.ContainsKey(player.Index))
                PlayerEditors.Add(player.Index, new PlayerEditor());
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Usage: /map help");
                return;
            }
            if (args.Parameters[0] == "help")
            {
                player.SendInfoMessage("help msegae");
                return;
            }
            else if (args.Parameters[0] == "new")
            {
                if (args.Parameters.Count < 3)
                {
                    args.Player.SendErrorMessage("Usage: /map new <arenaName> <mapName>");
                    return;
                }
                string mapName = args.Parameters[2];
                string arenaName = args.Parameters[1];
                if (!GameConfig.ArenaJson.ListArenaNames().Contains(arenaName))
                {
                    args.Player.SendErrorMessage($"Arena {arenaName} does not exist.");
                    return;
                }

                if (GameConfig.MapJson.ListMapNames(arenaName).Contains(mapName))
                {
                    args.Player.SendErrorMessage($"Map {mapName} already exists for the arena {arenaName}.");
                    return;
                }

                Map map = new(mapName, "", player.Account.Name, RiseType.Lava, new(), new());
                
                GameConfig.MapJson.SaveMap(map, arenaName);
                args.Player.SendSuccessMessage($"Created a new map {mapName}");
                StartMapEditing(player, map, arenaName);
                return;
            }
            else if (args.Parameters[0] == "delete")
            {
                if (args.Parameters.Count < 3)
                {
                    args.Player.SendErrorMessage("Usage: /map delete <arenaName> <mapName>");
                    return;
                }
                string mapName = args.Parameters[2];
                string arenaName = args.Parameters[1];
                if (!GameConfig.ArenaJson.ListArenaNames().Contains(arenaName))
                {
                    args.Player.SendErrorMessage($"Arena {arenaName} does not exist.");
                    return;
                }
                if (!GameConfig.MapJson.ListMapNames(arenaName).Contains(mapName))
                {
                    args.Player.SendErrorMessage($"Map {mapName} does not exist for the arena {arenaName}.");
                    return;
                }

                if (player.Account.Name != GameConfig.MapJson.LoadMap(arenaName, mapName).OwnerName && !player.HasPermission("spleef.edit.admin"))
                {
                    args.Player.SendErrorMessage($"You do not have permission to delete maps that are not made by you");
                    return;
                }

                if (!GameConfig.MapJson.DeleteMap(arenaName, mapName))
                {
                    args.Player.SendErrorMessage($"Failed to delete map {mapName} from arena {arenaName}");
                    return;
                }
                args.Player.SendSuccessMessage($"Deleted map {mapName} from arena {arenaName}");
            }
            else if (args.Parameters[0] == "edit")
            {
                if (args.Parameters.Count < 3)
                {
                    args.Player.SendErrorMessage("Usage: /map edit <arenaName> <mapName>");
                    return;
                }
                string arenaName = args.Parameters[1];
                string mapName = args.Parameters[2];
                if (!GameConfig.ArenaJson.ListArenaNames().Contains(arenaName))
                {
                    args.Player.SendErrorMessage($"Arena {arenaName} does not exist.");
                    return;
                }
                if (!GameConfig.MapJson.ListMapNames(arenaName).Contains(mapName))
                {
                    args.Player.SendErrorMessage($"Map {mapName} does not exist for the arena {arenaName}.");
                    return;
                }
                Map mapToEdit = GameConfig.MapJson.LoadMap(arenaName, mapName);
                if (player.Account.Name != mapToEdit.OwnerName && !player.HasPermission("spleef.edit.admin"))
                {
                    args.Player.SendErrorMessage($"You do not have permission to edit maps that are not made by you");
                    return;
                }
                StartMapEditing(player, mapToEdit, arenaName);
            }
            else
            {
                player.SendErrorMessage("idfk what you said");
            }   
        }

        private void OnTileEdit(object sender, GetDataHandlers.TileEditEventArgs args)
        {
            if (!PlayerEditors.ContainsKey(args.Player.Index))
                return;

            if (PlayerEditors[args.Player.Index].Mode == EditMode.None)
                return;

            if (PlayerEditors[args.Player.Index].TileEditCallback == null)
                return;

            PlayerEditors[args.Player.Index].TileEditCallback(args.Player, args.X, args.Y);
            args.Handled = true;
            NetMessage.SendTileSquare(args.Player.Index, args.X, args.Y, 1);
        }

        private void OnServerChat(ServerChatEventArgs args)
        {
            if (!PlayerEditors.ContainsKey(args.Who))
                return;

            if (PlayerEditors[args.Who].Mode == EditMode.None)
                return;

            if (PlayerEditors[args.Who].ChatCallback == null)
                return;

            if (args.Text.StartsWith("/"))
                return;

            if (args.Text == "cancel")
            {
                PlayerEditors.Remove(args.Who);
                TShock.Players[args.Who].SendInfoMessage($"Cancelled editing");
                DeletePlayerBuff(TShock.Players[args.Who], BuffID.Webbed);
                args.Handled = true;
                return;
            }

            if (args.Text == "undo" && 
                (PlayerEditors[args.Who].Mode == EditMode.MapEditSpawnAdd ||
                PlayerEditors[args.Who].Mode == EditMode.MapEditSpawnEdit ||
                PlayerEditors[args.Who].Mode == EditMode.MapEditSpawnMoving))
            {
                PlayerEditors[args.Who].Mode = EditMode.MapEdit;
                PlayerEditors[args.Who].MapSpawnPointID = -1;
                TShock.Players[args.Who].SendInfoMessage($"Cancelled adding spawn point");
                DeletePlayerBuff(TShock.Players[args.Who], BuffID.Webbed);
                args.Handled = true;
                return;
            }

            PlayerEditors[args.Who].ChatCallback(TShock.Players[args.Who], args.Text);
            args.Handled = true;
        }
    }
}

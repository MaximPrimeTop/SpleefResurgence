using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace SpleefResurgence
{
    public class InventoryEdit
    {
        public static PluginSettings Config => PluginSettings.Config;

        public int FindNextFreeSlot(TSPlayer player)
        {
            var inventory = player.TPlayer.inventory;

            for (int i = 0; i < player.TPlayer.inventory.Length; i++)
                if (inventory[i].IsAir)
                    return i;
            return -1;
        }

        public int FindNextFreeAccessorySlot(TSPlayer player)
        {
            var armor = player.TPlayer.armor;

            for (int i = 3; i <= 7; i++)
                if (armor[i].IsAir)
                    return i;
            return -1;
        }

        public void AddItem(TSPlayer player, int slot, int stack, int itemID)
        {
            player.TPlayer.inventory[slot].SetDefaults(itemID);
            player.TPlayer.inventory[slot].stack = stack;

            TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, player.Index, slot);
        }

        public void AddArmor(TSPlayer player, int slot, int itemID)
        {
            player.TPlayer.armor[slot].SetDefaults(itemID);
            player.TPlayer.armor[slot].stack = 1;

            TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, player.Index, 59 + slot);
        }

        public void AddMiscEquip(TSPlayer player, int slot, int itemID)
        {
            player.TPlayer.miscEquips[slot].SetDefaults(itemID);
            player.TPlayer.miscEquips[slot].stack = 1;

            TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, player.Index, 89 + slot);
        }

        public void ClearPlayerEverything(TSPlayer player)
        {
            for (int i = 0; i < player.TPlayer.inventory.Length; i++)
            {
                player.TPlayer.inventory[i].TurnToAir();
                TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, player.Index, i);
            }

            for (int i = 0; i < 10; i++)
            {
                player.TPlayer.armor[i].TurnToAir();
                TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, player.Index, 59 + i);
            }

            for (int i = 0; i < player.TPlayer.miscEquips.Length; i++)
            {
                player.TPlayer.miscEquips[i].TurnToAir();
                TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, player.Index, 89 + i);
            }

            player.TPlayer.trashItem.TurnToAir();
            TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, player.Index, 179);
        }

        public void InventoryReset(CommandArgs args)
        {
            string player = args.Parameters[0];
            var players = TSPlayer.FindByNameOrID(player);
            if (players == null || players.Count == 0)
            {
                args.Player.SendErrorMessage("worg");
                return;
            }
            var plr = players[0];
            ClearPlayerEverything(plr);
        }

        public void InventoryEditCommand(CommandArgs args)
        {
            string player = args.Parameters[0];
            var players = TSPlayer.FindByNameOrID(player);
            if (players == null || players.Count == 0)
            {
                args.Player.SendErrorMessage("worg");
                return;
            }
            var plr = players[0];
            int slot = Convert.ToInt32(args.Parameters[1]);
            int stack = Convert.ToInt32(args.Parameters[2]);
            int itemID = Convert.ToInt32(args.Parameters[3]);
            AddItem(plr, slot, stack, itemID);
        }

        public void ArmorEdit(CommandArgs args)
        {
            string player = args.Parameters[0];
            var players = TSPlayer.FindByNameOrID(player);
            if (players == null || players.Count == 0)
            {
                args.Player.SendErrorMessage("worg");
                return;
            }
            var plr = players[0];
            int slot = Convert.ToInt32(args.Parameters[1]);
            int itemID = Convert.ToInt32(args.Parameters[2]);
            AddArmor(plr, slot, itemID);
        }

        public void MiscEquipsEdit(CommandArgs args)
        {
            string player = args.Parameters[0];
            var players = TSPlayer.FindByNameOrID(player);
            if (players == null || players.Count == 0)
            {
                args.Player.SendErrorMessage("worg");
                return;
            }
            var plr = players[0];
            int slot = Convert.ToInt32(args.Parameters[1]);
            int itemID = Convert.ToInt32(args.Parameters[2]);
            AddMiscEquip(plr, slot, itemID);
        }

        public void SetInventory(List<InventorySlot> InvSlots, TSPlayer player)
        {
            ClearPlayerEverything(player);
            foreach (InventorySlot InvSlot in InvSlots)
                AddItem(player, InvSlot.Slot, InvSlot.Stack, InvSlot.ItemID);
        }

        public class InventorySlot
        {
            public int Slot;
            public int ItemID;
            public int Stack;
        }

        private InventorySlot ConvertInventorySlot(SpleefResurgence.InventorySlot InventorySlot)
        {
            return new InventorySlot
            {
                Slot = InventorySlot.Slot,
                ItemID = InventorySlot.ItemID,
                Stack = InventorySlot.Stack
            };
        }

        public void SetInventoryCommand(CommandArgs args)
        {
            var player = args.Player;
            string templateName = args.Parameters[0];
            var inventoryTemplate = PluginSettings.Config.InventoryTemplates.FirstOrDefault(c => c.Name == templateName);
            if (inventoryTemplate != null)
            {
                List<InventorySlot> InvSlots = new List<InventorySlot>();
                foreach (var InvSlot in inventoryTemplate.InvSlots)
                    InvSlots.Add(ConvertInventorySlot(InvSlot));
                SetInventory(InvSlots, player);
                player.SendSuccessMessage($"set {templateName}");
            }
            else
                player.SendErrorMessage("that's not a real template");
        }
    }
}
using System;
using Terraria;
using TShockAPI;

namespace SpleefResurgence
{
    public class InventoryEdit
    {
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
    }
}
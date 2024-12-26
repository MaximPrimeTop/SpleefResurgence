using System;
using Terraria;
using TShockAPI;

namespace SpleefResurgence
{
    public class InventoryEdit
    {
        public void AddItem(TSPlayer player, int slot, int stack, int itemID)
        {
            player.TPlayer.inventory[slot].SetDefaults(itemID);
            player.TPlayer.inventory[slot].stack = stack;

            // Sync changes with the modified player
            player.SendData(PacketTypes.PlayerSlot, null, player.Index, slot);

            // Sync changes with all other players
            TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, player.Index, slot);
        }

        public void AddArmor(TSPlayer player, int slot, int itemID)
        {
            player.TPlayer.armor[slot].SetDefaults(itemID);
            player.TPlayer.armor[slot].stack = 1;

            // Sync changes with the modified player
            player.SendData(PacketTypes.PlayerSlot, null, player.Index, slot);

            // Sync changes with all other players
            TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, player.Index, slot);
        }

        public void ClearPlayerEverything(TSPlayer player)
        {
            // Clear main inventory slots
            for (int i = 0; i < player.TPlayer.inventory.Length; i++)
            {
                player.TPlayer.inventory[i].TurnToAir();
                player.SendData(PacketTypes.PlayerSlot, null, player.Index, i);
                TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, player.Index, i);
            }

            // Clear armor and accessory slots
            for (int i = 0; i < player.TPlayer.armor.Length; i++)
            {
                player.TPlayer.armor[i].TurnToAir();
                player.SendData(PacketTypes.PlayerSlot, null, player.Index, 59 + i);
                TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, player.Index, 59 + i);
            }

            // Clear miscellaneous equipment slots
            for (int i = 0; i < player.TPlayer.miscEquips.Length; i++)
            {
                player.TPlayer.miscEquips[i].TurnToAir();
                player.SendData(PacketTypes.PlayerSlot, null, player.Index, 75 + i);
                TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, player.Index, 75 + i);
            }
        }
    }
}
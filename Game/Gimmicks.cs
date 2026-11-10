using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;
using Terraria;
using Terraria.DataStructures;
using TShockAPI;

namespace SpleefResurgence.Game
{
    [JsonConverter(typeof(GimmickJsonConverter))]
    public abstract class Gimmick
    {
        public int WaitTime { get; set; }

        public string AnnounceText { get; set; }

        public string TimedAnnounceText { get; set; }

        public async void Apply(List<TSPlayer> players, bool isSilent = false)
        {
            if (players == null || players.Count == 0)
                return;
            if (!isSilent)
                TShock.Utils.Broadcast(AnnounceText, Color.White);
            if (WaitTime != 0)
            {
                await Task.Delay(WaitTime);
                if (!isSilent)
                    TShock.Utils.Broadcast(TimedAnnounceText, Color.White);
            }
            Action(players);
        }

        public abstract void Action(List<TSPlayer> players);

        public abstract string GetInfo();

        public string GetFullInfo()
        {
            return
                $"Type: {this.GetType().Name.Remove(0, 7)}\n" +
                $"Details: {GetInfo()}\n" +
                $"Wait Time: {WaitTime}s\n" +
                $"Announce text: {AnnounceText}\n" +
                $"Times announce text: {TimedAnnounceText}";
        }
    }

    public class GimmickNone : Gimmick
    {
        public override void Action(List<TSPlayer> players) { }
        public override string GetInfo() => "No action";
        public GimmickNone()
        {
            WaitTime = 0;
            AnnounceText = "[i:776] [c/00FFFF:Normal round] [i:776]";
            TimedAnnounceText = "And... nothing happened";
        }
    }

    public class GimmickItem : Gimmick
    {
        public int ItemID { get; set; }
        public int Stack { get; set; } = 1;
        public int Slot { get; set; } = -1;

        public override void Action(List<TSPlayer> players) => players.ForEach(player => InventoryEdit.AddItem(player, Slot, Stack, ItemID));

        public override string GetInfo()
        {
            if (Slot == -1)
                return $"ItemID: {ItemID}, Stack: {Stack}, Slot: -1 (first free slot)";
            else
                return $"ItemID: {ItemID}, Stack: {Stack}, Slot: {Slot}";
        }

        public GimmickItem(int itemID, int waitTime, int stack = 1, int slot = -1, string announceText = "", string timedAnnounceText = "")
        {
            ItemID = itemID;
            Stack = stack;
            WaitTime = waitTime;
            Slot = slot;
            if (announceText == "")
                AnnounceText = $"[i:{itemID}] [c/00FFFF:{TShock.Utils.GetItemById(itemID).Name} round] [i:{itemID}]";
            else
                AnnounceText = announceText;
            if (timedAnnounceText == "")
                TimedAnnounceText = $"[i:{itemID}] [c/FF1493:{TShock.Utils.GetItemById(itemID).Name}s have been given out!] [i:{itemID}]";
            else
                TimedAnnounceText = timedAnnounceText;
        }
    }

    public class GimmickAccessory : Gimmick
    {
        public int ItemID { get; set; }
        public int Slot { get; set; } = -1;

        public override void Action(List<TSPlayer> players) => players.ForEach(player => InventoryEdit.AddArmor(player, Slot, ItemID));

        public override string GetInfo()
        {
            if (Slot == -1)
                return $"ItemID: {ItemID}, Slot: -1 (first free slot)";
            else
                return $"ItemID: {ItemID}, Slot: {Slot - 2}";
        }

        public GimmickAccessory(int itemID, int waitTime, int slot = -1, string announceText = "", string timedAnnounceText = "")
        {
            ItemID = itemID;
            Slot = slot;
            WaitTime = waitTime;
            if (announceText == "")
                AnnounceText = $"[i:{itemID}] [c/00FFFF:{TShock.Utils.GetItemById(itemID).Name} round] [i:{itemID}]";
            else
                AnnounceText = announceText;
            if (timedAnnounceText == "")
                TimedAnnounceText = $"[i:{itemID}] [c/FF1493:{TShock.Utils.GetItemById(itemID).Name}s have been given out! Check your accessories!] [i:{itemID}]";
            else
                TimedAnnounceText = timedAnnounceText;
        }
    }

    public class GimmickBuff : Gimmick
    {
        public int BuffID { get; set; }
        public int BuffDuration { get; set; }

        public override void Action(List<TSPlayer> players) => players.ForEach(player => player.SetBuff(BuffID, BuffDuration * 60));

        public override string GetInfo() => $"BuffID: {BuffID}, BuffDuration: {BuffDuration} seconds";

        public GimmickBuff(int buffID, int buffDuration, int waitTime, string announceText = "", string timedAnnounceText = "")
        {
            BuffID = buffID;
            BuffDuration = buffDuration;
            WaitTime = waitTime;
            if (announceText == "")
                AnnounceText = $"[c/00FFFF:{TShock.Utils.GetBuffName(buffID)} round]";
            else
                AnnounceText = announceText;
            if (timedAnnounceText == "")
                TimedAnnounceText = $"[c/FF1493:You were buffed with {TShock.Utils.GetBuffName(buffID)}!]";
            else
                TimedAnnounceText = timedAnnounceText;
        }
    }

    public class GimmickMount : Gimmick
    {
        private const int MountSlot = 3;
        public int ItemID { get; set; }

        public override void Action(List<TSPlayer> players) => players.ForEach(player => InventoryEdit.AddMiscEquip(player, MountSlot, ItemID));

        public override string GetInfo() => $"ItemID: {ItemID}";

        public GimmickMount(int itemID, int waitTime, string announceText = "", string timedAnnounceText = "")
        {
            ItemID = itemID;
            WaitTime = waitTime;
            if (announceText == "")
                AnnounceText = $"[i:{itemID}] [c/00FFFF:{TShock.Utils.GetItemById(itemID).Name} round] [i:{itemID}]";
            else
                AnnounceText = announceText;
            if (timedAnnounceText == "")
                TimedAnnounceText = $"[i:{itemID}] [c/FF1493:{TShock.Utils.GetItemById(itemID).Name}s have been given out! Check your accessories!] [i:{itemID}]";
            else
                TimedAnnounceText = timedAnnounceText;
        }
    }

    public class GimmickMob : Gimmick
    {
        public int MobID { get; set; }
        public int MobAmount { get; set; }
        public int MobSpawnTileX { get; set; }
        public int MobSpawnTileY { get; set; }

        public override void Action(List<TSPlayer> players)
        {
            var source = new EntitySource_DebugCommand();
            for (int i = 0; i < MobAmount; i++)
            {
                int spawnX = MobSpawnTileX * 16;
                int spawnY = MobSpawnTileY * 16;
                int index = NPC.NewNPC(source, spawnX, spawnY, MobID);
                Main.npc[index].netUpdate = true;
            }
        }

        public override string GetInfo() => $"MobID: {MobID}, MobAmount: {MobAmount}, SpawnTileX: {MobSpawnTileX}, SpawnTileY: {MobSpawnTileY}";

        public GimmickMob(int mobID, int mobAmount, int waitTime, int mobSpawnTileX, int mobSpawnTileY, string announceText = "", string timedAnnounceText = "")
        {
            MobID = mobID;
            MobAmount = mobAmount;
            MobSpawnTileX = mobSpawnTileX;
            MobSpawnTileY = mobSpawnTileY;
            WaitTime = waitTime;
            if (announceText == "")
                AnnounceText = $"[i:[c/00FFFF:mob round]";
            else
                AnnounceText = announceText;
            if (timedAnnounceText == "")
                TimedAnnounceText = $"[c/FF1493:<insert mob name> has been spawned in! :fear:]";
            else
                TimedAnnounceText = timedAnnounceText;
        }
    }
}

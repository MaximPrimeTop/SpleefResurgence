using TShockAPI;
using Terraria;
using Terraria.DataStructures;
using System.Text.Json.Serialization;

namespace SpleefResurgence.Game
{
    [JsonConverter(typeof(GimmickJsonConverter))]
    public abstract class Gimmick
    {
        public int WaitTime { get; set; }

        public async void Apply(List<TSPlayer> players)
        {
            if (players == null || players.Count == 0)
                return;
            if (WaitTime != 0)
                await Task.Delay(WaitTime);
            Action(players);
        }

        public abstract void Action(List<TSPlayer> players);

        public abstract string GetInfo();
    }

    public class GimmickNone : Gimmick
    {
        public override void Action(List<TSPlayer> players) { }
        public override string GetInfo() => "No action";
        public GimmickNone()
        {
            WaitTime = 0;
        }
    }

    public class GimmickItem : Gimmick
    {
        public int ItemID { get; set; }
        public int Stack { get; set; } = 1;
        public int Slot { get; set; } = -1;

        public override void Action(List<TSPlayer> players) => players.ForEach(player => InventoryEdit.AddItem(player, Slot, Stack, ItemID));

        public override string GetInfo() => $"ItemID: {ItemID}, Stack: {Stack}, Slot: {Slot}";

        public GimmickItem(int itemID, int waitTime, int stack = 1, int slot = -1)
        {
            ItemID = itemID;
            Stack = stack;
            WaitTime = waitTime;
            Slot = slot;
        }
    }

    public class GimmickAccessory : Gimmick
    {
        public int ItemID { get; set; }
        public int Slot { get; set; } = -1;

        public override void Action(List<TSPlayer> players) => players.ForEach(player => InventoryEdit.AddArmor(player, Slot, ItemID));

        public override string GetInfo() => $"ItemID: {ItemID}, Slot: {Slot}";

        public GimmickAccessory(int itemID, int waitTime, int slot = -1)
        {
            ItemID = itemID;
            Slot = slot;
            WaitTime = waitTime;
        }
    }

    public class GimmickBuff : Gimmick
    {
        public int BuffID { get; set; }
        public int BuffDuration { get; set; }

        public override void Action(List<TSPlayer> players) => players.ForEach(player => player.SetBuff(BuffID, BuffDuration * 60));

        public override string GetInfo() => $"BuffID: {BuffID}, BuffDuration: {BuffDuration} seconds";

        public GimmickBuff(int buffID, int buffDuration, int waitTime)
        {
            BuffID = buffID;
            BuffDuration = buffDuration;
            WaitTime = waitTime;
        }
    }

    public class GimmickMount : Gimmick
    {
        private const int MountSlot = 3;
        public int ItemID { get; set; }

        public override void Action(List<TSPlayer> players) => players.ForEach(player => InventoryEdit.AddMiscEquip(player, MountSlot, ItemID));

        public override string GetInfo() => $"ItemID: {ItemID}";

        public GimmickMount(int itemID, int waitTime)
        {
            ItemID = itemID;
            WaitTime = waitTime;
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

        public GimmickMob(int mobID, int mobAmount, int waitTime, int mobSpawnTileX, int mobSpawnTileY)
        {
            MobID = mobID;
            MobAmount = mobAmount;
            MobSpawnTileX = mobSpawnTileX;
            MobSpawnTileY = mobSpawnTileY;
            WaitTime = waitTime;
        }
    }
}

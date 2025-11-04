using TShockAPI;
using Terraria;
using Terraria.DataStructures;
using System.Text.Json.Serialization;

namespace SpleefResurgence.Game
{
    [JsonConverter(typeof(GimmickJsonConverter))]
    public abstract class Gimmick
    {
        public string Name { get; set; }
        public int WaitTime { get; set; }

        public async void ApplyGimmick(List<TSPlayer> players)
        {
            if (players == null || players.Count == 0)
                return;
            if (WaitTime != 0)
                await Task.Delay(WaitTime);
            GimmickAction(players);
        }

        public abstract void GimmickAction(List<TSPlayer> players);
    }

    public class GimmickNone : Gimmick
    {
        public override void GimmickAction(List<TSPlayer> players) { }
        public GimmickNone(string name)
        {
            Name = name;
            WaitTime = 0;
        }
    }

    public class GimmickItem : Gimmick
    {
        public int ItemID { get; set; }
        public int Stack { get; set; } = 1;

        public override void GimmickAction(List<TSPlayer> players) => players.ForEach(player => player.GiveItem(ItemID, Stack));

        public GimmickItem(string name, int itemID, int waitTime, int stack = 1)
        {
            Name = name;
            ItemID = itemID;
            Stack = stack;
            WaitTime = waitTime;
        }
    }

    public class GimmickAccessory : Gimmick
    {
        public int ItemID { get; set; }
        public int Slot { get; set; } = -1;

        public override void GimmickAction(List<TSPlayer> players) => players.ForEach(player => InventoryEdit.AddArmor(player, Slot, ItemID));

        public GimmickAccessory(string name, int itemID, int waitTime, int slot = -1)
        {
            Name = name;
            ItemID = itemID;
            Slot = slot;
            WaitTime = waitTime;
        }
    }

    public class GimmickBuff : Gimmick
    {
        public int BuffID { get; set; }
        public int BuffDuration { get; set; }

        public override void GimmickAction(List<TSPlayer> players) => players.ForEach(player => player.SetBuff(BuffID, BuffDuration * 60));

        public GimmickBuff(string name, int buffID, int buffDuration, int waitTime)
        {
            Name = name;
            BuffID = buffID;
            BuffDuration = buffDuration;
            WaitTime = waitTime;
        }
    }

    public class GimmickMount : Gimmick
    {
        private const int MountSlot = 3;
        public int ItemID { get; set; }
        public override void GimmickAction(List<TSPlayer> players) => players.ForEach(player => InventoryEdit.AddMiscEquip(player, MountSlot, ItemID));
        public GimmickMount(string name, int itemID, int waitTime)
        {
            Name = name;
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

        public override void GimmickAction(List<TSPlayer> players)
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

        public GimmickMob(string name, int mobID, int mobAmount, int waitTime, int mobSpawnTileX, int mobSpawnTileY)
        {
            Name = name;
            MobID = mobID;
            MobAmount = mobAmount;
            MobSpawnTileX = mobSpawnTileX;
            MobSpawnTileY = mobSpawnTileY;
            WaitTime = waitTime;
        }
    }
}

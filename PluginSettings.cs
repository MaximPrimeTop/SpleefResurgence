using Newtonsoft.Json;
using TShockAPI;

namespace SpleefResurgence
{
    public class DynamicCommand
    {
        public string Name { get; set; }
        public string Permission { get; set; }
        [JsonProperty("Command execution")]
        public List<string> CommandList { get; set; }
    }

    public class ItemGimmick
    {
        [JsonProperty("Type")]
        public string Type { get; set; } = "inventory";
        [JsonProperty("ID")]
        public int ID { get; set; } = 0;
        [JsonProperty("Stack")]
        public int Stack { get; set; } = 1;
        [JsonProperty("Slot")]
        public int Slot { get; set; } = -1;
    }

    public class BuffGimmick
    {
        [JsonProperty("ID")]
        public int ID { get; set; } = 0;
        [JsonProperty("Time")]
        public int TimeInSeconds { get; set; } = 60;
    }

    public class ArenaSpawn
    {
        [JsonProperty("X")]
        public int X { get; set; }
        [JsonProperty("Y")]
        public int Y { get; set; }
    }

    public class Map
    {
        [JsonProperty("Map Name")]
        public string MapName { get; set; }
        [JsonProperty("Map Command")]
        public string MapCommand { get; set; } = "null";
        [JsonProperty("Other lavarise command")]
        public string OtherLavariseCommand { get; set; } = "null";
        [JsonProperty("Other dirt randomize command")]
        public string OtherRandomizeDirtCommand { get; set; } = "null";
        [JsonProperty("Arena spawns")]
        public List<ArenaSpawn> ArenaSpawns { get; set; } = new();
        [JsonProperty("Additional items")]
        public List<ItemGimmick> Items { get; set; } = new();
        [JsonProperty("Additional buffs")]
        public List<BuffGimmick> Buffs { get; set; } = new();
    }

    public class GameTemplate
    {
        public string Name { get; set; }
        [JsonProperty("Lavarise command")]
        public string LavariseCommand { get; set; } = "null";
        [JsonProperty("Fill command")]
        public string FillCommand { get; set; } = "null";
        [JsonProperty("Dirt randomize command")]
        public string RandomizeDirtCommand { get; set; } = "null";
        public List<Map> Maps { get; set; }
    }

    public class InventorySlot
    {
        [JsonProperty("Inventory type")]
        public string InvType { get; set; } = "inventory";
        public int Slot { get; set; } = -1;
        [JsonProperty("Item ID")]
        public int ItemID { get; set; } = 0;
        public int Stack { get; set; } = 1;
    }

    public class InventoryTemplate
    {
        public string Name { get; set; }
        [JsonProperty("Inventory")]
        public List<InventorySlot> InvSlots { get; set; }
    }

    public class PluginSettings
    {
        private static readonly string filePath = Path.Combine(TShock.SavePath, "spleef.json");
        public static PluginSettings Config { get; set; } = new();
        
        [JsonProperty("Custom commands")]
        public List<DynamicCommand> AllCommands { get; set; } = new();

        [JsonProperty("Game templates")]
        public List<GameTemplate> GameTemplates { get; set; } = new();

        [JsonProperty("Inventory Templates")]
        public List<InventoryTemplate> InventoryTemplates { get; set; } = new();

        public static void Save()
        {
            string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static void Load()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                try
                {
                    Config = JsonConvert.DeserializeObject<PluginSettings>(json);
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError("Config could not load: " + ex.Message);
                    TShock.Log.ConsoleError(ex.StackTrace);
                }
            }
            else
            {
                Save();
            }
        }
    }
}
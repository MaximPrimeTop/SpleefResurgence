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

    public class GameTemplate
    {
        public string Name { get; set; }
        [JsonProperty("Lavarise command")]
        public string LavariseCommand { get; set; }
        [JsonProperty("Fill command")]
        public string FillCommand { get; set; }
        [JsonProperty("Arena spawn X")]
        public int tpposx { get; set; }
        [JsonProperty("Arena spawn Y")]
        public int tpposy { get; set; }
        [JsonProperty("Normal arena command")]
        public string NormalArenaCommand { get; set; }
        [JsonProperty("Snow arena command")]
        public string SnowArenaCommand { get; set; }
        [JsonProperty("Landmine arena command")]
        public string LandmineArenaCommand { get; set; }
    }

    public class PluginSettings
    {
        private static readonly string filePath = Path.Combine(TShock.SavePath, "spleef.json");
        public static PluginSettings Config { get; set; } = new();
        
        [JsonProperty("Custom commands")]
        public List<DynamicCommand> AllCommands { get; set; } = new();

        [JsonProperty("Game templates")]
        public List<GameTemplate> GameTemplates { get; set; } = new();

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
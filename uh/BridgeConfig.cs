using System.Text.Json;
using System.Xml;
using TShockAPI;

namespace SpleefResurgence.uh
{
    public class BridgeConfig
    {
        private static readonly string filePath = Path.Combine(TShock.SavePath,"Spleef", "spleefbridgeconfig.json");
        public bool isEnabled { get; set; }
        public string apiKey { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }

        public BridgeConfig(bool isEnabled, string apiKey, string iP, int port)
        {
            this.isEnabled = isEnabled;
            this.apiKey = apiKey;
            IP = iP;
            Port = port;
        }

        private static void DefaultValues()
        {
            var d = new BridgeConfig(false, "", "127.0.0.1", 8000);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(d, options);
            File.WriteAllText(filePath, json);
        }
        public static BridgeConfig Load()
        {
            Directory.CreateDirectory(Path.Combine(TShock.SavePath, "Spleef"));
            if (!File.Exists(filePath))
            {
                DefaultValues();
            }
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<BridgeConfig>(json);
        }
    }
}

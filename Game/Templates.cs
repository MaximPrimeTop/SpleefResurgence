using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TShockAPI;

namespace SpleefResurgence.Game
{
    public class GameConfig
    {
        private readonly static string folderPath = Path.Combine(TShock.SavePath, "Spleef");
        private readonly static string ArenaPath = Path.Combine(folderPath, "Arena Templates");
        private readonly static string GimmickPath = Path.Combine(folderPath, "Gimmick Templates");
        private readonly static string MapPath = Path.Combine(folderPath, "Map Templates");

        public static void SetupConfig()
        {
            Directory.CreateDirectory(folderPath);
            Directory.CreateDirectory(ArenaPath);
            Directory.CreateDirectory(MapPath);
            Directory.CreateDirectory(GimmickPath);

            foreach (var arenaName in Directory.EnumerateFiles(ArenaPath))
            {
                string fileName = Path.GetFileName(arenaName);
                fileName = fileName.Substring(0, fileName.Length - 5);
                Directory.CreateDirectory(Path.Combine(MapPath, fileName));
            }

            if (!File.Exists(Path.Combine(GimmickPath, "normal.json")))
            {
                Gimmick normal = new GimmickNone("normal");
                GimmickJson.SaveGimmick(normal, "normal");
            }      
        }

        public class ArenaJson
        {
            public static void SaveArena(Arena arena)
            {
                string json = JsonConvert.SerializeObject(arena);
                string filePath = Path.Combine(ArenaPath, $"{arena.Name.ToLowerInvariant}.json");
                File.WriteAllText(filePath, json);
            }

            public static Arena LoadArena(string name)
            {
                string filePath = Path.Combine(ArenaPath, $"{name}.json");
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Arena>(json);
            }

            public static List<string> ListArenaNames()
            {
                List<string> arenaNames = new List<string>();
                foreach (var arena in Directory.EnumerateFiles(ArenaPath))
                {
                    string fileName = Path.GetFileName(arena);
                    fileName = fileName.Substring(0, fileName.Length - 5);
                    arenaNames.Add(fileName);
                }
                return arenaNames;
            }
        }

        public class GimmickJson
        {
            public static void SaveGimmick(Gimmick gimmick, string name)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = System.Text.Json.JsonSerializer.Serialize(gimmick, options);
                string filePath = Path.Combine(GimmickPath, $"{name.ToLowerInvariant()}.json");
                File.WriteAllText(filePath, json);
            }

            public static Gimmick LoadGimmick(string name)
            {
                string filePath = Path.Combine(GimmickPath, $"{name}.json");
                var options = new JsonSerializerOptions();
                string json = File.ReadAllText(filePath);

                return System.Text.Json.JsonSerializer.Deserialize<Gimmick>(json, options);
            }

            public static List<string> ListGimmickNames()
            {
                List<string> gimmickNames = new List<string>();
                foreach (var gimmick in Directory.EnumerateFiles(GimmickPath))
                {
                    string fileName = Path.GetFileName(gimmick);
                    fileName = fileName.Substring(0, fileName.Length - 5);
                    gimmickNames.Add(fileName);
                }
                return gimmickNames;
            }
        }

        public class MapJson
        {
            public static void SaveMap(Map map, string name, string arenaName)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = System.Text.Json.JsonSerializer.Serialize(map, options);
                string filePath = Path.Combine(MapPath, arenaName, $"{name.ToLowerInvariant}.json");
                File.WriteAllText(filePath, json);
            }

            public static Map LoadMap(string name, string arenaName)
            {
                string filePath = Path.Combine(MapPath, arenaName, $"{name}.json");
                var options = new JsonSerializerOptions();
                string json = File.ReadAllText(filePath);
                return System.Text.Json.JsonSerializer.Deserialize<Map>(json, options);
            }

            public static List<string> ListMapNames(string arenaName)
            {
                List<string> mapNames = new List<string>();
                string directoryPath = Path.Combine(MapPath, arenaName);
                if (!Directory.Exists(directoryPath))
                    return mapNames;
                foreach (var map in Directory.EnumerateFiles(directoryPath))
                {
                    string fileName = Path.GetFileName(map);
                    fileName = fileName.Substring(0, fileName.Length - 5);
                    mapNames.Add(fileName);
                }
                return mapNames;
            }

            public static List<Tuple<string, List<string>>> ListAllMapNames()
            {
                List<Tuple<string, List<string>>> arenaMaps = new List<Tuple<string, List<string>>>();
                foreach (var arena in Directory.EnumerateDirectories(MapPath))
                {
                    string arenaName = Path.GetFileName(arena);
                    List<string> mapNames = new List<string>();
                    foreach (var map in Directory.EnumerateFiles(arena))
                    {
                        string fileName = Path.GetFileName(map);
                        fileName = fileName.Substring(0, fileName.Length - 5);
                        mapNames.Add(fileName);
                    }
                    arenaMaps.Add(new Tuple<string, List<string>>(arenaName, mapNames));
                }
                return arenaMaps;
            }
        }
    }
}

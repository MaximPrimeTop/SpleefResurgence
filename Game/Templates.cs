using System.Text.Json;
using TShockAPI;

namespace SpleefResurgence.Game
{
    public class GameConfig
    {
        private static readonly string FolderPath = Path.Combine(TShock.SavePath, "Spleef");
        private static readonly string ArenaPath = Path.Combine(FolderPath, "Arena Templates");
        private static readonly string GimmickPath = Path.Combine(FolderPath, "gimmicks.json");

        public static void SetupConfig()
        {
            if (!File.Exists(GimmickPath))
            {
                Directory.CreateDirectory(FolderPath);
                Directory.CreateDirectory(ArenaPath);

                GimmickJson.SaveGimmicks(new()
                {
                    ["normal"] = new GimmickNone()
                });
            }
    }

        public class ArenaJson
        {
            private static readonly JsonSerializerOptions Options = new()
            {
                WriteIndented = true
            };

            public static void SaveArena(Arena arena)
            {
                string folder = Path.Combine(ArenaPath, arena.Name);

                Directory.CreateDirectory(folder);

                File.WriteAllText(
                    Path.Combine(folder, "arena.json"),
                    JsonSerializer.Serialize(arena, Options));
            }

            public static Arena LoadArena(string arenaName)
            {
                string path = Path.Combine(ArenaPath, arenaName, "arena.json");
                if (!Path.Exists(path))
                    return null;
                string json = File.ReadAllText(path);

                return JsonSerializer.Deserialize<Arena>(json)!;
            }

            public static List<string> ListArenaNames()
            {
                return Directory.EnumerateDirectories(ArenaPath).Select(Path.GetFileName).ToList();
            }

            public static bool DeleteArena(string arenaName)
            {
                string folder = Path.Combine(ArenaPath, arenaName);
                if (!Directory.Exists(folder))
                    return false;
                Directory.Delete(folder, true);
                return true;
            }
        }

        public class MapJson
        {
            private static readonly JsonSerializerOptions Options = new()
            {
                WriteIndented = true
            };

            public static void SaveMaps(List<Map> maps, string arenaName)
            {
                string folder = Path.Combine(ArenaPath, arenaName);

                Directory.CreateDirectory(folder);

                File.WriteAllText(
                    Path.Combine(folder, "maps.json"),
                    JsonSerializer.Serialize(maps, Options));
            }

            public static List<Map> LoadMaps(string arenaName)
            {
                string file = Path.Combine(ArenaPath, arenaName, "maps.json");

                if (!File.Exists(file))
                    return new();

                return JsonSerializer.Deserialize<List<Map>>(
                    File.ReadAllText(file)) ?? new();
            }

            public static void SaveMap(Map map, string arenaName)
            {
                var maps = LoadMaps(arenaName);
                var existingMap = maps.FirstOrDefault(x => x.Name == map.Name);
                if (existingMap != null)
                    maps.Remove(existingMap);
                maps.Add(map);
                SaveMaps(maps, arenaName);
            }

            public static Map LoadMap(string arenaName, string mapName)
            {
                var maps = LoadMaps(arenaName);
                return maps.FirstOrDefault(x => x.Name == mapName) ?? throw new Exception($"Map '{mapName}' not found in arena '{arenaName}'.");
            }
                
            public static bool DeleteMap(string arenaName, string mapName)
            {
                var maps = LoadMaps(arenaName);
                var mapToRemove = maps.FirstOrDefault(x => x.Name == mapName);
                if (mapToRemove == null)
                    return false;
                maps.Remove(mapToRemove);
                SaveMaps(maps, arenaName);
                return true;
            }

            public static List<string> ListMapNames(string arenaName)
            {
                return LoadMaps(arenaName)
                    .Select(x => x.Name)
                    .ToList();
            }
        }

        public class GimmickJson
        {
            private static readonly JsonSerializerOptions Options = new()
            {
                WriteIndented = true
            };

            public static void SaveGimmicks(Dictionary<string, Gimmick> gimmicks)
            {
                File.WriteAllText(
                    GimmickPath,
                    JsonSerializer.Serialize(gimmicks, Options));
            }

            public static Dictionary<string, Gimmick> LoadGimmicks()
            {
                if (!File.Exists(GimmickPath))
                    return new();

                return JsonSerializer.Deserialize<Dictionary<string, Gimmick>>(
                    File.ReadAllText(GimmickPath)) ?? new();
            }

            public static Gimmick? GetGimmick(string name)
            {
                var gimmicks = LoadGimmicks();

                gimmicks.TryGetValue(name.ToLowerInvariant(), out var gimmick);

                return gimmick;
            }

            public static List<string> ListGimmickNames()
            {
                return LoadGimmicks().Keys.ToList();
            }

            public static void SaveGimmick(string name, Gimmick gimmick)
            {
                var gimmicks = LoadGimmicks();

                gimmicks[name.ToLowerInvariant()] = gimmick;

                SaveGimmicks(gimmicks);
            }

            public static bool RemoveGimmick(string name)
            {
                var gimmicks = LoadGimmicks();

                if (!gimmicks.Remove(name.ToLowerInvariant()))
                    return false;

                SaveGimmicks(gimmicks);

                return true;
            }
        }
    }
}

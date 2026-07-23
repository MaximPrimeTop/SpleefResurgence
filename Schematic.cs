using Terraria;
using OTAPI;
using TShockAPI;
using System.Text.Json.Serialization;
using System.Text.Json;
using SpleefResurgence.Game;
using Terraria.Enums;
using ModFramework;
using System.Linq;
using Terraria.ID;
using Terraria.Chat;
using ZstdSharp;

namespace SpleefResurgence;

public class Schematic
{
    public string Name { get; set; }
    public Tile[] Tiles { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Schematic() { }

    [JsonConstructor]
    public Schematic(string name, Tile[] tiles, int width, int height)
    {
        Name = name;
        Tiles = tiles;
        Width = width;
        Height = height;
    }

    public static Tile[] CopyArea(int x, int y, int width, int height)
    {
        Tile[] schematicArea = new Tile[width * height];
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                schematicArea[i + height * j] = new Tile(Main.tile[i + x, j + y]);
            }
        }

        return schematicArea;
    }

    public static void PasteSchematic(Schematic schematic, int x, int y)
    {
        for (int i = 0; i < schematic.Width; i++)
        {
            for (int j = 0; j < schematic.Height; j++)
            {
                Main.tile[x + i, y + j] = schematic.Tiles[i + j * schematic.Width];
            }
        }

        NetMessage.SendTileSquare(-1, x, y, schematic.Width, schematic.Height, TileChangeType.None);
    }
}

public class SchematicJson
{
    private static readonly string SchematicPath = Path.Combine(TShock.SavePath, "Schematics");
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public static void SaveSchematic(Schematic schematic)
    {
        string folder = Path.Combine(SchematicPath, schematic.Name + ".json");
        File.WriteAllText(folder, JsonSerializer.Serialize(schematic, Options));
    }

    public static Schematic LoadSchematic(string schematicName)
    {
        string schematicPath = Path.Combine(SchematicPath, schematicName + ".json");

        if (!File.Exists(schematicPath))
        {
            return null;
        }
        
        Schematic schematic = JsonSerializer.Deserialize<Schematic>(File.ReadAllText(schematicPath), Options);
        return schematic;
    }

    public static List<string> GetSchematicNames()
    {
        return Directory.EnumerateFiles(SchematicPath).Select(Path.GetFileNameWithoutExtension).ToList();
    }

    public static bool DeleteSchematic(string schematicName)
    {
        string folder = Path.Combine(SchematicPath, schematicName + ".json");

        if (!File.Exists(folder))
            return false;

        File.Delete(folder);
        return true;
    }

    public static void SaveSchematicCommand(CommandArgs args)
    {
        if (args.Parameters.Count < 4)
        {
            args.Player.SendErrorMessage("Invalid command usage!\n/saveschematic <name> <x> <y> <width> <height>");
            return;
        }

        string name = args.Parameters[0];   
        int x = Convert.ToInt32(args.Parameters[1]);
        int y = Convert.ToInt32(args.Parameters[2]);
        int width = Convert.ToInt32(args.Parameters[3]);
        int height = Convert.ToInt32(args.Parameters[4]);

        SaveSchematic(new Schematic(name, Schematic.CopyArea(x, y, width, height), width, height));
        args.Player.SendSuccessMessage("Successfully saved schematic");
    }

    public static void LoadSchematicCommand(CommandArgs args)
    {
        if (args.Parameters.Count < 3)
        {
            args.Player.SendErrorMessage("Invalid command usage!\n/loadschematic <name> <x> <y>");
            return;
        }

        Schematic schematic = LoadSchematic(args.Parameters[0]);
        if (schematic == null)
        {
            args.Player.SendErrorMessage($"Schematic \"{args.Parameters[0]}\" not found");
            return;
        }

        Schematic.PasteSchematic(schematic, Convert.ToInt32(args.Parameters[1]), Convert.ToInt32(args.Parameters[2]));
        args.Player.SendSuccessMessage($"Successfully loaded schematic \"{args.Parameters[0]}\"");
    }

    public static void DeleteSchematicCommand(CommandArgs args)
    {
        if (DeleteSchematic(args.Parameters[0]))
        {
            args.Player.SendSuccessMessage($"Successfully deleted schematic \"{args.Parameters[0]}\"");
        }
        else
        {
            args.Player.SendErrorMessage($"Couldn't find schematic \"{args.Parameters[0]}\"");
        }
    }

    public static void ListSchematicsCommand(CommandArgs args)
    {
        foreach (var schematic in GetSchematicNames())
        {
            args.Player.SendInfoMessage(schematic);
        }
    }
}
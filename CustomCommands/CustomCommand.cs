using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TShockAPI;

namespace SpleefResurgence.CustomCommands
{
    public class CustomCommand
    {
        public string Name { get; set; }
        public string Permission { get; set; }
        public List<string> CommandList { get; set; } = new List<string>();
        public bool hasParent = false;
        public CustomCommand Parent;
        public Stopwatch Timer = new();

        public CustomCommand(string name, string permission, List<string> commandList, CustomCommand parent)
        {
            Name = name;
            Permission = permission;
            CommandList = commandList;
            hasParent = true;
            Parent = parent;
        }

        public CustomCommand(string name, string permission, List<string> commandList)
        {
            Name = name;
            Permission = permission;
            CommandList = commandList;
        }

        public CustomCommand(string filename)
        {
            string filepath = Path.Combine(TShock.SavePath, "Spleef", "CustomCommands", $"{filename}.json");
            string json = File.ReadAllText(filepath);
            CustomCommand command = JsonConvert.DeserializeObject<CustomCommand>(json);
            Name = command.Name;
            Permission = command.Permission;
            CommandList = command.CommandList;
        }

        public void ToJson()
        {
            string filepath = Path.Combine(TShock.SavePath, "Spleef", "CustomCommands", $"{Name}.json");
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filepath, json);
        }

        public void CommandLogic(CommandArgs args)
        {
            TSPlayer player = args.Player;
            switch (args.Parameters[0])
            {
                default:
                    for (int i = 0; i < CommandList.Count; i++)
                    {

                    }
                    return;
            }
        }
    }
}

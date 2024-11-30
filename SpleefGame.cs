using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;

namespace SpleefResurgence
{
    public class SpleefGame
    {
        private readonly Spleef pluginInstance;

        public SpleefGame(Spleef plugin)
        {
            this.pluginInstance = plugin;
        }

        private bool isGaming = false;

        private string CommandToStartRound;
        private string CommandToEndRound;
        private int tpx;
        private int tpy;
        private int NumOfPlayers;

        private Dictionary<string, int> PlayerScore = new();

        public void TheGaming(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("erm no");
                return;
            }

            if (args.Parameters.Count == 1)
            {
                if (args.Parameters[0] == "help")
                {
                    args.Player.SendInfoMessage("To start a game - /game start <CommandToStartRound> <CommandToEndRound> <tpx> <tpy> <NumOfPlayers> <list all the players here with spaces, put the name in quotes if the player has spaces");
                    args.Player.SendInfoMessage("To start a new round (you must use the previous command first!) - /game start");
                    args.Player.SendInfoMessage("To end a game /game end");
                }
                else if (args.Parameters[0] == "stop")
                {
                    if (isGaming)
                    {
                        foreach (KeyValuePair<string, int> plr in PlayerScore)
                        {
                            TSPlayer.All.SendMessage($"{plr.Key} : {plr.Value}", Color.Coral);
                        }
                        PlayerScore.Clear();
                        isGaming = false;
                        TSPlayer.All.SendMessage("Game ended", Color.SandyBrown);
                    }
                    else
                    {
                        args.Player.SendErrorMessage("erm no");
                        return;
                    }
                }
                else if (args.Parameters[0] == "start" && isGaming)
                {
                    Commands.HandleCommand(TSPlayer.Server, CommandToStartRound);
                    foreach (KeyValuePair<string, int> player in PlayerScore)
                    {
                        var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                        plr.Teleport(tpx, tpy);
                        plr.SetBuff(BuffID.Webbed, 60);
                    }
                    TSPlayer.All.SendMessage("Round started", Color.DarkSeaGreen);
                    ServerApi.Hooks.NetGetData.Register(pluginInstance, OnGetData);
                }
                else
                {
                    args.Player.SendErrorMessage("dumbass");
                    return;
                }
            }

            if (args.Parameters.Count >= 5  && args.Parameters[0] == "start" && !isGaming)
            {
                    isGaming = true;
                    CommandToStartRound = args.Parameters[1];
                    CommandToEndRound = args.Parameters[2];
                    tpx = Convert.ToInt32(args.Parameters[3]) * 16;
                    tpy = Convert.ToInt32(args.Parameters[4]) * 16;
                    NumOfPlayers = Convert.ToInt32(args.Parameters[5]);
                    for (int i = 6; i < NumOfPlayers + 6; i++)
                    {
                        PlayerScore.Add(args.Parameters[i], 0);
                    }
                    TSPlayer.All.SendMessage("Game started", Color.SeaShell);
                    Commands.HandleCommand(TSPlayer.Server, CommandToStartRound);
                    foreach (KeyValuePair<string, int> player in PlayerScore)
                    {
                        var plr = TSPlayer.FindByNameOrID(player.Key)[0];
                        plr.Teleport(tpx, tpy);
                        plr.SetBuff(BuffID.Webbed, 100);
                    }
                    TSPlayer.All.SendMessage("Round started", Color.DarkSeaGreen);
                    ServerApi.Hooks.NetGetData.Register(pluginInstance, OnGetData);
            }
        }

        private int counter = 0;

        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.PlayerDeathV2)
            {
                using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    byte playerID = reader.ReadByte();

                    var plr = TSPlayer.FindByNameOrID(Convert.ToString(playerID))[0];
                    foreach (KeyValuePair<string, int> player in PlayerScore)
                    {
                        var plrOG = TSPlayer.FindByNameOrID(player.Key)[0];
                        if (plr == plrOG)
                        {
                            PlayerScore[player.Key] += counter;
                            counter++;
                            if (counter == NumOfPlayers-1)
                            {
                                Commands.HandleCommand(TSPlayer.Server, CommandToEndRound);
                            }
                            if (counter == NumOfPlayers)
                            {
                                counter = 0;
                                TSPlayer.All.SendMessage("Round ended", Color.DarkKhaki);
                                foreach (KeyValuePair<string, int> plrr in PlayerScore)
                                {
                                    TSPlayer.All.SendMessage($"{plrr.Key} : {plrr.Value}", Color.Coral);
                                }
                                CustomCommandHandler.isCommanding = false;
                                ServerApi.Hooks.NetGetData.Deregister(pluginInstance, OnGetData);
                            }
                        }
                    }
                    
                }
            }
        }
    }
}

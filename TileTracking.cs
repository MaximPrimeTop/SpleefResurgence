using System;
using TerrariaApi.Server;
using TShockAPI;
using Terraria;

namespace SpleefResurgence
{
    public class TileTracker
    {
        private readonly Spleef pluginInstance;

        public TileTracker(Spleef plugin)
        {
            this.pluginInstance = plugin;
        }

        private readonly Dictionary<int, bool> trackingPlayers = new();

        public void ToggleTileTracking(CommandArgs args)
        {
            int playerIndex = args.Player.Index;
            if (trackingPlayers.ContainsKey(playerIndex) && trackingPlayers[playerIndex])
            {
                trackingPlayers[playerIndex] = false;
                args.Player.SendSuccessMessage("Tile tracking disabled.");
                foreach (var plr in trackingPlayers)
                {
                    if (plr.Value)
                    {
                        break;
                    }
                }
                ServerApi.Hooks.NetGetData.Deregister(pluginInstance, OnTileEdit);
            }
            else
            {
                ServerApi.Hooks.NetGetData.Register(pluginInstance, OnTileEdit);
                trackingPlayers[playerIndex] = true;
                args.Player.SendSuccessMessage("Tile tracking enabled. Interact with a tile to see its position.");
            }
        }

        private void OnTileEdit(GetDataEventArgs args)
        {
            if (args.Handled || args.MsgID != PacketTypes.Tile)
                return;

            var player = TShock.Players[args.Msg.whoAmI];
            if (player == null || !player.Active || !trackingPlayers.ContainsKey(player.Index) || !trackingPlayers[player.Index])
                return;

            using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
            {
                var action = reader.ReadByte();
                var x = reader.ReadInt16();
                var y = reader.ReadInt16();

                if (action is 0 or 1)
                {
                    args.Handled = true;
                    NetMessage.SendTileSquare(player.Index, x, y, 1);
                    player.SendInfoMessage($"X={x}, Y={y}");
                }
            }
        }

    }
}

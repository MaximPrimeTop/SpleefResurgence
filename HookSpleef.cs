/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TShockAPI;
using Terraria;
using Microsoft.Xna.Framework;
using MySqlX.XDevAPI.Relational;
using ClientApi.Networking;

namespace SpleefResurgence
{
    public class HookSpleef
    {
        private readonly Spleef pluginInstance;
        public static PluginSettings Config => PluginSettings.Config;

        public Vector2 playerVelocity;

        public HookSpleef(Spleef plugin)
        {
            this.pluginInstance = plugin;
            ServerApi.Hooks.NetGetData.Register(pluginInstance, OnGetData);
        }

        public void AddPos(CommandArgs args)
        {
            if (args.Parameters.Count != 2)
            {
                args.Player.SendErrorMessage("Usage: /addpos <X> <Y>");
                return;
            }
            int x = Convert.ToInt32(args.Parameters[0]);
            int y = Convert.ToInt32(args.Parameters[1]);
            Config.HookPos.Add(new int[] {x, y});
            PluginSettings.Save();
            args.Player.SendSuccessMessage($"brooo im so bored of this shit kind of but anyways added a pos at X: {x} Y: {y}");
        }


        
        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.ProjectileNew)
            {
                using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    int projectileID = reader.ReadInt16();
                    float posX = reader.ReadSingle();
                    float posY = reader.ReadSingle();
                    float projectileVelocityX = reader.ReadSingle();
                    float projectileVelocityY = reader.ReadSingle();
                    int playerID = reader.ReadByte();
                    int type = reader.ReadInt16();

                    int tilePosX = (int)((posX + 1) / 16f);
                    int tilePosY = (int)((posY + 1) / 16f);
                    
                    //TSPlayer.All.SendMessage($"Projectile ID: {projectileID}, Position: ({tilePosX}, {tilePosY}), Velocity: ({velocityX}, {velocityY}), Owner: {playerID}, Type: {type}, ", Color.DarkOliveGreen);

                    var projectile = Main.projectile[projectileID];
                    TSPlayer player = TShock.Players[playerID];

                    if (type == 73 && projectileVelocityX == 0 && projectileVelocityY == 0)
                    {
                        if (player != null && player.Active)
                        {
                            player.SendInfoMessage($"Your hook hit at X: {tilePosX}, Y: {tilePosY}");
                            foreach (var item in Config.HookPos)
                            {
                                if (tilePosX == item[0] && tilePosY == item[1])
                                {
                                    projectile.active = false;
                                    NetMessage.SendData((int)PacketTypes.ProjectileDestroy, -1, -1, null, projectileID);
                                    player.SendMessage($"Hook deleted at X: {tilePosX}, Y: {tilePosY}!", Color.CadetBlue);

                                    var t = Task.Delay(17);
                                    t.Wait();

                                    player.TPlayer.velocity = new Vector2(playerVelocity.X, playerVelocity.Y);
                                    NetMessage.SendData(13, -1, -1, null, player.Index);
                                    player.SendMessage($"Set velocity to X: {playerVelocity.X}, Y: {playerVelocity.Y}!", Color.CadetBlue);
                                }
                            }
                        }
                    }
                    else if (type == 73)
                    {
                        ServerApi.Hooks.GameUpdate.Register(pluginInstance, (args) =>
                        {
                            playerVelocity = player.TPlayer.velocity;
                            player.SendMessage($"Recording velocity X: {playerVelocity.X}, Y: {playerVelocity.Y}!", Color.CadetBlue);
                        });
                    }
                }
            }
        }
    }
}
*/
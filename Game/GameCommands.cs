using System;
using TShockAPI;
using Microsoft.Xna.Framework;
using Terraria;
namespace SpleefResurgence.Game
{
    public class GameCommands
    {
        public static List<SpleefGame> Games = new();

        public static void GameCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("erm no");
                return;
            }
            switch (args.Parameters[0])
            {
                case "help":
                    args.Player.SendInfoMessage("/game list (gimmick, template, map <templatename>)");
                    args.Player.SendInfoMessage("/game template <templateName> [isJoinable] [isBettable] - creates a new game. By default it's joinable and people can't place bets(bets aren't implemented yet).");
                    args.Player.SendMessage("these commands only work if there's a game going on!", Color.OrangeRed);
                    args.Player.SendInfoMessage("/game start <gimmickAmount> <list gimmicks> <map> or /game start <gimmick> <map> or  - starts a round");
                    args.Player.SendInfoMessage("/game stop - ends the game (alias /game end), however if there's a round active, stops the round.");
                    args.Player.SendInfoMessage("/game add <playername> - adds a player to the ongoing game");
                    args.Player.SendInfoMessage("/game remove <playername> - removes a player from the game");
                    args.Player.SendInfoMessage("/game score - shows the current score");
                    args.Player.SendInfoMessage("/game edit score/payout <playername> <amount> - changes the player's score by the given amount");
                    args.Player.SendInfoMessage("/game bet open/close/off/pay - opens/closes bets, turns them off completely or pays out the bets immediately");
                    args.Player.SendInfoMessage("/game reload - reloads stuff duh, useful for bet payouts in 1v1s");
                    break;
                case "list": // /game list (gimmick, template, map <templatename>)
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("you need to specify what you want to list!");
                        args.Player.SendInfoMessage("/game list (gimmick, template, map <templatename>)");
                        return;
                    }
                    switch (args.Parameters[1])
                    {
                        case "gimmick":
                            List<string> gimmicksList = GameConfig.GimmickJson.ListGimmickNames();
                            args.Player.SendMessage("Gimmicks:", Color.OrangeRed);
                            args.Player.SendInfoMessage(string.Join(", ", gimmicksList));
                            break;
                        case "template":
                            List<string> templates = GameConfig.ArenaJson.ListArenaNames();
                            args.Player.SendMessage("Templates:", Color.OrangeRed);
                            args.Player.SendInfoMessage(string.Join(", ", templates));
                            break;
                        case "map":
                            if (args.Parameters.Count < 3)
                            {
                                List<Tuple<string, List<string>>> mapsList = GameConfig.MapJson.ListAllMapNames();
                                foreach (var map in mapsList)
                                {
                                    args.Player.SendMessage($"{map.Item1}:", Color.OrangeRed);
                                    args.Player.SendInfoMessage(string.Join(", ", map.Item2));
                                }
                                return;
                            }
                            string templateNameToList = args.Parameters[2];
                            List<string> maps = GameConfig.MapJson.ListMapNames(templateNameToList);
                            args.Player.SendMessage($"Maps for template {templateNameToList}:", Color.OrangeRed);
                            args.Player.SendInfoMessage(string.Join(", ", maps));
                            break;
                        default:
                            args.Player.SendErrorMessage("invalid list type! Use gimmick, template or map.");
                            break;
                    }
                    break;
                case "template":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("you need to specify a template name");
                        return;
                    }
                    string templateName = args.Parameters[1];
                    bool isJoinable = true;
                    bool isBettable = false;

                    Arena arena = GameConfig.ArenaJson.LoadArena(templateName);
                    SpleefGame game = new SpleefGame(arena, args.Player.Account.Name, isJoinable, isBettable);
                    Games.Add(game);
                    args.Player.SendSuccessMessage($"created a new game with the name {templateName}! It has the id {Games.Count - 1}");
                    break;
                case "start": // /game start <map> <gimmicks>

                    if (args.Parameters.Count < 3)
                    {
                        args.Player.SendErrorMessage("you need to specify the map and the gimmicks");
                        return;
                    }

                    if (Games.Count == 0)
                    {
                        args.Player.SendErrorMessage("there's no game going on");
                        return;
                    }

                    if (!Games.Exists(game => game.isPlayerHoster(args.Player.Name)))
                    {
                        args.Player.SendErrorMessage("You aren't a hoster in any of the maps!");
                        return;
                    }

                    SpleefGame currentGame = Games.Find(game => game.isPlayerHoster(args.Player.Name));
                    if (currentGame.isRound)
                    {
                        args.Player.SendErrorMessage("there's already a round going on!");
                        return;
                    }

                    if (currentGame.Players.Count < 2)
                    {
                        args.Player.SendErrorMessage("there needs to be at least 2 players in the game to start a round!");
                        return;
                    }

                    string mapName = args.Parameters[1];
                    int mapID;
                    if (int.TryParse(mapName, out mapID))
                    {
                        if (mapID < 0 || mapID >= currentGame.Arena.Maps.Count)
                        {
                            args.Player.SendErrorMessage("invalid map id!");
                            return;
                        }
                        mapName = currentGame.Arena.Maps[mapID];
                    }
                    List<Gimmick> gimmicks = new List<Gimmick>();
                    for (int i = 2; i < args.Parameters.Count; i++)
                    {
                        string gimmickName = args.Parameters[i];
                        Gimmick gimmick = GameConfig.GimmickJson.LoadGimmick(gimmickName);
                        if (gimmick == null)
                        {
                            args.Player.SendErrorMessage($"Gimmick {gimmickName} not found!");
                            return;
                        }
                        gimmicks.Add(gimmick);
                    }
                    currentGame.StartRound(gimmicks, mapName);
                    break;
                case "stop":
                case "end":
                    if (Games.Count == 0)
                    {
                        args.Player.SendErrorMessage("there's no game going on");
                        return;
                    }

                    if (Games.Exists(game => game.isPlayerHoster(args.Player.Name)))
                    {
                        SpleefGame gameToStop = Games.Find(game => game.isPlayerHoster(args.Player.Name));

                        if (gameToStop.isRound)
                        {
                            TShock.Utils.Broadcast("The round has been forcefully ended by a hoster!", Color.BlueViolet);
                            gameToStop.ForceStopRound();
                        }
                        else
                        {
                            TShock.Utils.Broadcast("The game has been forcefully ended by a hoster!", Color.BlueViolet);
                            gameToStop.EndGame();
                            Games.Remove(gameToStop);
                        }
                    }
                    else
                        args.Player.SendErrorMessage("You aren't a hoster in any of the maps!");
                    break;
                case "add": // /game add <playername>
                    if (Games.Count == 0)
                    {
                        args.Player.SendErrorMessage("there's no game going on");
                        return;
                    }

                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("not enough parameters!");
                        args.Player.SendInfoMessage("/game add <playername>");
                        return;
                    }

                    if (Games.Exists(game => game.isPlayerHoster(args.Player.Name)))
                    {
                        SpleefGame gameToAdd = Games.Find(game => game.isPlayerHoster(args.Player.Name));
                        string playerNameToAdd = string.Join(" ", args.Parameters.Skip(1));

                        if (!SpleefGame.isPlayerOnline(playerNameToAdd, out TSPlayer playerToAdd))
                        {
                            args.Player.SendErrorMessage("this player isn't online");
                            return;
                        }

                        if (gameToAdd.Players.Exists(p => p.Name == playerToAdd.Name && p.isIngame))
                        {
                            args.Player.SendErrorMessage("this player is already in the game!");
                            return;
                        }
                        gameToAdd.AddPlayer(playerToAdd.Name, playerToAdd.Account.Name);
                    }
                    else
                        args.Player.SendErrorMessage("You aren't a hoster in any of the maps!");
                    break;
                case "remove": // /game remove <playername>
                    if (Games.Count == 0)
                    {
                        args.Player.SendErrorMessage("there's no game going on");
                        return;
                    }

                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("not enough parameters!");
                        args.Player.SendInfoMessage("/game remove <playername>");
                        return;
                    }

                    if (Games.Exists(game => game.isPlayerHoster(args.Player.Name)))
                    {
                        SpleefGame gameToRemove = Games.Find(game => game.isPlayerHoster(args.Player.Name));
                        string playerNameToRemove = string.Join(" ", args.Parameters.Skip(1));

                        if (!SpleefGame.isPlayerOnline(playerNameToRemove, out TSPlayer playerToRemove))
                        {
                            if (gameToRemove.Players.Exists(p => p.Name == playerNameToRemove))
                            {
                                Player player = gameToRemove.Players.Find(p => p.Name == playerNameToRemove);
                                gameToRemove.RemovePlayer(playerNameToRemove, player.AccountName);
                                return;
                            }
                            args.Player.SendErrorMessage("this player isn't online");
                            return;
                        }

                        if (gameToRemove.Players.Exists(p => p.Name == playerToRemove.Name && p.isIngame))
                        {
                            gameToRemove.RemovePlayer(playerToRemove.Name, playerToRemove.Account.Name);
                            return;
                        }

                        if (gameToRemove.Players.Exists(p => p.Name == playerToRemove.Name && !p.isIngame))
                        {
                            gameToRemove.RemovePlayerCompletely(playerToRemove.Name, playerToRemove.Account.Name);
                            return;
                        }
                    }
                    else
                        args.Player.SendErrorMessage("You aren't a hoster in any of the maps!");
                    break;
                case "score": // /game score [game id]
                    if (Games.Count == 0)
                    {
                        args.Player.SendErrorMessage("there's no game going on");
                        return;
                    }

                    var thegame = Games[0];
                    thegame.ShowScore(TSPlayer.All);
                    break;
                case "edit": // /game edit score/payout <playername> <amount>
                    if (Games.Count == 0)
                    {
                        args.Player.SendErrorMessage("there's no game going on");
                        return;
                    }

                    if (args.Parameters.Count < 4)
                    {
                        args.Player.SendErrorMessage("not enough parameters!");
                        args.Player.SendInfoMessage("/game edit score/payout <playername> <amount>");
                        return;
                    }

                    if (Games.Exists(game => game.isPlayerHoster(args.Player.Name)))
                    {
                        SpleefGame gameToEdit = Games.Find(game => game.isPlayerHoster(args.Player.Name));
                        string playerNameToEdit = args.Parameters[2];
                        Player playerToEdit = gameToEdit.FindPlayer(playerNameToEdit);

                        if (playerToEdit == null)
                        {
                            args.Player.SendErrorMessage("this player isn't in the game!");
                            return;
                        }
                        if (!int.TryParse(args.Parameters[3], out int amount))
                        {
                            args.Player.SendErrorMessage("invalid amount specified!");
                            return;
                        }
                        switch (args.Parameters[1])
                        {
                            case "score":
                                gameToEdit.EditPlayerScore(playerToEdit.Name, playerToEdit.AccountName, amount);
                                break;
                            case "payout":
                                break;
                            default:
                                args.Player.SendErrorMessage("invalid edit type specified! Use score or payout.");
                                break;
                        }
                    }
                    else
                        args.Player.SendErrorMessage("You aren't a hoster in any of the maps!");
                    break;
                case "bet": // /game bet open/close/off/pay
                    break;
                default:
                    args.Player.SendErrorMessage("erm no");
                    break;
            }
        }

        public static void JoinGame(CommandArgs args)
        {
            if (Games.Count == 0)
            {
                args.Player.SendErrorMessage("There is no game to join!");
                return;
            }

            if (Games.Exists(g => g.Players.Exists(p => p.Name == args.Player.Name && p.isIngame)))
            {
                args.Player.SendErrorMessage("You are already in a game!");
                return;
            }

            if (Games.Count > 1 && args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("There are multiple games available. Please specify which game you want to join.");
                return;
            }
            string GameID = args.Parameters.Count > 0 && Games.Count == 1 ? args.Parameters[0] : "0";

            if (!int.TryParse(GameID, out int gameIndex) || gameIndex < 0 || gameIndex >= Games.Count)
            {
                args.Player.SendErrorMessage("Invalid game ID specified.");
                return;
            }
            SpleefGame game = Games[gameIndex];

            if (!game.isJoinable)
            {
                args.Player.SendErrorMessage("You can't join this game!");
                return;
            }

            if (game.Players.Exists(p => p.Name == args.Player.Name && p.isIngame))
            {
                args.Player.SendErrorMessage("You are already in the game!");
                return;
            }

            game.AddPlayer(args.Player.Name, args.Player.Account.Name);
        }

        public static void LeaveGame(CommandArgs args)
        {
            if (Games.Count == 0)
            {
                args.Player.SendErrorMessage("There is no game to leave!");
                return;
            }
            SpleefGame game = Games.Find(g => g.Players.Exists(p => p.Name == args.Player.Name && p.isIngame));
            if (game == null)
            {
                args.Player.SendErrorMessage("You are not in any game!");
                return;
            }
            game.RemovePlayer(args.Player.Name, args.Player.Account.Name);
        }
    }
}

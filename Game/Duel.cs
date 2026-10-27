using TShockAPI;
using Microsoft.Xna.Framework;

namespace SpleefResurgence.Game
{
    public class Duel
    {
        public SpleefGame Game { get; set; }

        public Player Player1 { get; }
        public Player Player2 { get; }

        public int Player1Wins { get; private set; }
        public int Player2Wins { get; private set; }

        public int WinsRequired { get; }

        public TaskCompletionSource<bool> player1Ready;
        public TaskCompletionSource<bool> player2Ready;

        public int currentRound;

        public Duel(Player player1, Player player2, int winsRequired)
        {
            Player1 = player1;
            Player2 = player2;
            Player1Wins = 0;
            Player2Wins = 0;
            WinsRequired = winsRequired;
            Initialize();
        }


        public void Initialize()
        {
            List<Arena> validArenas = new();
            foreach (var arenaName in GameConfig.ArenaJson.ListArenaNames())
            {
                var arena = GameConfig.ArenaJson.LoadArena(arenaName);
                if (arena.is1v1)
                    validArenas.Add(arena);
            }

            var randomArena = validArenas[Spleef.rnd.Next(validArenas.Count)];

            Game = new(randomArena, "duel", false, false);
            GameCommands.Games.Add(Game);
            Game.AddPlayer(Player1);
            Game.AddPlayer(Player2);
            currentRound = 0;
            Ready();
        }

        public async void Ready()
        {
            TShock.Utils.Broadcast("ready up gng", Color.IndianRed);

            var tcs1 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tcs2 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            GameDuelCommands.confirmationsReady[Player1.AccountName] = tcs1;
            GameDuelCommands.confirmationsReady[Player2.AccountName] = tcs2;

            await Task.WhenAll(tcs1.Task, tcs2.Task);

            GameDuelCommands.confirmationsReady.Remove(Player1.AccountName);
            GameDuelCommands.confirmationsReady.Remove(Player2.AccountName);

            TShock.Utils.Broadcast("both ready", Color.IndianRed);
            Start();
        }

        public async void Start()
        {
            TShock.Utils.Broadcast("round start in...", Color.White);
            await Task.Delay(1000);
            TShock.Utils.Broadcast("3", Color.White);
            await Task.Delay(1000);
            TShock.Utils.Broadcast("2", Color.White);
            await Task.Delay(1000);
            TShock.Utils.Broadcast("1", Color.White);
            await Task.Delay(1000);

            string randomMap = Game.Arena.MapNames[Spleef.rnd.Next(Game.Arena.MapNames.Count)];

            Game.StartRound([new GimmickNone()], randomMap);

            SpleefGame.RoundResult result = await Game.WaitForRoundEnd;

            if (result.isForced)
            {
                TShock.Utils.Broadcast("game was ended by a hoster or some shi", Color.White);
            }
            else
            {
                if (result.isSuicide)
                    TShock.Utils.Broadcast("suicide dumasses", Color.White);
                else
                {
                    if (result.playerWonName == Player1.AccountName)
                        Player1Wins++;       
                    else
                        Player2Wins++;
                    TShock.Utils.Broadcast($"{Player1.Name} won {Player1Wins} times out of {WinsRequired}", Color.White);
                    TShock.Utils.Broadcast($"{Player2.Name} won {Player2Wins} times out of {WinsRequired}", Color.White);
                    if (Player1Wins == WinsRequired)
                    {
                        End(Player1);
                        return;
                    }
                    if (Player2Wins == WinsRequired)
                    {
                        End(Player2);
                        return;
                    }
                    Ready();
                }
            }
        }

        public void End(Player winner)
        {
            Game.EndGame();
            TShock.Utils.Broadcast($"congrats yo {winner.Name} won a billion dolar", Color.LawnGreen);
            GameCommands.Games.Remove(Game);
        }
    }
}

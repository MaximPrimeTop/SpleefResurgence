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
            Game.AddPlayer(Player1);
            Game.AddPlayer(Player2);
        }

        public async void Ready()
        {
            var tcs1 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tcs2 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            GameDuelCommands.confirmationsReady[Player1.AccountName] = tcs1;
            GameDuelCommands.confirmationsReady[Player2.AccountName] = tcs2;

            await Task.WhenAll(tcs1.Task, tcs2.Task);

            TShock.Utils.Broadcast("both ready", Color.IndianRed);
            Start();
        }

        public void Start()
        {

        }

        public void End()
        {

        }
    }
}

using Microsoft.Xna.Framework;
using TShockAPI;

namespace SpleefResurgence.Game
{
    public class GameDuelCommands
    {
        public static List<Duel> Duels = new();

        public static Dictionary<int, TaskCompletionSource<bool>> confirmations;
        public static async void ChallengeDuel(CommandArgs args)
        {
            // /challenge <username>
            // wait until that person accepts or denies
            // if denies cancel the thing entirely
            // if accepts create a new 1v1 game with a valid 1v1 arena
            // put them in ready position and then proceed
            var player1 = args.Player;
            if (args.Parameters.Count < 1)
            {
                player1.SendErrorMessage("You have to input the username! Usage: /challenge <username>");
                return;
            }
            string player2Name = string.Join(" ", args.Parameters);
            if (!SpleefGame.isPlayerOnline(player2Name, out var player2))
            {
                player1.SendErrorMessage($"[c/ffffff:{player2Name}] is not online!");
                // later could implement the discord bot pinging or dming that person or smth instead
                return;
            }

            bool accepted = await ChallengeConfirm(player1, player2);
        }

        public static async Task<bool> ChallengeConfirm(TSPlayer player1, TSPlayer player2)
        {         
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            confirmations[player2.Index] = tcs;

            string player1Name;
            if (player1.Account.Name == player1.Name)
                player1Name = player1.Name;
            else
                player1Name = $"{player1.Name}({player2.Account.Name})";

            player2.SendMessage($"You have been challenged by {player1Name} to a duel! Type [c/00ff00:/accept] to accept or [c/ff0000:/decline] to decline.", Color.White);

            var timeout = Task.Delay(TimeSpan.FromSeconds(60));

            var completed = await Task.WhenAny(timeout, tcs.Task);

            if (completed != tcs.Task)
                return false;

            return await tcs.Task;
        }

        public static void Accept(CommandArgs args)
        {
            if (confirmations.TryGetValue(args.Player.Index, out var tcs))
                tcs.TrySetResult(true);
        }

        public static void Decline(CommandArgs args)
        {
            if (confirmations.TryGetValue(args.Player.Index, out var tcs))
                tcs.TrySetResult(false);
        }
    }
}

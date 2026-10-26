using Microsoft.Xna.Framework;
using TShockAPI;

namespace SpleefResurgence.Game
{
    public class GameDuelCommands
    {
        public static List<Duel> Duels = new();

        public static Dictionary<int, TaskCompletionSource<bool>> confirmations = new();
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

            player2Name = Spleef.GetCoolUsername(player2, true);
            player1.SendErrorMessage($"Challenged {player2Name}!");

            try
            {
                bool accepted = await ChallengeConfirm(player1, player2);

                if (accepted)
                    player1.SendMessage("woah enter duel time !! (never cus it's not implemented yet)", Color.Aqua);
                else
                    player1.SendMessage("you got rejected baka", Color.IndianRed);
            }
            catch (TimeoutException)
            {
                TShock.Utils.Broadcast("took too long, ya basically got rejected....\nbaka", Color.IndianRed);
            }
            catch (ArgumentException)
            {
                player1.SendErrorMessage("they already have a pending challenge zzz");
            }
            catch(Exception ex)
            {
                TShock.Utils.Broadcast($"An unexpected error occurred: {ex.Message}", Color.IndianRed);
                TShock.Log.ConsoleError($"{ex.StackTrace}");
            }
        }

        public static async Task<bool> ChallengeConfirm(TSPlayer player1, TSPlayer player2)
        {
            if (confirmations.ContainsKey(player2.Index))
            {
                throw new ArgumentException("already here");
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            confirmations[player2.Index] = tcs;

            string player1Name = Spleef.GetCoolUsername(player1, true);

            player2.SendMessage($"You have been challenged by {player1Name} to a duel! Type [c/00ff00:/accept] to accept or [c/ff0000:/decline] to decline.", Color.White);

            var timeout = Task.Delay(TimeSpan.FromSeconds(60));

            var completed = await Task.WhenAny(timeout, tcs.Task);

            if (completed != tcs.Task)
                throw new TimeoutException("took too long yo");

            bool accepted = await tcs.Task;

            confirmations.Remove(player2.Index);

            return accepted;
        }

        public static void Accept(CommandArgs args)
        {
            if (confirmations.TryGetValue(args.Player.Index, out var tcs))
            {
                tcs.TrySetResult(true);
                args.Player.SendSuccessMessage("thy hath accepted thy challenge");
            }
            else
            {
                args.Player.SendErrorMessage("what are you accepting stupi");
            }
        }

        public static void Decline(CommandArgs args)
        {
            if (confirmations.TryGetValue(args.Player.Index, out var tcs))
            {
                tcs.TrySetResult(false);
                args.Player.SendSuccessMessage("challenge declined, ya got scared or what?");
            }
            else
            {
                args.Player.SendErrorMessage("what are you declining stupi");
            }
        }
    }
}

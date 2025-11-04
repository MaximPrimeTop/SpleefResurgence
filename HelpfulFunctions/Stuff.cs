using Microsoft.Xna.Framework;
using System.Globalization;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SpleefResurgence.Utils
{
    public class Utils
    {
        public static Color HexToColor(string hex)
        {
            if (hex.Length != 6)
                throw new ArgumentException("Hex color must be 6 characters long.");
            int r = int.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            int g = int.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            int b = int.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            return new Color(r, g, b);
        }

        public static void ResetTimer(ref Timer timer, ElapsedEventHandler handler, double interval, bool autoReset = true)
        {
            timer.Stop();
            timer.Dispose();
            timer = new Timer(interval)
            {
                AutoReset = autoReset,
                Enabled = true
            };
            timer.Elapsed += handler;
        }
    }
}

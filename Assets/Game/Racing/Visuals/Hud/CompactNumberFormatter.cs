using System;
using System.Globalization;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>
    /// Presentation-only compact number formatting (950, 1.2K, 15.4K, 2.3M, 1.1B).
    /// Does not change underlying numeric values.
    /// </summary>
    public static class CompactNumberFormatter
    {
        public static string Format(long value)
        {
            bool negative = value < 0;
            long abs = negative ? -value : value;
            string body;

            if (abs < 1000L)
            {
                body = abs.ToString(CultureInfo.InvariantCulture);
            }
            else if (abs < 1_000_000L)
            {
                body = FormatScaled(abs, 1_000L, "K");
            }
            else if (abs < 1_000_000_000L)
            {
                body = FormatScaled(abs, 1_000_000L, "M");
            }
            else
            {
                body = FormatScaled(abs, 1_000_000_000L, "B");
            }

            return negative ? "-" + body : body;
        }

        private static string FormatScaled(long abs, long divisor, string suffix)
        {
            double scaled = abs / (double)divisor;
            // One decimal when useful; drop trailing .0
            string number = scaled >= 100
                ? Math.Floor(scaled).ToString("0", CultureInfo.InvariantCulture)
                : scaled.ToString("0.#", CultureInfo.InvariantCulture);
            return number + suffix;
        }
    }
}

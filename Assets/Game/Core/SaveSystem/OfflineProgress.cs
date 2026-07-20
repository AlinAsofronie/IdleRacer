using System;

namespace IdleRacer.Game.Core.SaveSystem
{
    /// <summary>
    /// Foundation for future offline rewards (v0.1D grants nothing yet). Computes how long the
    /// player was away from <c>LastSavedUtc</c>, clamped to be non-negative and optionally capped.
    /// </summary>
    public static class OfflineProgress
    {
        /// <summary>
        /// Returns the elapsed time between <paramref name="lastSavedUtcTicks"/> and
        /// <paramref name="nowUtcTicks"/>. Negative durations (e.g. from device clock changes) clamp
        /// to zero. If <paramref name="maxDuration"/> is given, the result is capped to it.
        /// </summary>
        public static TimeSpan CalculateOfflineDuration(long lastSavedUtcTicks, long nowUtcTicks, TimeSpan? maxDuration = null)
        {
            long diffTicks = nowUtcTicks - lastSavedUtcTicks;
            if (diffTicks < 0)
            {
                diffTicks = 0;
            }

            var duration = TimeSpan.FromTicks(diffTicks);
            if (maxDuration.HasValue && duration > maxDuration.Value)
            {
                duration = maxDuration.Value;
            }

            return duration;
        }
    }
}

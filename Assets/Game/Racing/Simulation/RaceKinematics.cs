using System;

namespace IdleRacer.Racing.Simulation
{
    /// <summary>
    /// Pure-C# kinematics for the v0.1 race model, shared between the simulator's physical
    /// model and the presentation layer so the visual layer never duplicates the formula.
    /// <para>
    /// Model: a car starts from rest, accelerates at a constant
    /// <see cref="CarRaceStats.Acceleration"/> until it reaches
    /// <see cref="CarRaceStats.TopSpeed"/>, then travels at constant TopSpeed.
    /// </para>
    /// Units: distance metres (m), time seconds (s), speed m/s, acceleration m/s^2.
    /// No dependency on UnityEngine.
    /// </summary>
    public static class RaceKinematics
    {
        /// <summary>
        /// Returns the distance travelled (metres) after <paramref name="elapsedSeconds"/>
        /// seconds for the given <paramref name="stats"/>, starting from rest.
        /// </summary>
        /// <param name="stats">Validated car stats (Acceleration and TopSpeed &gt; 0).</param>
        /// <param name="elapsedSeconds">Elapsed time in seconds; values &lt;= 0 return 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="stats"/> are invalid or <paramref name="elapsedSeconds"/> is NaN.
        /// </exception>
        public static double DistanceAtTime(CarRaceStats stats, double elapsedSeconds)
        {
            ValidateStats(stats);

            if (double.IsNaN(elapsedSeconds))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(elapsedSeconds), elapsedSeconds, "elapsedSeconds must be a number (seconds).");
            }

            if (elapsedSeconds <= 0.0)
            {
                return 0.0;
            }

            double a = stats.Acceleration;    // m/s^2
            double vMax = stats.TopSpeed;     // m/s
            double timeToTop = vMax / a;      // s, time to accelerate from rest to TopSpeed

            if (elapsedSeconds <= timeToTop)
            {
                // Constant-acceleration phase: s = 0.5 * a * t^2
                return 0.5 * a * elapsedSeconds * elapsedSeconds;
            }

            // Distance covered reaching TopSpeed (0.5 * a * timeToTop^2 == 0.5 * vMax^2 / a),
            // then constant-speed cruise for the remaining time.
            double distanceToTop = 0.5 * vMax * timeToTop;
            double cruiseTime = elapsedSeconds - timeToTop;
            return distanceToTop + vMax * cruiseTime;
        }

        private static void ValidateStats(CarRaceStats stats)
        {
            if (double.IsNaN(stats.Acceleration) || double.IsInfinity(stats.Acceleration) || stats.Acceleration <= 0.0 ||
                double.IsNaN(stats.TopSpeed) || double.IsInfinity(stats.TopSpeed) || stats.TopSpeed <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stats), "Car stats must have finite Acceleration > 0 (m/s^2) and TopSpeed > 0 (m/s).");
            }
        }
    }
}

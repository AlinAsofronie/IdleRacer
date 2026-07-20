using System;

namespace IdleRacer.Racing.Simulation
{
    /// <summary>
    /// Immutable per-car performance stats for Race Simulation v0.1.
    /// <para>
    /// Units:
    /// <list type="bullet">
    /// <item><description><see cref="Acceleration"/>: metres per second squared (m/s^2).</description></item>
    /// <item><description><see cref="TopSpeed"/>: metres per second (m/s).</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public readonly struct CarRaceStats
    {
        /// <summary>Constant forward acceleration, in metres per second squared (m/s^2). Always &gt; 0.</summary>
        public double Acceleration { get; }

        /// <summary>Maximum speed the car may reach, in metres per second (m/s). Always &gt; 0.</summary>
        public double TopSpeed { get; }

        /// <summary>
        /// Creates validated car stats.
        /// </summary>
        /// <param name="acceleration">Acceleration in m/s^2; must be finite and greater than zero.</param>
        /// <param name="topSpeed">Top speed in m/s; must be finite and greater than zero.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a value is not finite or not greater than zero.</exception>
        public CarRaceStats(double acceleration, double topSpeed)
        {
            if (double.IsNaN(acceleration) || double.IsInfinity(acceleration) || acceleration <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(acceleration), acceleration,
                    "Acceleration must be a finite value greater than zero (m/s^2).");
            }

            if (double.IsNaN(topSpeed) || double.IsInfinity(topSpeed) || topSpeed <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(topSpeed), topSpeed,
                    "TopSpeed must be a finite value greater than zero (m/s).");
            }

            Acceleration = acceleration;
            TopSpeed = topSpeed;
        }
    }
}

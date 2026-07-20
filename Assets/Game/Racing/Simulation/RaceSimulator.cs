using System;

namespace IdleRacer.Racing.Simulation
{
    /// <summary>
    /// Deterministic, pure-C# race simulator for two cars on a straight track (v0.1).
    /// <para>
    /// This type is authoritative: it computes race outcomes with no dependency on Unity,
    /// GameObjects, physics, or visuals. Future race visuals will merely display its result.
    /// </para>
    /// <para>
    /// Model (v0.1): each car starts from rest at distance 0, accelerates at a constant
    /// <see cref="CarRaceStats.Acceleration"/> (m/s^2), is capped at
    /// <see cref="CarRaceStats.TopSpeed"/> (m/s), and finishes when it reaches the track
    /// distance (m). There is no randomness; identical inputs always produce identical results.
    /// </para>
    /// Units: metres (m), metres/second (m/s), metres/second^2 (m/s^2), seconds (s).
    /// </summary>
    public sealed class RaceSimulator
    {
        /// <summary>
        /// Finish times within this many seconds of each other are reported as a
        /// <see cref="RaceWinner.Draw"/>. Identical cars finish at bit-identical times
        /// (margin 0), so this only needs to absorb negligible floating-point noise.
        /// </summary>
        public const double DrawToleranceSeconds = 1e-6;

        // Safety net against accidental infinite loops from pathological input. With validated
        // inputs (all values finite and > 0) a car always finishes, so this cap is never hit
        // in normal use.
        private const long MaxIterations = 100_000_000L;

        /// <summary>
        /// Simulates the race described by <paramref name="request"/> and returns the outcome.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <see cref="RaceSimulationRequest.TrackDistance"/> or
        /// <see cref="RaceSimulationRequest.FixedTimeStep"/> is not finite and &gt; 0, or when
        /// either car's stats are invalid (e.g. a default <see cref="CarRaceStats"/>).
        /// </exception>
        public RaceSimulationResult Simulate(RaceSimulationRequest request)
        {
            if (double.IsNaN(request.TrackDistance) || double.IsInfinity(request.TrackDistance) || request.TrackDistance <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(request), request.TrackDistance,
                    "TrackDistance must be a finite value greater than zero (m).");
            }

            if (double.IsNaN(request.FixedTimeStep) || double.IsInfinity(request.FixedTimeStep) || request.FixedTimeStep <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(request), request.FixedTimeStep,
                    "FixedTimeStep must be a finite value greater than zero (s).");
            }

            // CarRaceStats validates on construction, but a default(CarRaceStats) (all zeros)
            // can bypass that constructor, so guard the values again here.
            ValidateStats(request.PlayerStats, nameof(request.PlayerStats));
            ValidateStats(request.OpponentStats, nameof(request.OpponentStats));

            CarRunResult player = SimulateCar(request.PlayerStats, request.TrackDistance, request.FixedTimeStep);
            CarRunResult opponent = SimulateCar(request.OpponentStats, request.TrackDistance, request.FixedTimeStep);

            double margin = Math.Abs(player.FinishTime - opponent.FinishTime);

            RaceWinner winner;
            if (margin <= DrawToleranceSeconds)
            {
                winner = RaceWinner.Draw;
            }
            else if (player.FinishTime < opponent.FinishTime)
            {
                winner = RaceWinner.Player;
            }
            else
            {
                winner = RaceWinner.Opponent;
            }

            return new RaceSimulationResult(winner, player.FinishTime, opponent.FinishTime, margin);
        }

        private static void ValidateStats(CarRaceStats stats, string label)
        {
            if (double.IsNaN(stats.Acceleration) || double.IsInfinity(stats.Acceleration) || stats.Acceleration <= 0.0 ||
                double.IsNaN(stats.TopSpeed) || double.IsInfinity(stats.TopSpeed) || stats.TopSpeed <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    label, "Car stats must have finite Acceleration > 0 (m/s^2) and TopSpeed > 0 (m/s).");
            }
        }

        /// <summary>
        /// Simulates a single car over the track and returns its finish time and the maximum
        /// speed it reached. Exposed as <c>internal</c> so tests can assert internal invariants
        /// (such as the TopSpeed clamp) without making them part of the public API.
        /// <para>
        /// The loop advances in fixed <paramref name="dt"/> steps, but within each step the
        /// motion is integrated in closed form (constant-acceleration phase, then optional
        /// constant-<see cref="CarRaceStats.TopSpeed"/> cruise phase). The exact finish time is
        /// obtained by solving for the crossing instant inside the step, which makes the result
        /// effectively independent of <paramref name="dt"/> and avoids rounding the finish time
        /// up to the end of a step.
        /// </para>
        /// </summary>
        internal CarRunResult SimulateCar(CarRaceStats stats, double trackDistance, double dt)
        {
            double a = stats.Acceleration;      // m/s^2
            double topSpeed = stats.TopSpeed;   // m/s

            double distance = 0.0; // m
            double speed = 0.0;    // m/s
            double time = 0.0;     // s
            double maxSpeed = 0.0; // m/s

            long iterations = 0;
            while (distance < trackDistance)
            {
                if (++iterations > MaxIterations)
                {
                    throw new InvalidOperationException(
                        "Race simulation exceeded the maximum iteration guard; inputs may be invalid.");
                }

                double stepStartDistance = distance;
                double stepStartTime = time;

                // --- Phase 1: constant-acceleration, until TopSpeed is reached or the step ends ---
                // Time (s) to reach TopSpeed from the current speed at constant acceleration.
                double timeToTopSpeed = (topSpeed - speed) / a;
                double accelTime = Math.Min(dt, timeToTopSpeed);
                if (accelTime < 0.0)
                {
                    accelTime = 0.0; // already at TopSpeed (guarded by the clamp below)
                }

                // Distance under constant acceleration: s = v*t + 0.5*a*t^2
                double accelDistance = speed * accelTime + 0.5 * a * accelTime * accelTime;

                if (stepStartDistance + accelDistance >= trackDistance)
                {
                    // Finish is crossed during the acceleration phase. Solve for tau >= 0 in:
                    //   0.5*a*tau^2 + v*tau - remaining = 0
                    // via the quadratic formula (positive root).
                    double remaining = trackDistance - stepStartDistance;
                    double tau = (-speed + Math.Sqrt(speed * speed + 2.0 * a * remaining)) / a;
                    double speedAtFinish = Math.Min(topSpeed, speed + a * tau);
                    maxSpeed = Math.Max(maxSpeed, speedAtFinish);
                    return new CarRunResult(stepStartTime + tau, maxSpeed);
                }

                // Advance through the whole acceleration sub-phase.
                distance = stepStartDistance + accelDistance;
                speed += a * accelTime;
                if (speed > topSpeed)
                {
                    speed = topSpeed; // clamp: a car may never exceed its TopSpeed
                }
                time = stepStartTime + accelTime;
                maxSpeed = Math.Max(maxSpeed, speed);

                // --- Phase 2: cruise at TopSpeed for the remainder of the step ---
                double cruiseTime = dt - accelTime;
                if (cruiseTime > 0.0)
                {
                    double remaining = trackDistance - distance;
                    double cruiseDistance = topSpeed * cruiseTime;
                    if (cruiseDistance >= remaining)
                    {
                        // Finish is crossed while cruising: constant-speed interpolation.
                        double tau = remaining / topSpeed;
                        maxSpeed = Math.Max(maxSpeed, topSpeed);
                        return new CarRunResult(time + tau, maxSpeed);
                    }

                    distance += cruiseDistance;
                    time += cruiseTime;
                    // speed stays at topSpeed
                }
            }

            // Only reachable for a degenerate (already-finished) start, which validation excludes.
            return new CarRunResult(time, maxSpeed);
        }
    }

    /// <summary>
    /// Internal per-car simulation detail: the finish time (s) and the maximum speed (m/s)
    /// the car reached. Used by <see cref="RaceSimulator"/> and by tests via InternalsVisibleTo.
    /// </summary>
    internal readonly struct CarRunResult
    {
        /// <summary>Finish time in seconds (s).</summary>
        public double FinishTime { get; }

        /// <summary>Maximum speed reached during the run, in metres per second (m/s).</summary>
        public double MaxSpeedReached { get; }

        public CarRunResult(double finishTime, double maxSpeedReached)
        {
            FinishTime = finishTime;
            MaxSpeedReached = maxSpeedReached;
        }
    }
}

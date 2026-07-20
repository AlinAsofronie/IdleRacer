using System;
using NUnit.Framework;
using IdleRacer.Racing.Simulation;

namespace IdleRacer.Racing.Tests.EditMode
{
    /// <summary>
    /// Edit Mode tests for the deterministic headless race simulator (v0.1).
    /// All units follow the simulator: metres (m), m/s, m/s^2, seconds (s).
    /// </summary>
    public sealed class RaceSimulatorTests
    {
        // Documented tolerances (seconds) used by the assertions below.
        private const double DrawEqualityToleranceSeconds = 1e-9;   // identical cars => bit-identical times
        private const double TimestepConsistencyToleranceSeconds = 1e-3; // finish time across dt = 0.02 vs 0.01
        private const double SpeedEpsilon = 1e-9;                   // TopSpeed clamp slack (m/s)

        private const double DefaultTimeStep = 0.02; // s

        private static RaceSimulator NewSimulator() => new RaceSimulator();

        // 1. Faster acceleration wins on a short track when top speeds are equal.
        [Test]
        public void FasterAcceleration_WinsOnShortTrack_WhenTopSpeedsAreEqual()
        {
            // High, equal TopSpeed so neither car reaches it on a short track:
            // reaching 100 m/s at 5 m/s^2 needs ~500 m, far beyond the 20 m track.
            var player = new CarRaceStats(acceleration: 10.0, topSpeed: 100.0);
            var opponent = new CarRaceStats(acceleration: 5.0, topSpeed: 100.0);
            var request = new RaceSimulationRequest(player, opponent, trackDistance: 20.0, fixedTimeStep: DefaultTimeStep);

            RaceSimulationResult result = NewSimulator().Simulate(request);

            Assert.AreEqual(RaceWinner.Player, result.Winner);
            Assert.Less(result.PlayerFinishTime, result.OpponentFinishTime);
        }

        // 2. Higher top speed wins on a long track, even with worse acceleration.
        [Test]
        public void HigherTopSpeed_WinsOnLongTrack_EvenWithWorseAcceleration()
        {
            // Player: slower to accelerate but much higher top speed.
            // Opponent: quick off the line but capped at a low top speed.
            var player = new CarRaceStats(acceleration: 5.0, topSpeed: 100.0);
            var opponent = new CarRaceStats(acceleration: 20.0, topSpeed: 40.0);
            var request = new RaceSimulationRequest(player, opponent, trackDistance: 5000.0, fixedTimeStep: DefaultTimeStep);

            RaceSimulationResult result = NewSimulator().Simulate(request);

            Assert.AreEqual(RaceWinner.Player, result.Winner);
            Assert.Less(result.PlayerFinishTime, result.OpponentFinishTime);
        }

        // 3. Identical cars produce a Draw with effectively equal finish times.
        [Test]
        public void IdenticalCars_ProduceDraw_WithEqualFinishTimes()
        {
            var stats = new CarRaceStats(acceleration: 8.0, topSpeed: 60.0);
            var request = new RaceSimulationRequest(stats, stats, trackDistance: 1000.0, fixedTimeStep: DefaultTimeStep);

            RaceSimulationResult result = NewSimulator().Simulate(request);

            Assert.AreEqual(RaceWinner.Draw, result.Winner);
            Assert.AreEqual(result.PlayerFinishTime, result.OpponentFinishTime, DrawEqualityToleranceSeconds);
            Assert.LessOrEqual(result.VictoryMarginSeconds, RaceSimulator.DrawToleranceSeconds);
        }

        // 4. A car never exceeds its configured TopSpeed internally.
        // Verified through the internal per-car helper (InternalsVisibleTo), so the public API stays minimal.
        [Test]
        public void Car_NeverExceedsTopSpeed_Internally()
        {
            var stats = new CarRaceStats(acceleration: 25.0, topSpeed: 45.0);

            // Long track guarantees the car spends a long time cruising at the cap.
            CarRunResult run = NewSimulator().SimulateCar(stats, trackDistance: 10000.0, dt: DefaultTimeStep);

            Assert.LessOrEqual(run.MaxSpeedReached, stats.TopSpeed + SpeedEpsilon);
        }

        // 5. Identical inputs always produce identical results (determinism).
        [Test]
        public void IdenticalInputs_ProduceIdenticalResults()
        {
            var player = new CarRaceStats(acceleration: 7.5, topSpeed: 73.0);
            var opponent = new CarRaceStats(acceleration: 9.0, topSpeed: 55.0);
            var request = new RaceSimulationRequest(player, opponent, trackDistance: 1234.0, fixedTimeStep: DefaultTimeStep);

            RaceSimulationResult a = NewSimulator().Simulate(request);
            RaceSimulationResult b = NewSimulator().Simulate(request);

            Assert.AreEqual(a.Winner, b.Winner);
            // Exact equality: the simulation is fully deterministic with no randomness.
            Assert.AreEqual(a.PlayerFinishTime, b.PlayerFinishTime);
            Assert.AreEqual(a.OpponentFinishTime, b.OpponentFinishTime);
            Assert.AreEqual(a.VictoryMarginSeconds, b.VictoryMarginSeconds);
        }

        // 6. Invalid TrackDistance is rejected.
        [Test]
        public void InvalidTrackDistance_IsRejected()
        {
            var stats = new CarRaceStats(acceleration: 5.0, topSpeed: 50.0);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                NewSimulator().Simulate(new RaceSimulationRequest(stats, stats, trackDistance: 0.0, fixedTimeStep: DefaultTimeStep)));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                NewSimulator().Simulate(new RaceSimulationRequest(stats, stats, trackDistance: -100.0, fixedTimeStep: DefaultTimeStep)));
        }

        // 7. Invalid FixedTimeStep is rejected.
        [Test]
        public void InvalidFixedTimeStep_IsRejected()
        {
            var stats = new CarRaceStats(acceleration: 5.0, topSpeed: 50.0);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                NewSimulator().Simulate(new RaceSimulationRequest(stats, stats, trackDistance: 400.0, fixedTimeStep: 0.0)));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                NewSimulator().Simulate(new RaceSimulationRequest(stats, stats, trackDistance: 400.0, fixedTimeStep: -0.01)));
        }

        // 8. Invalid car stats are rejected (both at construction and by the simulator for default structs).
        [Test]
        public void InvalidCarStats_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CarRaceStats(acceleration: 0.0, topSpeed: 50.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CarRaceStats(acceleration: -1.0, topSpeed: 50.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CarRaceStats(acceleration: 5.0, topSpeed: 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CarRaceStats(acceleration: 5.0, topSpeed: -20.0));

            // A default(CarRaceStats) (all zeros) bypasses the constructor; the simulator must still reject it.
            var valid = new CarRaceStats(acceleration: 5.0, topSpeed: 50.0);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                NewSimulator().Simulate(new RaceSimulationRequest(default, valid, trackDistance: 400.0, fixedTimeStep: DefaultTimeStep)));
        }

        // 9. Finish-time interpolation is consistent across two reasonable timesteps (0.02 s vs 0.01 s).
        [Test]
        public void FinishTimeInterpolation_IsConsistentAcrossTimesteps()
        {
            var player = new CarRaceStats(acceleration: 6.0, topSpeed: 80.0);
            var opponent = new CarRaceStats(acceleration: 12.0, topSpeed: 50.0);

            RaceSimulationResult coarse = NewSimulator().Simulate(
                new RaceSimulationRequest(player, opponent, trackDistance: 2500.0, fixedTimeStep: 0.02));
            RaceSimulationResult fine = NewSimulator().Simulate(
                new RaceSimulationRequest(player, opponent, trackDistance: 2500.0, fixedTimeStep: 0.01));

            Assert.AreEqual(coarse.PlayerFinishTime, fine.PlayerFinishTime, TimestepConsistencyToleranceSeconds);
            Assert.AreEqual(coarse.OpponentFinishTime, fine.OpponentFinishTime, TimestepConsistencyToleranceSeconds);
            Assert.AreEqual(coarse.Winner, fine.Winner);
        }
    }
}

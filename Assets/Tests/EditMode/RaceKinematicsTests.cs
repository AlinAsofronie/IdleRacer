using System;
using NUnit.Framework;
using IdleRacer.Racing.Simulation;

namespace IdleRacer.Racing.Tests.EditMode
{
    /// <summary>
    /// Edit Mode tests for <see cref="RaceKinematics"/>, the pure-C# distance-at-time helper
    /// shared by the simulator model and the visual prototype. Units: metres, seconds, m/s, m/s^2.
    /// </summary>
    public sealed class RaceKinematicsTests
    {
        private const double Tolerance = 1e-9;

        // 1. Distance at time 0 is 0 (and non-positive times clamp to 0).
        [Test]
        public void DistanceAtTimeZero_IsZero()
        {
            var stats = new CarRaceStats(acceleration: 10.0, topSpeed: 50.0);

            Assert.AreEqual(0.0, RaceKinematics.DistanceAtTime(stats, 0.0), Tolerance);
            Assert.AreEqual(0.0, RaceKinematics.DistanceAtTime(stats, -5.0), Tolerance);
        }

        // 2. Distance strictly increases while accelerating.
        [Test]
        public void Distance_IncreasesWhileAccelerating()
        {
            var stats = new CarRaceStats(acceleration: 10.0, topSpeed: 100.0); // timeToTop = 10 s

            double d1 = RaceKinematics.DistanceAtTime(stats, 1.0);
            double d2 = RaceKinematics.DistanceAtTime(stats, 2.0);
            double d3 = RaceKinematics.DistanceAtTime(stats, 3.0);

            Assert.Greater(d2, d1);
            Assert.Greater(d3, d2);
            // s = 0.5 * a * t^2 => at t=2, 0.5*10*4 = 20 m.
            Assert.AreEqual(20.0, d2, Tolerance);
        }

        // 3. Speed/distance behaviour respects TopSpeed.
        [Test]
        public void Distance_RespectsTopSpeed_AfterReachingIt()
        {
            double a = 10.0, vMax = 40.0;                 // timeToTop = 4 s
            var stats = new CarRaceStats(a, vMax);

            // After TopSpeed is reached, distance grows exactly at vMax (constant-speed cruise).
            double t1 = 6.0, t2 = 7.0;                    // both > timeToTop
            double incremental = RaceKinematics.DistanceAtTime(stats, t2) - RaceKinematics.DistanceAtTime(stats, t1);
            Assert.AreEqual(vMax * (t2 - t1), incremental, Tolerance);

            // The average speed over any interval never exceeds TopSpeed.
            double avgSpeedEarly = RaceKinematics.DistanceAtTime(stats, 2.0) / 2.0; // still accelerating
            Assert.LessOrEqual(avgSpeedEarly, vMax + Tolerance);
        }

        // 4. Distance calculation is deterministic.
        [Test]
        public void Distance_IsDeterministic()
        {
            var stats = new CarRaceStats(acceleration: 7.5, topSpeed: 63.0);

            double first = RaceKinematics.DistanceAtTime(stats, 12.34);
            double second = RaceKinematics.DistanceAtTime(stats, 12.34);

            Assert.AreEqual(first, second); // exact
        }

        // 5. Distance at the simulator's finish time is approximately TrackDistance.
        [Test]
        public void DistanceAtSimulatorFinishTime_ApproximatelyTrackDistance()
        {
            var stats = new CarRaceStats(acceleration: 9.0, topSpeed: 55.0);
            const double trackDistance = 1500.0;

            // Use the authoritative simulator to obtain a finish time (identical cars => player time).
            var request = new RaceSimulationRequest(stats, stats, trackDistance, fixedTimeStep: 0.02);
            RaceSimulationResult result = new RaceSimulator().Simulate(request);

            double distanceAtFinish = RaceKinematics.DistanceAtTime(stats, result.PlayerFinishTime);

            // Analytic model matches the simulator's finish time closely.
            Assert.AreEqual(trackDistance, distanceAtFinish, 1e-6 * trackDistance);
        }

        // Invalid stats are rejected by the helper as well.
        [Test]
        public void InvalidStats_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RaceKinematics.DistanceAtTime(default, 1.0));
        }
    }
}

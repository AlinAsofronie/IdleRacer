using UnityEngine;

namespace IdleRacer.Racing.Visuals
{
    /// <summary>
    /// Presentation-only view of a single race car for the visual prototype.
    /// It maps a normalised race progress value in [0, 1] to a world position along its
    /// visual lane. It deliberately contains no race-outcome logic: it never decides who
    /// wins and never reads the simulation — it only displays a position it is told.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RaceCarView : MonoBehaviour
    {
        private float _startX;
        private float _finishX;
        private float _laneY;
        private float _z;

        /// <summary>Configures the visual lane endpoints (world units) for this car.</summary>
        public void Configure(float startX, float finishX, float laneY, float z)
        {
            _startX = startX;
            _finishX = finishX;
            _laneY = laneY;
            _z = z;
            SetNormalizedProgress(0f);
        }

        /// <summary>
        /// Positions the car along its lane. <paramref name="progress"/> is the fraction of the
        /// track completed (0 = start line, 1 = finish line) and is clamped to [0, 1].
        /// </summary>
        public void SetNormalizedProgress(float progress)
        {
            if (progress < 0f) progress = 0f;
            else if (progress > 1f) progress = 1f;

            float x = Mathf.Lerp(_startX, _finishX, progress);
            transform.position = new Vector3(x, _laneY, _z);
        }
    }
}

using UnityEngine;

namespace IdleRacer.Racing.Visuals
{
    /// <summary>
    /// Presentation-only view of a single race car. Maps normalised progress to world position
    /// and spins placeholder wheels — never decides race outcomes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RaceCarView : MonoBehaviour
    {
        private float _startX;
        private float _finishX;
        private float _laneY;
        private float _z;
        private Transform[] _wheels;

        public void Configure(float startX, float finishX, float laneY, float z)
        {
            _startX = startX;
            _finishX = finishX;
            _laneY = laneY;
            _z = z;
            CacheWheels();
            SetNormalizedProgress(0f);
        }

        public void SetNormalizedProgress(float progress)
        {
            if (progress < 0f) progress = 0f;
            else if (progress > 1f) progress = 1f;

            float x = Mathf.Lerp(_startX, _finishX, progress);
            transform.position = new Vector3(x, _laneY, _z);
        }

        public void SetWheelSpin(float degrees)
        {
            if (_wheels == null) return;
            for (int i = 0; i < _wheels.Length; i++)
            {
                if (_wheels[i] == null) continue;
                _wheels[i].Rotate(degrees, 0f, 0f, Space.Self);
            }
        }

        private void CacheWheels()
        {
            var list = new System.Collections.Generic.List<Transform>(4);
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("Wheel"))
                {
                    list.Add(child);
                }
            }
            _wheels = list.ToArray();
        }
    }
}

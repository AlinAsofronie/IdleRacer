using UnityEngine;

namespace IdleRacer.Racing.Visuals
{
    /// <summary>
    /// Presentation-only race world: track, lanes, markers, cars, and lightweight motion FX.
    /// Does not influence race outcomes.
    /// </summary>
    public sealed class RaceWorldView
    {
        private const float TrackHalfWidth = 5.2f;
        private const float VisualStartX = -4.6f;
        private const float VisualFinishX = 4.6f;
        private const float LaneOffset = 0.72f;
        private const float CarScale = 1.05f;

        private RaceCarView _playerView;
        private RaceCarView _opponentView;
        private Transform _markerRoot;
        private Transform _speedLineRoot;
        private float _trackCenterY;
        private float _scroll;

        public RaceCarView PlayerView => _playerView;
        public RaceCarView OpponentView => _opponentView;
        public float TrackCenterY => _trackCenterY;

        public void Build()
        {
            ConfigureCamera();
            BuildBackdrop();
            BuildTrack();
            BuildCars();
        }

        public void SetMotion(float normalizedProgress, float deltaTime)
        {
            // Scroll markers opposite to travel for a sense of speed (presentation only).
            float speed = 2.5f + normalizedProgress * 8f;
            _scroll += speed * deltaTime;
            if (_markerRoot != null)
            {
                float wrap = ((_scroll % 1.6f) + 1.6f) % 1.6f;
                _markerRoot.localPosition = new Vector3(-wrap, 0f, 0f);
            }
            if (_speedLineRoot != null)
            {
                float pulse = 0.35f + 0.45f * normalizedProgress;
                foreach (Transform child in _speedLineRoot)
                {
                    var renderer = child.GetComponent<Renderer>();
                    if (renderer == null) continue;
                    Color c = renderer.material.HasProperty("_BaseColor")
                        ? renderer.material.GetColor("_BaseColor")
                        : renderer.material.color;
                    c.a = pulse * 0.55f;
                    ApplyColor(child.gameObject, c);
                }
                _speedLineRoot.localPosition = new Vector3((-_scroll * 1.8f) % 2.2f, 0f, 0f);
            }

            _playerView.SetWheelSpin(speed * deltaTime * 220f);
            _opponentView.SetWheelSpin(speed * deltaTime * 200f);
        }

        public void ResetMotion()
        {
            _scroll = 0f;
            if (_markerRoot != null) _markerRoot.localPosition = Vector3.zero;
            if (_speedLineRoot != null) _speedLineRoot.localPosition = Vector3.zero;
            _playerView.SetWheelSpin(0f);
            _opponentView.SetWheelSpin(0f);
        }

        private void ConfigureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.transform.position = new Vector3(0f, 0.15f, -10f);
            cam.transform.rotation = Quaternion.identity;
            // Frame the track into the upper ~40% of a portrait view with less empty band.
            float aspect = Mathf.Max(0.35f, cam.aspect);
            cam.orthographicSize = TrackHalfWidth / aspect * 0.88f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.08f, 1f);
            _trackCenterY = cam.orthographicSize * 0.58f;
        }

        private void BuildBackdrop()
        {
            GameObject sky = CreatePrimitive(PrimitiveType.Quad, "SkyBand");
            sky.transform.position = new Vector3(0f, _trackCenterY + 1.85f, 1.2f);
            sky.transform.localScale = new Vector3(14f, 2.2f, 1f);
            ApplyColor(sky, new Color(0.08f, 0.12f, 0.20f, 1f));

            GameObject ground = CreatePrimitive(PrimitiveType.Quad, "GroundBand");
            ground.transform.position = new Vector3(0f, _trackCenterY - 1.85f, 1.1f);
            ground.transform.localScale = new Vector3(14f, 1.8f, 1f);
            ApplyColor(ground, new Color(0.07f, 0.08f, 0.10f, 1f));
        }

        private void BuildTrack()
        {
            GameObject road = CreatePrimitive(PrimitiveType.Cube, "Road");
            road.transform.position = new Vector3(0f, _trackCenterY, 0.7f);
            road.transform.localScale = new Vector3(10.6f, 3.1f, 0.12f);
            ApplyColor(road, new Color(0.14f, 0.15f, 0.18f, 1f));

            // Lane divider
            GameObject divider = CreatePrimitive(PrimitiveType.Cube, "LaneDivider");
            divider.transform.position = new Vector3(0f, _trackCenterY, 0.45f);
            divider.transform.localScale = new Vector3(10.4f, 0.06f, 0.08f);
            ApplyColor(divider, new Color(0.85f, 0.85f, 0.4f, 1f));

            // Shoulder lines
            CreateLaneLine("ShoulderTop", _trackCenterY + 1.4f, new Color(0.9f, 0.9f, 0.92f, 1f));
            CreateLaneLine("ShoulderBottom", _trackCenterY - 1.4f, new Color(0.9f, 0.9f, 0.92f, 1f));

            GameObject start = CreatePrimitive(PrimitiveType.Cube, "StartLine");
            start.transform.position = new Vector3(VisualStartX, _trackCenterY, 0.35f);
            start.transform.localScale = new Vector3(0.14f, 2.9f, 0.2f);
            ApplyColor(start, new Color(0.55f, 0.55f, 0.60f, 1f));

            GameObject finish = CreatePrimitive(PrimitiveType.Cube, "FinishLine");
            finish.transform.position = new Vector3(VisualFinishX, _trackCenterY, 0.35f);
            finish.transform.localScale = new Vector3(0.18f, 2.9f, 0.25f);
            ApplyColor(finish, Color.white);

            var markerRootGo = new GameObject("LaneMarkers");
            _markerRoot = markerRootGo.transform;
            _markerRoot.position = new Vector3(0f, _trackCenterY, 0.4f);
            for (int i = -8; i <= 10; i++)
            {
                GameObject dash = CreatePrimitive(PrimitiveType.Cube, "Dash_" + i);
                dash.transform.SetParent(_markerRoot, false);
                dash.transform.localPosition = new Vector3(i * 1.6f, LaneOffset * 0.02f, 0f);
                dash.transform.localScale = new Vector3(0.55f, 0.06f, 0.05f);
                ApplyColor(dash, new Color(0.95f, 0.95f, 0.98f, 0.85f));

                GameObject dash2 = CreatePrimitive(PrimitiveType.Cube, "DashB_" + i);
                dash2.transform.SetParent(_markerRoot, false);
                dash2.transform.localPosition = new Vector3(i * 1.6f + 0.8f, 0f, 0f);
                dash2.transform.localScale = new Vector3(0.55f, 0.06f, 0.05f);
                ApplyColor(dash2, new Color(0.75f, 0.75f, 0.78f, 0.55f));
            }

            var speedRootGo = new GameObject("SpeedLines");
            _speedLineRoot = speedRootGo.transform;
            _speedLineRoot.position = new Vector3(0f, _trackCenterY, 0.55f);
            for (int i = 0; i < 10; i++)
            {
                GameObject line = CreatePrimitive(PrimitiveType.Cube, "SpeedLine_" + i);
                line.transform.SetParent(_speedLineRoot, false);
                float y = (i % 2 == 0 ? 1f : -1f) * (0.35f + (i % 5) * 0.18f);
                line.transform.localPosition = new Vector3(-3f + i * 0.7f, y, 0f);
                line.transform.localScale = new Vector3(0.9f + (i % 3) * 0.25f, 0.03f, 0.04f);
                ApplyColor(line, new Color(0.55f, 0.75f, 1f, 0.25f));
            }
        }

        private void CreateLaneLine(string name, float y, Color color)
        {
            GameObject line = CreatePrimitive(PrimitiveType.Cube, name);
            line.transform.position = new Vector3(0f, y, 0.5f);
            line.transform.localScale = new Vector3(10.2f, 0.04f, 0.06f);
            ApplyColor(line, color);
        }

        private void BuildCars()
        {
            float playerY = _trackCenterY + LaneOffset;
            float opponentY = _trackCenterY - LaneOffset;

            GameObject playerGo = CreatePrimitive(PrimitiveType.Cube, "PlayerCar");
            playerGo.transform.localScale = new Vector3(CarScale * 1.35f, CarScale * 0.7f, CarScale);
            ApplyColor(playerGo, new Color(0.25f, 0.55f, 1f, 1f));
            AddCabin(playerGo, new Color(0.55f, 0.78f, 1f, 1f));
            AddWheels(playerGo);
            _playerView = playerGo.AddComponent<RaceCarView>();
            _playerView.Configure(VisualStartX, VisualFinishX, playerY, 0f);

            GameObject opponentGo = CreatePrimitive(PrimitiveType.Cube, "OpponentCar");
            opponentGo.transform.localScale = new Vector3(CarScale * 1.35f, CarScale * 0.7f, CarScale);
            ApplyColor(opponentGo, new Color(1f, 0.38f, 0.32f, 1f));
            AddCabin(opponentGo, new Color(1f, 0.65f, 0.55f, 1f));
            AddWheels(opponentGo);
            _opponentView = opponentGo.AddComponent<RaceCarView>();
            _opponentView.Configure(VisualStartX, VisualFinishX, opponentY, 0f);
        }

        private static void AddCabin(GameObject car, Color color)
        {
            GameObject cabin = CreatePrimitive(PrimitiveType.Cube, "Cabin");
            cabin.transform.SetParent(car.transform, false);
            cabin.transform.localPosition = new Vector3(0.05f, 0.35f, -0.1f);
            cabin.transform.localScale = new Vector3(0.45f, 0.45f, 0.7f);
            ApplyColor(cabin, color);
        }

        private static void AddWheels(GameObject car)
        {
            CreateWheel(car, "WheelFL", new Vector3(-0.38f, -0.45f, -0.35f));
            CreateWheel(car, "WheelFR", new Vector3(0.38f, -0.45f, -0.35f));
            CreateWheel(car, "WheelRL", new Vector3(-0.38f, -0.45f, 0.35f));
            CreateWheel(car, "WheelRR", new Vector3(0.38f, -0.45f, 0.35f));
        }

        private static void CreateWheel(GameObject car, string name, Vector3 localPos)
        {
            GameObject wheel = CreatePrimitive(PrimitiveType.Cylinder, name);
            wheel.transform.SetParent(car.transform, false);
            wheel.transform.localPosition = localPos;
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wheel.transform.localScale = new Vector3(0.28f, 0.12f, 0.28f);
            ApplyColor(wheel, new Color(0.12f, 0.12f, 0.14f, 1f));
        }

        private static GameObject CreatePrimitive(PrimitiveType type, string name)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }
            return go;
        }

        private static void ApplyColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;
            Material material = renderer.material;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }
    }
}

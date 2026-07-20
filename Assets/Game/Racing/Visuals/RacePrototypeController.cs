using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using IdleRacer.Racing.Simulation;

namespace IdleRacer.Racing.Visuals
{
    /// <summary>
    /// Milestone 0.1B visual race prototype driver.
    /// <para>
    /// This presentation-layer MonoBehaviour builds a simple portrait race scene at runtime,
    /// asks the authoritative <see cref="RaceSimulator"/> for the result, then plays the race
    /// back visually. The simulator remains authoritative: the winner and finish times shown
    /// come from <see cref="RaceSimulationResult"/>. Visual car positions are derived from the
    /// shared <see cref="RaceKinematics.DistanceAtTime"/> helper so acceleration and the
    /// top-speed cap are reflected on screen; the visuals never decide the outcome.
    /// </para>
    /// No authoritative race calculations live in this MonoBehaviour.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RacePrototypeController : MonoBehaviour
    {
        // ---------------------------------------------------------------------------------
        // PROTOTYPE-ONLY race configuration. These are NOT game-balance values; they exist
        // purely to produce a visually interesting demo (player leads early on stronger
        // acceleration, the higher-top-speed opponent overtakes and wins near the finish).
        // Units: Acceleration m/s^2, TopSpeed m/s, TrackDistance m, FixedTimeStep s.
        // ---------------------------------------------------------------------------------
        private static readonly CarRaceStats PrototypePlayerStats = new CarRaceStats(acceleration: 20.0, topSpeed: 60.0);
        private static readonly CarRaceStats PrototypeOpponentStats = new CarRaceStats(acceleration: 10.0, topSpeed: 80.0);
        private const double PrototypeTrackDistance = 700.0;
        private const double PrototypeFixedTimeStep = 0.02;

        // Race-flow timing (seconds, real time).
        private const float ReadySeconds = 1.25f;
        private const float GoSeconds = 0.6f;
        private const float ResultSeconds = 2.5f;

        // ---- Visual layout (world units) ----
        private const float TrackHalfWidth = 5.0f;   // camera fits this half-width horizontally
        private const float VisualStartX = -4.3f;
        private const float VisualFinishX = 4.3f;
        private const float LaneOffset = 0.5f;       // vertical separation of the two lanes
        private const float CarScale = 0.7f;
        private const float CarZ = 0.0f;

        private readonly RaceSimulator _simulator = new RaceSimulator();

        private RaceCarView _playerView;
        private RaceCarView _opponentView;
        private Text _statusText;
        private Text _playerTimeText;
        private Text _opponentTimeText;

        private float _trackCenterY;

        private void Start()
        {
            // Keep the auto-race playing even when the Editor/app is not focused. This suits an
            // idle/auto racer and lets the continuous autoplay loop run during development.
            Application.runInBackground = true;

            ConfigureCamera();
            BuildTrack();
            BuildCars();
            BuildUI();
            StartCoroutine(RaceLoop());
        }

        private void ConfigureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            // Fit the fixed visual track width horizontally regardless of the actual aspect,
            // so the track never overflows the screen in portrait or other aspect ratios.
            cam.orthographicSize = TrackHalfWidth / Mathf.Max(0.1f, cam.aspect);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.10f, 1f);

            // Place the track in the upper portion of the view (above the bottom UI panel).
            _trackCenterY = cam.orthographicSize * 0.45f;
        }

        private void BuildTrack()
        {
            // Road strip spanning the two lanes.
            GameObject road = CreatePrimitiveNoCollider(PrimitiveType.Cube, "Road");
            road.transform.position = new Vector3(0f, _trackCenterY, 0.6f);
            road.transform.localScale = new Vector3(9.2f, 1.8f, 0.1f);
            ApplyColor(road, new Color(0.16f, 0.17f, 0.20f, 1f));

            // Start line.
            GameObject startLine = CreatePrimitiveNoCollider(PrimitiveType.Cube, "StartLine");
            startLine.transform.position = new Vector3(VisualStartX, _trackCenterY, 0.3f);
            startLine.transform.localScale = new Vector3(0.10f, 1.8f, 0.2f);
            ApplyColor(startLine, new Color(0.5f, 0.5f, 0.55f, 1f));

            // Finish line (clearly visible, bright).
            GameObject finishLine = CreatePrimitiveNoCollider(PrimitiveType.Cube, "FinishLine");
            finishLine.transform.position = new Vector3(VisualFinishX, _trackCenterY, 0.3f);
            finishLine.transform.localScale = new Vector3(0.14f, 1.8f, 0.25f);
            ApplyColor(finishLine, Color.white);
        }

        private void BuildCars()
        {
            float playerLaneY = _trackCenterY + LaneOffset;
            float opponentLaneY = _trackCenterY - LaneOffset;

            GameObject playerGo = CreatePrimitiveNoCollider(PrimitiveType.Cube, "PlayerCar");
            playerGo.transform.localScale = Vector3.one * CarScale;
            ApplyColor(playerGo, new Color(0.30f, 0.55f, 1.0f, 1f)); // blue
            _playerView = playerGo.AddComponent<RaceCarView>();
            _playerView.Configure(VisualStartX, VisualFinishX, playerLaneY, CarZ);

            GameObject opponentGo = CreatePrimitiveNoCollider(PrimitiveType.Cube, "OpponentCar");
            opponentGo.transform.localScale = Vector3.one * CarScale;
            ApplyColor(opponentGo, new Color(1.0f, 0.45f, 0.35f, 1f)); // red/orange
            _opponentView = opponentGo.AddComponent<RaceCarView>();
            _opponentView.Configure(VisualStartX, VisualFinishX, opponentLaneY, CarZ);
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("PrototypeCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f); // portrait reference
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Bottom placeholder panel: bottom 55% of the screen (opaque, static).
            GameObject panel = CreateUiImage(
                "BottomPanel", canvasGo.transform,
                new Vector2(0f, 0f), new Vector2(1f, 0.55f),
                new Color(0.09f, 0.09f, 0.12f, 1f));
            CreateUiText(
                "BottomPanelLabel", panel.transform,
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                "Progression UI Coming Next", 48, TextAnchor.MiddleCenter,
                new Color(0.75f, 0.75f, 0.80f, 1f));

            // Race status (READY / GO! / winner) near the top, inset for safe areas.
            _statusText = CreateUiText(
                "StatusText", canvasGo.transform,
                new Vector2(0f, 0.80f), new Vector2(1f, 0.93f),
                string.Empty, 90, TextAnchor.MiddleCenter, Color.white);

            // Finish times, sitting just above the bottom panel.
            _playerTimeText = CreateUiText(
                "PlayerTimeText", canvasGo.transform,
                new Vector2(0.06f, 0.63f), new Vector2(0.94f, 0.70f),
                string.Empty, 42, TextAnchor.MiddleLeft, new Color(0.55f, 0.72f, 1f, 1f));
            _opponentTimeText = CreateUiText(
                "OpponentTimeText", canvasGo.transform,
                new Vector2(0.06f, 0.56f), new Vector2(0.94f, 0.63f),
                string.Empty, 42, TextAnchor.MiddleLeft, new Color(1f, 0.6f, 0.5f, 1f));
        }

        /// <summary>
        /// Continuous autoplay loop: run the authoritative simulation, play it back visually,
        /// show the result, then restart the same race. No user input.
        /// </summary>
        private IEnumerator RaceLoop()
        {
            var request = new RaceSimulationRequest(
                PrototypePlayerStats, PrototypeOpponentStats, PrototypeTrackDistance, PrototypeFixedTimeStep);

            while (true)
            {
                // Authoritative result (winner + finish times come from here, never from visuals).
                RaceSimulationResult result = _simulator.Simulate(request);

                _playerView.SetNormalizedProgress(0f);
                _opponentView.SetNormalizedProgress(0f);
                _playerTimeText.text = string.Empty;
                _opponentTimeText.text = string.Empty;

                _statusText.text = "READY";
                yield return new WaitForSeconds(ReadySeconds);

                _statusText.text = "GO!";
                yield return new WaitForSeconds(GoSeconds);
                _statusText.text = string.Empty;

                double maxFinish = Math.Max(result.PlayerFinishTime, result.OpponentFinishTime);
                double elapsed = 0.0;

                // Play back in real time. The faster car reaches the finish (progress clamps at 1)
                // while the slower car keeps moving until it also finishes.
                while (elapsed < maxFinish)
                {
                    elapsed += Time.deltaTime;
                    double e = Math.Min(elapsed, maxFinish);

                    _playerView.SetNormalizedProgress(NormalizedProgress(PrototypePlayerStats, e));
                    _opponentView.SetNormalizedProgress(NormalizedProgress(PrototypeOpponentStats, e));
                    yield return null;
                }

                _playerView.SetNormalizedProgress(1f);
                _opponentView.SetNormalizedProgress(1f);

                _statusText.text = result.Winner switch
                {
                    RaceWinner.Player => "PLAYER WINS",
                    RaceWinner.Opponent => "OPPONENT WINS",
                    _ => "DRAW"
                };
                _playerTimeText.text = $"Player: {result.PlayerFinishTime:0.00}s";
                _opponentTimeText.text = $"Opponent: {result.OpponentFinishTime:0.00}s";

                yield return new WaitForSeconds(ResultSeconds);
            }
        }

        /// <summary>Fraction of the track completed at <paramref name="elapsedSeconds"/>, clamped to [0,1].</summary>
        private static float NormalizedProgress(CarRaceStats stats, double elapsedSeconds)
        {
            double progress = RaceKinematics.DistanceAtTime(stats, elapsedSeconds) / PrototypeTrackDistance;
            if (progress < 0.0) progress = 0.0;
            else if (progress > 1.0) progress = 1.0;
            return (float)progress;
        }

        // ---- Small runtime construction helpers (prototype only) ----

        private static GameObject CreatePrimitiveNoCollider(PrimitiveType type, string name)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            // No physics in the prototype: remove the collider the primitive ships with.
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
            return go;
        }

        private static void ApplyColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            Material material = renderer.material; // instance (prototype only)
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color); // URP Lit
            }
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color); // fallback
            }
        }

        private static GameObject CreateUiImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            StretchRect(go.GetComponent<RectTransform>(), anchorMin, anchorMax);
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static Text CreateUiText(
            string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            string content, int fontSize, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            StretchRect(go.GetComponent<RectTransform>(), anchorMin, anchorMax);

            var text = go.GetComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void StretchRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}

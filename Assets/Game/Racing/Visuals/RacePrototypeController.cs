using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using IdleRacer.Game.Core;
using IdleRacer.Racing.Simulation;

namespace IdleRacer.Racing.Visuals
{
    /// <summary>
    /// Milestone 0.1C driver: hosts the incremental game loop's presentation. The top of the
    /// screen shows the autoplay race; the bottom hosts the progression UI (<see cref="ProgressionUiView"/>).
    /// <para>
    /// All game logic lives in the pure-C# <see cref="GameController"/>; this MonoBehaviour only
    /// orchestrates timing, visual playback, and UI wiring. The authoritative
    /// <see cref="RaceSimulator"/> (inside the controller) decides every outcome.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RacePrototypeController : MonoBehaviour
    {
        // Race-flow timing (seconds, real time).
        private const float ReadySeconds = 1.0f;
        private const float GoSeconds = 0.5f;
        private const float ResultSeconds = 1.75f;
        private const float AutoBuildIntervalSeconds = 1.0f;

        // ---- Visual layout (world units) ----
        private const float TrackHalfWidth = 5.0f;
        private const float VisualStartX = -4.3f;
        private const float VisualFinishX = 4.3f;
        private const float LaneOffset = 0.5f;
        private const float CarScale = 0.7f;
        private const float CarZ = 0.0f;

        private GameController _game;

        private RaceCarView _playerView;
        private RaceCarView _opponentView;
        private Text _statusText;
        private Text _playerTimeText;
        private Text _opponentTimeText;
        private ProgressionUiView _ui;

        private float _trackCenterY;

        /// <summary>Exposed for read-only inspection/verification. Not for gameplay wiring.</summary>
        public GameController Game => _game;

        private void Start()
        {
            // Keep the autoplay loop running even when the Editor/app is unfocused (idle racer).
            Application.runInBackground = true;

            _game = new GameController(GameConfig.CreatePrototype());

            ConfigureCamera();
            BuildTrack();
            BuildCars();
            BuildUi();

            _ui.Refresh(_game);

            StartCoroutine(RaceLoop());
            StartCoroutine(AutoBuildLoop());
        }

        private IEnumerator RaceLoop()
        {
            while (true)
            {
                // Authoritative simulation for the current stage (no rewards applied yet).
                RacePlan plan = _game.PrepareRace();

                _playerView.SetNormalizedProgress(0f);
                _opponentView.SetNormalizedProgress(0f);
                _playerTimeText.text = string.Empty;
                _opponentTimeText.text = string.Empty;

                _statusText.text = "READY";
                yield return new WaitForSeconds(ReadySeconds);

                _statusText.text = "GO!";
                yield return new WaitForSeconds(GoSeconds);
                _statusText.text = string.Empty;

                double maxFinish = Math.Max(plan.Result.PlayerFinishTime, plan.Result.OpponentFinishTime);
                double elapsed = 0.0;
                while (elapsed < maxFinish)
                {
                    elapsed += Time.deltaTime;
                    double e = Math.Min(elapsed, maxFinish);
                    _playerView.SetNormalizedProgress(Progress(plan.PlayerStats, e, plan.TrackDistance));
                    _opponentView.SetNormalizedProgress(Progress(plan.OpponentStats, e, plan.TrackDistance));
                    yield return null;
                }

                _playerView.SetNormalizedProgress(1f);
                _opponentView.SetNormalizedProgress(1f);

                _statusText.text = plan.Result.Winner switch
                {
                    RaceWinner.Player => "PLAYER WINS",
                    RaceWinner.Opponent => "OPPONENT WINS",
                    _ => "DRAW"
                };
                _playerTimeText.text = $"Player: {plan.Result.PlayerFinishTime:0.00}s";
                _opponentTimeText.text = $"Opponent: {plan.Result.OpponentFinishTime:0.00}s";

                // Apply rewards/advancement now that playback has finished, then refresh the UI.
                _game.ResolveRace(plan);
                _ui.Refresh(_game);

                yield return new WaitForSeconds(ResultSeconds);
            }
        }

        private IEnumerator AutoBuildLoop()
        {
            var wait = new WaitForSeconds(AutoBuildIntervalSeconds);
            while (true)
            {
                if (_game.CanAutoBuildStep())
                {
                    _game.TryAutoBuildStep();
                    _ui.Refresh(_game);
                }
                yield return wait;
            }
        }

        // ---- UI callbacks ----

        private void OnBuildPressed()
        {
            _game.TryBuildItem();
            _ui.Refresh(_game);
        }

        private void OnToggleAutoBuild()
        {
            _game.SetAutoBuildEnabled(!_game.IsAutoBuildEnabled);
            _ui.Refresh(_game);
        }

        private void OnEquipPressed()
        {
            _game.EquipPendingItem();
            _ui.Refresh(_game);
        }

        private void OnDiscardPressed()
        {
            _game.DiscardPendingItem();
            _ui.Refresh(_game);
        }

        // ---- Scene construction ----

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
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.orthographicSize = TrackHalfWidth / Mathf.Max(0.1f, cam.aspect);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.10f, 1f);

            _trackCenterY = cam.orthographicSize * 0.45f;
        }

        private void BuildTrack()
        {
            GameObject road = CreatePrimitiveNoCollider(PrimitiveType.Cube, "Road");
            road.transform.position = new Vector3(0f, _trackCenterY, 0.6f);
            road.transform.localScale = new Vector3(9.2f, 1.8f, 0.1f);
            ApplyColor(road, new Color(0.16f, 0.17f, 0.20f, 1f));

            GameObject startLine = CreatePrimitiveNoCollider(PrimitiveType.Cube, "StartLine");
            startLine.transform.position = new Vector3(VisualStartX, _trackCenterY, 0.3f);
            startLine.transform.localScale = new Vector3(0.10f, 1.8f, 0.2f);
            ApplyColor(startLine, new Color(0.5f, 0.5f, 0.55f, 1f));

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
            ApplyColor(playerGo, new Color(0.30f, 0.55f, 1.0f, 1f));
            _playerView = playerGo.AddComponent<RaceCarView>();
            _playerView.Configure(VisualStartX, VisualFinishX, playerLaneY, CarZ);

            GameObject opponentGo = CreatePrimitiveNoCollider(PrimitiveType.Cube, "OpponentCar");
            opponentGo.transform.localScale = Vector3.one * CarScale;
            ApplyColor(opponentGo, new Color(1.0f, 0.45f, 0.35f, 1f));
            _opponentView = opponentGo.AddComponent<RaceCarView>();
            _opponentView.Configure(VisualStartX, VisualFinishX, opponentLaneY, CarZ);
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("PrototypeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();

            // Bottom progression panel (bottom ~55%).
            var panelGo = new GameObject("ProgressionPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelRt = (RectTransform)panelGo.transform;
            panelRt.anchorMin = new Vector2(0f, 0f);
            panelRt.anchorMax = new Vector2(1f, 0.55f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            panelGo.GetComponent<Image>().color = new Color(0.09f, 0.09f, 0.12f, 1f);

            _ui = new ProgressionUiView(panelRt, OnBuildPressed, OnToggleAutoBuild, OnEquipPressed, OnDiscardPressed);

            // Top race status + finish times (top area, above the panel).
            _statusText = CreateText(canvasGo.transform, "StatusText", new Vector2(0f, 0.80f), new Vector2(1f, 0.93f), string.Empty, 90, TextAnchor.MiddleCenter, Color.white);
            _playerTimeText = CreateText(canvasGo.transform, "PlayerTimeText", new Vector2(0.06f, 0.70f), new Vector2(0.94f, 0.76f), string.Empty, 40, TextAnchor.MiddleLeft, new Color(0.55f, 0.72f, 1f));
            _opponentTimeText = CreateText(canvasGo.transform, "OpponentTimeText", new Vector2(0.06f, 0.64f), new Vector2(0.94f, 0.70f), string.Empty, 40, TextAnchor.MiddleLeft, new Color(1f, 0.6f, 0.5f));
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                return;
            }
            // The project uses the Input System package, so use its UI input module (the legacy
            // StandaloneInputModule throws when active input handling is Input System only).
            var es = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }

        private static float Progress(CarRaceStats stats, double elapsedSeconds, double trackDistance)
        {
            double progress = RaceKinematics.DistanceAtTime(stats, elapsedSeconds) / trackDistance;
            if (progress < 0.0) progress = 0.0;
            else if (progress > 1.0) progress = 1.0;
            return (float)progress;
        }

        private static GameObject CreatePrimitiveNoCollider(PrimitiveType type, string name)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
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
            if (renderer == null) return;
            Material material = renderer.material;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string content, int fontSize, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

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
    }
}

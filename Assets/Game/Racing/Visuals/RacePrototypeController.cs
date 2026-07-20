using System;
using System.Collections;
using UnityEngine;
using IdleRacer.Game.Core;
using IdleRacer.Game.Core.SaveSystem;
using IdleRacer.Game.Equipment;
using IdleRacer.Game.Infrastructure;
using IdleRacer.Racing.Simulation;
using IdleRacer.Racing.Visuals.Hud;

namespace IdleRacer.Racing.Visuals
{
    /// <summary>
    /// Race + game-loop host. World presentation lives in <see cref="RaceWorldView"/>;
    /// progression UI in <see cref="GameHudView"/>. Domain logic stays in <see cref="GameController"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RacePrototypeController : MonoBehaviour
    {
        private const float ReadySeconds = 1.0f;
        private const float GoSeconds = 0.5f;
        private const float ResultSeconds = 1.75f;
        private const float AutoBuildIntervalSeconds = 1.0f;

        private GameController _game;
        private IGameSaveRepository _saveRepository;
        private TimeSpan _offlineDuration;

        private RaceWorldView _world;
        private GameHudView _hud;

        public GameController Game => _game;
        public HudTab ActiveHudTab => _hud != null ? _hud.ActiveTab : HudTab.Build;

        private void Start()
        {
            Application.runInBackground = true;

            GameConfig config = GameConfig.CreatePrototype();
            _saveRepository = new LocalJsonSaveRepository(LocalJsonSaveRepository.DefaultSaveFilePath);
            SaveLoadResult load = _saveRepository.Load();

            if (load.Status == SaveLoadStatus.Loaded)
            {
                _game = new GameController(config, loadedData: load.Data);
                _offlineDuration = OfflineProgress.CalculateOfflineDuration(load.Data.lastSavedUtcTicks, DateTime.UtcNow.Ticks);
                Debug.Log($"[IdleRacer] Loaded save (v{load.Data.saveVersion}). Offline duration: {_offlineDuration} (no offline rewards).");
            }
            else
            {
                _game = new GameController(config);
                _offlineDuration = TimeSpan.Zero;
                Debug.Log($"[IdleRacer] No usable save ({load.Status}); starting a fresh player.");
            }

            _game.StateChanged += SaveNow;

            _world = new RaceWorldView();
            _world.Build();
            EnsureEventSystem();

            _hud = new GameHudView(
                transform,
                OnBuildPressed,
                OnToggleAutoBuild,
                OnEquipPressed,
                OnDiscardPressed,
                OnUpgradeSlotPressed);
            _hud.Refresh(_game);

            StartCoroutine(RaceLoop());
            StartCoroutine(AutoBuildLoop());
        }

        private void SaveNow()
        {
            if (_game != null && _saveRepository != null)
            {
                _saveRepository.Save(_game.CreateSaveData());
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveNow();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) SaveNow();
        }

        private void OnApplicationQuit() => SaveNow();

        private IEnumerator RaceLoop()
        {
            while (true)
            {
                RacePlan plan = _game.PrepareRace();
                _world.PlayerView.SetNormalizedProgress(0f);
                _world.OpponentView.SetNormalizedProgress(0f);
                _world.ResetMotion();
                _hud.PlayerTimeText.text = string.Empty;
                _hud.OpponentTimeText.text = string.Empty;

                _hud.StatusText.text = "READY";
                yield return new WaitForSeconds(ReadySeconds);

                _hud.StatusText.text = "GO!";
                yield return new WaitForSeconds(GoSeconds);
                _hud.StatusText.text = string.Empty;

                double maxFinish = Math.Max(plan.Result.PlayerFinishTime, plan.Result.OpponentFinishTime);
                double elapsed = 0.0;
                while (elapsed < maxFinish)
                {
                    elapsed += Time.deltaTime;
                    double e = Math.Min(elapsed, maxFinish);
                    float playerProgress = Progress(plan.PlayerStats, e, plan.TrackDistance);
                    float opponentProgress = Progress(plan.OpponentStats, e, plan.TrackDistance);
                    _world.PlayerView.SetNormalizedProgress(playerProgress);
                    _world.OpponentView.SetNormalizedProgress(opponentProgress);
                    _world.SetMotion(Mathf.Max(playerProgress, opponentProgress), Time.deltaTime);
                    yield return null;
                }

                _world.PlayerView.SetNormalizedProgress(1f);
                _world.OpponentView.SetNormalizedProgress(1f);

                _hud.StatusText.text = plan.Result.Winner switch
                {
                    RaceWinner.Player => "YOU WIN",
                    RaceWinner.Opponent => "OPPONENT WINS",
                    _ => "DRAW"
                };
                _hud.PlayerTimeText.text = $"You  {plan.Result.PlayerFinishTime:0.00}s";
                _hud.OpponentTimeText.text = $"Rival  {plan.Result.OpponentFinishTime:0.00}s";

                _game.ResolveRace(plan);
                _hud.Refresh(_game);
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
                    _hud.Refresh(_game);
                }
                yield return wait;
            }
        }

        private void OnBuildPressed()
        {
            _game.TryBuildItem();
            _hud.Refresh(_game);
        }

        private void OnToggleAutoBuild()
        {
            _game.SetAutoBuildEnabled(!_game.IsAutoBuildEnabled);
            _hud.Refresh(_game);
        }

        private void OnEquipPressed()
        {
            _game.EquipPendingItem();
            _hud.Refresh(_game);
        }

        private void OnDiscardPressed()
        {
            _game.DiscardPendingItem();
            _hud.Refresh(_game);
        }

        private void OnUpgradeSlotPressed(EquipmentSlotType slot)
        {
            _game.TryUpgradeSlot(slot);
            _hud.Refresh(_game);
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null) return;
            new GameObject("EventSystem",
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
    }
}

using System;
using System.IO;
using UnityEngine;
using IdleRacer.Game.Core.SaveSystem;

namespace IdleRacer.Game.Infrastructure
{
    /// <summary>
    /// Local, versioned JSON save file implementation of <see cref="IGameSaveRepository"/>.
    /// Lives in the infrastructure layer so the domain never touches the file system. Writes are
    /// done to a temporary file then atomically replaced, so a partial write is unlikely to destroy
    /// existing progression. Missing/corrupted/unsupported saves are reported (never thrown) so the
    /// game can fall back to a fresh state without crashing.
    /// </summary>
    public sealed class LocalJsonSaveRepository : IGameSaveRepository
    {
        private readonly string _filePath;

        public LocalJsonSaveRepository(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        /// <summary>Default production save path under the Unity persistent data location.</summary>
        public static string DefaultSaveFilePath =>
            Path.Combine(Application.persistentDataPath, "idleracer_save_v1.json");

        public SaveLoadResult Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new SaveLoadResult(SaveLoadStatus.NoSave, null);
                }

                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new SaveLoadResult(SaveLoadStatus.Corrupted, null);
                }

                GameSaveDataV1 data;
                try
                {
                    data = JsonUtility.FromJson<GameSaveDataV1>(json);
                }
                catch
                {
                    return new SaveLoadResult(SaveLoadStatus.Corrupted, null);
                }

                if (data == null || data.saveVersion <= 0)
                {
                    return new SaveLoadResult(SaveLoadStatus.Corrupted, null);
                }

                if (data.saveVersion > SaveConstants.CurrentVersion)
                {
                    return new SaveLoadResult(SaveLoadStatus.UnsupportedVersion, null);
                }

                return new SaveLoadResult(SaveLoadStatus.Loaded, data);
            }
            catch (Exception)
            {
                // Any unexpected I/O error: fail safely as corrupted rather than crashing.
                return new SaveLoadResult(SaveLoadStatus.Corrupted, null);
            }
        }

        public void Save(GameSaveDataV1 data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            string directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(data);
            string tempPath = _filePath + ".tmp";
            File.WriteAllText(tempPath, json);

            // Replace the primary file atomically where possible; fall back to move on platforms
            // that do not support File.Replace.
            try
            {
                if (File.Exists(_filePath))
                {
                    File.Replace(tempPath, _filePath, null);
                }
                else
                {
                    File.Move(tempPath, _filePath);
                }
            }
            catch (Exception)
            {
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath);
                }
                File.Move(tempPath, _filePath);
            }
        }
    }
}

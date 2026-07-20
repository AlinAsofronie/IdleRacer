namespace IdleRacer.Game.Core.SaveSystem
{
    /// <summary>Outcome of a load attempt.</summary>
    public enum SaveLoadStatus
    {
        /// <summary>A valid save was loaded.</summary>
        Loaded,

        /// <summary>No save exists yet (fresh install).</summary>
        NoSave,

        /// <summary>A save exists but could not be parsed (treat as fresh; do not crash).</summary>
        Corrupted,

        /// <summary>The save's version is newer than this build supports (treat as fresh; do not crash).</summary>
        UnsupportedVersion
    }

    /// <summary>Result of <see cref="IGameSaveRepository.Load"/>.</summary>
    public readonly struct SaveLoadResult
    {
        public SaveLoadStatus Status { get; }
        public GameSaveDataV1 Data { get; }

        public SaveLoadResult(SaveLoadStatus status, GameSaveDataV1 data)
        {
            Status = status;
            Data = data;
        }

        public bool HasData => Status == SaveLoadStatus.Loaded && Data != null;
    }

    /// <summary>
    /// Abstraction over save persistence. The domain/game controller depends only on this
    /// interface, never on file-system or platform APIs. Concrete implementations live in the
    /// infrastructure layer (local file) or tests (in-memory).
    /// </summary>
    public interface IGameSaveRepository
    {
        /// <summary>Loads the save, reporting missing/corrupted/unsupported cases without throwing.</summary>
        SaveLoadResult Load();

        /// <summary>Persists <paramref name="data"/>, overwriting any existing save.</summary>
        void Save(GameSaveDataV1 data);
    }
}

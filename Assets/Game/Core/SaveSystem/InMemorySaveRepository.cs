namespace IdleRacer.Game.Core.SaveSystem
{
    /// <summary>
    /// In-memory <see cref="IGameSaveRepository"/> for tests and headless use. Holds the last saved
    /// data; reports <see cref="SaveLoadStatus.NoSave"/> until something is saved.
    /// </summary>
    public sealed class InMemorySaveRepository : IGameSaveRepository
    {
        private GameSaveDataV1 _data;

        public SaveLoadResult Load()
        {
            return _data == null
                ? new SaveLoadResult(SaveLoadStatus.NoSave, null)
                : new SaveLoadResult(SaveLoadStatus.Loaded, _data);
        }

        public void Save(GameSaveDataV1 data)
        {
            _data = data;
        }
    }
}

using IdleRacer.Game.Equipment.Items;

namespace IdleRacer.Game.Core
{
    /// <summary>Why an item-build attempt succeeded or failed.</summary>
    public enum ItemBuildStatus
    {
        Success,
        NotEnoughWheels,
        PendingItemUnresolved
    }

    /// <summary>Result of attempting to build one item via the Item Creator.</summary>
    public readonly struct ItemBuildOutcome
    {
        public ItemBuildStatus Status { get; }
        public EquipmentItem Item { get; }

        public ItemBuildOutcome(ItemBuildStatus status, EquipmentItem item)
        {
            Status = status;
            Item = item;
        }

        public bool Success => Status == ItemBuildStatus.Success;
    }
}

using Game.Core.Events;
using UnityEngine;

public class WheelSpinApplyPendingsUpdate : IEconomyUpdate
{
    public void Commit(GameContext context, IEventBus eventBus)
    {
        foreach (var pending in context.WheelProgress.GetPendingItems().ToSerializable())
        {
            context.AddInventoryItem(pending.ItemId, pending.Amount);
            Debug.LogWarning($"ApplyItem || {pending.ItemId},{pending.Amount}");
            eventBus.Publish(new InventoryChangedEventDto
            {
                itemId = pending.ItemId,
                updatedAmount = context.GetItemAmount(pending.ItemId)
            });
        }
        context.WheelProgress.PendingItems.Clear();
        context.WheelProgress.Reset();
    }
}

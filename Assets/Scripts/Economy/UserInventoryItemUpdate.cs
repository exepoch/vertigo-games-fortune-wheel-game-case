using Game.Core.Events;
using UnityEngine;

public class UserInventoryItemUpdate : IEconomyUpdate
{
    public int RewardItemId;
    public int RewardAmount;

    public void Commit(GameContext context, IEventBus eventBus)
    {
        context.AddInventoryItem(RewardItemId, RewardAmount);
        Debug.LogWarning($"Publishing Usr InventoryUpdate: {RewardItemId} || {RewardAmount}");
        eventBus.Publish(new InventoryChangedEventDto()
        {
            itemId = RewardItemId,
            updatedAmount = context.GetItemAmount(RewardItemId)
        });
    }
}

using Game.Core.Events;
using UnityEngine;

public class WheelSpinEconomyUpdate : IEconomyUpdate
{
    public int RewardItemId;
    public int RewardAmount;

    public void Commit(GameContext context, IEventBus eventBus)
    {
        context.WheelProgress.AddReward(
            RewardItemId,
            RewardAmount
        );
        Debug.LogWarning($"WSEUptade Publishing: {RewardItemId} || {RewardAmount}");
        eventBus.Publish(new WheelItemChangedEventDto()
        {
            itemId = RewardItemId,
            updatedAmount = RewardAmount
        });
    }
}
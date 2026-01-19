using System.Collections.Generic;
using Features.WheelSpinGame.Core.Config;
using Features.WheelSpinGame.Core.Models;
using Features.WheelSpinGame.Network.DTO;
using UnityEngine;

namespace Features.WheelSpinGame.Presentation
{
    public class WheelConfigBuilder
    {
        public WheelViewSetupDto Build(
            WheelClientConfigRuntimeDTO clientConfigSo,
            WheelSpinProgressState progressState)
        {
            var levelRewardPool =
                clientConfigSo.SpinLevels[progressState.ProgressLevel].Rewards;

            var availableRewards = new List<SpinRewardEntryDto>(levelRewardPool);

            
            // Bomb is excluded from silver and gold spin modes
            // to represent safe and super zones as per game design
            var bombIncluded = false;
            
            
            // Bronze spin mode (index 0) always includes a bomb slot by design.
            // Bomb is inserted at index 0 so it can be deterministically excluded
            // from random fill operations later.
            if (progressState.spinModIndex == 0)
            {
                availableRewards.Insert(0,new SpinRewardEntryDto
                (
                    clientConfigSo.BombReward.ViewName,
                    clientConfigSo.BombReward,
                    0
                ));
                bombIncluded = true;
            }
        
            var slots = new List<WheelSlot>();

            var requiredRewardItemCount = clientConfigSo.SpinMods[progressState.spinModIndex].requiredRewardItemCount;
            if (availableRewards.Count < requiredRewardItemCount)
            {
                var fillSource = new List<SpinRewardEntryDto>(availableRewards);
                while (availableRewards.Count < requiredRewardItemCount)
                {
                    // If bomb is included, skip index 0 to avoid duplicating bomb slots
                    var startIndex = bombIncluded ? 1 : 0;
                    var randomPickedFillReward =
                        fillSource[Random.Range(startIndex, fillSource.Count)];

                    availableRewards.Add(new SpinRewardEntryDto(

                        randomPickedFillReward.Name,
                        randomPickedFillReward.Reward,
                        randomPickedFillReward.Amount
                    ));
                }
            }

            // If there are more rewards than required slots,
            // trim from the end to preserve priority ordering from config
            while (availableRewards.Count > requiredRewardItemCount)
            {
                availableRewards.RemoveAt(availableRewards.Count - 1); //If too much, remove the last item, (or select most low valuable one?)
            }
        
            // Distribute slots evenly around the wheel
            float anglePerSlot = 360f / availableRewards.Count;

            for (int i = 0; i < availableRewards.Count; i++)
            {
                var spinRewardItemSo = availableRewards[i];
                slots.Add(new WheelSlot
                {
                    rewardId = spinRewardItemSo.Reward.Id,
                    angle = i * anglePerSlot,
                    rewardName = spinRewardItemSo.Reward.ViewName,
                    rewardAmount = spinRewardItemSo.Amount,
                    sprite = spinRewardItemSo.Reward.ViewImage
                });
            }
            
            return new WheelViewSetupDto
            {
                SpinDuration = clientConfigSo.SpinDuration,
                progressLevel = progressState.ProgressLevel,
                Slots = slots,
                Pendings = progressState.Pendings,
                SpinMod = clientConfigSo.SpinMods[progressState.spinModIndex],

            };
        }
    }
}
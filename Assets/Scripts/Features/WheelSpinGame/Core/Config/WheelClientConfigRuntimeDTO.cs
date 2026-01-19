using System.Collections.Generic;
using UnityEngine;

namespace Features.WheelSpinGame.Core.Config
{
    public class WheelClientConfigRuntimeDTO
    {
        public float SpinDuration { get; }
        public int MaxProgressLevel { get; }
        public IReadOnlyList<RewardItemDto> RewardItemsUniq;
        public IReadOnlyList<SpinMod> SpinMods { get; }
        public IReadOnlyList<SpinLevelDto> SpinLevels { get; }

        public RewardItemDto BombReward { get; }

        public WheelClientConfigRuntimeDTO(
            float spinDuration,
            int maxProgressLevel,
            List<SpinMod> spinMods,
            List<SpinLevelDto> spinLevels,
            List<RewardItemDto> rewardItemDtos,
            RewardItemDto bombReward)
        {
            SpinDuration = spinDuration;
            MaxProgressLevel = maxProgressLevel;
            SpinMods = spinMods;
            RewardItemsUniq = rewardItemDtos;
            SpinLevels = spinLevels;
            BombReward = bombReward;
        }
    }

    
    public class RewardItemDto
    {
        public int Id { get; }
        public string ViewName { get; }
        public Sprite ViewImage { get; }

        public RewardItemDto(int id, string name, Sprite image)
        {
            Id = id;
            ViewName = name;
            ViewImage = image;
        }
    }
    
    public class SpinLevelDto
    {
        public float BombPossibility { get; }
        public IReadOnlyList<SpinRewardEntryDto> Rewards { get; }

        public SpinLevelDto(
            float bombPossibility,
            List<SpinRewardEntryDto> rewards)
        {
            BombPossibility = bombPossibility;
            Rewards = rewards;
        }
    }

    
    public class SpinRewardEntryDto
    {
        public string Name { get; }
        public RewardItemDto Reward { get; }
        public int Amount { get; }

        public SpinRewardEntryDto(string name, RewardItemDto reward, int amount)
        {
            Name = name;
            Reward = reward;
            Amount = amount;
        }
    }

}

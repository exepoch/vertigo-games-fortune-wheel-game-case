using System.Collections.Generic;
using Features.WheelSpinGame.Core.Config;

namespace Features.WheelSpinGame.Core.Models
{
    //Generates deep copy of configSO for runtime access
    public static class WheelClientConfigRuntimeFactory
    {
        public static WheelClientConfigRuntimeDTO Create(
            WheelClientConfigSO source)
        {
            var dto = new WheelClientConfigRuntimeDTO
            (
                source.spinDuration,
                source.maxProgressLevel, 
                MapSpinMods(source.SpinMods),
                MapSpinLevels(source.spinLevels),
                MapRewardItems(source.allRewardItemList),
                MapReward(source.bombReward)
            );

            return dto;
        }

        private static List<RewardItemDto> MapRewardItems(List<SpinRewardItemSO> rewardItemDtos)
        {
            var itemList = new List<RewardItemDto>();

            foreach (var dto in rewardItemDtos)
            {
                itemList.Add(new RewardItemDto(dto.RewardId,dto.RewardName,dto.RewardImage));
            }

            return itemList;
        }

        private static RewardItemDto MapReward(SpinRewardItemSO reward)
        {
            return new RewardItemDto
            (
                reward.RewardId,
                reward.RewardName,
                reward.RewardImage
            );
        }

        private static List<SpinMod> MapSpinMods(List<SpinMod> mods)
        {
            var list = new List<SpinMod>(mods.Count);

            foreach (var mod in mods)
            {
                list.Add(new SpinMod
                {
                    name = mod.name,
                    levelModPattern = mod.levelModPattern,
                    requiredRewardItemCount = mod.requiredRewardItemCount,
                    viewName = mod.viewName,
                    baseSprite = mod.baseSprite,
                    indicatorSprite = mod.indicatorSprite
                });
            }

            return list;
        }

        private static List<SpinLevelDto> MapSpinLevels(List<SpinLevel> levels)
        {
            var list = new List<SpinLevelDto>(levels.Count);

            var i = 0;
            foreach (var level in levels)
            {
                var levelDto = new SpinLevelDto
                (
                    level.bombPossibility,
                    MapRewards(level.rewards)
                );

                list.Add(levelDto);
                i++;
            }

            return list;
        }

        private static List<SpinRewardEntryDto> MapRewards(
            List<SpinRewardEntry> rewards)
        {
            var list = new List<SpinRewardEntryDto>(rewards.Count);

            foreach (var reward in rewards)
            {
                list.Add(new SpinRewardEntryDto
                (
                    reward.name,
                    MapReward(reward.reward),
                    reward.amount
                ));
            }

            return list;
        }
    }
}

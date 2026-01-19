using System.Collections.Generic;
using System.Threading.Tasks;
using Features.WheelSpinGame.Core.Config;
using Features.WheelSpinGame.Network.DTO;
using Game.Features.Wheel.Core;
using UnityEngine;

namespace Game.Features.Wheel.Network
{
    /// <summary>
    /// Fake API implementation used only to simulate server-side behavior
    /// for the case demo. In a production scenario, this would be replaced
    /// by a real backend service.
    /// </summary>
    public class MockWheelApi : IWheelApi
    {
        private readonly WheelClientConfigRuntimeDTO _configSo;
        private readonly GameContext _serverContext; //user backend data

        public MockWheelApi(WheelClientConfigRuntimeDTO configSo)
        {
            _configSo = configSo;
            _serverContext = SaveSystem.LoadGameContext();
        }
        
        public async Task<GameContext> LoadUserContext()
        {
            await FakeLatency();

            // Create a deep copy of the user data model
            var userDataCopy = new UserDataModel
            {
                inventory = new List<ItemAmountEntry>(_serverContext.userDataModel.inventory)
            };
            userDataCopy.ApplySaveData(); // Ensure internal state is consistent

            // Create a deep copy of the wheel progress model
            var wheelProgressCopy = new WheelSpinProgressModel
            {
                CurrentLevel = _serverContext.WheelProgress.CurrentLevel,
                PendingItems = new List<ItemAmountEntry>(_serverContext.WheelProgress.PendingItems)
            };
            wheelProgressCopy.ApplySaveData(); // Ensure internal state is consistent
            return new GameContext(userDataCopy, wheelProgressCopy);
        }

        public async Task<WheelSpinProgressState> GetProgress()
        {
            await FakeLatency();
            var wheelProgressCurrentLevel = _serverContext.WheelProgress.CurrentLevel;
            var spinMode = GetMode(_serverContext.WheelProgress.CurrentLevel);
            
            return new WheelSpinProgressState
            {
                ProgressLevel = wheelProgressCurrentLevel,
                spinModIndex = spinMode
            };
        }

        public async Task<WheelSpinResponse> Spin()
        {
            await FakeLatency();

            int currentLevel = _serverContext.WheelProgress.CurrentLevel;
            int spinMode = GetMode(currentLevel);

            // Bomb resolution (bronze mode only)
            if (IsBombSpin(spinMode) && RollBomb(currentLevel))
            {
                _serverContext.WheelProgress.CatchPendings();
                _serverContext.WheelProgress.Reset();
                SaveSystem.SaveGameContext(_serverContext);

                return CreateBombResponse();
            }

            var pool = _configSo.SpinLevels[currentLevel].Rewards;

            // Exclude bomb placeholder from reward selection
            int rewardBoundary = IsBombSpin(spinMode)
                ? pool.Count - 1
                : pool.Count;

            var selected = pool[Random.Range(0, rewardBoundary)];

            _serverContext.WheelProgress.AddReward(
                selected.Reward.Id,
                selected.Amount
            );

            bool isLastSpin =
                _serverContext.WheelProgress.CurrentLevel >=
                _configSo.MaxProgressLevel;

            if (isLastSpin)
            {
                ApplyPendingItemsToUser();
            }

            SaveSystem.SaveGameContext(_serverContext);

            return CreateRewardResponse(selected, isLastSpin);
        }


        public async Task<bool> Cashout()
        {
            await FakeLatency();
            ApplyPendingItemsToUser();
            SaveSystem.SaveGameContext(_serverContext);
            return true;
        }

        public async Task<bool> Continue()
        {
            await FakeLatency();

            // Continue cost (gold)
            bool canContinue = _serverContext.userDataModel.GetInventoryItem(2) >= 25;
            if (!canContinue)
            {
                await GiveUp();
                return false;
            }

            
            _serverContext.userDataModel.AddInventoryItem(2, -25);
            var cache = _serverContext.WheelProgress.catchedPendingItems.ToSerializable();
            foreach (var entry in cache)
            {
                _serverContext.WheelProgress.AddReward(entry.ItemId,entry.Amount);
            }
            
            _serverContext.WheelProgress.CurrentLevel = _serverContext.WheelProgress.catchedBombedLevel;
            SaveSystem.SaveGameContext(_serverContext);
            return true;
        }

        public async Task GiveUp()
        {
            await FakeLatency();
            _serverContext.WheelProgress.catchedPendingItems.Clear();
            _serverContext.WheelProgress.catchedBombedLevel = 0;
            SaveSystem.SaveGameContext(_serverContext);
        }

        public void AddGold(int amount)
        {
            _serverContext.userDataModel.AddInventoryItem(2, amount);
        }
        
        private void ApplyPendingItemsToUser()
        {
            foreach (var pendingItem in _serverContext.WheelProgress.PendingItems)
            {
                _serverContext.AddInventoryItem(pendingItem.ItemId, pendingItem.Amount);
            }

            _serverContext.WheelProgress.Reset();
            _serverContext.WheelProgress.catchedPendingItems.Clear();
        }

        
        // Determines spin mode based on progression milestones.
        // Highest matching mode wins (e.g. gold > silver > bronze).
        private int GetMode(int level)
        {
            level++; //bypass zero indexing
            
            for (int i = _configSo.SpinMods.Count - 1; i >= 0; i--)
            {
                if (level % _configSo.SpinMods[i].levelModPattern == 0)
                    return i;
            }

            return 0;
        }

        private async Task FakeLatency()
        {
            await Task.Delay(Random.Range(300, 600));
        }
        
        #region Helpers

        private WheelSpinResponse CreateBombResponse()
        {
            return new WheelSpinResponse
            {
                IsBomb = true,
                IsLastSpin = true,
                CurrentProgressLevel = _serverContext.WheelProgress.CurrentLevel,
                NextSpinModIndex = GetMode(_serverContext.WheelProgress.CurrentLevel)
            };
        }

        private WheelSpinResponse CreateRewardResponse(
            SpinRewardEntryDto selectedReward,
            bool isLastSpin)
        {
            return new WheelSpinResponse
            {
                RewardId = selectedReward.Reward.Id,
                Amount = selectedReward.Amount,
                CurrentProgressLevel = _serverContext.WheelProgress.CurrentLevel,
                NextSpinModIndex = GetMode(_serverContext.WheelProgress.CurrentLevel),
                IsBomb = false,
                IsLastSpin = isLastSpin
            };
        }

        private bool IsBombSpin(int spinMode)
        {
            // Bronze mode only (index 0) contains bomb
            return spinMode == 0;
        }

        private bool RollBomb(int level)
        {
            return Random.value <
                   _configSo.SpinLevels[level].BombPossibility;
        }

        #endregion
    }
}
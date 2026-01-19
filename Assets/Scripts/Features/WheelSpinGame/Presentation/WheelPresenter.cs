using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Features.WheelSpinGame.Core.Config;
using Features.WheelSpinGame.Core.Models;
using Features.WheelSpinGame.Network.DTO;
using Game.Core.Events;
using Game.Economy;
using Game.Features.Wheel.Network;
using Game.Features.Wheel.UI.Views;
using UnityEngine;

namespace Features.WheelSpinGame.Presentation
{
    public class WheelPresenter
    {
        private readonly IWheelApi api;
        private readonly WheelView view;
        private readonly IEconomyService wheelEconomyService;
        private IEventBus eventBus;
        private readonly WheelConfigBuilder configBuilder;
        private readonly WheelClientConfigRuntimeDTO _clientConfigSo;
        private WheelFeatureController _featureController;

        private WheelViewSetupDto _wheelViewSetupDto;

        public WheelPresenter(
            IWheelApi api,
            WheelView view,
            IEconomyService wheelEconomyService,
            WheelClientConfigRuntimeDTO clientConfigSo,
            IEventBus bus)
        {
            this.api = api;
            this.view = view;
            this.wheelEconomyService = wheelEconomyService;
            eventBus = bus;
            _clientConfigSo = clientConfigSo;

            configBuilder = new WheelConfigBuilder();
            view.SpinClicked += ViewOnSpinClicked;
            view.CashOutClicked += ViewOnCashOutClicked;
            view.GiveUpClicked += ViewOnGiveUpClicked;
            view.BuySpinClicked += ViewOnBuySpinClicked;
            eventBus.Subscribe<InventoryChangedEventDto>(OnInventoryChanged);
        }

        // Initializes wheel state, fetches progress and builds initial view configuration
        public async void Initialize(WheelFeatureController featureController)
        {
            _featureController = featureController;
            view.LockSpin(true);
            view.LockCashout(true);

            var progress = await api.GetProgress();

            var gameContext = wheelEconomyService.GetContext();
            
            UpdateViewBuildConfig(new WheelSpinProgressState
            {
                ProgressLevel = progress.ProgressLevel,
                spinModIndex = progress.spinModIndex,
                Pendings = CreatePendingSetupDto(gameContext)
            });

            view.Setup(_wheelViewSetupDto);
            view.UpdateCashText(gameContext.userDataModel.GetInventoryItem(1));
            view.UpdateGoldText(gameContext.userDataModel.GetInventoryItem(2));
            view.LockSpin(false);
            if(progress.ProgressLevel>0)
                view.LockCashout(false);
        }
        
        // Fetches latest server-side progress and rebuilds view configuration
        private async Task GetUpdatedProgress()
        {
            var progress = await api.GetProgress();

            UpdateViewBuildConfig(new WheelSpinProgressState
            {
                ProgressLevel = progress.ProgressLevel,
                spinModIndex = progress.spinModIndex
            });
        }

        // Converts pending rewards from game context into view-ready wheel slot data
        private List<WheelSlot> CreatePendingSetupDto(GameContext context)
        {
            var li = new List<WheelSlot>();

            var list = context.WheelProgress.GetPendingItems().ToSerializable();
            for (var i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                li.Add(new WheelSlot
                {
                    rewardId = entry.ItemId,
                    rewardAmount = entry.Amount,
                    sprite = GetRawRewardData(entry.ItemId).ViewImage

                });
            }

            return li;
        }

        private RewardItemDto GetRawRewardData(int rewardId)
        {
            return _clientConfigSo.RewardItemsUniq.First(x => x.Id == rewardId);
        }

        // Handles reward feedback once the wheel reaches the resolved slot
        private void OnSpinCompleted(WheelSpinResponse result, WheelSlot slotData)
        {
            if (result.IsBomb)
            {
                view.PlayBombClip();
                return;
            }
            view.PlayWinItemClip();
            view.SetReward(slotData);
        }

        // Finalizes spin flow by updating UI state and handling bomb / continuation logic
        private void OnSequenceCompleted(WheelSpinResponse result)
        {
            if(!result.IsBomb)
                view.IncreaseLevel(_wheelViewSetupDto);

            if (!result.IsLastSpin)
            {
                view.LockSpin(false);
                view.LockCashout(false);
            }

            if (result.IsBomb)
            {
                view.LockBuySpin(wheelEconomyService.GetContext().userDataModel.GetInventoryItem(2)<25);
                view.Bombed(true);
            }
        }

        #region ViewButtonRegisters
        
        // Executes full spin flow triggered from UI:
        // - Locks UI
        // - Calls spin API
        // - Resolves visual slot (including bomb override cases)
        // - Commits economy changes
        // - Plays spin animation
        // - Restores UI state on completion or failure
        private async void ViewOnSpinClicked()
        {
            view.LockSpin(true);
            view.LockCashout(true);
            
            // Most critical async flow in the feature; includes explicit error handling
            // to prevent UI soft-locks and handle edge-case reconciliation.
            try
            {
                // 1. Call API and wait for resolved spin result
                var result = await api.Spin();
                
                // 2. Try to find matching slot on the wheel
                var slotIndex = -1;
                for (var i = 0; i < _wheelViewSetupDto.Slots.Count; i++)
                {
                    var slot = _wheelViewSetupDto.Slots[i];

                    // Amount check is required because multiple slots may share the same reward id
                    if (slot.rewardId != result.RewardId || slot.rewardAmount != result.Amount) continue;
                    slotIndex = i;
                    break;
                }

                // 3. Handle case where resolved reward is not currently present on the wheel
                // (e.g. temporarily replaced by a bomb slot on specific levels)
                if (slotIndex < 0)
                {
                    slotIndex = 1; // fallback visual slot (non-bomb, non-edge index)

                    var rewardData = GetRawRewardData(result.RewardId);

                    view.ByPassSlotReward(new WheelSlot
                    {
                        rewardId = result.RewardId,
                        rewardName = rewardData.ViewName,
                        rewardAmount = result.Amount,
                        sprite = rewardData.ViewImage
                    });
                }

                // 4. Apply economy updates if spin was successful
                if (!result.IsBomb)
                {
                    wheelEconomyService.Commit(new WheelSpinEconomyUpdate
                    {
                        RewardItemId = result.RewardId,
                        RewardAmount = result.Amount
                    });

                    if (result.IsLastSpin)
                    {
                        wheelEconomyService.Commit(new WheelSpinApplyPendingsUpdate());
                    }
                }

                // 5. Build resolved slot data for reward feedback
                var resolvedSlot = new WheelSlot
                {
                    rewardId = result.RewardId,
                    rewardName = _wheelViewSetupDto.Slots[slotIndex].rewardName,
                    rewardAmount = result.Amount,
                    sprite = _wheelViewSetupDto.Slots[slotIndex].sprite
                };

                // 6. Play spin animation sequence
                view.PlaySpinClip();
                view.PlaySpin(
                    slotIndex,
                    _wheelViewSetupDto.SpinDuration,
                    onSpinComplete: () => OnSpinCompleted(result, resolvedSlot),
                    onSequenceComplete: () => OnSequenceCompleted(result)
                );

                // 7. Update view configuration for next state
                UpdateViewBuildConfig(new WheelSpinProgressState
                {
                    ProgressLevel = result.CurrentProgressLevel,
                    spinModIndex = result.NextSpinModIndex
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Spin flow failed: {e}");

                // Ensure UI is restored to a safe state to avoid soft-locks
                view.LockSpin(false);
                view.LockCashout(false);
            }
        }


        // Attempts to continue after bomb by spending gold and restoring spin flow
        private async void ViewOnBuySpinClicked()
        {
            view.LockBuySpin(true);
            view.LockGiveUp(true);
            var res = await api.Continue();
            
            if (res)
            {
                await GetUpdatedProgress();
                view.LockSpin(false);
                view.BuyIn(_wheelViewSetupDto);
                wheelEconomyService.Commit(new UserInventoryItemUpdate
                {
                    RewardItemId = 2,
                    RewardAmount = -25
                });
            }
            else
            {
                //Some error maybe
                view.LockBuySpin(false);
                view.LockGiveUp(false);
            }
        }

        // Abandons current wheel run and resets progression
        private async void ViewOnGiveUpClicked()
        {
            await api.GiveUp();
            wheelEconomyService.GetContext().WheelProgress.Reset();
            _featureController.Close();
        }
        
        // Applies pending rewards, cashes out and resets wheel progression
        private async void ViewOnCashOutClicked()
        {
            view.LockSpin(true);
            view.LockCashout(true);
            var result = await api.Cashout();

            wheelEconomyService.Commit(new WheelSpinApplyPendingsUpdate());

            await GetUpdatedProgress();
            
            view.CashOut(_wheelViewSetupDto);
            
            view.LockSpin(false);
        }

        #endregion

        // Rebuilds wheel view configuration based on updated progression state
        private void UpdateViewBuildConfig(WheelSpinProgressState state)

        {
            _wheelViewSetupDto = configBuilder.Build(
                _clientConfigSo,state);
        }
        
        private void OnInventoryChanged(InventoryChangedEventDto changedEvent)
        {
            if(changedEvent.itemId == 1) view.UpdateCashText(changedEvent.updatedAmount);
            if(changedEvent.itemId == 2) view.UpdateGoldText(changedEvent.updatedAmount);
        }
        
        public void Dispose()
        {
            view.SpinClicked -= ViewOnSpinClicked;
            view.CashOutClicked -= ViewOnCashOutClicked;
            view.GiveUpClicked -= ViewOnGiveUpClicked;
            view.BuySpinClicked -= ViewOnBuySpinClicked;
            eventBus.Unsubscribe<InventoryChangedEventDto>(OnInventoryChanged);
        }
    }
}

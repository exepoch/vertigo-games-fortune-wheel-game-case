using System;
using System.Collections.Generic;

namespace Game.Features.Wheel.Core
{
    [Serializable]
    public class WheelSpinProgressModel
    {
        public int CurrentLevel;
        private ItemCollection pendingItems = new();
        public List<ItemAmountEntry> PendingItems; //Serializable version for json
        
        public ItemCollection catchedPendingItems = new(); //Api only, continue feature req
        public List<ItemAmountEntry> CatchedPendingItems; //Serializable version for json
        public int catchedBombedLevel;
        
        public void SetPendingItems(ItemCollection collection) => pendingItems = collection;
        public ItemCollection GetPendingItems() => pendingItems;

        public void AddReward(int id,int amount)
        {
            pendingItems.Add(id, amount);
            CurrentLevel++;
        }

        public void CatchPendings()
        {
            catchedPendingItems.Clear();
            foreach (var entry in pendingItems.ToSerializable())
            {
                catchedPendingItems.Add(entry.ItemId, entry.Amount);
            }

            catchedBombedLevel = CurrentLevel;
        }

        public void Reset()
        {
            pendingItems = new ItemCollection();
            CurrentLevel = 0;
        }
        
        public void ApplySaveData()
        {
            pendingItems.FromSerializable(PendingItems);
        }

        public void CaptureSaveData()
        {
            PendingItems = pendingItems.ToSerializable();
            CatchedPendingItems = catchedPendingItems.ToSerializable();
        }
    }
}

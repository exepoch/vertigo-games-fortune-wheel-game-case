using System;
using System.Collections.Generic;

[Serializable]
public class UserDataModel
{
    private readonly ItemCollection userItems = new();
    public List<ItemAmountEntry> inventory = new();

    public int GetInventoryItem(int rewardId)
        => userItems.Get(rewardId);

    public int AddInventoryItem(int rewardId, int amount)
    {
        return userItems.Add(rewardId, amount);
    }
    
    public void ApplySaveData()
    {
        userItems.FromSerializable(inventory);
    }

    public void CaptureSaveData()
    {
        inventory = userItems.ToSerializable();
    }
}
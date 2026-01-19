using System;
using Game.Features.Wheel.Core;

[Serializable]
public class GameContext
{
    public UserDataModel userDataModel = new();
    public WheelSpinProgressModel WheelProgress = new();

    public GameContext(UserDataModel userData, WheelSpinProgressModel wheelProgress)
    {
        userDataModel = userData;
        WheelProgress = wheelProgress;
        CaptureSaveData();
    }

    public void AddInventoryItem(int productId, int amount)
    {
        userDataModel.AddInventoryItem(productId, amount);
    }

    public int GetItemAmount(int productId)
    {
        return userDataModel.GetInventoryItem(productId);
    }
    
    
    public void CaptureSaveData()
    {
        userDataModel.CaptureSaveData();
        WheelProgress.CaptureSaveData();
    }
    
    public void ApplySaveData()
    {
        userDataModel.ApplySaveData();
        WheelProgress.ApplySaveData();
    }
}

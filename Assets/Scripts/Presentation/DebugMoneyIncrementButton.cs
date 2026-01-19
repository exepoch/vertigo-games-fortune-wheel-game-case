using Game.Economy;
using Game.Features.Wheel.Network;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DebugMoneyIncrementButton : MonoBehaviour
{
    EconomyService economyService;
    private IWheelApi api;
    public void Init(IEconomyService service,IWheelApi api)
    {
        this.api = api;
        GetComponent<Button>().onClick.AddListener(() =>
        {
            service.Commit(new UserInventoryItemUpdate
            {
                RewardItemId = 2,
                RewardAmount = 50
            });
            api.AddGold(50);
            SaveSystem.SaveGameContext(service.GetContext());
        });
    }
}

using Features.WheelSpinGame.Core.Config;
using Game.Core.Events;
using Game.Economy;
using Game.Features.Wheel.Network;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DebugOpenGameButton : MonoBehaviour
{
    public void Init(
        IWheelApi wheelApi,
        IEconomyService economy,
        WheelClientConfigRuntimeDTO runtimeConfig,
        IEventBus bus,
        GameObject wheelFeaturePrefab,
        Transform uiRoot)
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            var wheel = WheelFeatureFactory.Create(
                wheelApi,
                economy,
                bus,
                runtimeConfig,
                wheelFeaturePrefab,
                uiRoot);
            wheel?.Open();
        });
    }
}

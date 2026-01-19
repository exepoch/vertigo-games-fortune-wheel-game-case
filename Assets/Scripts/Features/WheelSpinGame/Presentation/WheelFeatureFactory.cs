using Features.WheelSpinGame.Core.Config;
using Features.WheelSpinGame.Presentation;
using Game.Core.Events;
using UnityEngine;
using Game.Features.Wheel.Network;
using Game.Economy;
using Game.Features.Wheel.UI.Views;

public class WheelFeatureFactory
{
    public static WheelFeatureController Create(
        IWheelApi wheelApi,
        IEconomyService economy,
        IEventBus bus,
        WheelClientConfigRuntimeDTO clientConfig,
        GameObject wheelPrefab,
        Transform uiRoot)
    {
        if (WheelFeatureController.Instance != null) return WheelFeatureController.Instance;
        GameObject go = Object.Instantiate(wheelPrefab, uiRoot);
        var view = go.GetComponentInChildren<WheelView>();

        var presenter = new WheelPresenter(
            wheelApi,
            view,
            economy,
            clientConfig,
            bus
        );
        
        return new WheelFeatureController(presenter, go);
    }
}
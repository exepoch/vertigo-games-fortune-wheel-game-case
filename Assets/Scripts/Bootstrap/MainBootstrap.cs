using Features.WheelSpinGame.Core.Config;
using Features.WheelSpinGame.Core.Models;
using Game.Core.Events;
using Game.Economy;
using Game.Features.Wheel.Network;
using UnityEngine;

/// <summary>
/// Entry point responsible for bootstrapping core systems,
/// initializing configs, services and opening initial features.
/// </summary>
public class MainBootstrap : MonoBehaviour
{
    [SerializeField] Transform uiRoot; // Root transform for all UI features
    [Header("FeaturePrefabs")]
    [SerializeField] private GameObject wheelFeaturePrefab;

    [Header("ConfigFiles")] 
    [SerializeField] private WheelClientConfigSO wheelSpinConfigSoSo; // Static config source (replaced by remote config in production)

    [Header("Debug")] 
    [SerializeField] private DebugMoneyIncrementButton addGoldButton;
    [SerializeField] private DebugOpenGameButton openGameButton;
    
    private GameContext gameContext;
    public IEventBus EventBus { get; private set; }

    async void Start()
    {
        Application.targetFrameRate = 60;
        EventBus = new EventBus();
        var runtimeConfig = WheelClientConfigRuntimeFactory.Create(wheelSpinConfigSoSo);
        IWheelApi wheelApi = new MockWheelApi(runtimeConfig); // Using a fake API implementation to simulate server responses for the case demo

        gameContext = await wheelApi.LoadUserContext();
        IEconomyService economy = new EconomyService(gameContext, EventBus);
        
        //Clicks to open card game button
        OpenWheelGame(wheelApi, economy, runtimeConfig);
        
        addGoldButton.Init(economy, wheelApi);
        openGameButton.Init(wheelApi,economy,runtimeConfig,EventBus,wheelFeaturePrefab,uiRoot);
    }

    private void OpenWheelGame(IWheelApi wheelApi, IEconomyService economy, WheelClientConfigRuntimeDTO runtimeConfig)
    {
        var wheel = WheelFeatureFactory.Create(wheelApi,
            economy,
            EventBus,
            runtimeConfig,
            wheelFeaturePrefab,
            uiRoot);
        wheel?.Open();
    }
}

using Features.WheelSpinGame.Presentation;
using UnityEngine;

public class WheelFeatureController
{
    public static WheelFeatureController Instance;
    private readonly WheelPresenter presenter;
    private readonly GameObject root;
    public bool isOpen { get; private set; }

    public WheelFeatureController(
        WheelPresenter presenter,
        GameObject root)
    {
        Instance = this;
        this.presenter = presenter;
        this.root = root;
    }

    public void Open()
    {
        if(isOpen) return;
        presenter.Initialize(this);
        root.SetActive(true);
        isOpen = true;
    }

    public void Close()
    {
        presenter.Dispose();
        isOpen = false;
        Instance = null;
        Object.Destroy(root);
    }
}
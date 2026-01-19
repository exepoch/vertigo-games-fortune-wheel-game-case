using UnityEngine;

[CreateAssetMenu(
    fileName = "WheelSpinReward",
    menuName = "Scriptables/WheelSpin/RewardItem")]
public class SpinRewardItemSO : ScriptableObject
{
    public int RewardId;
    public string RewardName;
    public Sprite RewardImage;
}
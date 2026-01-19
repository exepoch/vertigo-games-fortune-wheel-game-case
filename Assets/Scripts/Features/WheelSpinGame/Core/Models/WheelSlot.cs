using System;
using UnityEngine;

namespace Features.WheelSpinGame.Core.Models
{
    [Serializable]
    public class WheelSlot
    {
        public int rewardId;
        public string rewardName;
        public float angle;
        public int rewardAmount;
        public Sprite sprite;
    }
}
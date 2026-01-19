using DG.Tweening;
using Features.WheelSpinGame.Core.Models;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game.Features.Wheel.UI.Views
{
    public class WheelSlotView : MonoBehaviour
    {
        [SerializeField] private Image rewardImage;
        [SerializeField] private TextMeshProUGUI rewardText;

        public int ValueOfAmount { get; set; }
        public int SlotId { get; private set; }
        public float Angle { get; private set; }

        public void Setup(WheelSlot slotData)
        {
            SlotId = slotData.rewardId;
            Angle = slotData.angle;
            ValueOfAmount = slotData.rewardAmount;
            rewardText.text = $"x{slotData.rewardAmount:N0}";
            ValueTextAnim();
            transform.localEulerAngles = new Vector3(0, 0, Angle);
            rewardImage.sprite = slotData.sprite;
        }
        
        public void UpdateAmount(int set)
        {
            ValueOfAmount = set;
            rewardText.text = $"x{ValueOfAmount:N0}";
            ValueTextAnim();
        }

        private void ValueTextAnim()
        {
            rewardText.color = Color.magenta;
            rewardText.DOColor(Color.white, .5f);
            rewardText.transform.localScale = Vector3.one * 1.5f;
            rewardText.transform.DOScale(1, .3f);
        }
    }
}
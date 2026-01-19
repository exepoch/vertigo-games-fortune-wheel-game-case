using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Features.WheelSpinGame.Core.Models;
using UnityEngine;
using TMPro;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace Game.Features.Wheel.UI.Views
{
    /// <summary>
    /// Handles all visual, animation and user interaction logic
    /// for the Wheel feature. This class intentionally contains
    /// UI-specific state and animations, while game logic is delegated
    /// to the presenter.
    /// </summary>
    public class WheelView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup wheelCg,bombResultCg;
        [SerializeField] private Transform wheelRoot,pendingSlotContent;
        [SerializeField] private Image wheelBaseImage,levelWheelBaseImage,indicatorImage;
        [SerializeField] private Button spinButton,cashoutButton,buySpinButton,giveUpButton;
        [SerializeField] private WheelSlotView wheelSlotPref,pendingRewardPref;
        [SerializeField] private TextMeshProUGUI spinModText,rewardText,cashText,goldText;

        [Header("AudioClips")] 
        [SerializeField] private AudioClip spinningClip;
        [SerializeField] private AudioClip winItemClip;
        [SerializeField] private AudioClip bombClip;
        
        [Header("Levels View")]
        [SerializeField] private Transform levelsWheelTransform;
        [SerializeField] private TextMeshProUGUI[] levelText;

        private List<WheelSlotView> slots = new();
        private Dictionary<int,WheelSlotView> pendingSlots = new();

        public event Action SpinClicked,CashOutClicked,GiveUpClicked,BuySpinClicked;
        private AudioSource _clipSource;
        private int _levelWheelLastIndex,_levelWheelCurrentIndex;
        
        // Used when the API resolves a reward that was temporarily removed
        // from the wheel (e.g. replaced by a bomb on specific levels).
        // This allows the spin result to visually match the API response
        // without permanently altering the wheel configuration.
        private WheelSlot pendingApiOverrideSlot;

        private Coroutine spinRoutine, idleRoutine;

        void Awake()
        {
            _clipSource = GetComponent<AudioSource>();
            spinButton?.onClick.AddListener(
                () => SpinClicked?.Invoke()
            );
            cashoutButton?.onClick.AddListener(
                () => CashOutClicked?.Invoke()
            );
            giveUpButton?.onClick.AddListener(
                () => GiveUpClicked?.Invoke()
            );
            buySpinButton?.onClick.AddListener(
                () => BuySpinClicked?.Invoke()
            );
        }

        // Initializes the wheel view with initial data and starts idle animation
        public void Setup(WheelViewSetupDto viewSetupDto)

        {
            GenerateWheelSlotElements(viewSetupDto);
            GeneratePendingItemContent(viewSetupDto);
            SetInitialViewElements(viewSetupDto);

            idleRoutine = StartCoroutine(IdleRoutine());
        }

        private void SetInitialViewElements(WheelViewSetupDto viewSetupDto)
        {
            SetWheelVisuals(viewSetupDto);
            bombResultCg.interactable = false;
            bombResultCg.blocksRaycasts = false;
            bombResultCg.alpha = 0;
            
            SetupLevelWheel(viewSetupDto);
        }

        // Prepares level wheel texts around the current progression level
        public void SetupLevelWheel(WheelViewSetupDto viewSetupDto)

        {
            int currentLevel = viewSetupDto.progressLevel;

            // Level wheel shows:
            // [previous levels] [current progression] [upcoming levels]
            // Negative or zero levels are intentionally hidden
            for (int i = 0; i < levelText.Length; i++)
            {
                int displayedLevel;

                bool isPreviousLevelSlot = i >= levelText.Length - 2;

                displayedLevel = isPreviousLevelSlot
                    ? currentLevel - (levelText.Length - 1 - i)
                    : currentLevel + 1 + i;

                levelText[i].text = displayedLevel > 0 
                    ? displayedLevel.ToString() 
                    : string.Empty;
            }


            _levelWheelCurrentIndex = 0;
            _levelWheelLastIndex = 5;
            rewardText.text = "";
        }

        private void GenerateWheelSlotElements(WheelViewSetupDto viewSetupDto)
        {
            for (int i = 0; i < viewSetupDto.Slots.Count; i++)
            {
                var slot = Instantiate(wheelSlotPref,wheelSlotPref.transform.parent);
                slots.Add(slot);
                slot.Setup(viewSetupDto.Slots[i]);
                slot.gameObject.SetActive(true);
            }
        }

        private void GeneratePendingItemContent(WheelViewSetupDto viewSetupDto)
        {
            foreach (var pending in viewSetupDto.Pendings)
            {
                AddPendingItem(pending);
            }
        }

        private void AddPendingItem(WheelSlot pending)
        {
            if (pendingSlots.ContainsKey(pending.rewardId))
            {
                pendingSlots[pending.rewardId].UpdateAmount(pendingSlots[pending.rewardId].ValueOfAmount+pending.rewardAmount);
            }
            else
            {
                var slot = Instantiate(pendingRewardPref, pendingSlotContent);
                slot.Setup(pending);
                slot.gameObject.SetActive(true);
                pendingSlots.TryAdd(pending.rewardId, slot);
            }
        }

        private void ClearPendingItems()
        {
            foreach (var slot in pendingSlots)
            {
                Destroy(slot.Value.gameObject);
            }
            pendingSlots.Clear();
        }

        public void CashOut(WheelViewSetupDto viewSetupDto)
        {
            SetupLevelWheel(viewSetupDto);
            ClearPendingItems();
            _levelWheelCurrentIndex = -1;
            _levelWheelLastIndex = 4;
            IncreaseLevel(viewSetupDto);
        }
        
        public void BuyIn(WheelViewSetupDto viewSetupDto)
        {
            SetupLevelWheel(viewSetupDto);
            _levelWheelCurrentIndex = -1;
            _levelWheelLastIndex = 4;
            IncreaseLevel(viewSetupDto);
            Bombed(false);
        }

        // Advances level wheel visuals and refreshes slots after a successful spin
        public void IncreaseLevel(WheelViewSetupDto viewSetupDto)
        {
            Debug.LogWarning($"IncreaaseUpdated to: {viewSetupDto.progressLevel}");
            // Update wheel visuals first so animations use the new spin mode assets
            SetWheelVisuals(viewSetupDto);

            rewardText.text = "";

            // Circular index is used to reuse level text slots
            var textChangeIndex = ++_levelWheelLastIndex % 8;
            var currentTextIndex = ++_levelWheelCurrentIndex % 8;

            levelsWheelTransform
                .DORotate(Vector3.forward * 360 / 8 * currentTextIndex, .5f)
                .OnComplete(() =>
                {
                    // New future level enters the wheel from the end
                    levelText[textChangeIndex].text = $"{viewSetupDto.progressLevel + 6}";

                    // Highlight current level momentarily for feedback
                    levelText[currentTextIndex].color = Color.red;
                    levelText[currentTextIndex].DOColor(Color.white, 1f);
                    levelText[currentTextIndex].transform.DOPunchScale(Vector3.one, .5f);
                });

            RefreshWheelSlots(viewSetupDto);
            wheelRoot.DOPunchScale(Vector3.one * .1f, .5f);
        }

        private void RefreshWheelSlots(WheelViewSetupDto viewSetupDto)
        {
            for (int i = 0; i < slots.Count && i < viewSetupDto.Slots.Count; i++)
            {
                slots[i].Setup(viewSetupDto.Slots[i]);
            }
        }


        private void SetWheelVisuals(WheelViewSetupDto viewSetupDto)
        {
            wheelBaseImage.sprite = viewSetupDto.SpinMod.baseSprite;
            levelWheelBaseImage.sprite = viewSetupDto.SpinMod.baseSprite;
            indicatorImage.sprite = viewSetupDto.SpinMod.indicatorSprite;
            spinModText.text = viewSetupDto.SpinMod.viewName;
        }

        public void Bombed(bool set)
        {
            LockResultScreen(!set);
            LockGiveUp(!set);
            bombResultCg.DOFade(set ? 1 : 0, .5f);
        }

        public void LockResultScreen(bool set)
        {
            bombResultCg.interactable = !set;
            bombResultCg.blocksRaycasts = !set;
        }
        public void LockSpin(bool set)   => spinButton.interactable = !set;
        public void LockCashout(bool set)   => cashoutButton.interactable = !set;
        public void LockGiveUp(bool set)   => giveUpButton.interactable = !set;
        public void LockBuySpin(bool set)   => buySpinButton.interactable = !set;
        

        public void SetReward(WheelSlot slotData)
        {
            rewardText.text = $"{slotData.rewardAmount:N0} {slotData.rewardName}";
            AddPendingItem(new WheelSlot
            {
                rewardId = slotData.rewardId,
                rewardName = slotData.rewardName,
                rewardAmount = slotData.rewardAmount,
                sprite = slotData.sprite
            });
        }
        
        public void UpdateGoldText(int set) => goldText.text = $"Gold: {set}";
        public void UpdateCashText(int set) => cashText.text = $"Cash: {set}";

        // Plays spin animation toward the resolved slot and triggers spin callbacks
        public void PlaySpin(
            int slotId,
            float duration,
            Action onSpinComplete,
            Action onSequenceComplete)
        {
            var target = slots[slotId];
            float finalAngle = 360f * 3 + target.Angle;

            if (pendingApiOverrideSlot != null)
            {
                // Override visual slot result to reflect API-resolved reward
                // even if the slot was temporarily removed from the wheel
                target.Setup(pendingApiOverrideSlot);
                pendingApiOverrideSlot = null;
            }

            if (idleRoutine != null)
                StopCoroutine(idleRoutine);
            if (spinRoutine != null)
                StopCoroutine(spinRoutine);

            spinRoutine = StartCoroutine(
                SpinRoutine(-finalAngle, duration, onSpinComplete,onSequenceComplete)
            );
        }

        // Smoothly rotates the wheel to target angle and finalizes spin sequence
        IEnumerator SpinRoutine(float target, float duration, Action onSpinComplete,Action onSequenceComplete)
        {
            float start = wheelRoot.eulerAngles.z;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                float angle = Mathf.Lerp(start, target, eased);
                wheelRoot.eulerAngles = new Vector3(0, 0, angle);
                yield return null;
            }

            wheelRoot.eulerAngles = new Vector3(0, 0, target);
            wheelRoot.DOPunchPosition(Vector3.one, 1f);
            
            onSpinComplete?.Invoke();
            yield return new WaitForSeconds(.5f); 
            // Small delay to allow reward animations / SFX to be perceived
            onSequenceComplete?.Invoke();
            idleRoutine = StartCoroutine(IdleRoutine());
        }
        
        IEnumerator IdleRoutine()
        {
            // Subtle rotation to keep the wheel visually alive
            // while waiting for user interaction
            while (true)
            {
                wheelRoot.eulerAngles += new Vector3(0, 0, -Time.deltaTime * 2);
                yield return null;
            }
        }

        public void ByPassSlotReward(WheelSlot slotByPass)
        {
            pendingApiOverrideSlot = slotByPass;
        }

        public void PlaySpinClip() => _clipSource.PlayOneShot(spinningClip);
        public void PlayWinItemClip() => _clipSource.PlayOneShot(winItemClip);
        public void PlayBombClip() => _clipSource.PlayOneShot(bombClip);
    }
}

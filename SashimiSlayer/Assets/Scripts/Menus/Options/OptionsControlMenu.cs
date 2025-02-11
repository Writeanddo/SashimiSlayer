using Events;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Options
{
    public class OptionsControlMenu : MonoBehaviour
    {
        private const string SwordAimMultiplier = "SwordAngleMultiplier";
        private const string FlipSwordAim = "SwordAngleFlip";

        [Header("Events (Out)")]

        [SerializeField]
        private FloatEvent _swordAngleMultiplierChangeEvent;

        [Header("Sword Angle Multiplier")]

        [SerializeField]
        private Slider _swordAngleMultiplierSlider;

        [Header("Sword Angle Flip")]

        [SerializeField]
        private Toggle _swordAngleFlipToggle;

        private float _swordAngleMultiplier;
        private bool _swordAngleFlip;

        private void Awake()
        {
            _swordAngleMultiplier = PlayerPrefs.GetFloat(SwordAimMultiplier, 1);
            _swordAngleFlip = PlayerPrefs.GetInt(FlipSwordAim, 0) == 1;

            _swordAngleMultiplierSlider.onValueChanged.AddListener(HandleSwordAngleMultiplierChange);
            _swordAngleFlipToggle.onValueChanged.AddListener(HandleSwordAngleFlipChange);
        }

        private void Start()
        {
            _swordAngleMultiplierSlider.value = _swordAngleMultiplier;
            _swordAngleFlipToggle.isOn = _swordAngleFlip;

            UpdateSwordAngleMultiplier();
        }

        private void OnDestroy()
        {
            _swordAngleMultiplierSlider.onValueChanged.RemoveListener(HandleSwordAngleMultiplierChange);
            _swordAngleFlipToggle.onValueChanged.RemoveListener(HandleSwordAngleFlipChange);
        }

        private void HandleSwordAngleMultiplierChange(float value)
        {
            _swordAngleMultiplier = value;
            PlayerPrefs.SetFloat(SwordAimMultiplier, _swordAngleMultiplier);
            UpdateSwordAngleMultiplier();
        }

        private void HandleSwordAngleFlipChange(bool value)
        {
            _swordAngleFlip = value;
            PlayerPrefs.SetInt(FlipSwordAim, _swordAngleFlip ? 1 : 0);
            UpdateSwordAngleMultiplier();
        }

        private void UpdateSwordAngleMultiplier()
        {
            _swordAngleMultiplierChangeEvent.Raise(_swordAngleMultiplier * (_swordAngleFlip ? -1 : 1));
        }
    }
}
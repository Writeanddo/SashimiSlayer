using Events;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Options
{
    public class OptionsMenu : MonoBehaviour
    {
        [Header("Depend")]

        [SerializeField]
        private CanvasGroup _canvasGroup;

        [Header("Events (Out)")]

        [SerializeField]
        private FloatEvent _swordAngleMultiplierChangeEvent;

        [SerializeField]
        private BoolEvent _menuToggleEvent;

        [Header("Sword Angle Multiplier")]

        [SerializeField]
        private Slider _swordAngleMultiplierSlider;

        [Header("Sword Angle Flip")]

        [SerializeField]
        private Toggle _swordAngleFlipToggle;

        private float _volume;
        private float _swordAngleMultiplier;
        private bool _swordAngleFlip;

        private bool _menuOpen;

        private void Awake()
        {
            ToggleMenu(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenu(!_menuOpen);
                _menuToggleEvent.Raise(_menuOpen);
            }
        }

        public void ToggleMenu(bool state)
        {
            _canvasGroup.alpha = state ? 1 : 0;
            _canvasGroup.interactable = state;
            _canvasGroup.blocksRaycasts = state;
            _menuOpen = state;
        }

        private void UpdateSwordAngleMultiplier()
        {
            _swordAngleMultiplierChangeEvent.Raise(_swordAngleMultiplier * (_swordAngleFlip ? -1 : 1));
        }
    }
}
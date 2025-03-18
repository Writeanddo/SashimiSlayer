using Events;
using UnityEngine;

namespace Menus.Options
{
    public class OptionsMenu : MonoBehaviour
    {
        [Header("Depend")]

        [SerializeField]
        private CanvasGroup _canvasGroup;

        [Header("Events (Out)")]

        [SerializeField]
        private BoolEvent _menuToggleEvent;

        [Header("Events (In)")]

        [SerializeField]
        private BoolEvent _setUseHardwareController;

        private bool _menuOpen;
        private bool _usingHardwareController;

        private void Awake()
        {
            ToggleMenu(false);

            _setUseHardwareController.AddListener(OnSetUseHardwareController);
        }

        private void Update()
        {
            bool rmbQuickOpen = Input.GetMouseButtonDown(1) && _usingHardwareController;
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab) || rmbQuickOpen)
            {
                ToggleMenu(!_menuOpen);
                _menuToggleEvent.Raise(_menuOpen);
            }
        }

        private void OnDestroy()
        {
            _setUseHardwareController.RemoveListener(OnSetUseHardwareController);
        }

        private void OnSetUseHardwareController(bool state)
        {
            _usingHardwareController = state;
        }

        public void ToggleMenu(bool state)
        {
            _canvasGroup.SetEnabled(state);
            _menuOpen = state;
        }
    }
}
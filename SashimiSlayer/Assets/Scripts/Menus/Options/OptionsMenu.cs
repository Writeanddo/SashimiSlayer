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
            _canvasGroup.SetEnabled(state);
            _menuOpen = state;
        }
    }
}
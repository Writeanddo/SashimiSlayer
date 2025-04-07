using EditorUtils.BoldHeader;
using Events;
using UnityEngine;

namespace InputScripts
{
    /// <summary>
    ///     Handles hiding/showing cursor
    /// </summary>
    public class CursorVisibilityScript : MonoBehaviour
    {
        [BoldHeader("Cursor Visibility")]
        [Header("Events (In)")]

        [SerializeField]
        private IntEvent _controlSchemeChanged;

        [SerializeField]
        private BoolEvent _optionsMenuToggled;

        private bool _isMenuOpen;
        private bool _isUsingHardwareController;

        private void Awake()
        {
            _controlSchemeChanged.AddListener(HandleControlSchemeChanged);
            _optionsMenuToggled.AddListener(HandleOptionsMenuToggled);
        }

        private void Update()
        {
            Cursor.visible = _isMenuOpen || !_isUsingHardwareController;
        }

        private void OnDestroy()
        {
            _controlSchemeChanged.RemoveListener(HandleControlSchemeChanged);
            _optionsMenuToggled.RemoveListener(HandleOptionsMenuToggled);
        }

        private void HandleControlSchemeChanged(int controlScheme)
        {
            _isUsingHardwareController =
                controlScheme is (int)ControlSchemes.CustomSword or (int)ControlSchemes.Gamepad;
        }

        private void HandleOptionsMenuToggled(bool isMenuOpen)
        {
            _isMenuOpen = isMenuOpen;
        }
    }
}
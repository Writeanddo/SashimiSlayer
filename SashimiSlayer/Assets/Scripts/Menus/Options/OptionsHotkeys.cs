using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;

namespace Menus.Options
{
    public class OptionsHotkeys : MonoBehaviour
    {
        [BoldHeader("Options Menu Hotkeys")]
        [InfoBox("Hotkeys for the options menu")]
        [Header("Depends")]

        [SerializeField]
        private InputDeviceOptionsMenu _inputDeviceOptionsMenu;

        [SerializeField]
        private AltControlOptionsMenu _altControlOptionsMenu;

        [SerializeField]
        private ResetToStartMenu _resetToStartMenu;

        [SerializeField]
        private LevelRosterSO _levelRoster;

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _resetToStartMenu.LoadLevel();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _inputDeviceOptionsMenu.ToggleInputMode();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _inputDeviceOptionsMenu.QuickConnect();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                _altControlOptionsMenu.ToggleFlipAngle();
            }

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha0))
            {
                _levelRoster.WipeHighScores();
            }
        }
    }
}
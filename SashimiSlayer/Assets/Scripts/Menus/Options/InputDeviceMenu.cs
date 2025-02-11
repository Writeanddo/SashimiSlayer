using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Options
{
    public class InputDeviceMenu : MonoBehaviour
    {
        [Header("Events (Out)")]

        [SerializeField]
        private BoolEvent _setUseHardwareController;

        [SerializeField]
        private StringEvent _connectToSerialPort;

        [Header("Events (In)")]

        [SerializeField]
        private StringEvent _serialPortConnectionStatus;

        [Header("Depends")]

        [SerializeField]
        private TMP_Dropdown _inputDeviceDropdown;

        [SerializeField]
        private TMP_Dropdown _serialPortDropdown;

        [SerializeField]
        private Button _connectButton;

        [SerializeField]
        private TMP_Text _connectionStatusText;

        private void Awake()
        {
            _inputDeviceDropdown.ClearOptions();
            _inputDeviceDropdown.AddOptions(new List<string> { "Default", "Alt-Control Sword" });
            _inputDeviceDropdown.onValueChanged.AddListener(HandleInputDeviceChanged);
            _connectButton.onClick.AddListener(HandleConnect);
            _serialPortConnectionStatus.AddListener(HandleSerialPortConnectionStatus);

            ReloadSerialPortDropdown();
        }

        private void OnDestroy()
        {
            _inputDeviceDropdown.onValueChanged.RemoveListener(HandleInputDeviceChanged);
            _connectButton.onClick.RemoveListener(HandleConnect);
            _serialPortConnectionStatus.RemoveListener(HandleSerialPortConnectionStatus);
        }

        private void HandleSerialPortConnectionStatus(string status)
        {
            _connectionStatusText.text = status;
        }

        private void HandleConnect()
        {
            string serialPort = _serialPortDropdown.options[_serialPortDropdown.value].text;
            _connectToSerialPort.Raise(serialPort);
        }

        private void HandleInputDeviceChanged(int index)
        {
            _setUseHardwareController.Raise(index == 1);
        }

        public void ReloadSerialPortDropdown()
        {
            List<string> serialPorts = SerialPort.GetPortNames().ToList();

            // Filter out ports that are not COM ports
            var comPortRegex = new Regex(@"COM\d+");
            var macPortRegex = new Regex(@"/dev/tty.*");

            serialPorts = serialPorts.Where(port =>
                comPortRegex.IsMatch(port) ||
                macPortRegex.IsMatch(port)).ToList();

            _serialPortDropdown.ClearOptions();
            _serialPortDropdown.AddOptions(serialPorts);
        }
    }
}
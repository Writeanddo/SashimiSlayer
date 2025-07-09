using System;
using System.Linq;
using Events;
using InputScripts;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ControlSchemes
{
    KeyboardMouse = 0,
    Gamepad = 1,
    CustomSword = 2
}

public class InputService : BaseUserInputProvider
{
    [Header("Event (Out)")]

    [SerializeField]
    private IntEvent _onControlSchemeChanged;

    [Header("Event (In)")]

    [SerializeField]
    private BoolEvent _onMenuToggled;

    [SerializeField]
    private BoolEvent _setUseHardwareController;

    [Header("Depends")]

    [SerializeField]
    private BaseUserInputProvider _gamepadInputProvider;

    [SerializeField]
    private SwordInputProvider _swordInputProvider;

    [SerializeField]
    private bool _useHardwareController;

    [SerializeField]
    private VoidEvent _onDrawDebugGUI;

    public static InputService Instance { get; private set; }

    private BaseUserInputProvider InputProvider => _useHardwareController ? _swordInputProvider : _gamepadInputProvider;

    public ControlSchemes ControlScheme { get; private set; }

    public override event Action<SharedTypes.BlockPoseStates> OnBlockPoseChanged;
    public override event Action<SharedTypes.SheathState> OnSheathStateChanged;

    /// <summary>
    ///     Disables input when menus are open
    /// </summary>
    private int _overlayMenus;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        EventPassthroughSub();

        _onDrawDebugGUI.AddListener(HandleDrawDebugGUI);
        _onMenuToggled.AddListener(HandleMenuToggled);
        _setUseHardwareController.AddListener(HandleSetUseHardwareController);

        InputSystem.onDeviceChange += (device, change) => { UpdateControlScheme(); };
    }

    private void OnDestroy()
    {
        EventPassthroughUnsub();

        _onDrawDebugGUI.RemoveListener(HandleDrawDebugGUI);
        _onMenuToggled.RemoveListener(HandleMenuToggled);
        _setUseHardwareController.RemoveListener(HandleSetUseHardwareController);
    }

    private void HandleSetUseHardwareController(bool useHardwareController)
    {
        SetUseHardwareController(useHardwareController);
    }

    private void SetUseHardwareController(bool useHardwareController)
    {
        Debug.Log($"Setting useHardwareController to {useHardwareController}");
        EventPassthroughUnsub();

        _useHardwareController = useHardwareController;

        UpdateControlScheme();

        EventPassthroughSub();
    }

    private void HandleMenuToggled(bool isMenuOpen)
    {
        Debug.Log("Menu toggled: " + isMenuOpen);
        _overlayMenus += isMenuOpen ? 1 : -1;
    }

    private void UpdateControlScheme()
    {
        if (_useHardwareController)
        {
            ControlScheme = ControlSchemes.CustomSword;
        }
        else
        {
            // See if gamepad is connected
            if (InputSystem.devices.Count(device => device is Gamepad) > 0)
            {
                ControlScheme = ControlSchemes.Gamepad;
            }
            else
            {
                ControlScheme = ControlSchemes.KeyboardMouse;
            }
        }

        _onControlSchemeChanged.Raise((int)ControlScheme);
    }

    private void HandleDrawDebugGUI()
    {
        GUILayout.Label($"hw control: {_useHardwareController}");
        GUILayout.Label($"control scheme: {ControlScheme}");
    }

    private void EventPassthroughSub()
    {
        InputProvider.OnBlockPoseChanged += HandleBlockPoseChanged;
        InputProvider.OnSheathStateChanged += HandleSheatheStateChanged;
    }

    private void EventPassthroughUnsub()
    {
        InputProvider.OnBlockPoseChanged -= HandleBlockPoseChanged;
        InputProvider.OnSheathStateChanged -= HandleSheatheStateChanged;
    }

    private void HandleBlockPoseChanged(SharedTypes.BlockPoseStates state)
    {
        if (_overlayMenus > 0)
        {
            return;
        }

        OnBlockPoseChanged?.Invoke(state);
    }

    private void HandleSheatheStateChanged(SharedTypes.SheathState state)
    {
        if (_overlayMenus > 0)
        {
            return;
        }

        OnSheathStateChanged?.Invoke(state);
    }

    public override float GetSwordAngle()
    {
        return InputProvider.GetSwordAngle();
    }

    public override SharedTypes.SheathState GetSheathState()
    {
        return InputProvider.GetSheathState();
    }

    public override SharedTypes.BlockPoseStates GetBlockPose()
    {
        return InputProvider.GetBlockPose();
    }
}
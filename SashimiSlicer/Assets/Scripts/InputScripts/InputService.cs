using System;
using Events;
using UnityEngine;

public class InputService : BaseUserInputProvider
{
    [SerializeField]
    private BaseUserInputProvider _gamepadInputProvider;

    [SerializeField]
    private BaseUserInputProvider _swordInputProvider;

    [SerializeField]
    private bool _useHardwareController;

    [SerializeField]
    private VoidEvent _onDrawDebugGUI;

    public static InputService Instance { get; private set; }

    public BaseUserInputProvider InputProvider => _useHardwareController ? _swordInputProvider : _gamepadInputProvider;

    public override event Action<SharedTypes.BlockPoseStates> OnBlockPoseChanged;
    public override event Action<SharedTypes.SheathState> OnSheathStateChanged;

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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            EventPassthroughUnsub();
            _useHardwareController = !_useHardwareController;
            EventPassthroughSub();
        }
    }

    private void OnDestroy()
    {
        EventPassthroughUnsub();

        _onDrawDebugGUI.RemoveListener(HandleDrawDebugGUI);
    }

    private void HandleDrawDebugGUI()
    {
        GUILayout.Label($"hw control: {_useHardwareController}");
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
        OnBlockPoseChanged?.Invoke(state);
    }

    private void HandleSheatheStateChanged(SharedTypes.SheathState state)
    {
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
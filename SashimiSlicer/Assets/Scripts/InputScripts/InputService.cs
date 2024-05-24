using System;
using UnityEngine;

public class InputService : BaseUserInputProvider
{
    [SerializeField]
    private BaseUserInputProvider _gamepadInputProvider;

    [SerializeField]
    private BaseUserInputProvider _swordInputProvider;

    public static InputService Instance { get; private set; }

    public BaseUserInputProvider InputProvider => _useHardwareController ? _swordInputProvider : _gamepadInputProvider;

    public override event Action<Gameplay.BlockPoseStates> OnBlockPoseChanged;
    public override event Action<Gameplay.SheathState> OnSheathStateChanged;

    private bool _useHardwareController = true;

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
    }

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
        {
            EventPassthroughUnsub();
            _useHardwareController = !_useHardwareController;
            EventPassthroughSub();
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(200);
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

    private void HandleBlockPoseChanged(Gameplay.BlockPoseStates state)
    {
        OnBlockPoseChanged?.Invoke(state);
    }

    private void HandleSheatheStateChanged(Gameplay.SheathState state)
    {
        OnSheathStateChanged?.Invoke(state);
    }

    public override float GetSwordAngle()
    {
        return InputProvider.GetSwordAngle();
    }

    public override Gameplay.SheathState GetSheathState()
    {
        return InputProvider.GetSheathState();
    }

    public override Gameplay.BlockPoseStates GetBlockPose()
    {
        return InputProvider.GetBlockPose();
    }
}
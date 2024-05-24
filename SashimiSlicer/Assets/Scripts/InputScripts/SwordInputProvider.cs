using System;
using UnityEngine;

public class SwordInputProvider : BaseUserInputProvider
{
    [SerializeField]
    private SerialReader _serialReader;

    [SerializeField]
    private Transform _quatDebugger;

    public override event Action<Gameplay.BlockPoseStates> OnBlockPoseChanged;
    public override event Action<Gameplay.SheathState> OnSheathStateChanged;

    private Gameplay.SheathState _sheathState;
    private Gameplay.BlockPoseStates _currentBlockPose;
    private float _swordAngle;

    private void Awake()
    {
        _serialReader.OnSerialRead += HandleSerialRead;
    }

    private void OnDestroy()
    {
        _serialReader.OnSerialRead -= HandleSerialRead;
    }

    private void HandleSerialRead(SerialReader.SerialReadResult data)
    {
        Gameplay.SheathState newSheatheState = data.LeftSheatheSwitch && data.RightSheatheSwitch
            ? Gameplay.SheathState.Unsheathed
            : Gameplay.SheathState.Sheathed;

        if (newSheatheState != _sheathState)
        {
            _sheathState = newSheatheState;
            OnSheathStateChanged?.Invoke(_sheathState);
        }

        Gameplay.BlockPoseStates newPose = 0;

        if (data.TopButton)
        {
            newPose |= Gameplay.BlockPoseStates.TopPose;
        }

        if (data.MiddleButton)
        {
            newPose |= Gameplay.BlockPoseStates.MidPose;
        }

        if (data.BottomButton)
        {
            newPose |= Gameplay.BlockPoseStates.BotPose;
        }

        if (newPose != _currentBlockPose)
        {
            _currentBlockPose = newPose;
            OnBlockPoseChanged?.Invoke(_currentBlockPose);
        }

        Vector3 up = data.SwordOrientation * Vector3.forward;
        Vector3 proj = Vector3.ProjectOnPlane(up, Vector3.forward);
        float angle = Mathf.Atan2(proj.y, proj.x) * Mathf.Rad2Deg;

        _swordAngle = angle;
        _quatDebugger.transform.rotation = data.SwordOrientation;
    }

    public override float GetSwordAngle()
    {
        return _swordAngle;
    }

    public override Gameplay.SheathState GetSheathState()
    {
        return _sheathState;
    }

    public override Gameplay.BlockPoseStates GetBlockPose()
    {
        return _currentBlockPose;
    }
}
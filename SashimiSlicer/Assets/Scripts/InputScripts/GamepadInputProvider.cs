using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadInputProvider : BaseUserInputProvider, GameplayControls.IGameplayActions
{
    public override event Action<SharedTypes.BlockPoseStates> OnBlockPoseChanged;
    public override event Action<SharedTypes.SheathState> OnSheathStateChanged;

    private GameplayControls _gameplayControls;

    private Vector2 _mousePos;
    private SharedTypes.BlockPoseStates _blockPoseStates;
    private SharedTypes.SheathState _sheathState;
    private Vector2 _swordAngle = Vector2.zero;

    private void OnEnable()
    {
        _gameplayControls = new GameplayControls();
        _gameplayControls.Enable();
        _gameplayControls.Gameplay.SetCallbacks(this);
        _blockPoseStates = 0;
    }

    private void OnDisable()
    {
        _gameplayControls.Disable();
    }

    public void OnSwordAngle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _swordAngle = context.ReadValue<Vector2>().normalized;
        }
    }

    public void OnUnsheathe(InputAction.CallbackContext context)
    {
        bool isPressed = context.ReadValueAsButton();

        SharedTypes.SheathState newState = isPressed
            ? SharedTypes.SheathState.Unsheathed
            : SharedTypes.SheathState.Sheathed;

        if (_sheathState != newState)
        {
            _sheathState = newState;
            OnSheathStateChanged?.Invoke(_sheathState);
        }
    }

    public void OnPoseButtonTop(InputAction.CallbackContext context)
    {
        bool isPressed = context.ReadValueAsButton();
        ChangePoseFlag(SharedTypes.BlockPoseStates.TopPose, isPressed);
    }

    public void OnPoseButtonMId(InputAction.CallbackContext context)
    {
        bool isPressed = context.ReadValueAsButton();
        ChangePoseFlag(SharedTypes.BlockPoseStates.MidPose, isPressed);
    }

    public void OnPoseButtonBot(InputAction.CallbackContext context)
    {
        bool isPressed = context.ReadValueAsButton();
        ChangePoseFlag(SharedTypes.BlockPoseStates.BotPose, isPressed);
    }

    public void OnMousePos(InputAction.CallbackContext context)
    {
        var newMousePos = context.ReadValue<Vector2>();
        if (newMousePos != _mousePos && Protaganist.Instance != null && Camera.main != null)
        {
            Vector2 screenCenter = Camera.main.WorldToScreenPoint(Protaganist.Instance.SwordPosition);
            Vector2 mouseDelta = newMousePos - screenCenter;
            _swordAngle = mouseDelta.normalized;
        }

        _mousePos = newMousePos;
    }

    private void ChangePoseFlag(SharedTypes.BlockPoseStates flag, bool flagState)
    {
        SharedTypes.BlockPoseStates newBlockPoseStates = _blockPoseStates;
        if (flagState)
        {
            newBlockPoseStates |= flag;
        }
        else
        {
            newBlockPoseStates &= ~flag;
        }

        if (_blockPoseStates != newBlockPoseStates)
        {
            _blockPoseStates = newBlockPoseStates;
            OnBlockPoseChanged?.Invoke(_blockPoseStates);
        }
    }

    public override float GetSwordAngle()
    {
        return Mathf.Atan2(_swordAngle.y, _swordAngle.x) * Mathf.Rad2Deg;
    }

    public override SharedTypes.SheathState GetSheathState()
    {
        return _sheathState;
    }

    public override SharedTypes.BlockPoseStates GetBlockPose()
    {
        return _blockPoseStates;
    }
}
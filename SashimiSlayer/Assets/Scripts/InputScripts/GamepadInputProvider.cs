using System;
using Core.Protag;
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
        if (context.ReadValueAsButton())
        {
            OnBlockPoseChanged?.Invoke(SharedTypes.BlockPoseStates.TopPose);
            _blockPoseStates = SharedTypes.BlockPoseStates.TopPose;
        }
    }

    public void OnPoseButtonMid(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            OnBlockPoseChanged?.Invoke(SharedTypes.BlockPoseStates.MidPose);
            _blockPoseStates = SharedTypes.BlockPoseStates.MidPose;
        }
    }

    public void OnPoseButtonBot(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            OnBlockPoseChanged?.Invoke(SharedTypes.BlockPoseStates.BotPose);
            _blockPoseStates = SharedTypes.BlockPoseStates.BotPose;
        }
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
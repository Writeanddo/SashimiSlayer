using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class GamepadInputProvider : BaseUserInputProvider, GameplayControls.IGameplayActions
    {
        public override event Action<Gameplay.BlockPoseStates> OnBlockPoseChanged;
        public override event Action<Gameplay.SheathState> OnSheathStateChanged;

        private GameplayControls _gameplayControls;

        private Vector2 _mousePos;
        private Gameplay.BlockPoseStates _BlockPoseStates;
        private Gameplay.SheathState _sheathState;
        private Vector2 _swordAngle = Vector2.zero;

        private void OnEnable()
        {
            _gameplayControls = new GameplayControls();
            _gameplayControls.Enable();
            _gameplayControls.Gameplay.SetCallbacks(this);
            _BlockPoseStates = 0;
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

            Gameplay.SheathState newState = isPressed
                ? Gameplay.SheathState.Unsheathed
                : Gameplay.SheathState.Sheathed;

            if (_sheathState != newState)
            {
                _sheathState = newState;
                OnSheathStateChanged?.Invoke(_sheathState);
            }
        }

        public void OnPoseButtonTop(InputAction.CallbackContext context)
        {
            bool isPressed = context.ReadValueAsButton();
            ChangePoseFlag(Gameplay.BlockPoseStates.TopPose, isPressed);
        }

        public void OnPoseButtonMId(InputAction.CallbackContext context)
        {
            bool isPressed = context.ReadValueAsButton();
            ChangePoseFlag(Gameplay.BlockPoseStates.MidPose, isPressed);
        }

        public void OnPoseButtonBot(InputAction.CallbackContext context)
        {
            bool isPressed = context.ReadValueAsButton();
            ChangePoseFlag(Gameplay.BlockPoseStates.BotPose, isPressed);
        }

        public void OnMousePos(InputAction.CallbackContext context)
        {
            var newMousePos = context.ReadValue<Vector2>();
            if (newMousePos != _mousePos)
            {
                var screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
                Vector2 mouseDelta = newMousePos - screenCenter;
                _swordAngle = mouseDelta.normalized;
            }

            _mousePos = newMousePos;
        }

        private void ChangePoseFlag(Gameplay.BlockPoseStates flag, bool flagState)
        {
            Gameplay.BlockPoseStates newBlockPoseStates = _BlockPoseStates;
            if (flagState)
            {
                newBlockPoseStates |= flag;
            }
            else
            {
                newBlockPoseStates &= ~flag;
            }

            if (_BlockPoseStates != newBlockPoseStates)
            {
                _BlockPoseStates = newBlockPoseStates;
                OnBlockPoseChanged?.Invoke(_BlockPoseStates);
            }
        }

        public override float GetSwordAngle()
        {
            return Mathf.Atan2(_swordAngle.y, _swordAngle.x) * Mathf.Rad2Deg;
        }

        public override Gameplay.SheathState GetSheathState()
        {
            return _sheathState;
        }

        public override Gameplay.BlockPoseStates GetBlockPose()
        {
            return _BlockPoseStates;
        }
    }
}
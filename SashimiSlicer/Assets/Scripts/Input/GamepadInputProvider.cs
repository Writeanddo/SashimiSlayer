using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class GamepadInputProvider : BaseUserInputProvider, GameplayControls.IGameplayActions
    {
        public override event Action<PoseState> OnPoseStateChanged;
        public override event Action<SheathState> OnSheathStateChanged;

        private GameplayControls _gameplayControls;

        private Vector2 _mousePos;
        private PoseState _poseState;
        private SheathState _sheathState;
        private Vector2 _swordAngle = Vector2.zero;

        private void OnEnable()
        {
            _gameplayControls = new GameplayControls();
            _gameplayControls.Enable();
            _gameplayControls.Gameplay.SetCallbacks(this);
            _poseState = 0;
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

            SheathState newState = isPressed
                ? SheathState.Unsheathed
                : SheathState.Sheathed;

            if (_sheathState != newState)
            {
                _sheathState = newState;
                OnSheathStateChanged?.Invoke(_sheathState);
            }
        }

        public void OnPoseButtonTop(InputAction.CallbackContext context)
        {
            bool isPressed = context.ReadValueAsButton();
            ChangePoseFlag(PoseState.TopPose, isPressed);
        }

        public void OnPoseButtonMId(InputAction.CallbackContext context)
        {
            bool isPressed = context.ReadValueAsButton();
            ChangePoseFlag(PoseState.MidPose, isPressed);
        }

        public void OnPoseButtonBot(InputAction.CallbackContext context)
        {
            bool isPressed = context.ReadValueAsButton();
            ChangePoseFlag(PoseState.BotPose, isPressed);
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

        private void ChangePoseFlag(PoseState flag, bool flagState)
        {
            PoseState newPoseState = _poseState;
            if (flagState)
            {
                newPoseState |= flag;
            }
            else
            {
                newPoseState &= ~flag;
            }

            if (_poseState != newPoseState)
            {
                _poseState = newPoseState;
                OnPoseStateChanged?.Invoke(_poseState);
            }
        }

        public override float GetSwordAngle()
        {
            return Mathf.Atan2(_swordAngle.y, _swordAngle.x) * Mathf.Rad2Deg;
        }

        public override SheathState GetSheathState()
        {
            return _sheathState;
        }

        public override PoseState GetPoseState()
        {
            return _poseState;
        }
    }
}
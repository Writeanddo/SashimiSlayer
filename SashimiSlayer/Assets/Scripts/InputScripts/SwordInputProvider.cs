using System;
using Events;
using UnityEngine;

namespace InputScripts
{
    public class SwordInputProvider : BaseUserInputProvider
    {
        [Header("Depends")]

        [SerializeField]
        private SerialReader _serialReader;

        [SerializeField]
        private Transform _quatDebugger;

        [Header("Events (In)")]

        [SerializeField]
        private FloatEvent _angleMultiplierEvent;

        public override event Action<SharedTypes.BlockPoseStates> OnBlockPoseChanged;
        public override event Action<SharedTypes.SheathState> OnSheathStateChanged;

        private SharedTypes.SheathState _sheathState = SharedTypes.SheathState.Sheathed;
        private SharedTypes.BlockPoseStates _currentBlockPose;
        private float _swordAngle = 90f;

        private float _angleMultiplier = 1f;

        private void Awake()
        {
            _serialReader.OnSerialRead += HandleSerialRead;
            _angleMultiplierEvent.AddListener(SetAngleMultiplier);
        }

        private void OnDestroy()
        {
            _serialReader.OnSerialRead -= HandleSerialRead;
            _angleMultiplierEvent.RemoveListener(SetAngleMultiplier);
        }

        private void SetAngleMultiplier(float angleMultiplier)
        {
            Debug.Log($"Setting angle multiplier to {angleMultiplier}");
            _angleMultiplier = angleMultiplier;
        }

        private void HandleSerialRead(SerialReader.SerialReadResult data)
        {
            SharedTypes.SheathState newSheatheState = data.LeftSheatheSwitch && data.RightSheatheSwitch
                ? SharedTypes.SheathState.Unsheathed
                : SharedTypes.SheathState.Sheathed;

            if (newSheatheState != _sheathState)
            {
                _sheathState = newSheatheState;
                OnSheathStateChanged?.Invoke(_sheathState);
            }

            SharedTypes.BlockPoseStates newPose = 0;

            if (data.TopButton)
            {
                newPose = SharedTypes.BlockPoseStates.TopPose;
            }
            else if (data.MiddleButton)
            {
                newPose = SharedTypes.BlockPoseStates.MidPose;
            }
            else if (data.BottomButton)
            {
                newPose = SharedTypes.BlockPoseStates.BotPose;
            }
            else
            {
                newPose = (SharedTypes.BlockPoseStates)(-1);
            }

            if (newPose != _currentBlockPose)
            {
                _currentBlockPose = newPose;
                if (newPose != (SharedTypes.BlockPoseStates)(-1))
                {
                    OnBlockPoseChanged?.Invoke(_currentBlockPose);
                }
            }

            Vector3 up = data.SwordOrientation * Vector3.forward;
            float angle = -Vector3.Angle(up, Vector3.up) + 90f;

            _swordAngle = angle * _angleMultiplier;
            _quatDebugger.transform.rotation = data.SwordOrientation;
        }

        public override float GetSwordAngle()
        {
            return _swordAngle;
        }

        public override SharedTypes.SheathState GetSheathState()
        {
            return _sheathState;
        }

        public override SharedTypes.BlockPoseStates GetBlockPose()
        {
            return _currentBlockPose;
        }

        public void ConnectToPort()
        {
            _serialReader.TryConnectToPort();
        }
    }
}
using System;
using UnityEngine;

namespace Input
{
    public abstract class BaseUserInputProvider : MonoBehaviour
    {
        [Flags]
        public enum PoseState
        {
            TopPose = 1 << 0,
            MidPose = 1 << 1,
            BotPose = 1 << 2
        }

        public enum SheathState
        {
            Sheathed,
            Unsheathed
        }

        public abstract event Action<PoseState> OnPoseStateChanged;

        public abstract event Action<SheathState> OnSheathStateChanged;

        /// <summary>
        ///     Get the sword angle in degrees
        /// </summary>
        /// <returns></returns>
        public abstract float GetSwordAngle();

        public abstract SheathState GetSheathState();

        public abstract PoseState GetPoseState();
    }
}
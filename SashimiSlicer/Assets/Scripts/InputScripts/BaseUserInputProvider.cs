using System;
using UnityEngine;

public abstract class BaseUserInputProvider : MonoBehaviour
{
    public abstract event Action<Gameplay.BlockPoseStates> OnBlockPoseChanged;

    public abstract event Action<Gameplay.SheathState> OnSheathStateChanged;

    /// <summary>
    ///     Get the sword angle in degrees
    /// </summary>
    /// <returns></returns>
    public abstract float GetSwordAngle();

    public abstract Gameplay.SheathState GetSheathState();

    public abstract Gameplay.BlockPoseStates GetBlockPose();
}
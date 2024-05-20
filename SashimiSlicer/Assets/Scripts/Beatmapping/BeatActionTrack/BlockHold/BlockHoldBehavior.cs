using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class BlockHoldBehavior : PlayableBehaviour
{
    [ReadOnly]
    [TextArea]
    public string description = "Block Hold Behavior";

    [Tooltip("This is a property")]
    public Color color = Color.white;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
    }
}
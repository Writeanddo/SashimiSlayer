using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

[Serializable]
public class BlockHoldBehavior : PlayableBehaviour
{
    [FormerlySerializedAs("description")]
    [ReadOnly]
    [TextArea]
    public string Description = "Block Hold Behavior";

    [FormerlySerializedAs("color")]
    [Tooltip("This is a property")]
    public Color Color = Color.white;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
    }
}
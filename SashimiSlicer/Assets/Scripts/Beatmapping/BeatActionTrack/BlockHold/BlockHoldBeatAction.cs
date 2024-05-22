using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

[Serializable]
[DisplayName("Beat Action/Block Hold")]
public class BlockHoldClip : BeatActionClip
{
    [FormerlySerializedAs("template")]
    public BlockHoldBehavior Template = new();

    // Creates the playable that represents the instance of this clip.
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // Using a template will clone the serialized values
        return ScriptPlayable<BlockHoldBehavior>.Create(graph, Template);
    }
}
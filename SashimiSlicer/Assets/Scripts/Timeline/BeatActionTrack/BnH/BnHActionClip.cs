using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
[DisplayName("Beat Action/BnH")]
public class BnHActionClip : BeatActionClip
{
    public BnHActionBehavior template = new();

    // Creates the playable that represents the instance of this clip.
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // Using a template will clone the serialized values
        return ScriptPlayable<BnHActionBehavior>.Create(graph, template);
    }
}
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace Timeline.BeatNoteTrack.BeatNote
{
    [Serializable]
    [DisplayName("Beat Note")]
    public class BeatNoteClip : PlayableAsset, ITimelineClipAsset
    {
        [FormerlySerializedAs("template")]
        public BeatNoteBehavior Template = new();

        // Implementation of ITimelineClipAsset. This specifies the capabilities of this timeline clip inside the editor.
        public ClipCaps clipCaps => ClipCaps.None;

        // Creates the playable that represents the instance of this clip.
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            // Using a template will clone the serialized values
            return ScriptPlayable<BeatNoteBehavior>.Create(graph, Template);
        }
    }
}
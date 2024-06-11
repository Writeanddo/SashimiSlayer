using UnityEngine.Playables;
using UnityEngine.Timeline;

public abstract class BeatActionClip : PlayableAsset, ITimelineClipAsset
{
    // Implementation of ITimelineClipAsset. This specifies the capabilities of this timeline clip inside the editor.
    public ClipCaps clipCaps => ClipCaps.None;
}
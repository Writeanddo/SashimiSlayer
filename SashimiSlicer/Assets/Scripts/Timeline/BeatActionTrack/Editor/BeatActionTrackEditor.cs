using UnityEditor.Timeline;
using UnityEngine.Timeline;

[CustomTimelineEditor(typeof(BeatActionTrack))]
public class BeatActionTrackEditor : TrackEditor
{
    public override void OnCreate(TrackAsset track, TrackAsset copiedFrom)
    {
        var timelineAsset = track.timelineAsset;
        base.OnCreate(track, copiedFrom);
    }
}
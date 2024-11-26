using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace Timeline.BeatNoteTrack.Editor
{
    [CustomTimelineEditor(typeof(BeatNoteTrack))]
    public class BeatNoteTrackEditor : TrackEditor
    {
        public override void OnCreate(TrackAsset track, TrackAsset copiedFrom)
        {
            TimelineAsset timelineAsset = track.timelineAsset;
            base.OnCreate(track, copiedFrom);
        }
    }
}
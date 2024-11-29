using System.ComponentModel;
using FMODUnity;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.FMODEditorEventTrack
{
    /// <summary>
    ///     Copy of the FMODEventTrack class from the FMOD Unity Integration package, but only plays in Editor when
    ///     beatmapping.
    ///     Soundtrack events during runtime are run independently of timeline.
    /// </summary>
    [TrackColor(0.066f, 0.134f, 0.244f)]
    [TrackClipType(typeof(FMODEventPlayable))]
    [TrackBindingType(typeof(GameObject))]
    [DisplayName("FMOD/Event Editor Track")]
    public class FMODBeatmappingEventTrack : TrackAsset
    {
        public FMODEventMixerBehaviour template = new();

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var director = go.GetComponent<PlayableDirector>();
            var trackTargetObject = director.GetGenericBinding(this) as GameObject;

            foreach (TimelineClip clip in GetClips())
            {
                var playableAsset = clip.asset as FMODEventPlayable;

                if (playableAsset)
                {
                    playableAsset.TrackTargetObject = trackTargetObject;
                    playableAsset.OwningClip = clip;
                }
            }

            ScriptPlayable<FMODEventMixerBehaviour> scriptPlayable =
                ScriptPlayable<FMODEventMixerBehaviour>.Create(graph, template, inputCount);

            if (Application.isPlaying)
            {
                return ScriptPlayable<DummyPlayableBehaviour>.Create(graph, inputCount);
            }

            return scriptPlayable;
        }
    }

    public class DummyPlayableBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
        }
    }
}
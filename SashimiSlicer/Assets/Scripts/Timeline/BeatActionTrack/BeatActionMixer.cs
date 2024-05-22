using UnityEngine.Playables;

// The runtime instance of a the TextTrack. It is responsible for blending and setting the final data
// on the Text binding
public class BeatActionMixer : PlayableBehaviour
{
    private BeatActionManager _mTrackBinding;

    // Called every frame that the timeline is evaluated. ProcessFrame is invoked after its' inputs.
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        _mTrackBinding = playerData as BeatActionManager;
        if (_mTrackBinding == null)
        {
        }

        /*int inputCount = playable.GetInputCount();
        for (var i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            var inputPlayable = (ScriptPlayable<BeatActionBehavior>)playable.GetInput(i);
            BeatActionBehavior input = inputPlayable.GetBehaviour();
        }*/
    }
}
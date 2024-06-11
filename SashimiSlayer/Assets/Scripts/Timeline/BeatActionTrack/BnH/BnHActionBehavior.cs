using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class BnHActionBehavior : PlayableBehaviour
{
    [ReadOnly]
    [TextArea]
    public string Description = "Simple Hit Behavior";

    [Tooltip("This is a property")]
    public BnHActionSo HitConfig;

    public BnHActionCore.BnHActionInstanceConfig ActionData;

    private BeatActionService _beatActionService;

    private BnHActionCore _blockAndHit;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (_beatActionService == null)
        {
            _beatActionService = playerData as BeatActionService;

            if (_beatActionService == null)
            {
                return;
            }
        }

        if (_blockAndHit == null)
        {
            _blockAndHit = _beatActionService.SpawnSimpleHit(HitConfig, ActionData);
        }

        Debug.DrawLine(ActionData.Positions[0], ActionData.Positions[0] + Vector2.up * 10, Color.red);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        Cleanup();
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (_blockAndHit == null)
        {
            return;
        }

        _beatActionService.CleanupBnHHit(_blockAndHit);
        _blockAndHit = null;
    }
}
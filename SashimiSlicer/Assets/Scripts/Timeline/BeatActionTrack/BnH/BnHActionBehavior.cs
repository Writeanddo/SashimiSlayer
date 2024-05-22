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

    private BeatActionManager _beatActionManager;

    private BnHActionCore _blockAndHit;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (_beatActionManager == null)
        {
            _beatActionManager = playerData as BeatActionManager;

            if (_beatActionManager == null)
            {
                return;
            }
        }

        if (_blockAndHit == null)
        {
            _blockAndHit = _beatActionManager.SpawnSimpleHit(HitConfig, ActionData);
        }

        Debug.DrawLine(ActionData.Position, ActionData.Position + Vector2.up * 10, Color.red);
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

        _beatActionManager.CleanupBnHHit(_blockAndHit);
        _blockAndHit = null;
    }
}
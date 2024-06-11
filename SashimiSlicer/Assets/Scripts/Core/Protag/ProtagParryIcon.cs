using Events.Core;
using UnityEngine;

/// <summary>
///     Handles the parry icon animation when the protag attempts a parry
/// </summary>
public class ProtagParryIcon : MonoBehaviour
{
    [Header("Listening Events")]

    [SerializeField]
    private ProtagSwordStateEvent _tryBlockEvent;

    [SerializeField]
    private ProtagSwordStateEvent _successfulBlockEvent;

    [Header("Visuals")]

    [SerializeField]
    private SimpleAnimator[] _animators;

    private void Awake()
    {
        _tryBlockEvent.AddListener(HandleTryBlock);
        _successfulBlockEvent.AddListener(HandleSuccessBlock);
    }

    private void OnDestroy()
    {
        _tryBlockEvent.RemoveListener(HandleTryBlock);
        _successfulBlockEvent.RemoveListener(HandleSuccessBlock);
    }

    private void HandleSuccessBlock(Protaganist.ProtagSwordState swordState)
    {
        for (var i = 0; i < _animators.Length; i++)
        {
            _animators[i].Stop();
        }
    }

    private void HandleTryBlock(Protaganist.ProtagSwordState swordState)
    {
        var state = (int)swordState.BlockPose;

        for (var i = 0; i < _animators.Length; i++)
        {
            bool isIncluded = state.IsIndexInFlag(i);
            if (isIncluded)
            {
                _animators[i].Play(true);
            }
        }
    }
}
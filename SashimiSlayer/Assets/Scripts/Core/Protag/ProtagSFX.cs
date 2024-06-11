using Events.Core;
using UnityEngine;

public class ProtagSFX : MonoBehaviour
{
    [Header("Listening Events")]

    [SerializeField]
    private ProtagSwordStateEvent _tryBlockEvent;

    [SerializeField]
    private ProtagSwordStateEvent _successfulBlockEvent;

    [Header("SFX")]

    [SerializeField]
    private AudioClip[] _successfulBlockSfx;

    [SerializeField]
    private AudioClip[] _tryBlockSfx;

    private void Awake()
    {
        _tryBlockEvent.AddListener(HandleTryBlock);
        _successfulBlockEvent.AddListener(HandleSuccessfulBlock);
    }

    private void OnDestroy()
    {
        _tryBlockEvent.RemoveListener(HandleTryBlock);
        _successfulBlockEvent.RemoveListener(HandleSuccessfulBlock);
    }

    private void HandleTryBlock(Protaganist.ProtagSwordState swordState)
    {
        BlockPoseSfx(_tryBlockSfx, swordState.BlockPose);
    }

    private void HandleSuccessfulBlock(Protaganist.ProtagSwordState swordState)
    {
        BlockPoseSfx(_successfulBlockSfx, swordState.BlockPose);
    }

    private void BlockPoseSfx(AudioClip[] clips, SharedTypes.BlockPoseStates blockPose)
    {
        if ((blockPose | SharedTypes.BlockPoseStates.BotPose) == blockPose)
        {
            SFXPlayer.Instance.PlaySFX(clips[2]);
        }

        if ((blockPose | SharedTypes.BlockPoseStates.MidPose) == blockPose)
        {
            SFXPlayer.Instance.PlaySFX(clips[1]);
        }

        if ((blockPose | SharedTypes.BlockPoseStates.TopPose) == blockPose)
        {
            SFXPlayer.Instance.PlaySFX(clips[0]);
        }
    }
}
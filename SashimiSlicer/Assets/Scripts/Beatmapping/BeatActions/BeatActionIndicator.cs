using UnityEngine;

public class BeatActionIndicator : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer[] _blockPoseSprites;

    [SerializeField]
    private GameObject[] _visuals;

    [SerializeField]
    private AnimationClip _attackClip;

    [SerializeField]
    private AnimationClip _vulnClip;

    [SerializeField]
    private SimpleAnimator _animator;

    public void TickWaitingForAttack(float normalizedTime, SharedTypes.BlockPoseStates blockPose)
    {
        _animator.Play(_attackClip);
        _animator.SetNormalizedTime(normalizedTime);
        SetBlockPoseIndicator(blockPose);
    }

    public void UpdateWaitingForVulnerable(float normalizedTime)
    {
        _animator.Play(_vulnClip);
        _animator.SetNormalizedTime(normalizedTime);
    }

    public void SetVisible(bool val)
    {
        if (!val)
        {
            _animator.Destroy();
        }
    }

    private void SetBlockPoseIndicator(SharedTypes.BlockPoseStates blockBlockPose)
    {
        var check = 1;
        for (var i = 0; i < _blockPoseSprites.Length; i++)
        {
            _blockPoseSprites[i].enabled = (check & (int)blockBlockPose) != 0;
            check <<= 1;
        }
    }
}
using UnityEngine;

public class BeatActionIndicator : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer[] _blockPoseSprites;

    [SerializeField]
    private SpriteRenderer _attackTargetRing;

    [SerializeField]
    private SpriteRenderer _attackShrinkRing;

    [SerializeField]
    private SpriteRenderer _vulnerableTargetRing;

    [SerializeField]
    private SpriteRenderer _vulnerableShrinkRing;

    [SerializeField]
    private float _shrinkScale;

    public void TickWaitingForAttack(float normalizedTime, SharedTypes.BlockPoseStates blockPose)
    {
        _attackShrinkRing.transform.localScale = Vector3.one * Mathf.Lerp(_shrinkScale, 1, normalizedTime);
        SetBlockPoseIndicator(blockPose);
        SetAttackPhaseIndicatorVisible(true);
        SetVulnerablePhaseIndicatorVisible(false);
    }

    public void UpdateWaitingForVulnerable(float normalizedTime)
    {
        _vulnerableShrinkRing.transform.localScale = Vector3.one * Mathf.Lerp(_shrinkScale, 1, normalizedTime);
        SetVulnerablePhaseIndicatorVisible(true);
        SetAttackPhaseIndicatorVisible(false);
    }

    public void SetVisible(bool val)
    {
        SetVulnerablePhaseIndicatorVisible(val);
        SetAttackPhaseIndicatorVisible(val);
    }

    private void SetAttackPhaseIndicatorVisible(bool val)
    {
        for (var i = 0; i < _blockPoseSprites.Length; i++)
        {
            _blockPoseSprites[i].gameObject.SetActive(val);
        }

        _attackTargetRing.enabled = val;
        _attackShrinkRing.enabled = val;
    }

    private void SetVulnerablePhaseIndicatorVisible(bool val)
    {
        _vulnerableTargetRing.enabled = val;
        _vulnerableShrinkRing.enabled = val;
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
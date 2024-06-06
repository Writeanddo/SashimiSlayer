using DG.Tweening;
using Events.Core;
using UnityEngine;

public class BeatActionIndicator : MonoBehaviour
{
    [SerializeField]
    private ProtagSwordStateEvent _protagSwordStateEvent;

    [SerializeField]
    private SpriteRenderer[] _blockPoseSprites;

    [SerializeField]
    private float _poseBurstDuration;

    [SerializeField]
    private float _poseBurstScale;

    [SerializeField]
    private SpriteRenderer[] _blockPoseBurstSprites;

    [SerializeField]
    private GameObject[] _vulnerableRotate;

    [SerializeField]
    private AnimationClip _attackClip;

    [SerializeField]
    private AnimationClip _vulnClip;

    [SerializeField]
    private SimpleAnimator _animator;

    private void Awake()
    {
        _protagSwordStateEvent.AddListener(OnProtagSwordState);
    }

    private void OnDestroy()
    {
        _protagSwordStateEvent.RemoveListener(OnProtagSwordState);
    }

    private void OnProtagSwordState(Protaganist.ProtagSwordState state)
    {
        foreach (GameObject vulnIndicator in _vulnerableRotate)
        {
            vulnIndicator.transform.rotation = Quaternion.AngleAxis(state.SwordAngle, Vector3.forward);
        }
    }

    public void TickWaitingForAttack(float normalizedTime, SharedTypes.BlockPoseStates blockPose)
    {
        _animator.Play(_attackClip);
        _animator.SetNormalizedTime(normalizedTime);
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
            _animator.Stop();
        }
    }

    public void SetBlockPoseIndicator(SharedTypes.BlockPoseStates blockBlockPose)
    {
        var check = 1;
        for (var i = 0; i < _blockPoseSprites.Length; i++)
        {
            bool includesPose = (check & (int)blockBlockPose) != 0;
            _blockPoseSprites[i].enabled = includesPose;
            check <<= 1;

            SpriteRenderer burstSprite = _blockPoseBurstSprites[i];

            if (includesPose)
            {
                burstSprite.enabled = true;
                burstSprite.transform.localScale = Vector3.one;
                burstSprite.color = new Color(1, 1, 1, 1);
                burstSprite.DOFade(0, _poseBurstDuration);
                burstSprite.transform.DOScale(_poseBurstScale, _poseBurstDuration);
            }
            else
            {
                burstSprite.enabled = false;
            }
        }
    }
}
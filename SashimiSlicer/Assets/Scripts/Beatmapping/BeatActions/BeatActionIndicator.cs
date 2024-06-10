using DG.Tweening;
using Events.Core;
using UnityEngine;

public class BeatActionIndicator : MonoBehaviour
{
    [Header("Events Listening")]

    [SerializeField]
    private ProtagSwordStateEvent _protagSwordStateEvent;

    [Header("Visuals")]

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

    private BnHActionCore _bnHActionCore;

    private void Awake()
    {
        _bnHActionCore = GetComponentInParent<BnHActionCore>();

        _protagSwordStateEvent.AddListener(OnProtagSwordState);
        _bnHActionCore.OnKilled += HandleKilled;
        _bnHActionCore.OnTickWaitingForVulnerable += HandleTickWaitingForVuln;
        _bnHActionCore.OnTickWaitingForAttack += HandleTickWaitingForAttack;
        _bnHActionCore.OnTransitionToWaitingToAttack += HandleTransitionToWaitingToAttack;
    }

    private void OnDestroy()
    {
        _protagSwordStateEvent.RemoveListener(OnProtagSwordState);
        _bnHActionCore.OnKilled -= HandleKilled;
        _bnHActionCore.OnTickWaitingForVulnerable -= HandleTickWaitingForVuln;
        _bnHActionCore.OnTickWaitingForAttack -= HandleTickWaitingForAttack;
        _bnHActionCore.OnTransitionToWaitingToAttack -= HandleTransitionToWaitingToAttack;
    }

    private void OnProtagSwordState(Protaganist.ProtagSwordState state)
    {
        foreach (GameObject vulnIndicator in _vulnerableRotate)
        {
            vulnIndicator.transform.rotation = Quaternion.AngleAxis(state.SwordAngle, Vector3.forward);
        }
    }

    private void HandleTickWaitingForAttack(BnHActionCore.Timing timing,
        BnHActionCore.ScheduledInteraction scheduledInteraction)
    {
        _animator.Play(_attackClip);
        _animator.SetNormalizedTime((float)timing.NormalizedInteractionWaitTime);
    }

    private void HandleTickWaitingForVuln(BnHActionCore.Timing timing,
        BnHActionCore.ScheduledInteraction scheduledInteraction)
    {
        _animator.Play(_vulnClip);
        _animator.SetNormalizedTime((float)timing.NormalizedInteractionWaitTime);
    }

    private void HandleKilled(BnHActionCore.Timing timing)
    {
        _animator.Stop();
    }

    private void HandleTransitionToWaitingToAttack(BnHActionCore.Timing timing,
        BnHActionCore.ScheduledInteraction interaction)
    {
        var blockBlockPose = (int)interaction.Interaction.BlockPose;
        var check = 1;
        for (var i = 0; i < _blockPoseSprites.Length; i++)
        {
            bool includesPose = (check & blockBlockPose) != 0;
            _blockPoseSprites[i].gameObject.SetActive(includesPose);
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
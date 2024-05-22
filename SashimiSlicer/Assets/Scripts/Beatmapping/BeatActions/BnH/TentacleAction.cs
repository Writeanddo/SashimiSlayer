using UnityEngine;

public class TentacleAction : MonoBehaviour
{
    [SerializeField]
    private BnHActionCore _bnhActionCore;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private float _attackAnimationWindup;

    [SerializeField]
    private ParticleSystem _damagedParticles;

    [Header("SFX")]

    [SerializeField]
    private AudioClip _landHitSfx;

    private BnHActionSo ActionConfigSo => _bnhActionCore.ActionConfigSo;

    private void Awake()
    {
        _bnhActionCore.OnSpawn += HandleSpawned;
        _bnhActionCore.OnDamagedByProtag += HandleOnDamaged;
        _bnhActionCore.OnTickWaitingForInteraction += HandleTickWaitingForInteraction;
        _bnhActionCore.OnKilled += HandleKilled;
        _bnhActionCore.OnTransitionToLeaving += HandleTransitionToLeaving;
        _bnhActionCore.OnLandHitOnProtag += HandleLandHitOnProtag;
    }

    private void OnDestroy()
    {
        _bnhActionCore.OnSpawn -= HandleSpawned;
        _bnhActionCore.OnDamagedByProtag -= HandleOnDamaged;
        _bnhActionCore.OnTickWaitingForInteraction -= HandleTickWaitingForInteraction;
        _bnhActionCore.OnKilled -= HandleKilled;
        _bnhActionCore.OnTransitionToLeaving -= HandleTransitionToLeaving;
        _bnhActionCore.OnLandHitOnProtag -= HandleLandHitOnProtag;
    }

    private void HandleLandHitOnProtag()
    {
        AudioSource.PlayClipAtPoint(_landHitSfx, Vector3.zero, 1f);
    }

    private void HandleTransitionToLeaving()
    {
        _animator.Play("Leave");
    }

    private void HandleKilled()
    {
        _animator.Play("Die");
    }

    private void HandleTickWaitingForInteraction(double time, BnHActionCore.ScheduledInteraction interaction)
    {
        if (interaction.Interaction.InteractionType == BnHActionCore.InteractionType.IncomingAttack)
        {
            double attackStartTime =
                interaction.TimeWhenInteractionStart;
            double attackMiddleTime = attackStartTime + ActionConfigSo.BlockWindowHalfDuration;

            if (time + _attackAnimationWindup >= attackMiddleTime)
            {
                _animator.Play("Attack");
            }
        }
    }

    private void HandleOnDamaged()
    {
        _damagedParticles.Play();
    }

    private void HandleSpawned()
    {
        _animator.Play("Spawn");
    }
}
using UnityEngine;

public class TentacleAction : MonoBehaviour
{
    [SerializeField]
    private BnHActionCore _bnhActionCore;

    [SerializeField]
    private SpriteRenderer _sprite;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private float _attackAnimationWindup;

    [SerializeField]
    private ParticleSystem[] _damagedParticles;

    private BnHActionSo ActionConfigSo => _bnhActionCore.ActionConfigSo;

    private void Awake()
    {
        _bnhActionCore.OnSpawn += HandleSpawned;
        _bnhActionCore.OnDamagedByProtag += HandleOnDamaged;

        _bnhActionCore.OnTickWaitingForAttack += HandleTickWaitingForAttack;

        _bnhActionCore.OnKilled += HandleKilled;
        _bnhActionCore.OnTransitionToLeaving += HandleTransitionToLeaving;
    }

    private void HandleTransitionToLeaving(BnHActionCore.Timing timing)
    {
        _animator.Play("TentacleLeave");
        _sprite.color = new Color(1, 1, 1, 0.7f);
    }

    private void HandleKilled(BnHActionCore.Timing timing)
    {
        _animator.gameObject.SetActive(false);
    }

    private void HandleTickWaitingForAttack(BnHActionCore.Timing timing, BnHActionCore.ScheduledInteraction interaction)
    {
        double attackStartTime =
            interaction.TimeWhenInteractionStart;
        double attackMiddleTime = attackStartTime + ActionConfigSo.BlockWindowHalfDuration;

        if (timing.CurrentBeatmapTime + _attackAnimationWindup >= attackMiddleTime)
        {
            _animator.Play("TentacleAttack");
        }
    }

    private void HandleOnDamaged()
    {
        foreach (ParticleSystem particle in _damagedParticles)
        {
            particle.Play();
        }
    }

    private void HandleSpawned()
    {
        _animator.Play("TentacleSpawn");
    }
}
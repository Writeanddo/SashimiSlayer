using System;
using Input;
using UnityEngine;

/// <summary>
///     A simple entity that attacks the player, then takes a hit (or leaves)
/// </summary>
public class BaseBnHAction : MonoBehaviour
{
    public enum AttackInteraction
    {
        NoBlock,
        InstantBlock,
        HoldBlock
    }

    public enum VulnerableInteraction
    {
        NoHit,
        InstantHit
    }

    private enum State
    {
        WaitingToAttack,
        AttackBlockWindow,
        WaitingToVulnerable,
        VulnerableWindow,
        Leaving
    }

    [Serializable]
    public struct BnHActionInstance
    {
        public double _beatsUntilAttack;

        public double _beatsUntilVulnerable;

        public Vector2 _position;

        public BaseUserInputProvider.PoseState _blockPose;

        public AttackInteraction _attackInteraction;

        public VulnerableInteraction _vulnerableInteraction;
    }

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private float _attackAnimationWindup;

    [SerializeField]
    private BeatActionIndicator _indicator;

    [SerializeField]
    private ParticleSystem _killedParticles;

    private double _actionStartTime;

    private bool _blocked;

    private BnHActionInstance _data;
    private BnHActionSO _hitConfig;

    private State _state;

    private double _timeWhenAttackStart;
    private double _timeWhenAttackBlockWindowEnd;
    private double _timeWhenVulnerableStart;
    private double _timeWhenVulnerableEnd;

    private void OnDrawGizmos()
    {
        if (_hitConfig == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_indicator.transform.position, _hitConfig.HitboxRadius);
    }

    public void Setup(BnHActionSO hitConfig, BnHActionInstance data)
    {
        _hitConfig = hitConfig;
        _data = data;
        _blocked = false;

        transform.position = data._position;

        _state = State.WaitingToAttack;
        _animator.Play("Spawn");

        SetupTiming(data);

        if (Protaganist.Instance == null)
        {
            return;
        }

        Protaganist.Instance.OnBlockAction += OnPlayerAttemptBlock;
        Protaganist.Instance.OnSliceAction += OnPlayerAttemptAttack;
    }

    private void SetupTiming(BnHActionInstance data)
    {
        var timingService = TimingService.Instance;
        if (timingService == null)
        {
            return;
        }

        _actionStartTime = timingService.CurrentTime;

        _timeWhenAttackStart = _actionStartTime
                               + timingService.TimePerBeat * data._beatsUntilAttack
                               - _hitConfig.BlockWindowHalfDuration;
        _timeWhenAttackBlockWindowEnd = _timeWhenAttackStart + 2 * _hitConfig.BlockWindowHalfDuration;

        _timeWhenVulnerableStart = _actionStartTime
                                   + timingService.TimePerBeat * data._beatsUntilVulnerable
                                   - _hitConfig.VulnerableWindowHalfDuration;

        _timeWhenVulnerableEnd = _timeWhenVulnerableStart + 2 * _hitConfig.VulnerableWindowHalfDuration;

        _indicator.SetVisible(true);
    }

    public void Tick()
    {
        var timingService = TimingService.Instance;
        double time = timingService.CurrentTime;

        if (_state == State.WaitingToAttack)
        {
            TickWaitingToAttack(time);
        }
        else if (_state == State.AttackBlockWindow)
        {
            TickAttackBlockWindow(time);
        }
        else if (_state == State.WaitingToVulnerable)
        {
            TickWaitingToVulnerable(time);
        }
        else if (_state == State.VulnerableWindow)
        {
            TickVulnerableWindow(time);
        }
        else if (_state == State.Leaving)
        {
            TickLeaving(time);
        }
    }

    private void OnPlayerAttemptBlock(Protaganist.BlockPose blockPose)
    {
        if (_state != State.AttackBlockWindow)
        {
            return;
        }

        if (blockPose.pose != _data._blockPose)
        {
            return;
        }

        Vector3 pos = _indicator.transform.position;
        float dist = Protaganist.Instance.DistanceToSwordPlane(pos);
        bool isBlockOnTarget = dist < _hitConfig.HitboxRadius;

        double time = TimingService.Instance.CurrentTime;
        Debug.Log($"Block offset: ${time - (_timeWhenAttackStart + _hitConfig.BlockWindowHalfDuration)}");

        if (isBlockOnTarget)
        {
            _blocked = true;
            Protaganist.Instance.SuccessfulBlock();
            AudioSource.PlayClipAtPoint(_hitConfig.BlockSound, Vector3.zero, 1f);
        }
    }

    private void OnPlayerAttemptAttack(Protaganist.AttackPose attackPose)
    {
        if (_state != State.VulnerableWindow)
        {
            return;
        }

        Vector3 pos = _indicator.transform.position;
        float dist = Protaganist.Instance.DistanceToSwordPlane(pos);
        bool isAttackOnTarget = dist < _hitConfig.HitboxRadius;

        double time = TimingService.Instance.CurrentTime;
        Debug.Log($"Attack offset: ${time - (_timeWhenVulnerableStart + _hitConfig.VulnerableWindowHalfDuration)}");

        if (isAttackOnTarget)
        {
            _killedParticles.Play();
            BossService.Instance.TakeDamage(_hitConfig.DamageTakenToBoss);
            _indicator.SetVisible(false);
            _state = State.Leaving;
            _animator.Play("Die", 0, 0);

            Protaganist.Instance.SuccessfulSlice();

            AudioSource.PlayClipAtPoint(_hitConfig.KilledSound, Vector3.zero, 1f);
        }
    }

    private void TickWaitingToAttack(double time)
    {
        var normalizedTime = (float)((time - _actionStartTime) / (_timeWhenAttackStart - _actionStartTime));
        _indicator.TickWaitingForAttack(normalizedTime, _data._blockPose);

        if (time >= _timeWhenAttackStart)
        {
            _state = State.AttackBlockWindow;
        }

        if (time + _attackAnimationWindup >= _timeWhenAttackStart)
        {
            _animator.Play("Attack");
        }
    }

    private void TickAttackBlockWindow(double time)
    {
        if (time >= _timeWhenAttackBlockWindowEnd)
        {
            if (_blocked)
            {
            }
            else
            {
                AudioSource.PlayClipAtPoint(_hitConfig.ImpactSound, Vector3.zero, 1f);
                Protaganist.Instance.TakeDamage(_hitConfig.DamageDealtToPlayer);
            }

            if (_data._vulnerableInteraction == VulnerableInteraction.InstantHit)
            {
                _state = State.WaitingToVulnerable;
            }
            else
            {
                _indicator.SetVisible(false);
                _state = State.Leaving;
                _animator.Play("Leave");
            }
        }
    }

    private void TickWaitingToVulnerable(double time)
    {
        var normalizedTime = (float)((time - _timeWhenAttackStart) / (_timeWhenVulnerableStart - _timeWhenAttackStart));
        _indicator.TickWaitingForVulnerable(normalizedTime);

        if (time >= _timeWhenVulnerableStart)
        {
            _indicator.SetVisible(false);
            _state = State.VulnerableWindow;
            _animator.Play("Leave");
        }
    }

    private void TickVulnerableWindow(double time)
    {
        if (time >= _timeWhenVulnerableEnd)
        {
            _indicator.SetVisible(false);
            _state = State.Leaving;
        }
    }

    private void TickLeaving(double time)
    {
        if (time >= _timeWhenVulnerableEnd + 1f)
        {
            BeatActionManager.Instance.CleanupBnHHit(this);
        }
    }
}
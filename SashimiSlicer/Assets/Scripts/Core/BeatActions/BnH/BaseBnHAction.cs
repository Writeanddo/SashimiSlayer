using System;
using System.Collections.Generic;
using Input;
using UnityEngine;

/// <summary>
///     A simple entity that attacks the player, then takes a hit (or leaves)
/// </summary>
public class BaseBnHAction : MonoBehaviour
{
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
        public List<AttackInstance> _attacks;

        public double _beatsUntilVulnerable;

        public Vector2 _position;

        public VulnerableInteraction _vulnerableInteraction;
    }

    [Serializable]
    public struct AttackInstance
    {
        public double _beatsUntilAttack;
        public string _attackTag;
        public bool _hold;
        public Gameplay.BlockPoseStates _blockPose;
    }

    private struct AttackTiming
    {
        public double _timeWhenAttackStart;
        public double _timeWhenAttackBlockWindowEnd;
    }

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private float _attackAnimationWindup;

    [SerializeField]
    private BeatActionIndicator _indicator;

    [SerializeField]
    private ParticleSystem _killedParticles;

    private BnHActionInstance _data;
    private BnHActionSO _hitConfig;

    private List<AttackTiming> _attackTimings;
    private double _actionStartTime;
    private double _timeWhenVulnerableStart;
    private double _timeWhenVulnerableEnd;

    private State _state;
    private int _attackIndex;
    private bool _blocked;

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

        _attackIndex = 0;
        if (_data._attacks.Count > 0)
        {
            _state = State.WaitingToAttack;
        }
        else
        {
            _state = State.WaitingToVulnerable;
        }

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

        _attackTimings = new List<AttackTiming>();

        foreach (AttackInstance attack in data._attacks)
        {
            double timeWhenAttackStart = _actionStartTime
                                         + timingService.TimePerBeat * attack._beatsUntilAttack
                                         - _hitConfig.BlockWindowHalfDuration;
            double timeWhenAttackBlockWindowEnd = timeWhenAttackStart + 2 * _hitConfig.BlockWindowHalfDuration;
            var attackTiming = new AttackTiming
            {
                _timeWhenAttackStart = timeWhenAttackStart,
                _timeWhenAttackBlockWindowEnd = timeWhenAttackBlockWindowEnd
            };

            _attackTimings.Add(attackTiming);
        }

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

    private void OnPlayerAttemptBlock(Protaganist.BlockInstance blockInstance)
    {
        if (_state != State.AttackBlockWindow)
        {
            return;
        }

        if (blockInstance.BlockPose != _data._attacks[_attackIndex]._blockPose)
        {
            return;
        }

        Vector3 pos = _indicator.transform.position;
        float dist = Protaganist.Instance.DistanceToSwordPlane(pos);
        bool isBlockOnTarget = dist < _hitConfig.HitboxRadius;

        double time = TimingService.Instance.CurrentTime;
        AttackTiming attackTiming = _attackTimings[_attackIndex];
        Debug.Log($"Block offset: ${time - (attackTiming._timeWhenAttackStart + _hitConfig.BlockWindowHalfDuration)}");

        if (isBlockOnTarget)
        {
            _blocked = true;
            Protaganist.Instance.SuccessfulBlock();
            AudioSource.PlayClipAtPoint(_hitConfig.BlockSound, Vector3.zero, 1f);
        }
    }

    private void OnPlayerAttemptAttack(Protaganist.AttackInstance attackInstance)
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
        double attackWaitingStartTime = _actionStartTime;
        if (_attackIndex > 0)
        {
            AttackTiming finalAttackTiming = _attackTimings[_attackIndex - 1];
            attackWaitingStartTime = finalAttackTiming._timeWhenAttackBlockWindowEnd;
        }

        AttackTiming attackTiming = _attackTimings[_attackIndex];
        double timeWhenAttackStart = attackTiming._timeWhenAttackStart;

        var normalizedTime = (float)((time - attackWaitingStartTime) / (timeWhenAttackStart - attackWaitingStartTime));
        _indicator.TickWaitingForAttack(normalizedTime, _data._attacks[_attackIndex]._blockPose);

        if (time >= timeWhenAttackStart)
        {
            _state = State.AttackBlockWindow;
        }

        if (time + _attackAnimationWindup >= timeWhenAttackStart)
        {
            _animator.Play("Attack");
        }
    }

    private void TickAttackBlockWindow(double time)
    {
        AttackTiming attackTiming = _attackTimings[_attackIndex];
        double timeWhenAttackBlockWindowEnd = attackTiming._timeWhenAttackBlockWindowEnd;
        if (time >= timeWhenAttackBlockWindowEnd)
        {
            if (_blocked)
            {
            }
            else
            {
                AudioSource.PlayClipAtPoint(_hitConfig.ImpactSound, Vector3.zero, 1f);
                Protaganist.Instance.TakeDamage(_hitConfig.DamageDealtToPlayer);
            }

            if (_attackIndex >= _attackTimings.Count - 1)
            {
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
            else
            {
                _state = State.WaitingToAttack;
                _blocked = false;
                _attackIndex++;
            }
        }
    }

    private void TickWaitingToVulnerable(double time)
    {
        double vulnerableWaitingStartTime = _actionStartTime;
        if (_attackTimings.Count > 0)
        {
            AttackTiming finalAttackTiming = _attackTimings[^1];
            vulnerableWaitingStartTime = finalAttackTiming._timeWhenAttackBlockWindowEnd;
        }

        var normalizedTime = (float)((time - vulnerableWaitingStartTime) /
                                     (_timeWhenVulnerableStart - vulnerableWaitingStartTime));
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
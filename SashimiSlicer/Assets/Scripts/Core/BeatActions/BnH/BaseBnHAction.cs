using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
///     Base class for all BnH actions. All timings in here are in beatmap time space
/// </summary>
public class BaseBnHAction : MonoBehaviour
{
    public enum InteractionType
    {
        IncomingAttack,

        // BlockHold,
        Vulnerable
    }

    private enum BnHActionState
    {
        WaitingForInteraction,
        InInteraction,
        WaitingToLeave,
        Leaving
    }

    [Serializable]
    public struct BnHActionInstanceConfig
    {
        public Vector2 Position;

        public bool AutoVulnerableAtEnd;

        [Header("Timing (Beatmap space)")]

        public double ActionStartTime;

        public double ActionEndTime;

        public double ActionBeatLength;

        [SerializeField]
        private List<InteractionInstanceConfig> _interactions;

        public List<InteractionInstanceConfig> Interactions
        {
            get
            {
                var list = new List<InteractionInstanceConfig>(_interactions);

                if (AutoVulnerableAtEnd)
                {
                    list.Add(new InteractionInstanceConfig
                    {
                        InteractionType = InteractionType.Vulnerable,
                        BeatsUntilStart = ActionBeatLength,
                        DieOnHit = true
                    });
                }

                return list;
            }
        }
    }

    [Serializable]
    public struct InteractionInstanceConfig
    {
        public InteractionType InteractionType;
        public double BeatsUntilStart;

        [Header("Vulnerable")]

        public bool DieOnHit;

        [Header("Blocking")]

        public Gameplay.BlockPoseStates BlockPose;
    }

    private struct ScheduledInteraction
    {
        public InteractionInstanceConfig Interaction;
        public double TimeWhenInteractionStart;
        public double TimeWhenInteractWindowEnd;
    }

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private float _attackAnimationWindup;

    [SerializeField]
    private BeatActionIndicator _indicator;

    [SerializeField]
    private ParticleSystem _killedParticles;

    private ScheduledInteraction CurrentInteraction => _sequencedInteractionInstances[_currentInteractionIndex];

    private BnHActionInstanceConfig _data;
    private BnHActionSo _hitConfig;

    private List<ScheduledInteraction> _sequencedInteractionInstances;
    private double _actionStartTime;
    private double _actionEndTime;

    private double _previousInteractionEndTime;

    private BnHActionState _bnHActionState;
    private int _currentInteractionIndex;
    private bool _blocked;

    protected event Action OnHitByProtag;
    protected event Action OnHitProtag;
    protected event Action OnBlockByProtag;

    private void OnDrawGizmos()
    {
        if (_hitConfig == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_indicator.transform.position, _hitConfig.HitboxRadius);

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Handles.Label(_indicator.transform.position + Vector3.up * 2,
                $"State: {_bnHActionState}" +
                $"\nIndex {_currentInteractionIndex}" +
                $"\nType: {CurrentInteraction.Interaction.InteractionType}");
        }
#endif
    }

    public void Setup(BnHActionSo hitConfig, BnHActionInstanceConfig data)
    {
        _hitConfig = hitConfig;
        _data = data;
        _blocked = false;

        transform.position = data.Position;

        _currentInteractionIndex = 0;
        if (_data.Interactions.Count > 0)
        {
            _bnHActionState = BnHActionState.WaitingForInteraction;
        }
        else
        {
            _bnHActionState = BnHActionState.WaitingToLeave;
        }

        _actionStartTime = data.ActionStartTime;
        _actionEndTime = data.ActionEndTime;
        _previousInteractionEndTime = _actionStartTime;

        ScheduleInteractions(data);

        if (Protaganist.Instance == null)
        {
            return;
        }

        Protaganist.Instance.OnBlockAction += OnPlayerAttemptBlock;
        Protaganist.Instance.OnSliceAction += OnPlayerAttemptAttack;

        _animator.Play("Spawn");
    }

    private void ScheduleInteractions(BnHActionInstanceConfig data)
    {
        var timingService = TimingService.Instance;
        if (timingService == null)
        {
            return;
        }

        _sequencedInteractionInstances = new List<ScheduledInteraction>();

        foreach (InteractionInstanceConfig interactionConfig in data.Interactions)
        {
            double halfWindow = 0;

            switch (interactionConfig.InteractionType)
            {
                case InteractionType.IncomingAttack:
                    halfWindow = _hitConfig.BlockWindowHalfDuration;
                    break;
                case InteractionType.Vulnerable:
                    halfWindow = _hitConfig.VulnerableWindowHalfDuration;
                    break;
            }

            double interactionStartTime = _actionStartTime
                                          + timingService.TimePerBeat * interactionConfig.BeatsUntilStart
                                          - halfWindow;
            double interactionWindowEndTime = interactionStartTime + 2 * halfWindow;

            var scheduledInteraction = new ScheduledInteraction
            {
                Interaction = interactionConfig,
                TimeWhenInteractionStart = interactionStartTime,
                TimeWhenInteractWindowEnd = interactionWindowEndTime
            };

            _sequencedInteractionInstances.Add(scheduledInteraction);
        }

        // Sort by start time
        _sequencedInteractionInstances.Sort(
            (a, b) =>
                a.TimeWhenInteractionStart.CompareTo(b.TimeWhenInteractionStart));
    }

    public void Tick()
    {
        var timingService = TimingService.Instance;
        double time = timingService.CurrentBeatmapTime;

        switch (_bnHActionState)
        {
            case BnHActionState.WaitingForInteraction:
                TickWaitingForInteraction(time);
                break;
            case BnHActionState.InInteraction:
                TickInInteraction(time);
                break;
            case BnHActionState.WaitingToLeave:
                TickWaitingToLeave(time);
                break;
            case BnHActionState.Leaving:
                TickLeaving(time);
                break;
        }
    }

    private void OnPlayerAttemptBlock(Protaganist.BlockInstance blockInstance)
    {
        if (_bnHActionState != BnHActionState.InInteraction ||
            CurrentInteraction.Interaction.InteractionType != InteractionType.IncomingAttack)
        {
            return;
        }

        if (blockInstance.BlockPose != CurrentInteraction.Interaction.BlockPose)
        {
            return;
        }

        Vector3 pos = _indicator.transform.position;
        float dist = Protaganist.Instance.DistanceToSwordPlane(pos);
        bool isBlockOnTarget = dist < _hitConfig.HitboxRadius;

        double time = TimingService.Instance.CurrentBeatmapTime;
        ScheduledInteraction attackInteraction = _sequencedInteractionInstances[_currentInteractionIndex];
        Debug.Log(
            $"Block offset: {time - (attackInteraction.TimeWhenInteractionStart + _hitConfig.BlockWindowHalfDuration)}");

        if (isBlockOnTarget)
        {
            _blocked = true;
            Protaganist.Instance.SuccessfulBlock();
            AudioSource.PlayClipAtPoint(_hitConfig.BlockSound, Vector3.zero, 1f);
        }
    }

    private void OnPlayerAttemptAttack(Protaganist.AttackInstance attackInstance)
    {
        if (_bnHActionState != BnHActionState.InInteraction ||
            CurrentInteraction.Interaction.InteractionType != InteractionType.Vulnerable)
        {
            return;
        }

        double vulnerableStartTime = CurrentInteraction.TimeWhenInteractionStart;

        Vector3 pos = _indicator.transform.position;
        float dist = Protaganist.Instance.DistanceToSwordPlane(pos);
        bool isAttackOnTarget = dist < _hitConfig.HitboxRadius;

        double time = TimingService.Instance.CurrentBeatmapTime;
        Debug.Log($"Attack offset: {time - (vulnerableStartTime + _hitConfig.VulnerableWindowHalfDuration)}");

        if (isAttackOnTarget)
        {
            _killedParticles.Play();
            BossService.Instance.TakeDamage(_hitConfig.DamageTakenToBoss);

            if (CurrentInteraction.Interaction.DieOnHit)
            {
                _indicator.SetVisible(false);
                _bnHActionState = BnHActionState.Leaving;
                _animator.Play("Die", 0, 0);
            }

            Protaganist.Instance.SuccessfulSlice();

            AudioSource.PlayClipAtPoint(_hitConfig.KilledSound, Vector3.zero, 1f);
        }
    }

    private void TickWaitingForInteraction(double time)
    {
        _indicator.SetVisible(true);

        InteractionType interactionType = CurrentInteraction.Interaction.InteractionType;

        if (interactionType == InteractionType.IncomingAttack)
        {
            TickWaitingForIncomingAttack(time);
        }
        else if (interactionType == InteractionType.Vulnerable)
        {
            TickWaitingForVulnerable(time);
        }
    }

    private void TickWaitingForIncomingAttack(double time)
    {
        double attackStartTime =
            CurrentInteraction.TimeWhenInteractionStart;
        double attackMiddleTime = attackStartTime + _hitConfig.BlockWindowHalfDuration;

        if (time >= attackStartTime)
        {
            _bnHActionState = BnHActionState.InInteraction;
        }

        if (time + _attackAnimationWindup >= attackMiddleTime)
        {
            _animator.Play("Attack");
        }

        var normalizedTime =
            (float)((time - _previousInteractionEndTime) / (attackMiddleTime - _previousInteractionEndTime));
        _indicator.TickWaitingForAttack(normalizedTime, _data.Interactions[_currentInteractionIndex].BlockPose);
    }

    private void TickWaitingForVulnerable(double time)
    {
        double vulnStartTime = CurrentInteraction.TimeWhenInteractionStart;
        double vulnMiddleTime = vulnStartTime + _hitConfig.VulnerableWindowHalfDuration;

        var normalizedTime = (float)((time - _previousInteractionEndTime) /
                                     (vulnMiddleTime - _previousInteractionEndTime));

        _indicator.UpdateWaitingForVulnerable(normalizedTime);

        if (time >= vulnStartTime)
        {
            _bnHActionState = BnHActionState.InInteraction;
        }
    }

    private void TickInInteraction(double time)
    {
        InteractionType interactionType =
            CurrentInteraction.Interaction.InteractionType;

        if (interactionType == InteractionType.IncomingAttack)
        {
            TickAttackBlockWindow(time);
        }
        else if (interactionType == InteractionType.Vulnerable)
        {
            TickVulnerableWindow(time);
        }
    }

    private void TickAttackBlockWindow(double time)
    {
        double blockWindowEndTime = CurrentInteraction.TimeWhenInteractWindowEnd;
        double attackMiddleTime = CurrentInteraction.TimeWhenInteractionStart + _hitConfig.BlockWindowHalfDuration;
        if (time >= blockWindowEndTime)
        {
            if (!_blocked)
            {
                AudioSource.PlayClipAtPoint(_hitConfig.ImpactSound, Vector3.zero, 1f);
                Protaganist.Instance.TakeDamage(_hitConfig.DamageDealtToPlayer);
            }

            TransitionToNextInteraction(time);
        }

        var normalizedTime =
            (float)((time - _previousInteractionEndTime) / (attackMiddleTime - _previousInteractionEndTime));
        _indicator.TickWaitingForAttack(normalizedTime, _data.Interactions[_currentInteractionIndex].BlockPose);
    }

    private void TickVulnerableWindow(double time)
    {
        double vulnWindowEndTime = CurrentInteraction.TimeWhenInteractWindowEnd;
        double vulnMiddleTime = CurrentInteraction.TimeWhenInteractionStart + _hitConfig.VulnerableWindowHalfDuration;
        if (time >= vulnWindowEndTime)
        {
            TransitionToNextInteraction(time);
        }

        var normalizedTime = (float)((time - _previousInteractionEndTime) /
                                     (vulnMiddleTime - _previousInteractionEndTime));
        _indicator.UpdateWaitingForVulnerable(normalizedTime);
    }

    private void TransitionToNextInteraction(double time)
    {
        if (_currentInteractionIndex >= _sequencedInteractionInstances.Count - 1)
        {
            _bnHActionState = BnHActionState.WaitingToLeave;
            _animator.Play("Idle");
        }
        else
        {
            _bnHActionState = BnHActionState.WaitingForInteraction;
            _blocked = false;
            _previousInteractionEndTime = CurrentInteraction.TimeWhenInteractWindowEnd;

            // Prevent overlaps
            while (true)
            {
                _currentInteractionIndex++;
                if (time < CurrentInteraction.TimeWhenInteractionStart)
                {
                    break;
                }

                if (_currentInteractionIndex >= _sequencedInteractionInstances.Count - 1)
                {
                    _bnHActionState = BnHActionState.WaitingToLeave;
                    _animator.Play("Idle");
                }
            }
        }
    }

    private void TickWaitingToLeave(double time)
    {
        _indicator.SetVisible(false);
        if (time >= _actionEndTime)
        {
            _bnHActionState = BnHActionState.Leaving;
            _animator.Play("Leave");
        }
    }

    private void TickLeaving(double time)
    {
        if (time >= _actionEndTime + 2f)
        {
            BeatActionManager.Instance.CleanupBnHHit(this);
        }
    }
}
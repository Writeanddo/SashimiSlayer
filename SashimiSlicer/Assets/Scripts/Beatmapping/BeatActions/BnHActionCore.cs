using System;
using System.Collections.Generic;
using Events.Core;
using UnityEditor;
using UnityEngine;

/// <summary>
///     Base class for all BnH actions. All timings in here are in beatmap time space
/// </summary>
public class BnHActionCore : MonoBehaviour
{
    public enum InteractionType
    {
        IncomingAttack,
        Vulnerable
    }

    private enum BlockState
    {
        Waiting,
        Lockout,
        Success
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

        public string AdditionalText;

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

        [Header("Attack")]

        public bool DieOnHitProtag;

        [Header("Vulnerable")]

        public bool DieOnHit;

        [Header("Blocking")]

        public bool DieOnBlocked;

        public SharedTypes.BlockPoseStates BlockPose;
    }

    public struct ScheduledInteraction
    {
        public InteractionInstanceConfig Interaction;
        public double TimeWhenInteractionStart;
        public double TimeWhenInteractWindowEnd;
    }

    [SerializeField]
    private BeatActionIndicator _indicator;

    [SerializeField]
    private BeatInteractionResultEvent _beatInteractionResultEvent;

    private ScheduledInteraction CurrentInteraction => _sequencedInteractionInstances[_currentInteractionIndex];

    public BnHActionSo ActionConfigSo => _actionConfigSo;
    public double LastInteractionEndTime => _previousInteractionEndTime;

    public event Action OnBlockByProtag;
    public event Action OnDamagedByProtag;
    public event Action OnKilled;
    public event Action OnLandHitOnProtag;

    public event Action<BnHActionCore> OnReadyForCleanup;
    public event Action OnSpawn;
    public event Action<double, ScheduledInteraction> OnTickInInteraction;
    public event Action<double, ScheduledInteraction> OnTickWaitingForInteraction;
    public event Action<double, BnHActionInstanceConfig> OnTickWaitingToLeave;
    public event Action OnTransitionToLeaving;
    public event Action<ScheduledInteraction> OnTransitionToNextInteraction;
    public event Action<ScheduledInteraction> OnTransitionToWaitingToLeave;

    private BnHActionInstanceConfig _data;
    private BnHActionSo _actionConfigSo;

    private List<ScheduledInteraction> _sequencedInteractionInstances;
    private double _actionStartTime;
    private double _actionEndTime;

    private double _previousInteractionEndTime;

    private BnHActionState _bnHActionState;
    private int _currentInteractionIndex;
    private BlockState _blockedState;

    private double _currentTime;

    private void OnDrawGizmos()
    {
        if (_actionConfigSo == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_indicator.transform.position, _actionConfigSo.HitboxRadius);

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
        _actionConfigSo = hitConfig;
        _data = data;
        _blockedState = BlockState.Waiting;

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

        if (TimingService.Instance != null)
        {
            _currentTime = TimingService.Instance.CurrentBeatmapTime;
        }

        ScheduleInteractions(data);

        if (Protaganist.Instance == null)
        {
            return;
        }

        OnSpawn?.Invoke();
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
                    halfWindow = _actionConfigSo.BlockWindowHalfDuration;
                    break;
                case InteractionType.Vulnerable:
                    halfWindow = _actionConfigSo.VulnerableWindowHalfDuration;
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
        _currentTime += timingService.DeltaTime;
        double time = _currentTime;

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

    /// <summary>
    ///     Handle protag's attempt to block
    /// </summary>
    /// <param name="protagSwordState"></param>
    public void ApplyPlayerBlock(Protaganist.ProtagSwordState protagSwordState)
    {
        if (CurrentInteraction.Interaction.InteractionType != InteractionType.IncomingAttack ||
            _blockedState != BlockState.Waiting)
        {
            return;
        }

        if ((protagSwordState.BlockPose | CurrentInteraction.Interaction.BlockPose) !=
            protagSwordState.BlockPose)
        {
            return;
        }

        double time = TimingService.Instance.CurrentBeatmapTime;
        ScheduledInteraction attackInteraction = _sequencedInteractionInstances[_currentInteractionIndex];
        double offset = time - (attackInteraction.TimeWhenInteractionStart + _actionConfigSo.BlockWindowHalfDuration);

        if (_bnHActionState == BnHActionState.WaitingForInteraction)
        {
            double prematureTime = attackInteraction.TimeWhenInteractionStart - time;

            // See if premature block lockout
            if (prematureTime < _actionConfigSo.BlockWindowFailDuration)
            {
                _blockedState = BlockState.Lockout;
            }
        }
        else if (_bnHActionState == BnHActionState.InInteraction)
        {
            _blockedState = BlockState.Success;
            Protaganist.Instance.SuccessfulBlock();

            _beatInteractionResultEvent.Raise(new SharedTypes.BeatInteractionResult
            {
                Action = _actionConfigSo,
                InteractionType = InteractionType.IncomingAttack,
                Result = SharedTypes.BeatInteractionResultType.Successful,
                TimingOffset = offset
            });

            OnBlockByProtag?.Invoke();
        }
    }

    /// <summary>
    ///     Handle protag's attempt to attack
    /// </summary>
    /// <param name="protagSwordState"></param>
    public void ApplyProtagSlice(Protaganist.ProtagSwordState protagSwordState)
    {
        if (_bnHActionState != BnHActionState.InInteraction ||
            CurrentInteraction.Interaction.InteractionType != InteractionType.Vulnerable)
        {
            return;
        }

        double vulnerableStartTime = CurrentInteraction.TimeWhenInteractionStart;

        Vector3 pos = _indicator.transform.position;
        float dist = Protaganist.Instance.DistanceToSwordPlane(pos);
        bool isAttackOnTarget = dist < _actionConfigSo.HitboxRadius;

        double time = TimingService.Instance.CurrentBeatmapTime;
        double offset = time - (vulnerableStartTime + _actionConfigSo.VulnerableWindowHalfDuration);

        if (isAttackOnTarget)
        {
            OnDamagedByProtag?.Invoke();

            if (CurrentInteraction.Interaction.DieOnHit)
            {
                Die();
            }

            _beatInteractionResultEvent.Raise(new SharedTypes.BeatInteractionResult
            {
                Action = _actionConfigSo,
                InteractionType = InteractionType.Vulnerable,
                Result = SharedTypes.BeatInteractionResultType.Successful,
                TimingOffset = offset
            });

            Protaganist.Instance.SuccessfulSlice();
        }
    }

    private void Die()
    {
        _indicator.SetVisible(false);
        _bnHActionState = BnHActionState.Leaving;
        OnKilled?.Invoke();
    }

    private void TickWaitingForInteraction(double time)
    {
        _indicator.SetVisible(true);

        InteractionType interactionType = CurrentInteraction.Interaction.InteractionType;

        OnTickWaitingForInteraction?.Invoke(time, CurrentInteraction);

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
        double attackMiddleTime = attackStartTime + _actionConfigSo.BlockWindowHalfDuration;

        if (time >= attackStartTime)
        {
            _bnHActionState = BnHActionState.InInteraction;
        }

        var normalizedTime =
            (float)((time - _previousInteractionEndTime) / (attackMiddleTime - _previousInteractionEndTime));
        _indicator.TickWaitingForAttack(normalizedTime, _data.Interactions[_currentInteractionIndex].BlockPose);
    }

    private void TickWaitingForVulnerable(double time)
    {
        double vulnStartTime = CurrentInteraction.TimeWhenInteractionStart;
        double vulnMiddleTime = vulnStartTime + _actionConfigSo.VulnerableWindowHalfDuration;

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

        OnTickInInteraction?.Invoke(time, CurrentInteraction);

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
        double attackMiddleTime = CurrentInteraction.TimeWhenInteractionStart + _actionConfigSo.BlockWindowHalfDuration;
        if (time >= blockWindowEndTime)
        {
            if (_blockedState != BlockState.Success)
            {
                OnLandHitOnProtag?.Invoke();
                Protaganist.Instance.TakeDamage(_actionConfigSo.DamageDealtToPlayer);
                if (CurrentInteraction.Interaction.DieOnHitProtag)
                {
                    Die();
                }

                _beatInteractionResultEvent.Raise(new SharedTypes.BeatInteractionResult
                {
                    Action = _actionConfigSo,
                    InteractionType = InteractionType.IncomingAttack,
                    Result = SharedTypes.BeatInteractionResultType.Failure
                });
            }
            else
            {
                if (CurrentInteraction.Interaction.DieOnBlocked)
                {
                    Die();
                }
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
        double vulnMiddleTime =
            CurrentInteraction.TimeWhenInteractionStart + _actionConfigSo.VulnerableWindowHalfDuration;

        if (time >= vulnWindowEndTime)
        {
            _beatInteractionResultEvent.Raise(new SharedTypes.BeatInteractionResult
            {
                Action = _actionConfigSo,
                InteractionType = InteractionType.Vulnerable,
                Result = SharedTypes.BeatInteractionResultType.Failure
            });
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
            OnTransitionToWaitingToLeave?.Invoke(CurrentInteraction);
        }
        else
        {
            _bnHActionState = BnHActionState.WaitingForInteraction;
            _blockedState = BlockState.Waiting;
            _previousInteractionEndTime = CurrentInteraction.TimeWhenInteractWindowEnd;

            // Prevent overlaps
            while (true)
            {
                _currentInteractionIndex++;
                if (time < CurrentInteraction.TimeWhenInteractionStart)
                {
                    OnTransitionToNextInteraction?.Invoke(CurrentInteraction);
                    break;
                }

                if (_currentInteractionIndex >= _sequencedInteractionInstances.Count - 1)
                {
                    _bnHActionState = BnHActionState.WaitingToLeave;
                    OnTransitionToWaitingToLeave?.Invoke(CurrentInteraction);
                }
            }
        }
    }

    private void TickWaitingToLeave(double time)
    {
        _indicator.SetVisible(false);
        OnTickWaitingToLeave?.Invoke(time, _data);
        if (time >= _actionEndTime)
        {
            _bnHActionState = BnHActionState.Leaving;
            OnTransitionToLeaving?.Invoke();
        }
    }

    private void TickLeaving(double time)
    {
        if (time >= _actionEndTime + 2f)
        {
            OnReadyForCleanup?.Invoke(this);
        }
    }
}
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
    public enum BnHActionState
    {
        Spawn,
        WaitingForInteraction,
        InInteraction,
        WaitingToLeave,
        Leaving
    }

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

    /// <summary>
    ///     Represents the data needed to create an action
    /// </summary>
    [Serializable]
    public struct BnHActionInstanceConfig
    {
        public Vector2[] Positions;

        public bool AutoVulnerableAtEnd;

        public int AutoVulnerableBeatOffset;

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
                        BeatsUntilStart = ActionBeatLength + AutoVulnerableBeatOffset,
                        DieOnHit = true
                    });
                }

                return list;
            }
        }
    }

    public struct Timing
    {
        public double CurrentBeatmapTime;

        // Normalized time in terms of the current interaction's wait time
        public double NormalizedInteractionWaitTime;
        public double SubdivSteppedNormalizedInteractionWaitTime;
        public double BeatSteppedNormalizedInteractionWaitTime;
        

        // Normalized time in terms of the total action time
        public double NormalizedTotalTime;
        public double NormalizedLeaveWaitTime;
    }

    /// <summary>
    ///     Represents the data needed to create an interaction
    /// </summary>
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

    /// <summary>
    ///     Represents a single interaction that has been scheduled
    /// </summary>
    public struct ScheduledInteraction
    {
        public InteractionInstanceConfig Interaction;
        public double TimeWhenInteractionStart;
        public double InteractionMiddleTime;
        public double TimeWhenInteractWindowEnd;
    }

    /* Inspector fields */
    [SerializeField]
    private BeatInteractionResultEvent _beatInteractionResultEvent;

    [SerializeField]
    private Transform _hitboxTransform;

    /* Props */
    private ScheduledInteraction CurrentInteraction => _sequencedInteractionInstances[_currentInteractionIndex];

    public BnHActionSo ActionConfigSo => _actionConfigSo;

    public BnHActionInstanceConfig Data => _data;

    /* Events */

    public event Action OnBlockByProtag;
    public event Action OnDamagedByProtag;
    public event Action OnLandHitOnProtag;

    public event Action OnSpawn;
    public event Action<Timing> OnKilled;
    public event Action<BnHActionCore> OnReadyForCleanup;

    public event Action<Timing, ScheduledInteraction> OnTransitionToWaitingToAttack;
    public event Action<Timing, ScheduledInteraction> OnTickWaitingForAttack;
    public event Action<Timing, ScheduledInteraction> OnTransitionToWaitingToVulnerable;
    public event Action<Timing, ScheduledInteraction> OnTickWaitingForVulnerable;
    public event Action<Timing, ScheduledInteraction> OnTickInAttack;
    public event Action<Timing, ScheduledInteraction> OnTickInVulnerable;

    public event Action<Timing> OnTransitionToLeaving;
    public event Action<Timing, ScheduledInteraction> OnTransitionToWaitingToLeave;
    public event Action<Timing, BnHActionInstanceConfig> OnTickWaitingToLeave;

    /* Internal fields */

    private BnHActionInstanceConfig _data;
    private BnHActionSo _actionConfigSo;

    private List<ScheduledInteraction> _sequencedInteractionInstances;
    private double _actionStartTime;
    private double _actionEndTime;

    private double _previousInteractionEndTime;

    private BnHActionState _bnHActionState;
    private int _currentInteractionIndex;
    private BlockState _blockedState;

    private Timing _currentTiming;

    private void OnDrawGizmos()
    {
        if (_actionConfigSo == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_hitboxTransform.transform.position, _actionConfigSo.HitboxRadius);

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            var style = new GUIStyle
            {
                fontSize = 12,
                normal = { textColor = Color.red }
            };
            Handles.Label(_hitboxTransform.transform.position + Vector3.up * 2,
                $"State: {_bnHActionState}" +
                $"\nIndex {_currentInteractionIndex}" +
                $"\nType: {CurrentInteraction.Interaction.InteractionType}" +
                $"\n Time: {JsonUtility.ToJson(_currentTiming)}", style);
        }
#endif
    }

    public void Setup(BnHActionSo hitConfig, BnHActionInstanceConfig data)
    {
        _actionConfigSo = hitConfig;
        _data = data;

        // Move to initial position
        transform.position = data.Positions[0];
        
        if (!Application.isPlaying)
        {
            return;
        }

        // Calculating timings
        _actionStartTime = data.ActionStartTime;
        _actionEndTime = data.ActionEndTime;
        _previousInteractionEndTime = _actionStartTime;

        UpdateTiming();


        // Setup initial state
        _blockedState = BlockState.Waiting;
        _bnHActionState = BnHActionState.Spawn;

        ScheduleInteractions(data);

        // Transition to first interaction, or else go straight to leaving
        _currentInteractionIndex = -1;
        if (_data.Interactions.Count > 0)
        {
            TransitionToNextInteraction(_currentTiming);
        }
        else
        {
            _bnHActionState = BnHActionState.WaitingToLeave;
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
                TimeWhenInteractWindowEnd = interactionWindowEndTime,
                InteractionMiddleTime = interactionStartTime + halfWindow
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
        UpdateTiming();

        switch (_bnHActionState)
        {
            case BnHActionState.WaitingForInteraction:
                TickWaitingForInteraction(_currentTiming);
                break;
            case BnHActionState.InInteraction:
                TickInInteraction(_currentTiming);
                break;
            case BnHActionState.WaitingToLeave:
                TickWaitingToLeave(_currentTiming);
                break;
            case BnHActionState.Leaving:
                TickLeaving(_currentTiming);
                break;
        }
    }

    private void UpdateTiming()
    {
        var timingService = TimingService.Instance;
        
        if(timingService == null)
        {
            return;
        }
        
        _currentTiming.CurrentBeatmapTime = timingService.CurrentBeatmapTime;
        double time = _currentTiming.CurrentBeatmapTime;

        // Calculate timings for within the interaction
        if (_bnHActionState == BnHActionState.WaitingForInteraction || _bnHActionState == BnHActionState.InInteraction)
        {
            double interactionMiddleTime = CurrentInteraction.InteractionMiddleTime;
            var normalizedTime = (float)((time - _previousInteractionEndTime) /
                                         (interactionMiddleTime - _previousInteractionEndTime));

            _currentTiming.NormalizedInteractionWaitTime = normalizedTime;
            
            if(timingService.DidCrossBeatThisFrame)
            {
                _currentTiming.BeatSteppedNormalizedInteractionWaitTime = normalizedTime;
            } 
            if(timingService.DidCrossSubdivThisFrame)
            {
                _currentTiming.SubdivSteppedNormalizedInteractionWaitTime = normalizedTime;
            }
        }

        if (_bnHActionState == BnHActionState.WaitingToLeave)
        {
            _currentTiming.NormalizedLeaveWaitTime = (time - _previousInteractionEndTime) /
                                                     (_actionEndTime - _previousInteractionEndTime);
        }

        _currentTiming.NormalizedTotalTime = (time - _actionStartTime) / (_actionEndTime - _actionStartTime);
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
                TimingOffset = offset,
                NormalizedTimingOffset = offset / _actionConfigSo.BlockWindowHalfDuration
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

        UpdateTiming();

        double vulnerableStartTime = CurrentInteraction.TimeWhenInteractionStart;

        Vector3 pos = _hitboxTransform.transform.position;
        float dist = Protaganist.Instance.DistanceToSwordPlane(pos);
        bool isAttackOnTarget = dist < _actionConfigSo.HitboxRadius;

        double time = _currentTiming.CurrentBeatmapTime;
        double offset = time - (vulnerableStartTime + _actionConfigSo.VulnerableWindowHalfDuration);

        if (isAttackOnTarget)
        {
            OnDamagedByProtag?.Invoke();

            if (CurrentInteraction.Interaction.DieOnHit)
            {
                Die(_currentTiming);
            }

            _beatInteractionResultEvent.Raise(new SharedTypes.BeatInteractionResult
            {
                Action = _actionConfigSo,
                InteractionType = InteractionType.Vulnerable,
                Result = SharedTypes.BeatInteractionResultType.Successful,
                TimingOffset = offset,
                NormalizedTimingOffset = offset / _actionConfigSo.BlockWindowHalfDuration
            });

            Protaganist.Instance.SuccessfulSlice();
        }
    }

    private void Die(Timing timing)
    {
        _bnHActionState = BnHActionState.Leaving;
        OnKilled?.Invoke(timing);
    }

    private void TickWaitingForInteraction(Timing timing)
    {
        InteractionType interactionType = CurrentInteraction.Interaction.InteractionType;
        if (interactionType == InteractionType.IncomingAttack)
        {
            TickWaitingForIncomingAttack(timing);
            OnTickWaitingForAttack?.Invoke(timing, CurrentInteraction);
        }
        else if (interactionType == InteractionType.Vulnerable)
        {
            TickWaitingForVulnerable(timing);
            OnTickWaitingForVulnerable?.Invoke(timing, CurrentInteraction);
        }
    }

    private void TickWaitingForIncomingAttack(Timing timing)
    {
        double attackStartTime =
            CurrentInteraction.TimeWhenInteractionStart;

        if (timing.CurrentBeatmapTime >= attackStartTime)
        {
            _bnHActionState = BnHActionState.InInteraction;
        }
        else
        {
            OnTickWaitingForAttack?.Invoke(timing, CurrentInteraction);
        }
    }

    private void TickWaitingForVulnerable(Timing timing)
    {
        double vulnStartTime = CurrentInteraction.TimeWhenInteractionStart;

        if (timing.CurrentBeatmapTime >= vulnStartTime)
        {
            _bnHActionState = BnHActionState.InInteraction;
        }
        else
        {
            OnTickWaitingForVulnerable?.Invoke(timing, CurrentInteraction);
        }
    }

    private void TickInInteraction(Timing timing)
    {
        InteractionType interactionType =
            CurrentInteraction.Interaction.InteractionType;

        if (interactionType == InteractionType.IncomingAttack)
        {
            TickAttackBlockWindow(timing);
            OnTickInAttack?.Invoke(timing, CurrentInteraction);
        }
        else if (interactionType == InteractionType.Vulnerable)
        {
            TickVulnerableWindow(timing);
            OnTickInVulnerable?.Invoke(timing, CurrentInteraction);
        }
    }

    private void TickAttackBlockWindow(Timing timing)
    {
        double blockWindowEndTime = CurrentInteraction.TimeWhenInteractWindowEnd;
        if (timing.CurrentBeatmapTime >= blockWindowEndTime)
        {
            if (_blockedState != BlockState.Success)
            {
                OnLandHitOnProtag?.Invoke();
                Protaganist.Instance.TakeDamage(_actionConfigSo.DamageDealtToPlayer);

                _beatInteractionResultEvent.Raise(new SharedTypes.BeatInteractionResult
                {
                    Action = _actionConfigSo,
                    InteractionType = InteractionType.IncomingAttack,
                    Result = SharedTypes.BeatInteractionResultType.Failure
                });

                if (CurrentInteraction.Interaction.DieOnHitProtag)
                {
                    Die(timing);
                }
            }
            else
            {
                if (CurrentInteraction.Interaction.DieOnBlocked)
                {
                    Die(timing);
                }
            }

            TransitionToNextInteraction(timing);
        }
    }

    private void TickVulnerableWindow(Timing timing)
    {
        double vulnWindowEndTime = CurrentInteraction.TimeWhenInteractWindowEnd;

        if (timing.CurrentBeatmapTime >= vulnWindowEndTime)
        {
            _beatInteractionResultEvent.Raise(new SharedTypes.BeatInteractionResult
            {
                Action = _actionConfigSo,
                InteractionType = InteractionType.Vulnerable,
                Result = SharedTypes.BeatInteractionResultType.Failure
            });
            TransitionToNextInteraction(timing);
        }
    }

    private void TransitionToNextInteraction(Timing timing)
    {
        bool isSpawning = _bnHActionState == BnHActionState.Spawn;

        if (_bnHActionState != BnHActionState.InInteraction
            && _bnHActionState != BnHActionState.Spawn)
        {
            return;
        }

        _previousInteractionEndTime = isSpawning ? _actionStartTime : CurrentInteraction.TimeWhenInteractWindowEnd;

        if (_currentInteractionIndex >= _sequencedInteractionInstances.Count - 1)
        {
            // No more interactions
            _bnHActionState = BnHActionState.WaitingToLeave;
            OnTransitionToWaitingToLeave?.Invoke(timing, CurrentInteraction);
        }
        else
        {
            _bnHActionState = BnHActionState.WaitingForInteraction;
            _blockedState = BlockState.Waiting;

            // Prevent overlapping interactions
            while (true)
            {
                _currentInteractionIndex++;
                if (timing.CurrentBeatmapTime < CurrentInteraction.TimeWhenInteractionStart)
                {
                    if (CurrentInteraction.Interaction.InteractionType == InteractionType.IncomingAttack)
                    {
                        OnTransitionToWaitingToAttack?.Invoke(timing, CurrentInteraction);
                    }
                    else if (CurrentInteraction.Interaction.InteractionType == InteractionType.Vulnerable)
                    {
                        OnTransitionToWaitingToVulnerable?.Invoke(timing, CurrentInteraction);
                    }

                    break;
                }

                if (_currentInteractionIndex >= _sequencedInteractionInstances.Count - 1)
                {
                    _bnHActionState = BnHActionState.WaitingToLeave;
                    OnTransitionToWaitingToLeave?.Invoke(timing, CurrentInteraction);
                }
            }
        }
    }

    private void TickWaitingToLeave(Timing timing)
    {
        OnTickWaitingToLeave?.Invoke(timing, _data);
        if (timing.CurrentBeatmapTime >= _actionEndTime)
        {
            _bnHActionState = BnHActionState.Leaving;
            OnTransitionToLeaving?.Invoke(timing);
        }
    }

    private void TickLeaving(Timing timing)
    {
        if (timing.CurrentBeatmapTime >= _actionEndTime + 2f)
        {
            OnReadyForCleanup?.Invoke(this);
        }
    }
}
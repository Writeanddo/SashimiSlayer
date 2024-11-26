using System;
using System.Collections.Generic;
using Events.Core;
using UnityEditor;
using UnityEngine;

namespace Beatmapping.Notes
{
    /// <summary>
    ///     Represents a single sequenced note with some interactions
    /// </summary>
    public class BeatNote : MonoBehaviour
    {
        private enum NoteState
        {
            /// <summary>
            ///     Note is in process of spawning
            /// </summary>
            Spawn,

            /// <summary>
            ///     Note is timing down towards the next interaction
            /// </summary>
            WaitingForInteraction,

            /// <summary>
            ///     Note is currently in an interaction's timing window
            /// </summary>
            InInteractionWindow,

            /// <summary>
            ///     No more interactions, timing down to note end
            /// </summary>
            WaitingToEnd,

            /// <summary>
            ///     Note is in process of leaving
            /// </summary>
            Leaving
        }

        public struct NoteTiming
        {
            public double CurrentBeatmapTime;

            /// <summary>
            ///     Normalized from the previous interaction target time
            ///     (or spawn time, if no previous interaction) to the upcoming interaction's target time
            /// </summary>
            public double NormalizedInteractionWaitTime;

            /// <summary>
            ///     Normalized interaction wait time, but stepped at each subdivision
            /// </summary>
            public double SubdivSteppedNormalizedInteractionWaitTime;

            /// <summary>
            ///     Normalized interaction wait time, but stepped at each beat
            /// </summary>
            public double BeatSteppedNormalizedInteractionWaitTime;

            /// <summary>
            ///     Normalized time to the end of the note from the start of the note
            /// </summary>
            public double NormalizedNoteTime;

            /// <summary>
            ///     Normalized time from the final interaction's end time to the note's end time
            /// </summary>
            public double NormalizedLeaveWaitTime;
        }

        // Serialized fields
        [SerializeField]
        private Transform _hitboxTransform;

        [Header("Events Invoking")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

        // Properties
        private NoteInteraction CurrentInteraction =>
            _currentInteractionIndex >= _interactions.Count || _currentInteractionIndex < 0
                ? null
                : _interactions[_currentInteractionIndex];

        public List<Vector2> Positions { get; private set; }

        // Events
        public event Action OnBlockByProtag;
        public event Action OnDamagedByProtag;
        public event Action OnLandHitOnProtag;

        public event Action OnSpawn;
        public event Action<NoteTiming> OnNoteEnded;
        public event Action<BeatNote> OnReadyForCleanup;

        public event Action<NoteTiming, NoteInteraction> OnTransitionToWaitingToAttack;
        public event Action<NoteTiming, NoteInteraction> OnTickWaitingForAttack;
        public event Action<NoteTiming, NoteInteraction> OnTransitionToWaitingToVulnerable;
        public event Action<NoteTiming, NoteInteraction> OnTickWaitingForVulnerable;
        public event Action<NoteTiming, NoteInteraction> OnTickInAttack;
        public event Action<NoteTiming, NoteInteraction> OnTickInVulnerable;

        public event Action<NoteTiming> OnTransitionToLeaving;
        public event Action<NoteTiming, NoteInteraction> OnTransitionToWaitingToLeave;
        public event Action<NoteTiming> OnTickWaitingToLeave;

        // State
        private NoteState _noteState;
        private int _currentInteractionIndex;

        /// <summary>
        ///     Time of the last note event (either an interaction's target time, or spawn time)
        /// </summary>
        private double _lastNoteEventTime;

        private NoteTiming _noteTiming;

        // Note configuration
        private List<NoteInteraction> _interactions;

        // Arbitrary position data; used by specific note behaviors
        private float _hitboxRadius;
        private int _damageDealtToPlayer;
        private double _noteStartTime;
        private double _noteEndTime;

        private void OnDrawGizmos()
        {
            DrawDebug();
        }

        /// <summary>
        ///     Initialize the note with the given interactions and positions
        /// </summary>
        public void Initialize(
            List<NoteInteraction> noteInteractions,
            List<Vector2> interactionPositions,
            double noteStartTime,
            double noteEndTime,
            float hitboxRadius,
            int damageDealtToPlayer
        )
        {
            _interactions = new List<NoteInteraction>(noteInteractions);
            Positions = new List<Vector2>(interactionPositions);
            _noteStartTime = noteStartTime;
            _noteEndTime = noteEndTime;
            _hitboxRadius = hitboxRadius;
            _damageDealtToPlayer = damageDealtToPlayer;

            // Timings
            _lastNoteEventTime = noteStartTime;
            _noteState = NoteState.Spawn;

            // Transition to first interaction, or else go straight to leaving
            _currentInteractionIndex = -1;
            if (_interactions.Count > 0)
            {
                TransitionToNextInteraction(_noteTiming);
            }
            else
            {
                _noteState = NoteState.WaitingToEnd;
            }

            OnSpawn?.Invoke();
        }

        /// <summary>
        ///     Update timing and Tick the FSM
        /// </summary>
        /// <param name="tickInfo"></param>
        public void Tick(TimingService.TickInfo tickInfo)
        {
            UpdateTiming(tickInfo);

            switch (_noteState)
            {
                case NoteState.WaitingForInteraction:
                    TickWaitingForInteraction(_noteTiming);
                    break;
                case NoteState.InInteractionWindow:
                    TickInInteraction(_noteTiming);
                    break;
                case NoteState.WaitingToEnd:
                    TickWaitingToLeave(_noteTiming);
                    break;
                case NoteState.Leaving:
                    TickLeaving(_noteTiming);
                    break;
            }
        }

        private void UpdateTiming(TimingService.TickInfo tickInfo)
        {
            double currentBeatmapTime = tickInfo.CurrentBeatmapTime;
            double beatQuantizedTime = tickInfo.BeatQuantizedBeatmapTime;
            double subdivQuantizedTime = tickInfo.SubdivQuantizedBeatmapTime;

            _noteTiming.CurrentBeatmapTime = currentBeatmapTime;

            // Calculate interaction relative timings
            if (_noteState is NoteState.WaitingForInteraction or NoteState.InInteractionWindow)
            {
                double nextTargetTime = CurrentInteraction.TargetTime;

                float CalculateNormalizedTime(double time)
                {
                    return (float)((time - _lastNoteEventTime) /
                                   (nextTargetTime - _lastNoteEventTime));
                }

                _noteTiming.NormalizedInteractionWaitTime = CalculateNormalizedTime(currentBeatmapTime);
                _noteTiming.BeatSteppedNormalizedInteractionWaitTime = CalculateNormalizedTime(beatQuantizedTime);
                _noteTiming.SubdivSteppedNormalizedInteractionWaitTime = CalculateNormalizedTime(subdivQuantizedTime);
            }

            if (_noteState == NoteState.WaitingToEnd)
            {
                _noteTiming.NormalizedLeaveWaitTime = (currentBeatmapTime - _lastNoteEventTime) /
                                                      (_noteEndTime - _lastNoteEventTime);
            }

            _noteTiming.NormalizedNoteTime = (currentBeatmapTime - _noteStartTime) / (_noteEndTime - _noteStartTime);
        }

        /// <summary>
        ///     Handle protag's attempt to block
        /// </summary>
        public void AttemptPlayerBlock(Protaganist.ProtagSwordState protagSwordState)
        {
            NoteInteraction interaction = CurrentInteraction;

            if (interaction == null)
            {
                return;
            }

            double currentBeatmapTime = _noteTiming.CurrentBeatmapTime;

            NoteInteraction.InteractionAttemptResult interactionAttemptResult = interaction.TryInteraction(
                currentBeatmapTime,
                NoteInteraction.InteractionType.IncomingAttack,
                protagSwordState.BlockPose);

            if (!interactionAttemptResult.Passed)
            {
                return;
            }

            Protaganist.Instance.SuccessfulBlock();

            var finalResult = new SharedTypes.InteractionFinalResult
            {
                Successful = true,
                InteractionType = NoteInteraction.InteractionType.IncomingAttack,
                TimingResult = interactionAttemptResult.TimingResult
            };

            _noteInteractionFinalResultEvent.Raise(finalResult);

            OnBlockByProtag?.Invoke();
        }

        /// <summary>
        ///     Handle protag's attempt to attack
        /// </summary>
        /// <param name="protagSwordState"></param>
        public void AttemptPlayerSlice(Protaganist.ProtagSwordState protagSwordState)
        {
            NoteInteraction interaction = CurrentInteraction;

            if (interaction == null)
            {
                return;
            }

            // Check if slice is in hitbox
            Vector3 pos = _hitboxTransform.transform.position;
            float dist = protagSwordState.DistanceToSwordPlane(pos);
            bool isAttackOnTarget = dist < _hitboxRadius;

            if (!isAttackOnTarget)
            {
                return;
            }

            // Call interaction logic
            double currentBeatmapTime = _noteTiming.CurrentBeatmapTime;

            NoteInteraction.InteractionAttemptResult interactionAttemptResult = interaction.TryInteraction(
                currentBeatmapTime,
                NoteInteraction.InteractionType.TargetToHit,
                protagSwordState.BlockPose);

            // Fails do nothing; failures get handled when the interaction window ends
            if (!interactionAttemptResult.Passed)
            {
                return;
            }

            // Success!
            OnDamagedByProtag?.Invoke();

            if (interactionAttemptResult.Flags.HasFlag(NoteInteraction.InteractionFlags.EndNoteOnHitByProtag))
            {
                EndNote(_noteTiming);
            }

            Protaganist.Instance.SuccessfulSlice();

            var finalResult = new SharedTypes.InteractionFinalResult
            {
                Successful = true,
                InteractionType = NoteInteraction.InteractionType.TargetToHit,
                TimingResult = interactionAttemptResult.TimingResult
            };

            _noteInteractionFinalResultEvent.Raise(finalResult);
        }

        private void EndNote(NoteTiming noteTiming)
        {
            _noteState = NoteState.Leaving;
            OnNoteEnded?.Invoke(noteTiming);
        }

        private void TickWaitingForInteraction(NoteTiming noteTiming)
        {
            NoteInteraction.InteractionType interactionType = CurrentInteraction.Type;
            if (interactionType == NoteInteraction.InteractionType.IncomingAttack)
            {
                TickWaitingForIncomingAttack(noteTiming);
                OnTickWaitingForAttack?.Invoke(noteTiming, CurrentInteraction);
            }
            else if (interactionType == NoteInteraction.InteractionType.TargetToHit)
            {
                TickWaitingForVulnerable(noteTiming);
                OnTickWaitingForVulnerable?.Invoke(noteTiming, CurrentInteraction);
            }
        }

        private void TickWaitingForIncomingAttack(NoteTiming noteTiming)
        {
            // Transition to interaction window if we enter window
            if (CurrentInteraction.IsTimeInWindow(_noteTiming.CurrentBeatmapTime))
            {
                _noteState = NoteState.InInteractionWindow;
            }
            else
            {
                OnTickWaitingForAttack?.Invoke(noteTiming, CurrentInteraction);
            }
        }

        private void TickWaitingForVulnerable(NoteTiming noteTiming)
        {
            // Transition to interaction window if we enter window
            if (CurrentInteraction.IsTimeInWindow(_noteTiming.CurrentBeatmapTime))
            {
                _noteState = NoteState.InInteractionWindow;
            }
            else
            {
                OnTickWaitingForVulnerable?.Invoke(noteTiming, CurrentInteraction);
            }
        }

        private void TickInInteraction(NoteTiming noteTiming)
        {
            NoteInteraction.InteractionType interactionType = CurrentInteraction.Type;

            if (interactionType == NoteInteraction.InteractionType.IncomingAttack)
            {
                TickAttackBlockWindow(noteTiming);
                OnTickInAttack?.Invoke(noteTiming, CurrentInteraction);
            }
            else if (interactionType == NoteInteraction.InteractionType.TargetToHit)
            {
                TickVulnerableWindow(noteTiming);
                OnTickInVulnerable?.Invoke(noteTiming, CurrentInteraction);
            }
        }

        private void TickAttackBlockWindow(NoteTiming noteTiming)
        {
            bool isInWindow = CurrentInteraction.IsTimeInWindow(noteTiming.CurrentBeatmapTime);

            // Still in window, so don't transition out yet
            if (isInWindow)
            {
                return;
            }

            // Successes trigger immediately, so are handled in the block attempt
            // Failures are only certain when we leave the window, so we handle them here
            if (CurrentInteraction.DidSucceed)
            {
                if (CurrentInteraction.Flags.HasFlag(NoteInteraction.InteractionFlags.EndNoteOnBlocked))
                {
                    EndNote(noteTiming);
                }
            }
            else
            {
                OnLandHitOnProtag?.Invoke();
                Protaganist.Instance.TakeDamage(1);

                if (CurrentInteraction.Flags.HasFlag(NoteInteraction.InteractionFlags.EndNoteOnHittingProtag))
                {
                    EndNote(noteTiming);
                }

                var finalResult = new SharedTypes.InteractionFinalResult
                {
                    Successful = false,
                    InteractionType = NoteInteraction.InteractionType.IncomingAttack,
                    TimingResult = default
                };

                _noteInteractionFinalResultEvent.Raise(finalResult);
            }

            TransitionToNextInteraction(noteTiming);
        }

        private void TickVulnerableWindow(NoteTiming noteTiming)
        {
            bool isInWindow = CurrentInteraction.IsTimeInWindow(noteTiming.CurrentBeatmapTime);

            // Still in window, so don't transition out yet
            if (isInWindow)
            {
                return;
            }

            // Successes trigger immediately, so are handled in the block attempt
            // Failures are only certain when we leave the window, so we handle them here
            if (!CurrentInteraction.DidSucceed)
            {
                var finalResult = new SharedTypes.InteractionFinalResult
                {
                    Successful = false,
                    InteractionType = NoteInteraction.InteractionType.TargetToHit,
                    TimingResult = default
                };

                _noteInteractionFinalResultEvent.Raise(finalResult);
            }

            TransitionToNextInteraction(noteTiming);
        }

        private void TransitionToNextInteraction(NoteTiming noteTiming)
        {
            bool isSpawning = _noteState == NoteState.Spawn;

            // We transition to the next interaction from spawn or the current one (when the window ends)
            if (_noteState is not (NoteState.Spawn or NoteState.InInteractionWindow))
            {
                return;
            }

            _lastNoteEventTime = isSpawning ? _noteStartTime : CurrentInteraction.TargetTime;

            _noteState = NoteState.WaitingForInteraction;

            while (true)
            {
                _currentInteractionIndex++;

                // No more interactions, so we're done
                if (_currentInteractionIndex >= _interactions.Count)
                {
                    _noteState = NoteState.WaitingToEnd;
                    OnTransitionToWaitingToLeave?.Invoke(noteTiming, CurrentInteraction);
                    break;
                }

                // Skip over interactions that are already in the past
                if (noteTiming.CurrentBeatmapTime >= CurrentInteraction.TargetTime)
                {
                    continue;
                }

                switch (CurrentInteraction.Type)
                {
                    case NoteInteraction.InteractionType.IncomingAttack:
                        OnTransitionToWaitingToAttack?.Invoke(noteTiming, CurrentInteraction);
                        break;
                    case NoteInteraction.InteractionType.TargetToHit:
                        OnTransitionToWaitingToVulnerable?.Invoke(noteTiming, CurrentInteraction);
                        break;
                }

                break;
            }
        }

        private void TickWaitingToLeave(NoteTiming noteTiming)
        {
            OnTickWaitingToLeave?.Invoke(noteTiming);

            if (noteTiming.CurrentBeatmapTime >= _noteEndTime)
            {
                _noteState = NoteState.Leaving;
                OnTransitionToLeaving?.Invoke(noteTiming);
            }
        }

        private void TickLeaving(NoteTiming noteTiming)
        {
            if (noteTiming.CurrentBeatmapTime >= _noteEndTime + 2f)
            {
                // Call for note service to clean this up
                OnReadyForCleanup?.Invoke(this);
            }
        }

        private void DrawDebug()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_hitboxTransform.transform.position, _hitboxRadius);

#if UNITY_EDITOR
            if (Application.isPlaying && CurrentInteraction != null)
            {
                var style = new GUIStyle
                {
                    fontSize = 12,
                    normal = { textColor = Color.red }
                };
                Handles.Label(_hitboxTransform.transform.position + Vector3.up * 2,
                    $"State: {_noteState}" +
                    $"\nIndex {_currentInteractionIndex}" +
                    $"\nType: {CurrentInteraction.Type}" +
                    $"\n Time: {JsonUtility.ToJson(_noteTiming)}", style);
            }
#endif
        }
    }
}
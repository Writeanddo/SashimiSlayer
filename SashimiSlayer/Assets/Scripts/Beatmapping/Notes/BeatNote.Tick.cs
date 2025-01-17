using Beatmapping.Timing;
using UnityEngine;

namespace Beatmapping.Notes
{
    public partial class BeatNote : MonoBehaviour
    {
        /// <summary>
        ///     Update timing and invoke tick events
        /// </summary>
        /// <param name="tickInfo"></param>
        public void Tick(BeatmapTimeManager.TickInfo tickInfo, TickFlags tickFlags)
        {
            UpdateTiming(tickInfo, tickFlags);

            int currentSegmentIndex = _noteTickInfo.SegmentIndex;
            NoteTimeSegment currentSegment = _noteTimeSegments[currentSegmentIndex];
            TimeSegmentType currentSegmentType = currentSegment.Type;

            NoteTimeSegment prevSegment = _prevTickInfo.NoteSegment;
            TimeSegmentType prevSegmentType = prevSegment.Type;

            bool triggerInteractions = tickFlags.HasFlag(TickFlags.TriggerInteractions);

            // Only if this is NOT the first tick
            if (triggerInteractions && !_isFirstTick)
            {
                HandleExitingInteractionWindow(_noteTickInfo, _prevTickInfo);
            }

            // We might've skipped the spawn segment (e.g if the first tick is past the spawn segment)
            // NOTE: AS of now, spawn segments aren't even used...
            if ((_isFirstTick || prevSegmentType == TimeSegmentType.Spawn) && currentSegmentType != prevSegmentType)
            {
                OnNoteStart?.Invoke();
            }

            if (prevSegmentType != TimeSegmentType.Ending && currentSegmentType == TimeSegmentType.Ending)
            {
                OnNoteEnd?.Invoke();
            }

            OnTick?.Invoke(_noteTickInfo);

            // Confusing logic to detect when the note should be reset.
            // The idea is that if we were previously in some non-interaction segment
            // and we suddenly enter an interaction segment,
            // then we're either going from the start, or we looped back after the note ended
            // upon which we should reset the state
            if (prevSegmentType != TimeSegmentType.Interaction && currentSegmentType == TimeSegmentType.Interaction)
            {
                ResetState();
            }

            switch (currentSegmentType)
            {
                case TimeSegmentType.Spawn:
                    // No special behavior
                    break;
                case TimeSegmentType.Interaction:
                    // No special behavior
                    break;
                case TimeSegmentType.PreEnding:
                    // No special behavior
                    break;
                case TimeSegmentType.Ending:
                    // No special behavior
                    break;
                case TimeSegmentType.CleanedUp:
                    // Invoke event asking for manager to clean this up
                    OnReadyForCleanup?.Invoke(this);
                    break;
            }

            _isFirstTick = false;
        }

        private void UpdateTiming(BeatmapTimeManager.TickInfo tickInfo, TickFlags tickFlags)
        {
            double currentBeatmapTime = tickInfo.CurrentBeatmapTime;
            double previousBeatmapTime = _prevTickInfo.BeatmapTime;

            int currentSegmentIndex = CalculateCurrentSegmentIndex(currentBeatmapTime);
            NoteTimeSegment currentSegment = _noteTimeSegments[currentSegmentIndex];

            // Note timespace timings
            double noteTime = currentBeatmapTime - _noteStartTime;
            double normalizedNoteTime = noteTime / (_noteEndTime - _noteStartTime);

            // Segment timespace timings
            double currentSegmentStartTime = currentSegment.SegmentStartTime;

            double currentSegmentTime = currentBeatmapTime - currentSegment.SegmentStartTime;

            // If we're in cleanup segment (last segment) we don't have an end time
            double normalizedSegmentTime = 0;
            if (currentSegmentIndex < _noteTimeSegments.Count - 1)
            {
                double currentSegmentEndTime = _noteTimeSegments[currentSegmentIndex + 1].SegmentStartTime;
                normalizedSegmentTime = currentSegmentTime / (currentSegmentEndTime - currentSegmentStartTime);
            }

            // Other timings
            double timeSinceNoteEnd = currentBeatmapTime - _noteEndTime;

            // See if we're in an interaction window
            NoteInteraction inWindowInteraction = null;
            NoteInteraction inPassingInteraction = null;

            // Assume we have no overlaps, so we break early
            // A passing window will be narrower than the interaction window
            foreach (NoteInteraction interaction in _allInteractions)
            {
                bool inInteractionWindow = interaction.IsInInteractTimingWindow(currentBeatmapTime);
                if (inInteractionWindow)
                {
                    inWindowInteraction = interaction;

                    bool inPassingInteractionWindow = interaction.IsInPassTimingWindow(currentBeatmapTime);
                    if (inPassingInteractionWindow)
                    {
                        inPassingInteraction = interaction;
                    }

                    break;
                }
            }

            int currentInteractionIndex = GetInteractionIndex(currentSegment.Interaction);

            _prevTickInfo = _noteTickInfo;
            _noteTickInfo = new NoteTickInfo
            {
                BeatmapTime = currentBeatmapTime,
                DeltaTime = currentBeatmapTime - previousBeatmapTime,
                NoteSegment = _noteTimeSegments[currentSegmentIndex],
                NoteTime = noteTime,
                NormalizedNoteTime = normalizedNoteTime,
                SegmentTime = currentSegmentTime,
                NormalizedSegmentTime = normalizedSegmentTime,
                TimeSinceNoteEnd = timeSinceNoteEnd,
                InsideInteractionWindow = inWindowInteraction,
                InsidePassInteractionWindow = inPassingInteraction,

                SegmentIndex = currentSegmentIndex,
                InteractionIndex = currentInteractionIndex,

                Flags = tickFlags
            };
        }

        private int CalculateCurrentSegmentIndex(double currentBeatmapTime)
        {
            int segmentIndex = -1;
            for (var i = 1; i < _noteTimeSegments.Count; i++)
            {
                if (currentBeatmapTime < _noteTimeSegments[i].SegmentStartTime)
                {
                    segmentIndex = i - 1;
                    break;
                }
            }

            // If we're past the last segment, we're in the last segment (cleanup)
            if (segmentIndex == -1)
            {
                segmentIndex = _noteTimeSegments.Count - 1;
            }

            return segmentIndex;
        }

        private void HandleExitingInteractionWindow(NoteTickInfo noteTickInfo, NoteTickInfo previousTiming)
        {
            NoteInteraction currentInsidePassWindowInteraction = noteTickInfo.InsidePassInteractionWindow;
            NoteInteraction prevInsidePassWindowInteraction = previousTiming.InsidePassInteractionWindow;

            // If we were previously in a passing window, and now we're not, we've exited the window
            // This does not work properly with overlapping windows...
            bool didJustExit = prevInsidePassWindowInteraction != null && currentInsidePassWindowInteraction == null;
            if (!didJustExit)
            {
                return;
            }

            NoteInteraction.NoteInteractionState interactionState = prevInsidePassWindowInteraction.State;

            // If default state, then no action happened, so we need to handle a 'Late Miss'
            // An early miss (a lockout) or a success were handled immediately, so no need to handle it here
            if (interactionState == NoteInteraction.NoteInteractionState.Default)
            {
                var finalResult = new NoteInteraction.FinalResult(default,
                    prevInsidePassWindowInteraction.Type,
                    false)
                {
                    Pose = prevInsidePassWindowInteraction.BlockPose
                };

                // Failure events. Use previous tick info, since that is the tick with the interaction failed
                switch (prevInsidePassWindowInteraction.Type)
                {
                    case NoteInteraction.InteractionType.IncomingAttack:
                        OnProtagFailBlock?.Invoke(previousTiming, finalResult);
                        break;
                    case NoteInteraction.InteractionType.TargetToHit:
                        OnProtagMissedHit?.Invoke(previousTiming, finalResult);
                        break;
                }

                _noteInteractionFinalResultEvent.Raise(finalResult);
                OnInteractionFinalResult?.Invoke(previousTiming, finalResult);
            }

            // Only apply player damage at the end of the window, EVEN in the case of an early fail
            if (interactionState != NoteInteraction.NoteInteractionState.Success
                && prevInsidePassWindowInteraction.Type == NoteInteraction.InteractionType.IncomingAttack)
            {
                if (Protaganist.Instance)
                {
                    Protaganist.Instance.TakeDamage(1);
                }
            }
        }
    }
}
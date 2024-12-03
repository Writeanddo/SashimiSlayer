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

            NoteTimeSegment prevSegment = _previousNoteTickTiming.NoteSegment;
            TimeSegmentType prevSegmentType = prevSegment.Type;

            bool triggerInteractions = tickFlags.HasFlag(TickFlags.TriggerInteractions);

            // Only if this is NOT the first tick
            if (triggerInteractions && !_isFirstTick)
            {
                CheckForFailures(_noteTickInfo, _previousNoteTickTiming);
            }

            // We might've skipped the spawn segment (e.g if the first tick is past the spawn segment)
            if ((_isFirstTick || prevSegmentType == TimeSegmentType.Spawn) && currentSegmentType != prevSegmentType)
            {
                OnNoteStart?.Invoke();
            }

            if (prevSegmentType != TimeSegmentType.Ending && currentSegmentType == TimeSegmentType.Ending)
            {
                OnNoteEnd?.Invoke();
            }

            OnTick?.Invoke(_noteTickInfo);

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

            _previousNoteTickTiming = _noteTickInfo;
            _noteTickInfo = new NoteTickInfo
            {
                CurrentBeatmapTime = currentBeatmapTime,
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

        private void CheckForFailures(NoteTickInfo noteTickInfo, NoteTickInfo previousTiming)
        {
            NoteInteraction currentInsidePassWindowInteraction = noteTickInfo.InsidePassInteractionWindow;
            NoteInteraction prevInsidePassWindowInteraction = previousTiming.InsidePassInteractionWindow;

            // If we were previously in a passing window, and now we're not, check for the final results of that interaction
            if (prevInsidePassWindowInteraction == null || currentInsidePassWindowInteraction != null)
            {
                return;
            }

            bool didSucceed = prevInsidePassWindowInteraction.DidSucceed;

            if (didSucceed)
            {
                // Do nothing, successes were handled immediately on the successful interaction attempt
                // In BeatNote.Interaction.cs
            }
            else
            {
                var finalResult = new SharedTypes.InteractionFinalResult
                {
                    Successful = false,
                    InteractionType = prevInsidePassWindowInteraction.Type,
                    // We don't care about the exact timing info for failures
                    TimingResult = default
                };

                // Failure events. Use previous tick info, since that is the tick with the interaction failed
                switch (prevInsidePassWindowInteraction.Type)
                {
                    case NoteInteraction.InteractionType.IncomingAttack:
                        OnProtagFailBlock?.Invoke(previousTiming, finalResult);
                        if (Protaganist.Instance)
                        {
                            Protaganist.Instance.TakeDamage(1);
                        }

                        break;
                    case NoteInteraction.InteractionType.TargetToHit:
                        OnProtagMissedHit?.Invoke(previousTiming, finalResult);
                        break;
                }

                _noteInteractionFinalResultEvent.Raise(finalResult);
            }
        }
    }
}
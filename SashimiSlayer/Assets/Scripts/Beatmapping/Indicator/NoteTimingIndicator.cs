using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Script that handles the visual timing indicators for a beat note.
    /// </summary>
    public class NoteTimingIndicator : BeatNoteModule
    {
        [BoldHeader("Note Timing Indicator")]
        [InfoBox("Handles a note's timing indicators for slicing and blocking")]
        [Header("Visuals")]

        [SerializeField]
        private PipTimingIndicator _sliceTimingIndicators;

        [SerializeField]
        private List<PipTimingIndicator> _blockTimingIndicators;

        private BeatNote _beatNote;

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickInfo)
        {
            BeatNote.TimeSegmentType segmentType = tickInfo.NoteSegment.Type;

            if (segmentType != BeatNote.TimeSegmentType.Interaction)
            {
                _sliceTimingIndicators.SetVisible(false);
                foreach (PipTimingIndicator blockTimingIndicator in _blockTimingIndicators)
                {
                    blockTimingIndicator.SetVisible(false);
                }

                return;
            }

            NoteInteraction interaction = tickInfo.NoteSegment.Interaction;

            if (!tickInfo.BeatmapTickInfo.CrossedSubdivThisTick)
            {
                return;
            }

            if (interaction.Type == NoteInteraction.InteractionType.Block)
            {
                _sliceTimingIndicators.SetVisible(false);
                TickBlockIndicators(tickInfo, interaction.BlockPose);
            }
            else if (interaction.Type == NoteInteraction.InteractionType.Slice)
            {
                _sliceTimingIndicators.SetVisible(true);
                _sliceTimingIndicators.Tick(tickInfo).Forget();
                foreach (PipTimingIndicator blockIndicator in _blockTimingIndicators)
                {
                    blockIndicator.SetVisible(false);
                }
            }
        }

        private void TickBlockIndicators(BeatNote.NoteTickInfo tickInfo, SharedTypes.BlockPoseStates blockPose)
        {
            var blockIndex = (int)blockPose;
            _blockTimingIndicators[blockIndex].Tick(tickInfo).Forget();
            ;
            for (var i = 0; i < _blockTimingIndicators.Count; i++)
            {
                _blockTimingIndicators[i].SetVisible(i == blockIndex);
            }
        }

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _beatNote = GetComponentInParent<BeatNote>();
            _beatNote.OnTick += BeatNote_OnTick;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnTick -= BeatNote_OnTick;
        }
    }
}
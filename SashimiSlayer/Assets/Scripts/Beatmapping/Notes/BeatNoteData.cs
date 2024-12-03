using System;
using System.Collections.Generic;
using Beatmapping.Interactions;
using UnityEngine;

namespace Beatmapping.Notes
{
    /// <summary>
    ///     Data for serializing a beat note instance
    /// </summary>
    [Serializable]
    public struct BeatNoteData
    {
        public Vector2[] Positions;

        public bool AutoVulnerableAtEnd;

        public int AutoVulnerableBeatOffset;

        [Header("Timing (Beatmap space)")]

        public double NoteStartTime;

        public double NoteEndTime;

        public uint NoteBeatCount;

        [SerializeField]
        private List<SequencedNoteInteraction> _interactions;

        public IEnumerable<SequencedNoteInteraction> Interactions
        {
            get
            {
                foreach (SequencedNoteInteraction sequencedNoteInteraction in _interactions)
                {
                    yield return sequencedNoteInteraction;
                }

                if (AutoVulnerableAtEnd)
                {
                    yield return CreateAutoVulnerableNoteInteraction();
                }
            }
        }

        private SequencedNoteInteraction CreateAutoVulnerableNoteInteraction()
        {
            var autoVulnerableInteraction = new NoteInteractionData
            {
                InteractionType = NoteInteraction.InteractionType.TargetToHit
            };
            return new SequencedNoteInteraction
            {
                BeatsFromNoteStart = (uint)(NoteBeatCount + AutoVulnerableBeatOffset),
                InteractionData = autoVulnerableInteraction
            };
        }
    }

    [Serializable]
    public struct SequencedNoteInteraction
    {
        public NoteInteractionData InteractionData;

        [Tooltip(" Offset from the start of the note, in beats")]
        public uint BeatsFromNoteStart;
    }
}
using System;
using Beatmapping.Notes;
using UnityEngine;

namespace Beatmapping.Interactions
{
    /// <summary>
    ///     Data for serializing a note interaction
    /// </summary>
    [Serializable]
    public struct NoteInteractionData
    {
        [Tooltip("The correct block pose, if this interaction is an attack that can be blocked")]
        public SharedTypes.BlockPoseStates BlockPose;

        [Tooltip("The type of interaction this is")]
        public NoteInteraction.InteractionType InteractionType;

        [Tooltip("Additional interaction flags")]
        public NoteInteraction.InteractionFlags Flags;

        public NoteInteraction ToNoteInteraction(TimingWindow window)
        {
            return new NoteInteraction(InteractionType, Flags, BlockPose, window);
        }
    }
}
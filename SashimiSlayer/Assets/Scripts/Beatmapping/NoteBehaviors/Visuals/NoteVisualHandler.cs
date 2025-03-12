using System;
using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using NaughtyAttributes;

namespace Beatmapping.NoteBehaviors.Visuals
{
    /// <summary>
    ///     Handles varying note visuals for notes of the same behavior
    /// </summary>
    public class NoteVisualHandler : BeatNoteModule
    {
        [Serializable]
        public struct NoteVisual
        {
            public NoteVisualObject VisualObject;

            [AllowNesting]
            [HideIf("IsForSlicing")]
            public SharedTypes.BlockPoseStates Pose;

            public bool IsForSlicing;
        }

        public List<NoteVisual> Visuals;

        private int _prevInteractionIndex = -1;

        private int _currentVisualIndex = -1;

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            beatNote.OnTick += BeatNote_OnTick;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            beatNote.OnTick -= BeatNote_OnTick;
        }

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            NoteInteraction interaction = tickinfo.NoteSegment.Interaction;
            if (interaction == null)
            {
                return;
            }

            int interactionIndex = tickinfo.InteractionIndex;
            if (interactionIndex == _prevInteractionIndex)
            {
                return;
            }

            _prevInteractionIndex = interactionIndex;

            int newVisual = _currentVisualIndex;

            if (interaction.Type == NoteInteraction.InteractionType.Slice)
            {
                newVisual = GetSliceVisualIndex(interaction);
            }
            else
            {
                newVisual = GetBlockVisualIndex(interaction);
            }

            if (newVisual == _currentVisualIndex)
            {
                return;
            }

            _currentVisualIndex = newVisual;
            HideAllVisuals();
            Visuals[newVisual].VisualObject.SetVisible(true);
        }

        public void SetHitParticle(bool visible)
        {
            Visuals[_currentVisualIndex].VisualObject.SetHitParticle(visible);
        }

        public void SetTrailParticle(bool visible)
        {
            Visuals[_currentVisualIndex].VisualObject.SetTrailParticle(visible);
        }

        public void SetFlipX(bool flip)
        {
            Visuals[_currentVisualIndex].VisualObject.SetFlipX(flip);
        }

        public void SetSpriteAlpha(float alpha)
        {
            Visuals[_currentVisualIndex].VisualObject.SetSpriteAlpha(alpha);
        }

        public void SetVisible(bool visible)
        {
            Visuals[_currentVisualIndex].VisualObject.SetVisible(visible);
        }

        public void SetRotation(float rot)
        {
            Visuals[_currentVisualIndex].VisualObject.SetRotation(rot);
        }

        private void HideAllVisuals()
        {
            foreach (NoteVisual visual in Visuals)
            {
                visual.VisualObject.SetVisible(false);
            }
        }

        private int GetSliceVisualIndex(NoteInteraction interaction)
        {
            return Visuals.FindIndex(v => v.IsForSlicing);
        }

        private int GetBlockVisualIndex(NoteInteraction interaction)
        {
            return Visuals.FindIndex(v => !v.IsForSlicing && v.Pose == interaction.BlockPose);
        }
    }
}
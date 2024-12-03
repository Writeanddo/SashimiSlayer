using System.Collections.Generic;
using Beatmapping.Notes;
using UnityEngine;

namespace Beatmapping.BeatNotes.NoteBehaviors.Kraken
{
    public class TentacleNote : BeatNoteListener
    {
        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private SpriteRenderer _sprite;

        [SerializeField]
        private Transform _bodyTransform;

        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private float _attackAnimationWindup;

        [SerializeField]
        private ParticleSystem[] _damagedParticles;

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.NoteTimeSegment segment = tickinfo.NoteSegment;
            if (segment.Type != BeatNote.TimeSegmentType.Interaction)
            {
                return;
            }

            NoteInteraction interaction = segment.Interaction;

            if (interaction.Type == NoteInteraction.InteractionType.IncomingAttack)
            {
                IncomingAttackVisuals(tickinfo.BeatmapTime, interaction);
            }

            // Update animator when beatmapping
            if (!Application.isPlaying)
            {
                _animator.Update((float)tickinfo.DeltaTime);
            }
        }

        private void BeatNote_OnEnd()
        {
            _animator.Play("TentacleLeave");
            _sprite.color = new Color(1, 1, 1, 0.7f);
        }

        private void IncomingAttackVisuals(double currentBeatmapTime, NoteInteraction noteInteraction)
        {
            if (currentBeatmapTime + _attackAnimationWindup >= noteInteraction.TargetTime)
            {
                _animator.Play("TentacleAttack");
            }
        }

        private void HandleOnSliced(int interactionIndex, NoteInteraction.InteractionAttemptResult result)
        {
            foreach (ParticleSystem particle in _damagedParticles)
            {
                particle.Play();
            }

            _animator.gameObject.SetActive(false);
        }

        private void HandleSpawned()
        {
            _animator.Play("TentacleSpawn");
        }

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _beatNote.OnNoteStart += HandleSpawned;
            _beatNote.OnSlicedByProtag += HandleOnSliced;
            _beatNote.OnTick += BeatNote_OnTick;
            _beatNote.OnNoteEnd += BeatNote_OnEnd;

            _bodyTransform.position = beatNote.StartPosition;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnNoteStart -= HandleSpawned;
            _beatNote.OnSlicedByProtag -= HandleOnSliced;
            _beatNote.OnTick -= BeatNote_OnTick;
            _beatNote.OnNoteEnd -= BeatNote_OnEnd;
        }
    }
}
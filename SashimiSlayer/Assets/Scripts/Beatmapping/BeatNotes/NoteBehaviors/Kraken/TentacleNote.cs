using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using Feel.Notes;
using UnityEngine;

namespace Beatmapping.BeatNotes.NoteBehaviors.Kraken
{
    public class TentacleNote : BeatNoteModule
    {
        [Header("Depends")]

        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private SpriteRenderer _sprite;

        [SerializeField]
        private Transform _bodyTransform;

        [Header("Animations")]

        [SerializeField]
        private BeatAnimatedSprite _spawnAnimation;

        [SerializeField]
        private BeatAnimatedSprite _attackAnimation;

        [SerializeField]
        private BeatAnimatedSprite _leaveAnimation;

        [SerializeField]
        private BeatAnimatedSprite _idleAnimation;

        [SerializeField]
        private int _attackAnimationWindup;

        [SerializeField]
        private ParticleSystem[] _damagedParticles;

        /// <summary>
        ///     Used to prevent the attack animation from playing multiple times
        /// </summary>
        private NoteInteraction _attackAnimationPlayedInteraction;

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.NoteTimeSegment segment = tickinfo.NoteSegment;
            if (segment.Type != BeatNote.TimeSegmentType.Interaction)
            {
                return;
            }

            NoteInteraction interaction = segment.Interaction;

            if (interaction.Type == NoteInteraction.InteractionType.Block)
            {
                if (interaction != _attackAnimationPlayedInteraction)
                {
                    IncomingAttackVisuals(tickinfo, interaction);
                }
            }
        }

        private void BeatNote_OnEnd(BeatNote.NoteTickInfo tickinfo)
        {
            _idleAnimation.ForceTransition(_leaveAnimation, tickinfo.SubdivisionIndex);
            _sprite.color = new Color(1, 1, 1, 0.7f);
        }

        /// <summary>
        ///     Play the attack animation some number of subdivisions before the target time
        /// </summary>
        /// <param name="noteTickInfo"></param>
        /// <param name="noteInteraction"></param>
        private void IncomingAttackVisuals(BeatNote.NoteTickInfo noteTickInfo, NoteInteraction noteInteraction)
        {
            int targetSubdivIndex = noteTickInfo.BeatmapTickInfo.GetClosestSubdivisionIndex(noteInteraction.TargetTime);
            int currentSubdiv = noteTickInfo.SubdivisionIndex;

            if (currentSubdiv + _attackAnimationWindup >= targetSubdivIndex)
            {
                _idleAnimation.ForceTransition(_attackAnimation, currentSubdiv);
                _attackAnimation.SetupTransitionOnEnd(_idleAnimation);
                _attackAnimationPlayedInteraction = noteInteraction;
            }
        }

        private void HandleOnSliced(int interactionIndex, NoteInteraction.AttemptResult result)
        {
            foreach (ParticleSystem particle in _damagedParticles)
            {
                particle.Play();
            }

            _sprite.gameObject.SetActive(false);
        }

        private void HandleSpawned(BeatNote.NoteTickInfo tickinfo)
        {
            _spawnAnimation.Play(tickinfo.SubdivisionIndex);
            _spawnAnimation.SetupTransitionOnEnd(_idleAnimation);
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
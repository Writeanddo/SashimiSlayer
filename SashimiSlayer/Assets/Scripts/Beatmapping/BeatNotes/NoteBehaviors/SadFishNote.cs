using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using UnityEngine;

namespace Beatmapping.BeatNotes.NoteBehaviors
{
    public class SadFishNote : BeatNoteModule
    {
        [SerializeField]
        private SpriteRenderer _sprite;

        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private ParticleSystem[] _dieParticles;

        [SerializeField]
        private AnimationCurve _moveCurve;

        [SerializeField]
        private Transform _bodyTransform;

        private Vector2 _startPos;
        private Vector2 _targetPos;
        private Vector2 _endPos;

        private bool _landedHit;

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.NoteTimeSegment segment = tickinfo.NoteSegment;

            if (segment.Type == BeatNote.TimeSegmentType.PreEnding)
            {
                PreEndingOnTick((float)tickinfo.NormalizedSegmentTime);
            }

            if (segment.Type == BeatNote.TimeSegmentType.Interaction)
            {
                InteractionOnTick((float)tickinfo.NormalizedSegmentTime);
            }
        }

        /// <summary>
        ///     Note exits the screen
        /// </summary>
        /// <param name="normalizedTime"></param>
        private void PreEndingOnTick(float normalizedTime)
        {
            if (_landedHit)
            {
                return;
            }

            float t = _moveCurve.Evaluate(1 - normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_targetPos.x, _endPos.x, normalizedTime),
                Mathf.Lerp(_targetPos.y, _endPos.y, 1 - t)
            );

            _sprite.transform.rotation = Quaternion.Euler(0, 0, 90 * (1 - t));
            _sprite.SetAlpha(0.5f);
        }

        /// <summary>
        ///     Note enters the screen and moves to target position
        /// </summary>
        /// <param name="normalizedTime"></param>
        private void InteractionOnTick(float normalizedTime)
        {
            float t = _moveCurve.Evaluate(normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_startPos.x, _targetPos.x, normalizedTime),
                Mathf.Lerp(_startPos.y, _targetPos.y, t)
            );

            _sprite.transform.rotation = Quaternion.Euler(0, 0, -90 * (1 - t));

            _sprite.SetAlpha(1);
        }

        private void BeatNote_SlicedByProtag(int interactionIndex,
            NoteInteraction.AttemptResult result)
        {
            _sprite.enabled = false;
            foreach (ParticleSystem particle in _dieParticles)
            {
                particle.Play();
            }
        }

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return new List<IInteractionUser.InteractionUsage>
            {
                new(NoteInteraction.InteractionType.Slice, 0, 1)
            };
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            // Form an arc from start, with the peak at the target position
            _startPos = _beatNote.StartPosition;
            _bodyTransform.position = _startPos;
            _targetPos = _beatNote.GetInteractionPosition(0, 0);
            _endPos = _beatNote.EndPosition;
            _sprite.flipX = _targetPos.x > _startPos.x;

            _beatNote.OnTick += BeatNote_OnTick;
            _beatNote.OnSlicedByProtag += BeatNote_SlicedByProtag;
            _beatNote.OnNoteEnd += HandleNoteEnd;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnTick -= BeatNote_OnTick;
            _beatNote.OnSlicedByProtag -= BeatNote_SlicedByProtag;
            _beatNote.OnNoteEnd -= HandleNoteEnd;
        }

        private void HandleNoteEnd(BeatNote.NoteTickInfo tickInfo)
        {
            _sprite.enabled = true;
        }
    }
}
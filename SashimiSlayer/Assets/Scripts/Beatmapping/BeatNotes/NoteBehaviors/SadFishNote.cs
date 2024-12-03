using System.Collections.Generic;
using Beatmapping.Notes;
using UnityEngine;

namespace Beatmapping.BeatNotes.NoteBehaviors
{
    public class SadFishNote : BeatNoteListener
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
                PreEndingVisual((float)tickinfo.NormalizedSegmentTime);
            }

            if (segment.Type == BeatNote.TimeSegmentType.Interaction)
            {
                TargetToHitVisuals((float)tickinfo.NormalizedSegmentTime);
            }
        }

        private void PreEndingVisual(float normalizedTime)
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
            _sprite.color = new Color(1, 1, 1, 0.5f);
        }

        private void TargetToHitVisuals(float normalizedTime)
        {
            float t = _moveCurve.Evaluate(normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_startPos.x, _targetPos.x, normalizedTime),
                Mathf.Lerp(_startPos.y, _targetPos.y, t)
            );

            _sprite.transform.rotation = Quaternion.Euler(0, 0, -90 * (1 - t));
        }

        private void BeatNote_SlicedByProtag(int interactionIndex,
            NoteInteraction.InteractionAttemptResult result)
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
                new(NoteInteraction.InteractionType.TargetToHit, 0, 1)
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
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnTick -= BeatNote_OnTick;
            _beatNote.OnSlicedByProtag -= BeatNote_SlicedByProtag;
        }
    }
}
using Beatmapping.Notes;
using UnityEngine;

namespace Beatmapping.BeatNotes.NoteBehaviors
{
    public class SadTutorialFishAction : BeatNoteListener
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
                Mathf.Lerp(_targetPos.x, _targetPos.x + (_targetPos.x - _startPos.x), normalizedTime),
                Mathf.Lerp(_targetPos.y, _startPos.y, 1 - t)
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

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            // Form an arc from start, with the peak at the target position
            _startPos = _beatNote.Positions[0];
            _bodyTransform.position = _startPos;
            _targetPos = _beatNote.Positions[1];
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
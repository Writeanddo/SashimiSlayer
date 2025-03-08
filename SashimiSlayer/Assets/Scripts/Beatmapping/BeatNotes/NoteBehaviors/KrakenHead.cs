using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using UnityEngine;

namespace Beatmapping.BeatNotes.NoteBehaviors
{
    /// <summary>
    ///     Simple note behavior that moves the note from start to slice position within a fixed time,
    ///     then to the end position
    ///     when ending.
    /// </summary>
    public class KrakenHeadNote : BeatNoteModule
    {
        [Header("Depends")]

        [SerializeField]
        private SpriteRenderer _sprite;

        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private ParticleSystem[] _dieParticles;

        [SerializeField]
        private Transform _bodyTransform;

        [Header("Movement")]

        [SerializeField]
        private AnimationCurve _enterCurve;

        [SerializeField]
        private AnimationCurve _exitCurve;

        [SerializeField]
        private float _entranceDuration;

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
                InteractionOnTick((float)tickinfo.NoteTime);
            }
        }

        /// <summary>
        ///     Note exiting the screen
        /// </summary>
        /// <param name="normalizedTime"></param>
        private void PreEndingOnTick(float normalizedTime)
        {
            if (_landedHit)
            {
                return;
            }

            float t = _exitCurve.Evaluate(normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_targetPos.x, _endPos.x, t),
                Mathf.Lerp(_targetPos.y, _endPos.y, t)
            );
        }

        private void InteractionOnTick(float rawTime)
        {
            float t = _enterCurve.Evaluate(rawTime / _entranceDuration);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_startPos.x, _targetPos.x, t),
                Mathf.Lerp(_startPos.y, _targetPos.y, t)
            );
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
            // Lerp from start to target position with fixed time, then to end
            _startPos = _beatNote.StartPosition;
            _bodyTransform.position = _startPos;
            _targetPos = _beatNote.GetInteractionPosition(0, 0);
            _endPos = _beatNote.EndPosition;

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
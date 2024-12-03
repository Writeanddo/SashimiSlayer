using System.Collections.Generic;
using Beatmapping.Notes;
using UnityEngine;
using UnityEngine.Events;

namespace Beatmapping.BeatNotes.NoteBehaviors
{
    public class SpinToVulnerable : BeatNoteListener
    {
        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private Transform _bodyTransform;

        [SerializeField]
        private int _interactionIndex;

        [Header("Visuals")]

        [SerializeField]
        private SpriteRenderer _sprite;

        [SerializeField]
        private ParticleSystem[] _dieParticles;

        [SerializeField]
        private AnimationCurve _moveCurve;

        [Header("Events")]

        public UnityEvent OnEnterDazed;

        private Vector2 _startPos;
        private Vector2 _endPos;
        private Vector2 _vulnerablePos;

        private bool _enteredDazed;

        private void BeatNote_OnSlicedByProtag(int interactionIndex,
            NoteInteraction.InteractionAttemptResult result)
        {
            if (interactionIndex != _interactionIndex)
            {
                return;
            }

            _sprite.enabled = false;
            foreach (ParticleSystem particle in _dieParticles)
            {
                particle.Play();
            }
        }

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.NoteTimeSegment segment = tickinfo.NoteSegment;

            if (tickinfo.InteractionIndex != _interactionIndex)
            {
                return;
            }

            NoteInteraction interaction = segment.Interaction;
            _sprite.color = new Color(1, 1, 1, 1);

            if (interaction.Type == NoteInteraction.InteractionType.TargetToHit)
            {
                TargetToHitVisuals((float)tickinfo.NormalizedSegmentTime, (float)tickinfo.CurrentBeatmapTime);
            }
        }

        private void TargetToHitVisuals(float normalizedTime, float beatmapTime)
        {
            if (!_enteredDazed)
            {
                _enteredDazed = true;
                OnEnterDazed.Invoke();
            }

            // Lerp from target pos to vulnerable pos

            float t = _moveCurve.Evaluate(normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_startPos.x, _vulnerablePos.x, normalizedTime),
                Mathf.Lerp(_startPos.y, _vulnerablePos.y, t)
            );

            _sprite.transform.rotation = Quaternion.Euler(0, 0, 360f * beatmapTime);
        }

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return new List<IInteractionUser.InteractionUsage>
            {
                new(NoteInteraction.InteractionType.TargetToHit, _interactionIndex, 1)
            };
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _vulnerablePos = _beatNote.GetInteractionPosition(_interactionIndex, 0);
            _startPos = _beatNote.GetPreviousPosition(_interactionIndex);
            _endPos = _beatNote.EndPosition;

            _beatNote.OnTick += BeatNote_OnTick;
            _beatNote.OnSlicedByProtag += BeatNote_OnSlicedByProtag;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnTick -= BeatNote_OnTick;
            _beatNote.OnSlicedByProtag -= BeatNote_OnSlicedByProtag;
        }
    }
}
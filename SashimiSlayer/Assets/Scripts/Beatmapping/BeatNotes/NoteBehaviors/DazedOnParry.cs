using Beatmapping.Notes;
using UnityEngine;
using UnityEngine.Events;

namespace Beatmapping.BeatNotes.NoteBehaviors
{
    public class DazedOnParry : BeatNoteListener
    {
        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private Transform _bodyTransform;

        [SerializeField]
        private int _interactionIndex;

        [Header("Positions")]

        [SerializeField]
        private int _startPosIndex;

        [SerializeField]
        private int _vulnerablePosIndex;

        [Header("Visuals")]

        [SerializeField]
        private SpriteRenderer _sprite;

        [SerializeField]
        private ParticleSystem[] _dieParticles;

        [SerializeField]
        private AnimationCurve _moveCurve;

        [Header("Events")]

        public UnityEvent OnEnterDazed;

        private Vector2 _targetPos;
        private Vector2 _startPos;
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

            if (interaction.Type == NoteInteraction.InteractionType.TargetToHit)
            {
                TargetToHitVisuals((float)tickinfo.NormalizedSegmentTime, (float)tickinfo.CurrentBeatmapTime);
            }
        }

        /*private void HandleTickWaitingToLeave(BeatNote.NoteTickInfo noteTickInfo)
        {
            // Lerp from the vulnerable position to the start position y (basically falling back into the sea)
            var normalizedTime = (float)noteTickInfo.NormalizedLeaveWaitTime;

            float t = _moveCurve.Evaluate(1 - normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                _bodyTransform.position.x,
                Mathf.Lerp(_vulnerablePos.y, _startPos.y, 1 - t)
            );

            _sprite.transform.rotation = Quaternion.Euler(0, 0, 90 * (1 - t));
            _sprite.color = new Color(1, 1, 1, 0.5f);
        }
        */

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
                Mathf.Lerp(_targetPos.x, _vulnerablePos.x, normalizedTime),
                Mathf.Lerp(_targetPos.y, _vulnerablePos.y, t)
            );

            _sprite.transform.rotation = Quaternion.Euler(0, 0, 360f * beatmapTime);
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _vulnerablePos = _beatNote.Positions[_vulnerablePosIndex];
            _startPos = _beatNote.Positions[_startPosIndex];

            if (Protaganist.Instance != null)
            {
                _targetPos = Protaganist.Instance.SpritePosition;
            }

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
using Beatmapping.Notes;
using UnityEngine;
using UnityEngine.Events;

namespace Beatmapping.BeatNotes.BnH
{
    public class DazedOnParry : MonoBehaviour
    {
        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private Transform _bodyTransform;

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

        private void Awake()
        {
            _beatNote.OnTickWaitingForVulnerable += HandleTickWaitingForVulnerable;

            _beatNote.OnTickInVulnerable += HandleTickWaitingForVulnerable;

            _beatNote.OnTickWaitingToLeave += HandleTickWaitingToLeave;

            _beatNote.OnNoteEnded += HandleNoteEnded;
        }

        private void Start()
        {
            _vulnerablePos = _beatNote.Positions[_vulnerablePosIndex];
            _startPos = _beatNote.Positions[_startPosIndex];
            _targetPos = Protaganist.Instance.SpritePosition;
        }

        private void HandleTickWaitingToLeave(BeatNote.NoteTiming noteTiming)
        {
            // Lerp from the vulnerable position to the start position y (basically falling back into the sea)
            var normalizedTime = (float)noteTiming.NormalizedLeaveWaitTime;

            float t = _moveCurve.Evaluate(1 - normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                _bodyTransform.position.x,
                Mathf.Lerp(_vulnerablePos.y, _startPos.y, 1 - t)
            );

            _sprite.transform.rotation = Quaternion.Euler(0, 0, 90 * (1 - t));
            _sprite.color = new Color(1, 1, 1, 0.5f);
        }

        private void HandleTickWaitingForVulnerable(BeatNote.NoteTiming noteTiming,
            NoteInteraction noteInteraction)
        {
            if (!_enteredDazed)
            {
                _enteredDazed = true;
                OnEnterDazed.Invoke();
            }

            // Lerp from target pos to vulnerable pos
            var normalizedTime = (float)noteTiming.NormalizedInteractionWaitTime;

            float t = _moveCurve.Evaluate(normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_targetPos.x, _vulnerablePos.x, normalizedTime),
                Mathf.Lerp(_targetPos.y, _vulnerablePos.y, t)
            );

            _sprite.transform.rotation = Quaternion.Euler(0, 0, 360f * (float)noteTiming.CurrentBeatmapTime);
        }

        private void HandleNoteEnded(BeatNote.NoteTiming noteTiming)
        {
            _sprite.enabled = false;
            foreach (ParticleSystem particle in _dieParticles)
            {
                particle.Play();
            }
        }
    }
}
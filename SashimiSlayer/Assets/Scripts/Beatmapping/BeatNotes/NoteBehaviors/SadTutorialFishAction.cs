using Beatmapping.Notes;
using UnityEngine;

namespace Beatmapping.BeatNotes.BnH
{
    public class SadTutorialFishAction : MonoBehaviour
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

        private void Awake()
        {
            _beatNote.OnNoteEnded += HandleNoteEnded;

            _beatNote.OnTickWaitingForVulnerable += HandleTickWaitingForVulnerable;
            _beatNote.OnTickInVulnerable += HandleTickWaitingForVulnerable;

            _beatNote.OnTickWaitingToLeave += HandleTickWaitingToLeave;
        }

        private void Start()
        {
            // Form an arc from start, with the peak at the target position
            _startPos = _beatNote.Positions[0];
            _bodyTransform.position = _startPos;
            _targetPos = _beatNote.Positions[1];
            _sprite.flipX = _targetPos.x > _startPos.x;
        }

        private void HandleTickWaitingToLeave(BeatNote.NoteTiming noteTiming)
        {
            if (_landedHit)
            {
                return;
            }

            var normalizedTime = (float)noteTiming.NormalizedLeaveWaitTime;

            float t = _moveCurve.Evaluate(1 - normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_targetPos.x, _targetPos.x + (_targetPos.x - _startPos.x), normalizedTime),
                Mathf.Lerp(_targetPos.y, _startPos.y, 1 - t)
            );

            _sprite.transform.rotation = Quaternion.Euler(0, 0, 90 * (1 - t));
            _sprite.color = new Color(1, 1, 1, 0.5f);
        }

        private void HandleTickWaitingForVulnerable(BeatNote.NoteTiming noteTiming,
            NoteInteraction noteInteraction)
        {
            var normalizedTime = (float)noteTiming.NormalizedInteractionWaitTime;

            float t = _moveCurve.Evaluate(normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_startPos.x, _targetPos.x, normalizedTime),
                Mathf.Lerp(_startPos.y, _targetPos.y, t)
            );

            _sprite.transform.rotation = Quaternion.Euler(0, 0, -90 * (1 - t));
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
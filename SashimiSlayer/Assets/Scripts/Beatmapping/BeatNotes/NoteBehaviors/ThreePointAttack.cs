using Beatmapping.Notes;
using UnityEngine;
using UnityEngine.Events;

namespace Beatmapping.BeatNotes.NoteBehaviors
{
    /// <summary>
    ///     Attack Note that moves in three stages
    ///     Spawn -> Attack ready position
    ///     Attack ready position -> Player (Block interaction)
    /// </summary>
    public class ThreePointAttack : BeatNoteListener
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
        private int _peakPosIndex;

        [Header("Visuals")]

        [SerializeField]
        private SpriteRenderer _sprite;

        [SerializeField]
        private AnimationCurve _moveCurve;

        [SerializeField]
        private ParticleSystem _explosionParticles;

        [Header("Events")]

        public UnityEvent OnHitPeak;

        private Vector2 _startPos;
        private Vector2 _peakPos;
        private Vector2 _targetPos;
        private float _angleToTarget;

        private bool _hitPeak;

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.NoteTimeSegment segment = tickinfo.NoteSegment;

            if (tickinfo.InteractionIndex != _interactionIndex)
            {
                return;
            }

            WaitingForAttackVisual((float)tickinfo.NormalizedSegmentTime);
        }

        private void BeatNote_ProtagFailBlock(BeatNote.NoteTickInfo tickInfo,
            SharedTypes.InteractionFinalResult finalResult)
        {
            _explosionParticles.Play();
        }

        private void WaitingForAttackVisual(float normalizedTime)
        {
            // Lerp from start pos to peak pos, then to target pos

            var thresh = 0.75f;
            if (normalizedTime <= thresh)
            {
                float t = _moveCurve.Evaluate(normalizedTime / thresh);
                _bodyTransform.position = new Vector2(
                    Mathf.Lerp(_startPos.x, _peakPos.x, normalizedTime / thresh),
                    Mathf.Lerp(_startPos.y, _peakPos.y, t)
                );
                _sprite.transform.localRotation = Quaternion.Euler(0, 0, -90 * (1 - t));
            }
            else
            {
                if (!_hitPeak && normalizedTime >= thresh)
                {
                    _hitPeak = true;
                    OnHitPeak.Invoke();
                }

                float remappedTime = (normalizedTime - thresh) / (1 - thresh);

                _bodyTransform.position = new Vector2(
                    Mathf.Lerp(_peakPos.x, _targetPos.x, remappedTime),
                    Mathf.Lerp(_peakPos.y, _targetPos.y, remappedTime)
                );

                // angle towards target
                _sprite.transform.localRotation = Quaternion.Euler(0, 0, 180 + _angleToTarget);
            }
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            // Form an arc from start, with the peak at the target position
            _startPos = _beatNote.Positions[_startPosIndex];
            _peakPos = _beatNote.Positions[_peakPosIndex];

            if (Protaganist.Instance != null)
            {
                _targetPos = Protaganist.Instance.SpritePosition;
            }

            _sprite.flipX = _peakPos.x > _startPos.x;

            _bodyTransform.position = _startPos;

            _angleToTarget = Mathf.Atan2(_targetPos.y - _peakPos.y, _targetPos.x - _peakPos.x) * Mathf.Rad2Deg;

            _beatNote.OnTick += BeatNote_OnTick;
            _beatNote.OnProtagFailBlock += BeatNote_ProtagFailBlock;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnTick -= BeatNote_OnTick;
            _beatNote.OnProtagFailBlock -= BeatNote_ProtagFailBlock;
        }
    }
}
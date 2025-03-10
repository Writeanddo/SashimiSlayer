using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using UnityEngine;
using UnityEngine.Events;

namespace Beatmapping.BeatNotes.NoteBehaviors
{
    public class HitAndBounceAwayNote : BeatNoteModule
    {
        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private int _interactionIndex;

        [SerializeField]
        private Transform _bodyTransform;

        [Header("Visuals")]

        [SerializeField]
        private SpriteRenderer _sprite;

        [SerializeField]
        private ParticleSystem _explosionParticles;

        [Header("Events")]

        public UnityEvent OnBounce;

        private Vector2 _endPos;
        private Vector2 _startPos;

        private bool _started;

        private void BeatNote_ProtagFailBlock(BeatNote.NoteTickInfo tickInfo,
            NoteInteraction.FinalResult finalresult)
        {
            if (tickInfo.InteractionIndex != _interactionIndex)
            {
                return;
            }

            _explosionParticles.Play();
        }

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickInfo)
        {
            BeatNote.NoteTimeSegment segment = tickInfo.NoteSegment;

            if (segment.Type != BeatNote.TimeSegmentType.PreEnding)
            {
                _sprite.SetAlpha(1f);
                _started = false;
                return;
            }

            if (!_started)
            {
                _started = true;
                OnBounce.Invoke();
            }

            _bodyTransform.position = Vector2.Lerp(_startPos, _endPos, (float)tickInfo.NormalizedSegmentTime);

            _sprite.transform.rotation = Quaternion.Euler(0, 0, 1200 * (float)tickInfo.SegmentTime);
            _sprite.SetAlpha(0.7f);
        }

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _startPos = beatNote.GetFinalInteractionPosition();
            _endPos = beatNote.EndPosition;
            _started = false;

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
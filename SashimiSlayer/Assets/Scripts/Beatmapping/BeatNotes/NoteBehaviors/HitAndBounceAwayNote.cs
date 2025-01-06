using System.Collections.Generic;
using Beatmapping.Notes;
using UnityEngine;

namespace Beatmapping.BeatNotes.NoteBehaviors
{
    public class HitAndBounceAwayNote : BeatNoteListener
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

        private Vector2 _endPos;
        private Vector2 _startPos;

        private void BeatNote_ProtagFailBlock(BeatNote.NoteTickInfo tickInfo,
            SharedTypes.InteractionFinalResult finalresult)
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
                _sprite.color = new Color(1, 1, 1, 1f);
                return;
            }

            _bodyTransform.position = Vector2.Lerp(_startPos, _endPos, (float)tickInfo.NormalizedSegmentTime);

            _sprite.transform.rotation = Quaternion.Euler(0, 0, 1200 * (float)tickInfo.SegmentTime);
            _sprite.color = new Color(1, 1, 1, 0.7f);
        }

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _startPos = beatNote.GetFinalInteractionPosition();
            _endPos = beatNote.EndPosition;

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
using System.Collections.Generic;
using Beatmapping.Notes;
using UnityEngine;

namespace Beatmapping.BeatNotes.NoteBehaviors
{
    /// <summary>
    ///     Visual where the note falls into the sea from End to Cleanup
    /// </summary>
    public class FallIntoSeaEnd : BeatNoteListener
    {
        private const float Gravity = 9.8f;

        [Header("Dependencies")]

        [SerializeField]
        private SpriteRenderer _sprite;

        [SerializeField]
        private Transform _bodyTransform;

        private BeatNote _beatNote;

        private Vector2 _startPos;
        private Vector2 _endPos;
        private float _expectedFallTime;

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            beatNote.OnTick += BeatNote_OnTick;
            _startPos = beatNote.GetFinalInteractionPosition();
            _endPos = beatNote.EndPosition;

            if (_startPos.y > _endPos.y)
            {
                _expectedFallTime = Mathf.Sqrt(2 * (_startPos.y - _endPos.y) / Gravity);
            }
            else
            {
                _expectedFallTime = 1;
            }
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            beatNote.OnTick -= BeatNote_OnTick;
        }

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.NoteTimeSegment segment = tickinfo.NoteSegment;

            if (segment.Type == BeatNote.TimeSegmentType.Ending)
            {
                FallDownVisuals(tickinfo);
            }
        }

        private void FallDownVisuals(BeatNote.NoteTickInfo noteTickInfo)
        {
            double time = noteTickInfo.SegmentTime;

            float startY = _startPos.y;

            Vector2 pos = _bodyTransform.position;

            pos.y = startY - (float)(Gravity * time * time / 2);

            _bodyTransform.position = pos;

            var normalizedTime = (float)(time / _expectedFallTime);

            _sprite.transform.rotation = Quaternion.Euler(0, 0, normalizedTime * 90f);
            _sprite.color = new Color(1, 1, 1, 0.5f);
        }
    }
}
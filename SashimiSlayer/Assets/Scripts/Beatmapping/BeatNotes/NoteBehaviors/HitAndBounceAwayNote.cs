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

        [Header("Visuals")]

        [SerializeField]
        private SpriteRenderer _sprite;

        [SerializeField]
        private ParticleSystem _explosionParticles;

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
                return;
            }

            transform.position += Vector3.up * Time.deltaTime * 7f
                                  + Vector3.left * Time.deltaTime * 15f;

            _sprite.transform.rotation = Quaternion.Euler(0, 0, 1200 * (float)tickInfo.CurrentBeatmapTime);
            _sprite.color = new Color(1, 1, 1, 0.7f);
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
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
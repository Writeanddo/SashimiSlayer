using Beatmapping.Notes;
using FMODUnity;
using UnityEngine;

namespace Feel
{
    public class BeatNoteAudioPlayer : BeatNoteListener
    {
        [SerializeField]
        private BeatNote _beatNote;

        [Header("Audio Events")]

        [SerializeField]
        private EventReference _startSound;

        private void PlayStartSound()
        {
            if (!_startSound.IsNull && Application.isPlaying)
            {
                RuntimeManager.PlayOneShot(_startSound);
            }
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _beatNote.OnNoteStart += PlayStartSound;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnNoteStart -= PlayStartSound;
        }
    }
}
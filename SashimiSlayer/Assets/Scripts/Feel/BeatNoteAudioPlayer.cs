using System.Collections.Generic;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using FMODUnity;
using UnityEngine;

namespace Feel
{
    public class BeatNoteAudioPlayer : BeatNoteModule
    {
        [SerializeField]
        private BeatNote _beatNote;

        [Header("Audio Events")]

        [SerializeField]
        private EventReference _startSound;

        private void PlayStartSound(BeatNote.NoteTickInfo tickInfo)

        {
            if (!_startSound.IsNull && Application.isPlaying)
            {
                RuntimeManager.PlayOneShot(_startSound);
            }
        }

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
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
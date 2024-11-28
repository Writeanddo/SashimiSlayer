using Beatmapping.Notes;
using FMODUnity;
using UnityEngine;

namespace Feel
{
    public class BeatNoteAudioPlayer : MonoBehaviour
    {
        [SerializeField]
        private BeatNote _beatNote;

        [Header("Audio Events")]

        [SerializeField]
        private EventReference _spawnSound;

        [SerializeField]
        private EventReference _startAttackSound;

        [SerializeField]
        private EventReference _startVulnerableSound;

        private void Awake()
        {
            _beatNote.OnSpawn += PlaySpawnSound;
            _beatNote.OnTransitionToWaitingToAttack += PlayTransitionToWaitingToAttack;
            _beatNote.OnTransitionToWaitingToVulnerable += PlayTransitionToWaitingToVulnerable;
        }

        private void OnDestroy()
        {
            _beatNote.OnSpawn -= PlaySpawnSound;
            _beatNote.OnTransitionToWaitingToAttack -= PlayTransitionToWaitingToAttack;
            _beatNote.OnTransitionToWaitingToVulnerable -= PlayTransitionToWaitingToVulnerable;
        }

        private void PlaySpawnSound()
        {
            if (!_spawnSound.IsNull)
            {
                RuntimeManager.PlayOneShot(_spawnSound);
            }
        }

        private void PlayTransitionToWaitingToAttack(BeatNote.NoteTiming noteTiming,
            NoteInteraction scheduledInteraction)
        {
            if (!_startAttackSound.IsNull)
            {
                RuntimeManager.PlayOneShot(_startAttackSound);
            }
        }

        private void PlayTransitionToWaitingToVulnerable(BeatNote.NoteTiming noteTiming,
            NoteInteraction scheduledInteraction)
        {
            if (!_startVulnerableSound.IsNull)
            {
                RuntimeManager.PlayOneShot(_startVulnerableSound);
            }
        }
    }
}
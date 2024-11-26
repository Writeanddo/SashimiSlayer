using Beatmapping.Notes;
using UnityEngine;

namespace Feel
{
    public class BeatNoteAudioPlayer : MonoBehaviour
    {
        [SerializeField]
        private BeatNote _beatNote;

        [Header("Audio Clips")]

        [SerializeField]
        private AudioClip _spawnSound;

        [SerializeField]
        private AudioClip _startAttackSound;

        [SerializeField]
        private AudioClip _startVulnerableSound;

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
            if (_spawnSound != null)
            {
                SFXPlayer.Instance.PlaySFX(_spawnSound);
            }
        }

        private void PlayTransitionToWaitingToAttack(BeatNote.NoteTiming noteTiming,
            NoteInteraction scheduledInteraction)
        {
            if (_startAttackSound != null)
            {
                SFXPlayer.Instance.PlaySFX(_startAttackSound);
            }
        }

        private void PlayTransitionToWaitingToVulnerable(BeatNote.NoteTiming noteTiming,
            NoteInteraction scheduledInteraction)
        {
            if (_startVulnerableSound != null)
            {
                SFXPlayer.Instance.PlaySFX(_startVulnerableSound);
            }
        }
    }
}
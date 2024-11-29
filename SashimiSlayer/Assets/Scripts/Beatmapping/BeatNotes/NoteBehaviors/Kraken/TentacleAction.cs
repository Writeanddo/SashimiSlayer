using Beatmapping.Notes;
using UnityEngine;

namespace Beatmapping.BeatNotes.BnH.Kraken
{
    public class TentacleAction : MonoBehaviour
    {
        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private SpriteRenderer _sprite;

        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private float _attackAnimationWindup;

        [SerializeField]
        private ParticleSystem[] _damagedParticles;

        private void Awake()
        {
            _beatNote.OnSpawn += HandleSpawned;
            _beatNote.OnDamagedByProtag += HandleOnDamaged;

            _beatNote.OnTickWaitingForAttack += HandleTickWaitingForAttack;

            _beatNote.OnNoteEnded += HandleNoteEnded;
            _beatNote.OnTransitionToLeaving += HandleTransitionToLeaving;
        }

        private void HandleTransitionToLeaving(BeatNote.NoteTiming noteTiming)
        {
            _animator.Play("TentacleLeave");
            _sprite.color = new Color(1, 1, 1, 0.7f);
        }

        private void HandleNoteEnded(BeatNote.NoteTiming noteTiming)
        {
            _animator.gameObject.SetActive(false);
        }

        private void HandleTickWaitingForAttack(BeatNote.NoteTiming noteTiming, NoteInteraction noteInteraction)
        {
            if (noteTiming.CurrentBeatmapTime + _attackAnimationWindup >= noteInteraction.TargetTime)
            {
                _animator.Play("TentacleAttack");
            }
        }

        private void HandleOnDamaged()
        {
            foreach (ParticleSystem particle in _damagedParticles)
            {
                particle.Play();
            }
        }

        private void HandleSpawned()
        {
            _animator.Play("TentacleSpawn");
        }
    }
}
using Beatmapping.Notes;
using UnityEngine;

namespace Beatmapping.BeatNotes.BnH
{
    public class FlyAwayOnParry : MonoBehaviour
    {
        [SerializeField]
        private BeatNote _beatNote;

        [Header("Visuals")]

        [SerializeField]
        private SpriteRenderer _sprite;

        [SerializeField]
        private ParticleSystem _explosionParticles;

        private bool _landedHit;

        private void Awake()
        {
            _beatNote.OnTickWaitingToLeave += HandleTickWaitingToLeave;
            _beatNote.OnLandHitOnProtag += HandleLandHitOnProtag;
            _beatNote.OnNoteEnded += HandleDied;
        }

        private void HandleDied(BeatNote.NoteTiming noteTiming)
        {
            _sprite.enabled = false;
        }

        private void HandleLandHitOnProtag()
        {
            _landedHit = true;
            _explosionParticles.Play();
        }

        private void HandleTickWaitingToLeave(BeatNote.NoteTiming noteTiming)
        {
            if (_landedHit)
            {
                return;
            }

            transform.position += Vector3.up * Time.deltaTime * 7f;
            transform.position += Vector3.right * Time.deltaTime * 15f;

            _sprite.transform.rotation = Quaternion.Euler(0, 0, 1200 * (float)noteTiming.CurrentBeatmapTime);
            _sprite.color = new Color(1, 1, 1, 0.7f);
        }
    }
}
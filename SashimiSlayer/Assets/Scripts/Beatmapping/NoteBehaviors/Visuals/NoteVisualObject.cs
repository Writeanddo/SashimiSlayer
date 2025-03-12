using UnityEngine;

namespace Beatmapping.NoteBehaviors.Visuals
{
    public class NoteVisualObject : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem _hitParticle;

        [SerializeField]
        private ParticleSystem _trailParticle;

        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        public void SetSpriteAlpha(float alpha)
        {
            Color color = _spriteRenderer.color;
            color.a = alpha;
            _spriteRenderer.color = color;
        }

        public void SetVisible(bool visible)
        {
            _spriteRenderer.enabled = visible;
        }

        public void SetHitParticle(bool visible)
        {
            ToggleParticle(_hitParticle, visible);
        }

        public void SetTrailParticle(bool visible)
        {
            ToggleParticle(_trailParticle, visible);
        }

        public void SetRotation(float rot)
        {
            _spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, rot);
        }

        private void ToggleParticle(ParticleSystem particle, bool visible)
        {
            if (particle == null)
            {
                return;
            }

            if (visible)
            {
                particle.Play();
            }
            else
            {
                particle.Stop();
            }
        }

        public void SetFlipX(bool flip)
        {
            _spriteRenderer.flipX = flip;
        }
    }
}
using UnityEngine;

public class SlicedButtonVFX : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private ParticleSystem[] _particles;

    public void Sliced()
    {
        _spriteRenderer.enabled = false;
        foreach (ParticleSystem particle in _particles)
        {
            particle.Play();
        }
    }
}
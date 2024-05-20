using UnityEngine;

public class ProtagBody : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _damagedParticles;

    [SerializeField]
    private ParticleSystem _blockParticles;

    private void Start()
    {
        Protaganist.Instance.OnDamageTaken += OnDamageTaken;
        Protaganist.Instance.OnSuccessfulBlock += OnSuccessfulBlock;
        Protaganist.Instance.SpritePosition = transform.position;
    }

    private void OnDestroy()
    {
        Protaganist.Instance.OnDamageTaken -= OnDamageTaken;
        Protaganist.Instance.OnSuccessfulBlock -= OnSuccessfulBlock;
    }

    private void OnDamageTaken()
    {
        _damagedParticles.Play();
    }

    private void OnSuccessfulBlock()
    {
        _blockParticles.Play();
    }
}
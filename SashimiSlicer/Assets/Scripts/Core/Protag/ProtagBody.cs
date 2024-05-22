using Events;
using UnityEngine;

public class ProtagBody : MonoBehaviour
{
    [Header("Events")]

    [SerializeField]
    private VoidEvent _damageTakenEvent;

    [SerializeField]
    private VoidEvent _successfulBlockEvent;

    [Header("Visuals")]

    [SerializeField]
    private ParticleSystem _damagedParticles;

    [SerializeField]
    private ParticleSystem _blockParticles;

    [SerializeField]
    private Transform _targetTransform;

    public Vector3 TargetPosition => _targetTransform.position;

    private void Start()
    {
        _damageTakenEvent.AddListener(OnDamageTaken);
        _successfulBlockEvent.AddListener(OnSuccessfulBlock);
        Protaganist.Instance.SpritePosition = TargetPosition;
    }

    private void OnDestroy()
    {
        _damageTakenEvent.RemoveListener(OnDamageTaken);
        _successfulBlockEvent.RemoveListener(OnSuccessfulBlock);
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
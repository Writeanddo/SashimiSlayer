using Events;
using UnityEngine;

public class EventParticleTogger : MonoBehaviour
{
    [Header("Event")]

    [SerializeField]
    private SOEvent _startEvent;

    [SerializeField]
    private SOEvent _stopEvent;

    [SerializeField]
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _startEvent.AddListener(PlayParticle);
        _stopEvent.AddListener(StopParticle);
    }

    private void OnDestroy()
    {
        _startEvent.RemoveListener(PlayParticle);
        _stopEvent.RemoveListener(StopParticle);
    }

    private void PlayParticle()
    {
        _particleSystem.Play();
    }

    private void StopParticle()
    {
        _particleSystem.Stop();
    }
}
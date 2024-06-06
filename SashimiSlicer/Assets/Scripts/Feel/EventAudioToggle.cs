using Events;
using UnityEngine;

public class EventAudioToggle : MonoBehaviour
{
    [SerializeField]
    private AudioSource _source;

    [SerializeField]
    private SOEvent _playEvent;

    [SerializeField]
    private SOEvent _stopEvent;

    private void Awake()
    {
        _playEvent.AddListener(PlayAudio);
        _stopEvent.AddListener(StopAudio);
    }

    private void OnDestroy()
    {
        _playEvent.RemoveListener(PlayAudio);
        _stopEvent.RemoveListener(StopAudio);
    }

    private void PlayAudio()
    {
        _source.Play();
    }

    private void StopAudio()
    {
        _source.Stop();
    }
}
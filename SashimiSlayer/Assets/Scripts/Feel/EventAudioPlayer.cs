using Events;
using UnityEngine;

namespace Feel
{
    /// <summary>
    ///  Plays an audio clip in response to an event.
    /// </summary>
    public class EventAudioPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioClip _clip;

        [SerializeField]
        private SOEvent _event;

        private void Awake()
        {
            _event.AddListener(PlayAudio);
        }

        private void OnDestroy()
        {
            _event.RemoveListener(PlayAudio);
        }

        private void PlayAudio()
        {
            SFXPlayer.Instance.PlaySFX(_clip);
        }
    }
}
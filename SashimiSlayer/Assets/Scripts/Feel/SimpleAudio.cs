using UnityEngine;

namespace Feel
{
    /// <summary>
    /// Exposes an audio clip to be played. Intended to be used with UnityEvents.
    /// </summary>
    public class SimpleAudio : MonoBehaviour
    {
        [SerializeField]
        private AudioClip _clip;

        public void Play()
        {
            SFXPlayer.Instance.PlaySFX(_clip);
        }
    }
}
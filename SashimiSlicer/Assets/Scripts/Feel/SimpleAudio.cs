using UnityEngine;

public class SimpleAudio : MonoBehaviour
{
    [SerializeField]
    private AudioClip _clip;

    public void Play()
    {
        AudioSource.PlayClipAtPoint(_clip, Vector3.zero);
    }
}
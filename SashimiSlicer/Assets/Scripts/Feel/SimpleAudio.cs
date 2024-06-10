using UnityEngine;

public class SimpleAudio : MonoBehaviour
{
    [SerializeField]
    private AudioClip _clip;

    public void Play()
    {
        SFXPlayer.Instance.PlaySFX(_clip);
    }
}
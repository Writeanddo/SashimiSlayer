using UnityEngine;

public class BnHAudioPlayer : MonoBehaviour
{
    [SerializeField]
    private BnHActionCore _bnHActionCore;

    [Header("Audio Clips")]

    [SerializeField]
    private AudioClip _spawnSound;

    [SerializeField]
    private AudioClip _startAttackSound;

    [SerializeField]
    private AudioClip _startVulnerableSound;

    private void Awake()
    {
        _bnHActionCore.OnSpawn += PlaySpawnSound;
        _bnHActionCore.OnTransitionToWaitingToAttack += PlayTransitionToWaitingToAttack;
        _bnHActionCore.OnTransitionToWaitingToVulnerable += PlayTransitionToWaitingToVulnerable;
    }

    private void OnDestroy()
    {
        _bnHActionCore.OnSpawn -= PlaySpawnSound;
        _bnHActionCore.OnTransitionToWaitingToAttack -= PlayTransitionToWaitingToAttack;
        _bnHActionCore.OnTransitionToWaitingToVulnerable -= PlayTransitionToWaitingToVulnerable;
    }

    private void PlaySpawnSound()
    {
        if (_spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(_spawnSound, Vector3.zero);
        }
    }

    private void PlayTransitionToWaitingToAttack(BnHActionCore.Timing timing,
        BnHActionCore.ScheduledInteraction scheduledInteraction)
    {
        if (_startAttackSound != null)
        {
            AudioSource.PlayClipAtPoint(_startAttackSound, Vector3.zero);
        }
    }

    private void PlayTransitionToWaitingToVulnerable(BnHActionCore.Timing timing,
        BnHActionCore.ScheduledInteraction scheduledInteraction)
    {
        if (_startVulnerableSound != null)
        {
            AudioSource.PlayClipAtPoint(_startVulnerableSound, Vector3.zero);
        }
    }
}
using UnityEngine;

public class FlyAwayOnParry : MonoBehaviour
{
    [SerializeField]
    private BnHActionCore _bnhActionCore;

    [Header("Visuals")]

    [SerializeField]
    private SpriteRenderer _sprite;

    [SerializeField]
    private ParticleSystem _explosionParticles;

    private bool _landedHit;

    private void Awake()
    {
        _bnhActionCore.OnTickWaitingToLeave += HandleTickWaitingToLeave;
        _bnhActionCore.OnLandHitOnProtag += HandleLandHitOnProtag;
        _bnhActionCore.OnKilled += HandleDied;
    }

    private void HandleDied(BnHActionCore.Timing timing)
    {
        _sprite.enabled = false;
    }

    private void HandleLandHitOnProtag()
    {
        _landedHit = true;
        _explosionParticles.Play();
    }

    private void HandleTickWaitingToLeave(BnHActionCore.Timing timing,
        BnHActionCore.BnHActionInstanceConfig bnHActionInstanceConfig)
    {
        if (_landedHit)
        {
            return;
        }

        transform.position += Vector3.up * Time.deltaTime * 7f;
        transform.position += Vector3.right * Time.deltaTime * 15f;

        _sprite.transform.rotation = Quaternion.Euler(0, 0, 1200 * (float)timing.CurrentBeatmapTime);
        _sprite.color = new Color(1, 1, 1, 0.7f);
    }
}
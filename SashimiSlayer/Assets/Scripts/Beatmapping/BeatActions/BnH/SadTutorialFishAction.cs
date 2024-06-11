using UnityEngine;

public class SadTutorialFishAction : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer _sprite;

    [SerializeField]
    private BnHActionCore _bnhActionCore;

    [SerializeField]
    private ParticleSystem[] _dieParticles;

    [SerializeField]
    private AnimationCurve _moveCurve;

    private Vector2 _startPos;
    private Vector2 _targetPos;

    private bool _landedHit;

    private void Awake()
    {
        _bnhActionCore.OnKilled += HandleKilled;

        _bnhActionCore.OnTickWaitingForVulnerable += HandleTickWaitingForVulnerable;
        _bnhActionCore.OnTickInVulnerable += HandleTickWaitingForVulnerable;

        _bnhActionCore.OnTickWaitingToLeave += HandleTickWaitingToLeave;
    }

    private void Start()
    {
        // Form an arc from start, with the peak at the target position
        _startPos = transform.position;
        _targetPos = _bnhActionCore.Data.Positions[1];
        _sprite.flipX = _targetPos.x > _startPos.x;
    }

    private void HandleTickWaitingToLeave(BnHActionCore.Timing timing,
        BnHActionCore.BnHActionInstanceConfig bnHActionInstanceConfig)
    {
        if (_landedHit)
        {
            return;
        }

        var normalizedTime = (float)timing.NormalizedLeaveWaitTime;

        float t = _moveCurve.Evaluate(1 - normalizedTime);

        // X velocity is constant, y uses curve
        transform.position = new Vector2(
            Mathf.Lerp(_targetPos.x, _targetPos.x + (_targetPos.x - _startPos.x), normalizedTime),
            Mathf.Lerp(_targetPos.y, _startPos.y, 1 - t)
        );

        _sprite.transform.rotation = Quaternion.Euler(0, 0, 90 * (1 - t));
        _sprite.color = new Color(1, 1, 1, 0.7f);
    }

    private void HandleTickWaitingForVulnerable(BnHActionCore.Timing timing,
        BnHActionCore.ScheduledInteraction interaction)
    {
        var normalizedTime = (float)timing.NormalizedInteractionWaitTime;

        float t = _moveCurve.Evaluate(normalizedTime);

        // X velocity is constant, y uses curve
        transform.position = new Vector2(
            Mathf.Lerp(_startPos.x, _targetPos.x, normalizedTime),
            Mathf.Lerp(_startPos.y, _targetPos.y, t)
        );

        _sprite.transform.rotation = Quaternion.Euler(0, 0, -90 * (1 - t));
    }

    private void HandleKilled(BnHActionCore.Timing timing)
    {
        _sprite.enabled = false;
        foreach (ParticleSystem particle in _dieParticles)
        {
            particle.Play();
        }
    }
}
using UnityEngine;
using UnityEngine.Events;

public class DazedOnParry : MonoBehaviour
{
    [SerializeField]
    private BnHActionCore _bnhActionCore;

    [Header("Positions")]

    [SerializeField]
    private int _startPosIndex;

    [SerializeField]
    private int _vulnerablePosIndex;

    [Header("Visuals")]

    [SerializeField]
    private SpriteRenderer _sprite;

    [SerializeField]
    private ParticleSystem[] _dieParticles;

    [SerializeField]
    private AnimationCurve _moveCurve;

    [Header("Events")]

    public UnityEvent OnEnterDazed;

    private Vector2 _targetPos;
    private Vector2 _startPos;
    private Vector2 _vulnerablePos;

    private bool _enteredDazed;

    private void Awake()
    {
        _bnhActionCore.OnTickWaitingForVulnerable += HandleTickWaitingForVulnerable;

        _bnhActionCore.OnTickInVulnerable += HandleTickWaitingForVulnerable;

        _bnhActionCore.OnTickWaitingToLeave += HandleTickWaitingToLeave;

        _bnhActionCore.OnKilled += HandleKilled;
    }

    private void Start()
    {
        _vulnerablePos = _bnhActionCore.Data.Positions[_vulnerablePosIndex];
        _startPos = _bnhActionCore.Data.Positions[_startPosIndex];
        _targetPos = Protaganist.Instance.SpritePosition;
    }

    private void HandleTickWaitingToLeave(BnHActionCore.Timing timing,
        BnHActionCore.BnHActionInstanceConfig bnHActionInstanceConfig)
    {
        // Lerp from the vulnerable position to the start position vertical (basically disappearing back into the sea)
        var normalizedTime = (float)timing.NormalizedLeaveWaitTime;

        float t = _moveCurve.Evaluate(1 - normalizedTime);

        // X velocity is constant, y uses curve
        transform.position = new Vector2(
            transform.position.x,
            Mathf.Lerp(_vulnerablePos.y, _startPos.y, 1 - t)
        );

        _sprite.transform.rotation = Quaternion.Euler(0, 0, 90 * (1 - t));
        _sprite.color = new Color(1, 1, 1, 0.7f);
    }

    private void HandleTickWaitingForVulnerable(BnHActionCore.Timing timing,
        BnHActionCore.ScheduledInteraction interaction)
    {
        if (!_enteredDazed)
        {
            _enteredDazed = true;
            OnEnterDazed.Invoke();
        }

        // Lerp from target pos to vulnerable pos
        var normalizedTime = (float)timing.NormalizedInteractionWaitTime;

        float t = _moveCurve.Evaluate(normalizedTime);

        // X velocity is constant, y uses curve
        transform.position = new Vector2(
            Mathf.Lerp(_targetPos.x, _vulnerablePos.x, normalizedTime),
            Mathf.Lerp(_targetPos.y, _vulnerablePos.y, t)
        );

        _sprite.transform.rotation = Quaternion.Euler(0, 0, 360f * (float)timing.CurrentBeatmapTime);
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
using UnityEngine;
using UnityEngine.Events;

public class ThreePointAttack : MonoBehaviour
{
    [SerializeField]
    private BnHActionCore _bnhActionCore;

    [Header("Positions")]

    [SerializeField]
    private int _startPosIndex;

    [SerializeField]
    private int _peakPosIndex;

    [Header("Visuals")]

    [SerializeField]
    private SpriteRenderer _sprite;

    [SerializeField]
    private AnimationCurve _moveCurve;

    [SerializeField]
    private ParticleSystem _explosionParticles;

    [Header("Events")]

    public UnityEvent OnHitPeak;

    private Vector2 _startPos;
    private Vector2 _peakPos;
    private Vector2 _targetPos;
    private float _angleToTarget;

    private bool _hitPeak;

    private void Awake()
    {
        _bnhActionCore.OnTickWaitingForAttack += HandleTickWaitingForAttack;
        _bnhActionCore.OnTickInAttack += HandleTickWaitingForAttack;
        _bnhActionCore.OnLandHitOnProtag += HandleLandHitOnProtag;
    }

    private void Start()
    {
        // Form an arc from start, with the peak at the target position
        _startPos = _bnhActionCore.Data.Positions[_startPosIndex];
        _peakPos = _bnhActionCore.Data.Positions[_peakPosIndex];
        _targetPos = Protaganist.Instance.SpritePosition;
        _sprite.flipX = _peakPos.x > _startPos.x;

        _angleToTarget = Mathf.Atan2(_targetPos.y - _peakPos.y, _targetPos.x - _peakPos.x) * Mathf.Rad2Deg;
    }

    private void HandleLandHitOnProtag()
    {
        _explosionParticles.Play();
    }

    private void HandleTickWaitingForAttack(BnHActionCore.Timing timing, BnHActionCore.ScheduledInteraction interaction)
    {
        // Lerp from start pos to peak pos, then to target pos
        var normalizedTime = (float)timing.NormalizedInteractionWaitTime;

        var thresh = 0.75f;
        if (normalizedTime <= thresh)
        {
            float t = _moveCurve.Evaluate(normalizedTime / thresh);
            transform.position = new Vector2(
                Mathf.Lerp(_startPos.x, _peakPos.x, normalizedTime / thresh),
                Mathf.Lerp(_startPos.y, _peakPos.y, t)
            );
            _sprite.transform.rotation = Quaternion.Euler(0, 0, -90 * (1 - t));
        }
        else
        {
            if (!_hitPeak && normalizedTime >= thresh)
            {
                _hitPeak = true;
                OnHitPeak.Invoke();
            }

            float remappedTime = (normalizedTime - thresh) / (1 - thresh);

            transform.position = new Vector2(
                Mathf.Lerp(_peakPos.x, _targetPos.x, remappedTime),
                Mathf.Lerp(_peakPos.y, _targetPos.y, remappedTime)
            );

            // angle towards target
            _sprite.transform.rotation = Quaternion.Euler(0, 0, 180 + _angleToTarget);
        }
    }
}
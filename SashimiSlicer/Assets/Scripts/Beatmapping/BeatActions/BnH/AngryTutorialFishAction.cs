using UnityEngine;

public class AngryTutorialFishAction : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer _sprite;

    [SerializeField]
    private BnHActionCore _bnhActionCore;

    [SerializeField]
    private ParticleSystem _explosionParticles;

    [SerializeField]
    private AnimationCurve _moveCurve;

    private BnHActionSo ActionConfigSo => _bnhActionCore.ActionConfigSo;

    private Vector2 _startPos;
    private Vector2 _peakPos;
    private Vector2 _targetPos;

    private bool _landedHit;

    private float _angleToTarget;

    private void Awake()
    {
        _bnhActionCore.OnTickWaitingForInteraction += HandleTickWaitingForInteraction;
        _bnhActionCore.OnTickInInteraction += HandleTickWaitingForInteraction;
        _bnhActionCore.OnTickWaitingToLeave += HandleTickWaitingToLeave;
        _bnhActionCore.OnLandHitOnProtag += HandleLandHitOnProtag;
        _bnhActionCore.OnKilled += HandleDied;
    }

    private void Start()
    {
        // Form an arc from start, with the peak at the target position
        _startPos = transform.position;
        _peakPos = _bnhActionCore.Data.Positions[1];
        _targetPos = Protaganist.Instance.SpritePosition;

        _sprite.flipX = _peakPos.x > _startPos.x;
        _angleToTarget = Mathf.Atan2(_targetPos.y - _peakPos.y, _targetPos.x - _peakPos.x) * Mathf.Rad2Deg;
    }

    private void HandleDied()
    {
        _sprite.enabled = false;
    }

    private void HandleLandHitOnProtag()
    {
        _landedHit = true;
        _explosionParticles.Play();
    }

    private void HandleTickWaitingToLeave(double time, BnHActionCore.BnHActionInstanceConfig bnHActionInstanceConfig)
    {
        if (_landedHit)
        {
            return;
        }

        transform.position += Vector3.up * Time.deltaTime * 7f;
        transform.position += Vector3.right * Time.deltaTime * 15f;

        _sprite.transform.rotation = Quaternion.Euler(0, 0, 1200 * (float)time);
    }

    private void HandleTickWaitingForInteraction(double time, BnHActionCore.ScheduledInteraction interaction)
    {
        if (interaction.Interaction.InteractionType == BnHActionCore.InteractionType.IncomingAttack)
        {
            UpdatePosition(time, interaction);
        }
    }

    private void UpdatePosition(double time, BnHActionCore.ScheduledInteraction interaction)
    {
        double attackMiddleTime = interaction.TimeWhenInteractWindowEnd;

        float normalizedTime = Mathf.InverseLerp(
            (float)_bnhActionCore.LastInteractionEndTime,
            (float)attackMiddleTime,
            (float)time);

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
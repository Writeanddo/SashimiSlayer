using UnityEngine;

public class MootTutorialFishAction : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer _sprite;

    [SerializeField]
    private BnHActionCore _bnhActionCore;

    [SerializeField]
    private ParticleSystem _explosionParticles;

    [SerializeField]
    private ParticleSystem[] _dieParticles;

    [SerializeField]
    private AnimationCurve _moveCurve;

    private BnHActionSo ActionConfigSo => _bnhActionCore.ActionConfigSo;

    private Vector2 _startPos;
    private Vector2 _peakPos;
    private Vector2 _targetPos;
    private Vector2 _vulnerablePos;

    private bool _landedHit;

    private float _angleToTarget;

    private void Awake()
    {
        _bnhActionCore.OnTickWaitingForInteraction += HandleTickWaitingForInteraction;
        _bnhActionCore.OnTickInInteraction += HandleTickWaitingForInteraction;
        _bnhActionCore.OnTickWaitingToLeave += HandleTickWaitingToLeave;
        _bnhActionCore.OnLandHitOnProtag += HandleLandHitOnProtag;
        _bnhActionCore.OnKilled += HandleKilled;
    }

    private void Start()
    {
        // Form an arc from start, with the peak at the target position
        _startPos = transform.position;
        _peakPos = _bnhActionCore.Data.Positions[1];
        _vulnerablePos = _bnhActionCore.Data.Positions[2];
        _targetPos = Protaganist.Instance.SpritePosition;

        _sprite.flipX = _peakPos.x > _startPos.x;
        _angleToTarget = Mathf.Atan2(_targetPos.y - _peakPos.y, _targetPos.x - _peakPos.x) * Mathf.Rad2Deg;
    }

    private void HandleLandHitOnProtag()
    {
        _landedHit = true;
        _explosionParticles.Play();

        // Hide death particles (since the action technically dies on landing a hit)
        foreach (ParticleSystem particle in _dieParticles)
        {
            particle.gameObject.SetActive(false);
        }
    }

    private void HandleTickWaitingToLeave(double time, BnHActionCore.BnHActionInstanceConfig bnHActionInstanceConfig)
    {
        if (_landedHit)
        {
            return;
        }

        // Lerp from the vulnerable position to the start position vertical (basically disappearing back into the sea)

        double leaveTime = _bnhActionCore.Data.ActionEndTime;

        float normalizedTime = Mathf.InverseLerp(
            (float)_bnhActionCore.LastInteractionEndTime,
            (float)leaveTime,
            (float)time);

        float t = _moveCurve.Evaluate(1 - normalizedTime);

        // X velocity is constant, y uses curve
        transform.position = new Vector2(
            transform.position.x,
            Mathf.Lerp(_vulnerablePos.y, _startPos.y, 1 - t)
        );

        _sprite.transform.rotation = Quaternion.Euler(0, 0, 90 * (1 - t));
        _sprite.color = new Color(1, 1, 1, 0.7f);
    }

    private void HandleTickWaitingForInteraction(double time, BnHActionCore.ScheduledInteraction interaction)
    {
        if (interaction.Interaction.InteractionType == BnHActionCore.InteractionType.IncomingAttack)
        {
            UpdatePositionIncomingAttack(time, interaction);
        }
        else
        {
            UpdatePositionVulnerable(time, interaction);
        }
    }

    private void UpdatePositionIncomingAttack(double time, BnHActionCore.ScheduledInteraction interaction)
    {
        // Lerp from start pos to peak pos, then to target pos

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

    private void UpdatePositionVulnerable(double time, BnHActionCore.ScheduledInteraction interaction)
    {
        // Lerp from target pos to vulnerable pos

        double attackMiddleTime = interaction.TimeWhenInteractWindowEnd;

        float normalizedTime = Mathf.InverseLerp(
            (float)_bnhActionCore.LastInteractionEndTime,
            (float)attackMiddleTime,
            (float)time);

        float t = _moveCurve.Evaluate(normalizedTime);

        // X velocity is constant, y uses curve
        transform.position = new Vector2(
            Mathf.Lerp(_targetPos.x, _vulnerablePos.x, normalizedTime),
            Mathf.Lerp(_targetPos.y, _vulnerablePos.y, t)
        );

        _sprite.transform.rotation = Quaternion.Euler(0, 0, 360f * (float)time);
    }

    private void HandleKilled()
    {
        _sprite.enabled = false;
        foreach (ParticleSystem particle in _dieParticles)
        {
            particle.Play();
        }
    }
}
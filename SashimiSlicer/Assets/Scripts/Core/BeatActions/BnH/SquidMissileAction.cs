using UnityEngine;

public class SquidMissileAction : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private BnHActionCore _bnhActionCore;

    [SerializeField]
    private ParticleSystem _dieParticles;

    private BnHActionSo ActionConfigSo => _bnhActionCore.ActionConfigSo;

    private Vector2 _startPos;
    private Vector2 _targetPos;

    private bool _landedHit;

    private void Awake()
    {
        _bnhActionCore.OnKilled += HandleKilled;
        _bnhActionCore.OnTickWaitingForInteraction += HandleTickWaitingForInteraction;
        _bnhActionCore.OnTickInInteraction += HandleTickInInteraction;
        _bnhActionCore.OnTickWaitingToLeave += HandleTickWaitingToLeave;
        _bnhActionCore.OnLandHitOnProtag += HandleLandHitOnProtag;

        _targetPos = Protaganist.Instance.SpritePosition;
    }

    private void Start()
    {
        _startPos = transform.position;
    }

    private void HandleLandHitOnProtag()
    {
        _landedHit = true;
        _animator.Play("Explode");
        _dieParticles.Play();
    }

    private void HandleTickWaitingToLeave(double time, BnHActionCore.BnHActionInstanceConfig bnHActionInstanceConfig)
    {
        if (_landedHit)
        {
            return;
        }

        _animator.transform.Rotate(Vector3.forward, 2000f * Time.deltaTime);

        transform.position += Vector3.up * Time.deltaTime * 10f;
        transform.position += Vector3.right * Time.deltaTime * 10f;
    }

    private void HandleTickWaitingForInteraction(double time, BnHActionCore.ScheduledInteraction interaction)
    {
        if (interaction.Interaction.InteractionType == BnHActionCore.InteractionType.IncomingAttack)
        {
            UpdatePosition(time, interaction);
        }
    }

    private void HandleTickInInteraction(double time, BnHActionCore.ScheduledInteraction interaction)
    {
        if (interaction.Interaction.InteractionType == BnHActionCore.InteractionType.IncomingAttack)
        {
            UpdatePosition(time, interaction);
        }
    }

    private void UpdatePosition(double time, BnHActionCore.ScheduledInteraction interaction)
    {
        double attackStartTime =
            interaction.TimeWhenInteractionStart;
        double attackMiddleTime = interaction.TimeWhenInteractWindowEnd;

        float t = Mathf.InverseLerp((float)_bnhActionCore.LastInteractionEndTime, (float)attackMiddleTime,
            (float)time);

        t *= t;

        transform.position = Vector2.Lerp(_startPos, _targetPos,
            t);

        _animator.transform.rotation = Quaternion.Euler(0, 0,
            Mathf.Atan2(_targetPos.y - _startPos.y, _targetPos.x - _startPos.x) * Mathf.Rad2Deg);
    }

    private void HandleKilled()
    {
        _dieParticles.Play();
    }
}
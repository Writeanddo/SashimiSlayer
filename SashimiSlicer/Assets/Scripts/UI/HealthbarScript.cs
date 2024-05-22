using DG.Tweening;
using Events;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarScript : MonoBehaviour
{
    [SerializeField]
    private Slider _healthbar;

    [SerializeField]
    private FloatEvent _healthChangeEvent;

    [SerializeField]
    private FloatEvent _maxHealthChangeEvent;

    private readonly float _damageLingerTime = 0.5f;

    private void Awake()
    {
        _healthChangeEvent.AddListener(TakeDamage);
        _maxHealthChangeEvent.AddListener(InitializeBar);
    }

    private void OnDestroy()
    {
        _healthChangeEvent.RemoveListener(TakeDamage);
        _maxHealthChangeEvent.RemoveListener(InitializeBar);
    }

    public void InitializeBar(float maxHealth)
    {
        _healthbar.DOKill();
        _healthbar.maxValue = maxHealth;
        _healthbar.value = maxHealth;
    }

    public void TakeDamage(float newHealth)
    {
        _healthbar.DOValue(newHealth, _damageLingerTime);
    }
}
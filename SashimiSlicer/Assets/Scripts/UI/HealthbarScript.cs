using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarScript : MonoBehaviour
{
    [SerializeField]
    private Slider _healthbar;

    private readonly float _damageLingerTime = 0.5f;

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
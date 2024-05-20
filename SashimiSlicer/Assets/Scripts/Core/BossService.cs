using UnityEngine;

public class BossService : MonoBehaviour
{
    [SerializeField]
    private HealthbarScript _healthbar;

    public static BossService Instance { get; private set; }

    private float _health;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeBoss(BeatmapConfigSO beatmapConfigSo)
    {
        _healthbar.InitializeBar(beatmapConfigSo.BossHealth);
        _health = beatmapConfigSo.BossHealth;
    }

    public void TakeDamage(float damage)
    {
        float newHealth = Mathf.Max(0, _health - damage);
        _health = newHealth;
        _healthbar.TakeDamage(newHealth);
    }
}
using Events;
using Events.Core;
using UnityEngine;

public class BossService : MonoBehaviour
{
    [SerializeField]
    private BeatmapEvent _startBeatmapEvent;

    [SerializeField]
    private FloatEvent _bossHealthEvent;

    [SerializeField]
    private FloatEvent _bossMaxHealthEvent;

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

        _startBeatmapEvent.AddListener(HandleStartBeatmap);
    }

    private void OnDestroy()
    {
        _startBeatmapEvent.RemoveListener(HandleStartBeatmap);
    }

    private void HandleStartBeatmap(BeatmapConfigSo beatmapConfigSo)
    {
        _health = beatmapConfigSo.BossHealth;
        _bossMaxHealthEvent.Raise(beatmapConfigSo.BossHealth);
    }

    public void TakeDamage(float damage)
    {
        float newHealth = Mathf.Max(0, _health - damage);
        _health = newHealth;
        _bossHealthEvent.Raise(newHealth);
    }
}
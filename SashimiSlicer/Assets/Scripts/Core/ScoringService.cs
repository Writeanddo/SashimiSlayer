using Events;
using Events.Core;
using UnityEngine;

public class ScoringService : MonoBehaviour
{
    [Header("Listening Events")]

    [SerializeField]
    private BeatInteractionResultEvent _beatInteractionResultEvent;

    [SerializeField]
    private BeatmapEvent _loadBeatmapEvent;

    [SerializeField]
    private VoidEvent _playerDeathEvent;

    private int _successes;
    private int _failures;
    private bool _didPlayerDie;

    private void Awake()
    {
        _beatInteractionResultEvent.AddListener(OnBeatInteractionResult);
        _loadBeatmapEvent.AddListener(OnLoadBeatmap);
        _playerDeathEvent.AddListener(HandlePlayerDeath);
    }

    private void OnDestroy()
    {
        _beatInteractionResultEvent.RemoveListener(OnBeatInteractionResult);
        _loadBeatmapEvent.RemoveListener(OnLoadBeatmap);
        _playerDeathEvent.RemoveListener(HandlePlayerDeath);
    }

    private void OnGUI()
    {
        GUILayout.Space(100);
        GUILayout.Label($"Successes: {_successes}");
        GUILayout.Label($"Failures: {_failures}");
    }

    private void OnBeatInteractionResult(BeatActionService.BeatInteractionResult result)
    {
        if (result.WasSuccessful)
        {
            _successes++;
        }
        else
        {
            _failures++;
        }
    }

    private void HandlePlayerDeath()
    {
        _didPlayerDie = true;
    }

    private void OnLoadBeatmap(BeatmapConfigSo beatmap)
    {
        _successes = 0;
        _failures = 0;
        _didPlayerDie = false;
    }
}
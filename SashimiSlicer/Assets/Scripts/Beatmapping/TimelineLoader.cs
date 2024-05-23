using Events;
using Events.Core;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineLoader : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector _director;

    [Header("Listening Events")]

    [SerializeField]
    private BeatmapEvent _beatmapLoadedEvent;

    [SerializeField]
    private VoidEvent _playerDeathEvent;

    [SerializeField]
    private GameLevelSO _levelResultLevel;

    private bool _inProgress;

    private void Awake()
    {
        _beatmapLoadedEvent.AddListener(HandleStartBeatmap);
        _playerDeathEvent.AddListener(HandlePlayerDeath);
    }

    private void Update()
    {
        if (_inProgress && _director.state != PlayState.Playing)
        {
            _director.Stop();
            LevelLoader.Instance.LoadLevel(_levelResultLevel);
            _inProgress = false;
        }
    }

    private void OnDestroy()
    {
        _beatmapLoadedEvent.RemoveListener(HandleStartBeatmap);
        _playerDeathEvent.RemoveListener(HandlePlayerDeath);
    }

    private void HandlePlayerDeath()
    {
        _inProgress = false;
        _director.Pause();
    }

    private void HandleStartBeatmap(BeatmapConfigSo beatmap)
    {
        _director.playableAsset = beatmap.BeatmapTimeline;
        _director.Play();
        _inProgress = true;
    }
}
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

    [SerializeField]
    private DoubleEvent _syncTimeEvent;

    private bool _inProgress;

    private void Awake()
    {
        _beatmapLoadedEvent.AddListener(HandleStartBeatmap);
        _playerDeathEvent.AddListener(HandlePlayerDeath);
        _syncTimeEvent.AddListener(HandleSyncTime);
        _director.stopped += HandleTimelineStopped;
    }

    private void Update()
    {
        Debug.Log("Dir: " + _director.time);
    }

    private void OnDestroy()
    {
        _beatmapLoadedEvent.RemoveListener(HandleStartBeatmap);
        _playerDeathEvent.RemoveListener(HandlePlayerDeath);
        _syncTimeEvent.RemoveListener(HandleSyncTime);
    }

    private void HandleSyncTime(double time)
    {
        _director.time = time;
    }

    private void HandleTimelineStopped(PlayableDirector obj)
    {
        // Check if reached end of timeline
        if (_inProgress)
        {
            _director.Stop();
            LevelLoader.Instance.LoadLevel(_levelResultLevel);
            _inProgress = false;
        }
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
        _director.Evaluate();
        _inProgress = true;
    }
}
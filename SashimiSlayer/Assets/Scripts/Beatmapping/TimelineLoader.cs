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
        _director.stopped += HandleTimelineStopped;

        // Use manual to play with FMOD dsp time
        _director.timeUpdateMode = DirectorUpdateMode.Manual;
    }

    private void OnEnable()
    {
        TimingService.Instance.OnTick += TimeService_OnTick;
    }

    private void OnDisable()
    {
        TimingService.Instance.OnTick -= TimeService_OnTick;
    }

    private void OnDestroy()
    {
        _beatmapLoadedEvent.RemoveListener(HandleStartBeatmap);
        _playerDeathEvent.RemoveListener(HandlePlayerDeath);
    }

    private void TimeService_OnTick(TimingService.TickInfo tickInfo)
    {
        if (_inProgress)
        {
            _director.time = tickInfo.TimeSinceBeatmapLoad;

            if (_director.time >= _director.duration)
            {
                _director.Stop();
            }
        }

        Debug.Log("Dir: " + _director.time);
        _director.Evaluate();
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
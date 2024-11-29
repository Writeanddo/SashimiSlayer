using Beatmapping.Timing;
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

    private bool _inProgress;

    private void Awake()
    {
        _beatmapLoadedEvent.AddListener(HandleStartBeatmap);
        _playerDeathEvent.AddListener(HandlePlayerDeath);

        // Use manual to play with FMOD dsp time
        _director.timeUpdateMode = DirectorUpdateMode.Manual;
    }

    private void OnEnable()
    {
        BeatmapTimeManager.Instance.OnTick += TimeService_OnTick;
    }

    private void OnDisable()
    {
        BeatmapTimeManager.Instance.OnTick -= TimeService_OnTick;
    }

    private void OnDestroy()
    {
        _beatmapLoadedEvent.RemoveListener(HandleStartBeatmap);
        _playerDeathEvent.RemoveListener(HandlePlayerDeath);
    }

    private void TimeService_OnTick(BeatmapTimeManager.TickInfo tickInfo)
    {
        if (_inProgress)
        {
            _director.time = tickInfo.CurrentLevelTime;

            if (_director.time >= _director.duration)
            {
                _director.Stop();
            }
        }

        _director.Evaluate();
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
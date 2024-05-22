using Events.Core;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineLoader : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector _director;

    [SerializeField]
    private BeatmapEvent _beatmapLoadedEvent;

    private void Awake()
    {
        _beatmapLoadedEvent.AddListener(HandleStartBeatmap);
    }

    private void OnDestroy()
    {
        _beatmapLoadedEvent.RemoveListener(HandleStartBeatmap);
    }

    private void HandleStartBeatmap(BeatmapConfigSo beatmap)
    {
        _director.playableAsset = beatmap.BeatmapTimeline;
        _director.Play();
    }
}
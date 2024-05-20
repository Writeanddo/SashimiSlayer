using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

public class BeatmapService : MonoBehaviour
{
    [SerializeField]
    private BeatmapConfigSO _beatmapConfig;

    [SerializeField]
    private TimingService _timingService;

    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private PlayableDirector _timelineDirector;

    public static BeatmapService Instance { get; private set; }

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

        Application.targetFrameRate = 60;
    }

    [Button("START!!!", ButtonSizes.Large)]
    public void StartBeatmap()
    {
        /*if (!Application.isPlaying)
        {
            Debug.LogError("Cannot start beatmap in edit mode!");
            return;
        }*/

        _timelineDirector.playableAsset = _beatmapConfig.BeatmapTimeline;
        _timelineDirector.Play();

        _timingService.StartBeatmap(_beatmapConfig);

        BossService.Instance.InitializeBoss(_beatmapConfig);
    }

    /// <summary>
    ///     Restart the beatmap, resyncing the time manager
    /// </summary>
    public void LoopSynced()
    {
        _timelineDirector.time = 0;
        _timelineDirector.Play();
        _timelineDirector.Evaluate();
        _timingService.Resync();
        Debug.Log("Resynced");
    }
}
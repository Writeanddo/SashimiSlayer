using Events;
using Events.Core;
using UnityEngine;

public class ScoringService : MonoBehaviour
{
    public struct BeatmapScore
    {
        public int Successes;
        public int Failures;
    }

    [Header("Listening Events")]

    [SerializeField]
    private BeatInteractionResultEvent _beatInteractionResultEvent;

    [SerializeField]
    private BeatmapEvent _loadBeatmapEvent;

    [SerializeField]
    private VoidEvent _playerDeathEvent;

    [SerializeField]
    private VoidEvent _onDrawGuiEvent;

    public static ScoringService Instance { get; private set; }

    public BeatmapScore CurrentScore => _currentScore;

    private bool _didPlayerDie;

    private BeatmapScore _currentScore;

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

        _beatInteractionResultEvent.AddListener(OnBeatInteractionResult);
        _loadBeatmapEvent.AddListener(OnLoadBeatmap);
        _playerDeathEvent.AddListener(HandlePlayerDeath);
        _onDrawGuiEvent.AddListener(DrawGUI);
    }

    private void OnDestroy()
    {
        _beatInteractionResultEvent.RemoveListener(OnBeatInteractionResult);
        _loadBeatmapEvent.RemoveListener(OnLoadBeatmap);
        _playerDeathEvent.RemoveListener(HandlePlayerDeath);
        _onDrawGuiEvent.RemoveListener(DrawGUI);
    }

    private void DrawGUI()
    {
        GUILayout.Label(JsonUtility.ToJson(_currentScore));
    }

    private void OnBeatInteractionResult(SharedTypes.BeatInteractionResult result)
    {
        if (result.Result == SharedTypes.BeatInteractionResultType.Successful)
        {
            _currentScore.Successes++;
        }
        else
        {
            _currentScore.Failures++;
        }
    }

    private void HandlePlayerDeath()
    {
        _didPlayerDie = true;
    }

    private void OnLoadBeatmap(BeatmapConfigSo beatmap)
    {
        _currentScore = new BeatmapScore();
        _didPlayerDie = false;
    }
}
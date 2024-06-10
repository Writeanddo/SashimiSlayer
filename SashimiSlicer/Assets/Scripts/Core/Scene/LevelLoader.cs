using Cysharp.Threading.Tasks;
using Events.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField]
    private SceneTransitionUI _sceneTransitionUI;

    [Header("Invoking Events")]

    [SerializeField]
    private BeatmapEvent _beatmapStartEvent;

    public static LevelLoader Instance { get; private set; }

    private string _currentLevel = string.Empty;
    private GameLevelSO _currentLevelSo;
    private GameLevelSO _previousBeatmapLevel;

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
#if UNITY_EDITOR
        if (SceneManager.sceneCount > 1)
        {
            _currentLevel = SceneManager.GetSceneAt(1).name;
        }
#endif
    }

    public async UniTask LoadLevel(GameLevelSO gameLevel)
    {
        _sceneTransitionUI.SetTitleText(gameLevel.LevelTitle);

        await _sceneTransitionUI.FadeOut();

        if (gameLevel.LevelType == GameLevelSO.LevelTypes.Gameplay)
        {
            gameLevel.PreloadMusic.LoadAudioData();
            while (gameLevel.PreloadMusic.loadState != AudioDataLoadState.Loaded)
            {
                await UniTask.Yield();
            }
        }

        if (_currentLevelSo != null && _currentLevelSo.LevelType == GameLevelSO.LevelTypes.Gameplay)
        {
            _currentLevelSo.PreloadMusic.UnloadAudioData();
        }

        string sceneName = gameLevel.GameSceneName;

        if (SceneManager.GetSceneByName(_currentLevel).isLoaded)
        {
            await SceneManager.UnloadSceneAsync(_currentLevel);
        }

        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _currentLevel = sceneName;
        _currentLevelSo = gameLevel;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        if (gameLevel.LevelType == GameLevelSO.LevelTypes.Gameplay)
        {
            _beatmapStartEvent.Raise(gameLevel.Beatmap);
            _previousBeatmapLevel = gameLevel;
        }

        await _sceneTransitionUI.FadeIn();
    }

    public void LoadLastBeatmapLevel()
    {
        if (_previousBeatmapLevel == null)
        {
            Debug.LogWarning("No previous beatmap level to load");
        }

        LoadLevel(_previousBeatmapLevel);
    }
}
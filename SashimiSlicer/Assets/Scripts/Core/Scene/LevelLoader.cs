using Cysharp.Threading.Tasks;
using Events.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField]
    private SceneTransitionUI _sceneTransitionUI;

    [SerializeField]
    private BeatmapEvent _beatmapStartEvent;

    public static LevelLoader Instance { get; private set; }

    private string currentLevel = string.Empty;

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
            currentLevel = SceneManager.GetSceneAt(1).name;
        }
#endif
    }

    public async UniTask LoadLevel(GameLevelSO gameLevel)
    {
        await _sceneTransitionUI.FadeOut();

        string sceneName = gameLevel.GameSceneName;

        if (SceneManager.GetSceneByName(currentLevel).isLoaded)
        {
            await SceneManager.UnloadSceneAsync(currentLevel);
        }

        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        currentLevel = sceneName;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        _sceneTransitionUI.SetTitleText(gameLevel.LevelTitle);

        if (gameLevel.LevelType == GameLevelSO.LevelTypes.Gameplay)
        {
            _beatmapStartEvent.Raise(gameLevel.Beatmap);
        }

        await _sceneTransitionUI.FadeIn();
    }
}
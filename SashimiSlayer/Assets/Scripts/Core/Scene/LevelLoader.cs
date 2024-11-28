using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Events.Core;
using FMODUnity;
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

    public GameLevelSO CurrentLevel { get; private set; }

    private string _currentLevel = string.Empty;
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

    private void OnDestroy()
    {
        Debug.Log($"Killed {DOTween.KillAll()} tweens");
    }

    public async UniTask LoadLevel(GameLevelSO gameLevel)
    {
        _sceneTransitionUI.SetTitleText(gameLevel.LevelTitle);

        await _sceneTransitionUI.FadeOut();
        Debug.Log($"Killed {DOTween.KillAll()} tweens");

        if (CurrentLevel != null && CurrentLevel.LevelType == GameLevelSO.LevelTypes.Gameplay)
        {
            UnloadBanks(CurrentLevel.FmodBanksToPreLoad);
        }

        if (gameLevel.LevelType == GameLevelSO.LevelTypes.Gameplay)
        {
            LoadBanks(gameLevel.FmodBanksToPreLoad);
        }

        string sceneName = gameLevel.GameSceneName;

        if (SceneManager.GetSceneByName(_currentLevel).isLoaded)
        {
            await SceneManager.UnloadSceneAsync(_currentLevel);
        }

        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _currentLevel = sceneName;
        CurrentLevel = gameLevel;
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

    private void LoadBanks(List<string> banks)
    {
        foreach (string bankRef in banks)
        {
            try
            {
                // Preload sample data to avoid latency on play
                RuntimeManager.LoadBank(bankRef, true);
            }
            catch (BankLoadException e)
            {
                RuntimeUtils.DebugLogException(e);
            }
        }

        RuntimeManager.WaitForAllSampleLoading();
    }

    private void UnloadBanks(List<string> banks)
    {
        foreach (string bankRef in banks)
        {
            RuntimeManager.UnloadBank(bankRef);
        }
    }
}
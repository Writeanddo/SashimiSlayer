using Core.Scene;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Dependencies")]

    [SerializeField]
    private LevelRosterSO _levelRoster;

    [SerializeField]
    private LevelSelectButton _levelSelectButtonPrefab;

    [SerializeField]
    private Transform _layoutGroup;

    private bool _loaded;

    private void Awake()
    {
        SetupLevelSelectUI();
    }

    private void SetupLevelSelectUI()
    {
        foreach (GameLevelSO level in _levelRoster.Levels)
        {
            LevelSelectButton levelSelectButton = Instantiate(_levelSelectButtonPrefab, _layoutGroup);
            levelSelectButton.SetupUI(level);
            levelSelectButton.OnLevelSelected += OnLevelSelected;
        }
    }

    private void OnLevelSelected(GameLevelSO level)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        LevelLoader.Instance.LoadLevel(level).Forget();
    }
}
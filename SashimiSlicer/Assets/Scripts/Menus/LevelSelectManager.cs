using System.Collections.Generic;
using Events.Core;
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

    [SerializeField]
    private ProtagSwordStateEvent _protagTryBlockEvent;

    private readonly List<LevelSelectButton> _levelSelectButtons = new();

    private bool _loaded;

    private int _hoveredLevelIndex;

    private bool _levelChosen;

    private void Awake()
    {
        _protagTryBlockEvent.AddListener(HandleProtagTryBlock);

        SetupLevelSelectUI();
    }

    private void OnDestroy()
    {
        _protagTryBlockEvent.RemoveListener(HandleProtagTryBlock);
    }

    private void HandleProtagTryBlock(Protaganist.ProtagSwordState swordState)
    {
        int newLevelIndex = _hoveredLevelIndex;
        if (swordState.BlockPose == SharedTypes.BlockPoseStates.TopPose)
        {
            newLevelIndex = Mathf.Clamp(_hoveredLevelIndex - 1, 0, _levelSelectButtons.Count - 1);
        }
        else if (swordState.BlockPose == SharedTypes.BlockPoseStates.BotPose)
        {
            newLevelIndex = Mathf.Clamp(_hoveredLevelIndex + 1, 0, _levelSelectButtons.Count - 1);
        }
        else if (swordState.BlockPose == SharedTypes.BlockPoseStates.MidPose)
        {
            StartSelectedLevel();
            _levelChosen = true;
        }

        if (newLevelIndex != _hoveredLevelIndex)
        {
            _levelSelectButtons[_hoveredLevelIndex].SetHovered(false);
            _hoveredLevelIndex = newLevelIndex;
            _levelSelectButtons[_hoveredLevelIndex].SetHovered(true);
        }
    }

    private void StartSelectedLevel()
    {
        if (_levelChosen)
        {
            return;
        }

        if (LevelLoader.Instance == null)
        {
            Debug.LogError("LevelLoader is null");
            return;
        }

        LevelLoader.Instance.LoadLevel(_levelRoster.Levels[_hoveredLevelIndex]);
        _levelChosen = true;
    }

    private void SetupLevelSelectUI()
    {
        foreach (GameLevelSO level in _levelRoster.Levels)
        {
            LevelSelectButton levelSelectButton = Instantiate(_levelSelectButtonPrefab, _layoutGroup);
            levelSelectButton.SetupUI(level);
            _levelSelectButtons.Add(levelSelectButton);
        }

        _levelSelectButtons[_hoveredLevelIndex].SetHovered(true);
    }
}
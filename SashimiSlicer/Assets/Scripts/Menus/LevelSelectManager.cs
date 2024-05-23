using System.Collections.Generic;
using Events;
using Events.Core;
using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Listening Events")]

    [SerializeField]
    private ProtagSwordStateEvent _protagTryBlockEvent;

    [SerializeField]
    private VoidEvent _startLevelEvent;

    [Header("Dependencies")]

    [SerializeField]
    private LevelRosterSO _levelRoster;

    [SerializeField]
    private LevelSelectButton _levelSelectButtonPrefab;

    [SerializeField]
    private Transform _layoutGroup;

    private readonly List<LevelSelectButton> _levelSelectButtons = new();

    private int _hoveredLevelIndex;

    private bool _levelChosen;

    private void Awake()
    {
        _protagTryBlockEvent.AddListener(OnProtagTryBlock);
        _startLevelEvent.AddListener(StartSelectedLevel);
        SetupLevelSelectUI();
    }

    private void OnDestroy()
    {
        _protagTryBlockEvent.RemoveListener(OnProtagTryBlock);
        _startLevelEvent.RemoveListener(StartSelectedLevel);
    }

    private void OnProtagTryBlock(Protaganist.ProtagSwordState swordState)
    {
        int newLevelIndex = _hoveredLevelIndex;
        if (swordState.BlockPose == Gameplay.BlockPoseStates.TopPose)
        {
            newLevelIndex = Mathf.Clamp(_hoveredLevelIndex - 1, 0, _levelSelectButtons.Count - 1);
        }
        else if (swordState.BlockPose == Gameplay.BlockPoseStates.BotPose)
        {
            newLevelIndex = Mathf.Clamp(_hoveredLevelIndex + 1, 0, _levelSelectButtons.Count - 1);
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
            _levelSelectButtons.Add(levelSelectButton);
            levelSelectButton.SetupUI(level);
        }

        _levelSelectButtons[_hoveredLevelIndex].SetHovered(true);
    }
}
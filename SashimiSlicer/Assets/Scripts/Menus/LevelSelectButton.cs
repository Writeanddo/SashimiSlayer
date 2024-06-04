using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _levelNameText;

    [SerializeField]
    private TMP_Text _levelDescriptionText;

    public event Action<GameLevelSO> OnLevelSelected;

    private GameLevelSO _level;

    public void SetupUI(GameLevelSO level)
    {
        _levelNameText.text = level.LevelTitle;
        _levelDescriptionText.text = level.LevelDescription;
        _level = level;
    }

    public void SetHovered(bool val)
    {
        if (val)
        {
            transform.DOScale(1.25f, 0.15f);
        }
        else
        {
            transform.DOScale(1f, 0.15f);
        }
    }

    public void SelectLevel()
    {
        OnLevelSelected?.Invoke(_level);
    }
}
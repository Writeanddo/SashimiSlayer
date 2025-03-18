using System.Collections.Generic;
using Core.Scene;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelRoster", menuName = "MainMenu/LevelRoster")]
public class LevelRosterSO : ScriptableObject
{
    [field: SerializeField]
    public List<GameLevelSO> Levels { get; private set; }

    public void WipeHighScores()
    {
        Debug.Log("Wiping high scores");
        foreach (GameLevelSO level in Levels)
        {
            PlayerPrefs.SetFloat($"{level.Beatmap.BeatmapName}.highscore", 0);
        }
    }
}
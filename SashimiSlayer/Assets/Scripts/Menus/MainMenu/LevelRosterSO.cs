using System.Collections.Generic;
using Core.Scene;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelRoster", menuName = "MainMenu/LevelRoster")]
public class LevelRosterSO : ScriptableObject
{
    [field: SerializeField]
    public List<GameLevelSO> Levels { get; private set; }
}
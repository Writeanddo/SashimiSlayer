using UnityEngine;

[CreateAssetMenu(fileName = "BootupConfigSO", menuName = "BootupConfigSO", order = 51)]
public class BootupConfigSO : ScriptableObject
{
    [field: SerializeField]
    public GameLevelSO InitialGameLevel { get; private set; }
}
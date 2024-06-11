using UnityEngine;

public class UtilsPrefs : ScriptableObject
{
    [field: SerializeField]
    public string StartupScenePath { get; private set; }
}
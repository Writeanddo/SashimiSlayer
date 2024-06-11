using UnityEngine;

public class LevelLoaderMono : MonoBehaviour
{
    public void LoadLevel(GameLevelSO level)
    {
        LevelLoader.Instance.LoadLevel(level);
    }

    public void LoadLastBeatmapLevel()
    {
        LevelLoader.Instance.LoadLastBeatmapLevel();
    }
}
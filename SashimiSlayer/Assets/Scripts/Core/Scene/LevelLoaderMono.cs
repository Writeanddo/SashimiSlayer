using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Scene
{
    public class LevelLoaderMono : MonoBehaviour
    {
        public void LoadLevel(GameLevelSO level)
        {
            LevelLoader.Instance.LoadLevel(level).Forget();
        }

        public void LoadLastBeatmapLevel()
        {
            LevelLoader.Instance.LoadLastBeatmapLevel();
        }
    }
}
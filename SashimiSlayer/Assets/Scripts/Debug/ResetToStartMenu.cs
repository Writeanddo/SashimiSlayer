using Core.Scene;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ResetToStartMenu : MonoBehaviour
{
    [SerializeField]
    private GameLevelSO _levelToLoad;

    public void LoadLevel()
    {
        LevelLoader.Instance.LoadLevel(_levelToLoad).Forget();
    }
}
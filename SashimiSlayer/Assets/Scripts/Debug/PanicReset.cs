using Cysharp.Threading.Tasks;
using UnityEngine;

public class PanicReset : MonoBehaviour
{
    [SerializeField]
    private GameLevelSO _levelToLoad;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            LoadLevel();
        }
    }

    public void LoadLevel()
    {
        LevelLoader.Instance.LoadLevel(_levelToLoad).Forget();
    }
}
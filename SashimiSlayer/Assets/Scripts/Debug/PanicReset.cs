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
            LevelLoader.Instance.LoadLevel(_levelToLoad).Forget();
        }
    }
}
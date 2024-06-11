using Cysharp.Threading.Tasks;
using UnityEngine;

public class SceneStarter : MonoBehaviour
{
    [SerializeField]
    private BootupConfigSO _bootupConfigSO;

    private void Start()
    {
        LevelLoader.Instance.LoadLevel(_bootupConfigSO.InitialGameLevel).Forget();
    }
}
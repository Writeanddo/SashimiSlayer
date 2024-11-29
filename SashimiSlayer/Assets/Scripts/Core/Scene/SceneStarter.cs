using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;

public class SceneStarter : MonoBehaviour
{
    [SerializeField]
    private BootupConfigSO _bootupConfigSO;

    [SerializeField]
    private StudioBankLoader _studioBankLoader;

    private void Start()
    {
        Startup().Forget();
    }

    private void OnDestroy()
    {
        _studioBankLoader.Unload();
    }

    private async UniTaskVoid Startup()
    {
        Debug.Log("Loading starting banks");
        _studioBankLoader.Load();

        await UniTask.WaitUntil(() => RuntimeManager.HaveAllBanksLoaded);
        await UniTask.WaitUntil(() => !RuntimeManager.AnySampleDataLoading());

        Debug.Log("All banks loaded");

        LevelLoader.Instance.LoadLevel(_bootupConfigSO.InitialGameLevel).Forget();
    }
}
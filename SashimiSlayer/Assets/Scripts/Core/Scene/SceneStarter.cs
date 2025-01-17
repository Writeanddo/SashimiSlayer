using Beatmapping.Tooling;
using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;

namespace Core.Scene
{
    /// <summary>
    ///     Bootstrapping script inside base scene to startup and load initial resources.
    /// </summary>
    public class SceneStarter : MonoBehaviour
    {
        [SerializeField]
        private BootupConfigSO _bootupConfigSO;

        [SerializeField]
        private LevelRosterSO _levelRosterSO;

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

            // Load startup level
            if (BeatmappingUtilities.PlayFromEditedBeatmap)
            {
                foreach (GameLevelSO level in _levelRosterSO.Levels)
                {
                    if (level.Beatmap == BeatmappingUtilities.CurrentEditingBeatmapConfig)
                    {
                        LevelLoader.Instance.LoadLevel(level).Forget();
                        return;
                    }
                }
            }

            LevelLoader.Instance.LoadLevel(_bootupConfigSO.InitialGameLevel).Forget();
        }
    }
}
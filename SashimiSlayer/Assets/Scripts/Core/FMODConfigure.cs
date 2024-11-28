using FMODUnity;
using UnityEngine;
using ADVANCEDSETTINGS = FMOD.Studio.ADVANCEDSETTINGS;

/// <summary>
///     Applies FMOD settings to the game on startup
/// </summary>
public class FMODConfigure : MonoBehaviour
{
    [SerializeField]
    private int _fmodStudioUpdatePeriodMs;

    private void Awake()
    {
        FMOD.Studio.System studioSystem = RuntimeManager.StudioSystem;
        ADVANCEDSETTINGS advancedSettings = default;
        studioSystem.getAdvancedSettings(out advancedSettings);

        advancedSettings.studioupdateperiod = _fmodStudioUpdatePeriodMs;

        studioSystem.setAdvancedSettings(advancedSettings);
    }
}
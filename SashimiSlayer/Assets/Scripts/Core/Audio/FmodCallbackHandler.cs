using System;
using FMOD;
using FMODUnity;
using UnityEngine;
using ADVANCEDSETTINGS = FMOD.Studio.ADVANCEDSETTINGS;
using Debug = UnityEngine.Debug;

namespace Core.Audio
{
    [CreateAssetMenu(menuName = "FMOD Callback Handler")]
    public class FmodCallbackHandler : PlatformCallbackHandler
    {
        [SerializeField]
        private FmodConfigSo _config;

        public override void PreInitialize(FMOD.Studio.System studioSystem, Action<RESULT, string> reportResult)
        {
            FMOD.System coreSystem;
            RESULT result = studioSystem.getCoreSystem(out coreSystem);
            reportResult(result, "studioSystem.getCoreSystem");

            // Sample rate
            coreSystem.getSoftwareFormat(out int sampleRate, out _, out _);

            Debug.Log($"FMOD sample rate: {sampleRate}");

            // Buffer size
            coreSystem.getDSPBufferSize(out uint bufferLength, out int numBuffers);
            coreSystem.setDSPBufferSize(_config.DspBufferLength, numBuffers);

            Debug.Log($"FMOD DSP buffer length set to {_config.DspBufferLength}");

            // Update period
            studioSystem.getAdvancedSettings(out ADVANCEDSETTINGS settings);
            settings.studioupdateperiod = _config.UpdatePeriodMs;
            studioSystem.setAdvancedSettings(settings);

            Debug.Log($"FMOD update period set to {_config.UpdatePeriodMs}");
        }
    }
}
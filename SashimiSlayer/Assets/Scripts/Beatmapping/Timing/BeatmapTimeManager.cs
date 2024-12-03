using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Events;
using Events.Core;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Debug = UnityEngine.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Beatmapping.Timing
{
    public class BeatmapTimeManager : MonoBehaviour
    {
        private enum BeatmapState
        {
            Unloaded,
            WaitingForSoundtrackEvent,
            Playing
        }

        public struct TickInfo
        {
            /// <summary>
            ///     DSP time since beatmap started (the 0th beat, different from load time)
            /// </summary>
            public double CurrentBeatmapTime;

            /// <summary>
            ///     DSP time in level timespace (since the level began)
            /// </summary>
            public double CurrentLevelTime;

            public double DeltaTime;
        }

        [Header("Dependencies")]

        [SerializeField]
        private GameLevelSO _levelResultLevel;

        [Header("Listening Events")]

        [SerializeField]
        private BeatmapEvent _beatmapLoadedEvent;

        [SerializeField]
        private BeatmapEvent _beatmapUnloadedEvent;

        [Header("Invoking Events")]

        [SerializeField]
        private IntEvent _beatPassedEvent;

        public static BeatmapTimeManager Instance { get; private set; }

        public TickInfo CurrentTickInfo { get; private set; }

        public event Action<TickInfo> OnTick;

        private BeatmapConfigSo _currentBeatmap;

        /// <summary>
        ///     Global dsp time of previous tick
        /// </summary>
        private double _previousDspTime;

        /// <summary>
        ///     Raw dsp time of when the beatmap starts
        /// </summary>
        private double _beatmapDspStartTime;

        private double _timeIntervalPerBeat;
        private double _timeIntervalPerSubdiv;

        private int _sampleRate;
        private ChannelGroup _masterChannel;

        private EventInstance _beatmapSoundtrackInstance;

        private BeatmapState _beatmapState = BeatmapState.Unloaded;

        private void Awake()
        {
            DOTween.KillAll();
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            Application.targetFrameRate = 100;

            _beatmapLoadedEvent.AddListener(HandleStartBeatmap);
            _beatmapUnloadedEvent.AddListener(HandleBeatmapUnloaded);
        }

        private void Update()
        {
            if (_currentBeatmap == null)
            {
                return;
            }

            switch (_beatmapState)
            {
                case BeatmapState.Unloaded:
                    break;
                case BeatmapState.WaitingForSoundtrackEvent:
                    WaitForSoundtrackEventSync();
                    break;
                case BeatmapState.Playing:
                    TickBeatmapPlaying();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDestroy()
        {
            _beatmapLoadedEvent.RemoveListener(HandleStartBeatmap);
            _beatmapUnloadedEvent.RemoveListener(HandleBeatmapUnloaded);
        }

        private void EndBeatmap()
        {
            LevelLoader.Instance.LoadLevel(_levelResultLevel).Forget();
            _beatmapState = BeatmapState.Unloaded;
        }

        private void TickBeatmapPlaying()
        {
            // If the soundtrack instsance is no longer valid, the level has ended
            if (!_beatmapSoundtrackInstance.isValid())
            {
                EndBeatmap();
            }
            else
            {
                TickPlaying();
            }
        }

        private void TickPlaying()
        {
            // Update dps time
            double currentDspTime = GetCurrentDspTime();
            double dspDeltaTime = currentDspTime - _previousDspTime;

            // Calculate time since start of beatmap
            double currentBeatmapTime = currentDspTime - _beatmapDspStartTime;
            double previousBeatmapTime = _previousDspTime - _beatmapDspStartTime;

            var currentBeatIndex = (int)Math.Floor(currentBeatmapTime / _timeIntervalPerBeat);
            var previousBeatIndex = (int)Math.Floor(previousBeatmapTime / _timeIntervalPerBeat);

            bool crossedBeatThisTick = currentBeatIndex > previousBeatIndex;

            var currentSubdivIndex = (int)Math.Floor(currentBeatmapTime / _timeIntervalPerSubdiv);
            var previousSubdivIndex = (int)Math.Floor(previousBeatmapTime / _timeIntervalPerSubdiv);

            bool crossedSubdivThisTick = currentSubdivIndex > previousSubdivIndex;

            if (crossedBeatThisTick)
            {
                _beatPassedEvent.Raise(currentBeatIndex);
            }

            // Invoke tick event
            CurrentTickInfo = new TickInfo
            {
                CurrentBeatmapTime = currentBeatmapTime,
                CurrentLevelTime = currentBeatmapTime + _currentBeatmap.StartTime,
                DeltaTime = dspDeltaTime
            };

            OnTick?.Invoke(CurrentTickInfo);

            _previousDspTime = currentDspTime;
        }

        /// <summary>
        ///     After loading a beatmap, wait for the soundtrack event to begin playing before starting the timing.
        /// </summary>
        private void WaitForSoundtrackEventSync()
        {
            if (!_beatmapSoundtrackInstance.isValid())
            {
                return;
            }

            _beatmapSoundtrackInstance.getTimelinePosition(out int currentEventTime);
            double eventTimeSeconds = currentEventTime / 1000.0;

            if (currentEventTime > 0)
            {
                double startTime = GetCurrentDspTime() - eventTimeSeconds;
                _beatmapDspStartTime = startTime + _currentBeatmap.StartTime;
                _previousDspTime = startTime;

                _timeIntervalPerBeat = 60 / _currentBeatmap.Bpm;
                _timeIntervalPerSubdiv = _timeIntervalPerBeat / _currentBeatmap.Subdivisions;

                _beatmapState = BeatmapState.Playing;
            }
        }

        private void HandleStartBeatmap(BeatmapConfigSo beatmap)
        {
            GetDspInfo();

            _currentBeatmap = beatmap;

            StartBeatmapTrack(beatmap);

            _beatmapState = BeatmapState.WaitingForSoundtrackEvent;
        }

        private void HandleBeatmapUnloaded(BeatmapConfigSo beatmap)
        {
            if (_beatmapSoundtrackInstance.isValid())
            {
                _beatmapSoundtrackInstance.stop(STOP_MODE.ALLOWFADEOUT);
            }

            _currentBeatmap = null;
        }

        private void StartBeatmapTrack(BeatmapConfigSo beatmap)
        {
            EventInstance soundtrack = RuntimeManager.CreateInstance(beatmap.BeatmapSoundtrackEvent);
            soundtrack.start();
            soundtrack.release();

            _beatmapSoundtrackInstance = soundtrack;
        }

        private double GetCurrentDspTime()
        {
            _masterChannel.getDSPClock(out ulong dspClock, out ulong parentClock);
            return (double)dspClock / _sampleRate;
        }

        private void GetDspInfo()
        {
            FMOD.System coreSystem = RuntimeManager.CoreSystem;
            coreSystem.getMasterChannelGroup(out ChannelGroup masterChannelGroup);

            RESULT getFormat =
                coreSystem.getSoftwareFormat(out int sampleRate, out SPEAKERMODE speakerMode, out int numrawspeakers);

            coreSystem.getDSPBufferSize(out uint bufferLength, out int numBuffers);

            if (getFormat != RESULT.OK)
            {
                Debug.LogError("Failed to get software format");
            }

            _sampleRate = sampleRate;
            _masterChannel = masterChannelGroup;
        }
    }
}
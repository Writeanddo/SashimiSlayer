using System;
using DG.Tweening;
using Events;
using Events.Core;
using FMOD;
using FMODUnity;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TimingService : MonoBehaviour
{
    public struct TickInfo
    {
        /// <summary>
        ///     DSP time since beatmap started (the 0th beat, different from load time)
        /// </summary>
        public double CurrentBeatmapTime;

        /// <summary>
        ///     DSP time when the beatmap was loaded
        /// </summary>
        public double TimeSinceBeatmapLoad;

        public double DeltaTime;

        public int CurrentBeatIndex;
        public int CurrentSubdivIndex;

        public double BeatQuantizedBeatmapTime;
        public double SubdivQuantizedBeatmapTime;
    }

    [Header("Listening Events")]

    [SerializeField]
    private BeatmapEvent _beatmapLoadedEvent;

    [SerializeField]
    private BeatmapEvent _beatmapUnloadedEvent;

    [Header("Invoking Events")]

    [SerializeField]
    private IntEvent _beatPassedEvent;

    [SerializeField]
    private DoubleEvent _syncTimeEvent;

    public static TimingService Instance { get; private set; }

    public TickInfo CurrentTickInfo { get; private set; }

    public event Action<TickInfo> OnTick;

    private BeatmapConfigSo _currentBeatmap;

    private double _previousDspTime;

    /// <summary>
    ///     DSP time when the beatmap loaded
    /// </summary>
    private double _beatmapDspStartTime;

    private double _timeIntervalPerBeat;
    private double _timeIntervalPerSubdiv;

    private int _ticksToSync;

    private int _sampleRate;
    private ChannelGroup _masterChannel;

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

        Tick();
        /*if (_ticksToSync > 0)
        {
            _ticksToSync--;
            _syncTimeEvent.Raise(CurrentTickInfo.CurrentBeatmapTime);
        }*/
    }

    private void OnDestroy()
    {
        _beatmapLoadedEvent.RemoveListener(HandleStartBeatmap);
        _beatmapUnloadedEvent.RemoveListener(HandleBeatmapUnloaded);
    }

    private void Tick()
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
            TimeSinceBeatmapLoad = currentBeatmapTime + _currentBeatmap.StartTime,
            DeltaTime = dspDeltaTime,
            CurrentBeatIndex = currentBeatIndex,
            CurrentSubdivIndex = currentSubdivIndex,
            BeatQuantizedBeatmapTime = currentBeatIndex * _timeIntervalPerBeat,
            SubdivQuantizedBeatmapTime = currentSubdivIndex * _timeIntervalPerSubdiv
        };

        Debug.Log("service: " + CurrentTickInfo.TimeSinceBeatmapLoad);

        OnTick?.Invoke(CurrentTickInfo);

        _previousDspTime = currentDspTime;
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

        if (getFormat != RESULT.OK)
        {
            Debug.LogError("Failed to get software format");
        }

        _sampleRate = sampleRate;
        _masterChannel = masterChannelGroup;

        Debug.Log("Sample rate: " + _sampleRate);
    }

    private void HandleStartBeatmap(BeatmapConfigSo beatmap)
    {
        GetDspInfo();

        _currentBeatmap = beatmap;

        Resync();
    }

    /// <summary>
    ///     Resync, using current time as the new start time.
    /// </summary>
    public void Resync()
    {
        Debug.Log("Resyncing to new start time");

        double dspTime = GetCurrentDspTime();
        _beatmapDspStartTime = dspTime + _currentBeatmap.StartTime;
        _previousDspTime = dspTime;

        _timeIntervalPerBeat = 60 / _currentBeatmap.Bpm;
        _timeIntervalPerSubdiv = _timeIntervalPerBeat / _currentBeatmap.Subdivisions;

        _ticksToSync = 5;
        Tick();
    }

    private void HandleBeatmapUnloaded(BeatmapConfigSo beatmap)
    {
        _currentBeatmap = null;
    }
}
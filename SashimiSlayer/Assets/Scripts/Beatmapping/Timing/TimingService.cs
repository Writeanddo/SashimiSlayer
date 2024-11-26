using System;
using DG.Tweening;
using Events;
using Events.Core;
using UnityEngine;

public class TimingService : MonoBehaviour
{
    public struct TickInfo
    {
        public double CurrentBeatmapTime;
        public double DeltaTime;

        public int CurrentBeatIndex;
        public int CurrentSubdivIndex;

        public double BeatQuantizedBeatmapTime;
        public double SubdivQuantizedBeatmapTime;
    }

    [Header("Listening Events")]

    [SerializeField]
    private BeatmapEvent _beatmapLoadedEvent;

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
    private double _beatmapDspStartTime;

    private double _timeIntervalPerBeat;
    private double _timeIntervalPerSubdiv;

    private int _ticksToSync;

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

        Application.targetFrameRate = 60;

        _beatmapLoadedEvent.AddListener(HandleStartBeatmap);
    }

    private void Update()
    {
        Tick();
        if (_ticksToSync > 0)
        {
            _ticksToSync--;
            _syncTimeEvent.Raise(CurrentTickInfo.CurrentBeatmapTime);
        }
    }

    private void OnDestroy()
    {
        _beatmapLoadedEvent.RemoveListener(HandleStartBeatmap);
    }

    private void Tick()
    {
        // Update dps time
        double currentDspTime = AudioSettings.dspTime;
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
            DeltaTime = dspDeltaTime,
            CurrentBeatIndex = currentBeatIndex,
            CurrentSubdivIndex = currentSubdivIndex,
            BeatQuantizedBeatmapTime = currentBeatIndex * _timeIntervalPerBeat,
            SubdivQuantizedBeatmapTime = currentSubdivIndex * _timeIntervalPerSubdiv
        };

        OnTick?.Invoke(CurrentTickInfo);

        _previousDspTime = currentDspTime;
    }

    private void HandleStartBeatmap(BeatmapConfigSo beatmap)
    {
        _currentBeatmap = beatmap;
        _beatmapDspStartTime = AudioSettings.dspTime + beatmap.StartTime;
        _timeIntervalPerBeat = 60 / _currentBeatmap.Bpm;
        _timeIntervalPerSubdiv = _timeIntervalPerBeat / _currentBeatmap.Subdivisions;
    }

    /// <summary>
    ///     Resync to a new start time. Used for looping
    /// </summary>
    public void Resync()
    {
        Debug.Log("Resyncing to new start time");
        _beatmapDspStartTime = AudioSettings.dspTime + _currentBeatmap.StartTime;
        _previousDspTime = AudioSettings.dspTime;
        _ticksToSync = 100000;
        Tick();
    }
}
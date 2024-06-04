using Events;
using Events.Core;
using UnityEngine;

public class TimingService : MonoBehaviour
{
    [Header("Listening Events")]

    [SerializeField]
    private BeatmapEvent _beatmapLoadedEvent;

    [Header("Invoking Events")]

    [SerializeField]
    private IntEvent _beatPassedEvent;

    public static TimingService Instance { get; private set; }

    public double DeltaTime => _deltaTime;
    public bool DidCrossBeatThisFrame => _didCrossBeatThisFrame;
    public double TimePastBeat => _timePastBeat;
    public int BeatNumber => _beatNumber;
    public double TimePerBeat => _intervalPerBeat;
    public double CurrentTime => _currentTime;
    public double CurrentBeatmapTime => _currentTime - _startTime;

    private BeatmapConfigSo _currentBeatmap;
    private int _beatNumber;

    private double _currentTime;
    private double _deltaTime;

    private bool _didCrossBeatThisFrame;

    private double _intervalPerBeat;

    private double _lastFrameTime;

    private double _startTime;
    private double _timePastBeat;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Application.targetFrameRate = 120;

        _beatmapLoadedEvent.AddListener(HandleStartBeatmap);
    }

    private void Update()
    {
        Tick();
    }

    private void OnDestroy()
    {
        _beatmapLoadedEvent.RemoveListener(HandleStartBeatmap);
    }

    private void Tick()
    {
        // Update times
        _lastFrameTime = _currentTime;
        double newTime = AudioSettings.dspTime;

        if (newTime == _lastFrameTime)
        {
            // estimate using Deltatime
            newTime += Time.deltaTime;
        }
        else
        {
            // If we somehow need to go back in time, just hold
            if (newTime < _lastFrameTime)
            {
                newTime = _lastFrameTime;
            }
        }

        _deltaTime = newTime - _currentTime;
        _currentTime = newTime;

        // Calculate if we crossed a beat
        CalculateBeatTiming(
            _lastFrameTime,
            _currentTime,
            out _timePastBeat,
            out _didCrossBeatThisFrame,
            out _beatNumber);

        if (_didCrossBeatThisFrame)
        {
            Debug.DrawLine(Vector3.right * (_beatNumber % 4), Vector3.one * 1000f, Color.magenta,
                (float)_intervalPerBeat - 0.01f);

            _beatPassedEvent.Raise(_beatNumber);
        }
    }

    private void HandleStartBeatmap(BeatmapConfigSo beatmap)
    {
        _currentBeatmap = beatmap;
        _startTime = AudioSettings.dspTime + beatmap.StartTime;
        _intervalPerBeat = 60 / _currentBeatmap.Bpm;
    }

    private void CalculateBeatTiming(
        double lastTime,
        double currentTime,
        out double timePastBeat,
        out bool didCrossBeat,
        out int beatNumber)
    {
        double elapsedTime = currentTime - _startTime;
        double lastElapsedTime = lastTime - _startTime;
        timePastBeat = elapsedTime % _intervalPerBeat;

        beatNumber = (int)(elapsedTime / _intervalPerBeat);
        var lastBeat = (int)(lastElapsedTime / _intervalPerBeat);

        didCrossBeat = beatNumber > lastBeat;
    }

    /// <summary>
    ///     Resync to a new start time. Used for looping
    /// </summary>
    /// <param name="newStartTime"></param>
    public void Resync()
    {
        Debug.Log("Resyncing to new start time");
        _startTime = AudioSettings.dspTime + _currentBeatmap.StartTime;
        _lastFrameTime = _currentTime;
        Tick();
    }
}